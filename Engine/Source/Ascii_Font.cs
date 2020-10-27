using System.Numerics;
using System.Runtime.InteropServices;

namespace R
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Ascii_Glyph
    {
        public Vector2 uv_bottom_left;
        public Vector2 uv_top_right;
        public float width, height;
        public int base_line_offset;
        public int min_x, max_x, min_y, max_y; // used only while building font.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Ascii_Font
    {
        public Ascii_Glyph[] glyphs;
        public string texture_path; // can be null if loaded from TTF file.
        public uint texture;
        public int texture_width, texture_height, glyph_size_in_pixels;
        public float pixel_size;
        public int natural_offset;

        //this values can be changed freely.
        public float d_ratio_between_characters;
        public float d_ratio_line_height;
    }

    public class Ascii_Font_Utils
    {
        public static uint shader;

        static string vert_ascii_text =
        Opengl.ShaderVersion + @"
        layout (location = 0) in vec4 position;
        layout (location = 1) in vec2 tex_coord;
        layout (location = 3) in vec4 color;

        uniform mat4 mat_model; 
        uniform mat4 mat_view_projection; 

        out vec2 _tex_coord;
        out vec4 char_color;

        void main() {
            _tex_coord = tex_coord;
            char_color = color;
            gl_Position = mat_view_projection * mat_model * position;
        }";

        static string frag_ascii_text =
        Opengl.ShaderVersion + @"
            uniform sampler2D tex_0;
            uniform vec4 tint;

            in vec2 _tex_coord;
            in vec4 char_color;

	        out vec4 fragColor;

            void main() {
                fragColor = texture(tex_0, _tex_coord) * char_color * tint;
            }
        ";

        public static readonly float character_in_line = 16;

        static Ascii_Font_Utils()
        {
            shader = GFX.LoadShader(vert_ascii_text, frag_ascii_text);
        }

        public unsafe static Mesh GenerateTextMesh(Ascii_Font font, string text, float size, Vector4[] colors)
        {
            fixed(Vector4* colors_ptr = colors)
            {
                return GenerateTextMesh(font, text, size, colors_ptr);
            }
        }

        public unsafe static Mesh GenerateTextMesh(Ascii_Font font, string text, float size, ArrayList<Vector4> colors)
        {
            fixed (Vector4* colors_ptr = colors.data)
            {
                return GenerateTextMesh(font, text, size, colors_ptr);
            }
        }

        public unsafe static Mesh GenerateTextMesh(Ascii_Font font, string text, float size, Vector4* colors)
        {
            Mesh mesh = new Mesh();
            int char_size = 4 * (3 + 2 + 4); // 4 vertices ,3 pos components, 2 uv components , 4 color components (component is f32).
            float[] vertices = new float[text.Length * char_size];
            uint[] indices = new uint[text.Length * 6];

            float padding_x = 0;
            float padding_y = font.pixel_size * (float)font.natural_offset * size * character_in_line;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    padding_y -= font.d_ratio_line_height * size * character_in_line;
                    padding_x = 0;
                    continue;
                }

                int g = (int)text[i];
                if (g > 255) g = 0;
                Ascii_Glyph glyph = font.glyphs[g];

                //verts
                {
                    int v_pos = i * char_size; 

                    float line_base_offset = (font.pixel_size * (float)glyph.base_line_offset) * size * character_in_line; 

                    float height = (glyph.height * character_in_line * size);
                    float width = (glyph.width * character_in_line * size) + padding_x;
                    float offset_y = -1 * (height - size);
                    Vector4 color = colors[i];

                    //vert 1 - top/right
                    vertices[v_pos] = width;
                    vertices[v_pos + 1] = padding_y + line_base_offset - offset_y;
                    vertices[v_pos + 2] = 0;
                    //uv
                    vertices[v_pos + 3] = glyph.uv_top_right.X;
                    vertices[v_pos + 4] = glyph.uv_top_right.Y;
                    //color
                    vertices[v_pos + 5] = color.X;
                    vertices[v_pos + 6] = color.Y;
                    vertices[v_pos + 7] = color.Z;
                    vertices[v_pos + 8] = color.W;

                    //vert 2 - bottom/right
                    vertices[v_pos + 9] = width;
                    vertices[v_pos + 10] = padding_y + line_base_offset - height - offset_y;
                    vertices[v_pos + 11] = 0;
                    //uv
                    vertices[v_pos + 12] = glyph.uv_top_right.X;
                    vertices[v_pos + 13] = glyph.uv_bottom_left.Y;
                    //color
                    vertices[v_pos + 14] = color.X;
                    vertices[v_pos + 15] = color.Y;
                    vertices[v_pos + 16] = color.Z;
                    vertices[v_pos + 17] = color.W;

                    //vert 3 - bottom/left
                    vertices[v_pos + 18] = padding_x;
                    vertices[v_pos + 19] = padding_y + line_base_offset - height - offset_y;
                    vertices[v_pos + 20] = 0;
                    //uv
                    vertices[v_pos + 21] = glyph.uv_bottom_left.X;
                    vertices[v_pos + 22] = glyph.uv_bottom_left.Y;
                    //color
                    vertices[v_pos + 23] = color.X;
                    vertices[v_pos + 24] = color.Y;
                    vertices[v_pos + 25] = color.Z;
                    vertices[v_pos + 26] = color.W;

                    //vert 4 - top/left
                    vertices[v_pos + 27] = padding_x;
                    vertices[v_pos + 28] = padding_y + line_base_offset - offset_y;
                    vertices[v_pos + 29] = 0;
                    //uv
                    vertices[v_pos + 30] = glyph.uv_bottom_left.X;
                    vertices[v_pos + 31] = glyph.uv_top_right.Y;
                    //color
                    vertices[v_pos + 32] = color.X;
                    vertices[v_pos + 33] = color.Y;
                    vertices[v_pos + 34] = color.Z;
                    vertices[v_pos + 35] = color.W;

                    padding_x = width + (size * font.pixel_size * font.d_ratio_between_characters * character_in_line);
                }

                //inds
                {
                    int i_pos = i * 6;
                    uint s = (uint)i * 4;

                    indices[i_pos] = s;
                    indices[i_pos + 1] = s + 1;
                    indices[i_pos + 2] = s + 2;
                    indices[i_pos + 3] = s;
                    indices[i_pos + 4] = s + 2;
                    indices[i_pos + 5] = s + 3;
                }
            }

            mesh.vertex_buffer = GFX.CreateBuffer();
            GFX.BufferFloats(mesh.vertex_buffer, vertices, BufferType.VERTEX);

            mesh.index_buffer = GFX.CreateBuffer();
            GFX.BufferUints(mesh.index_buffer, indices, BufferType.INDEX);

            mesh.format = VertexFormat.POSITION | VertexFormat.TEX_COORD | VertexFormat.COLOR;
            mesh.indices_count = (uint)indices.Length;

            GFX.BufferMesh(ref mesh);

            return mesh;
        }

        //return the maximum length of the text in pixels.
        public static float GetTextWidth(Ascii_Font font, string text, float size, int number_of_chars = -1, int start_idx = -1)
        {
            float text_width = 0;
            float max_width = 0;

            if(number_of_chars == -1)
            {
                number_of_chars = text.Length;
            }

            if(start_idx == -1)
            {
                start_idx = 0;
            }

            for (int i = start_idx; i < start_idx + number_of_chars; i++)
            {
                if (text[i] == '\n')
                {
                    if (max_width < text_width)
                    {
                        max_width = text_width;
                    }

                    text_width = 0;
                    continue;
                }

                int g = (int)text[i];
                Ascii_Glyph glyph = font.glyphs[g];
                //text_width += (glyph.width + (font.d_ratio_between_characters * font.glyph_size_in_pixels * (i == text.Length - 1 ? 0 : font.pixel_size))) * character_in_line * size;
                text_width += (glyph.width + (font.d_ratio_between_characters * (i == text.Length - 1 ? 0 : font.pixel_size))) * character_in_line * size;
            }

            if (max_width < text_width)
            {
                max_width = text_width;
            }

            return max_width;
        }

        //return the height of the text in .
        public static float GetTextHeight(Ascii_Font font, string text, float size)
        {
            
            float line_count = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    line_count += 1;
                }
            }

            float single_line = (font.pixel_size * font.d_ratio_line_height * font.glyph_size_in_pixels) * size * character_in_line;
            float first_line = (font.pixel_size * font.glyph_size_in_pixels) * size * character_in_line;
            return first_line + (line_count * single_line);
        }

        public static float GlyphHeight(Ascii_Font font, char c, float size)
        {
            return font.glyphs[(int)c].height * character_in_line * (size / font.glyph_size_in_pixels);
           // return font.pixel_size * font.glyph_size_in_pixels * size  * glyph_height;
        }

        public static Vector3 GetCharPosition(Ascii_Font font, string text, float size, int character_idx)
        {
            float character_in_line = 16;
            Vector3 position = Vector3.Zero;

            // y axis

            float line_number = 0;
            int line_first_char_idx = 0;

            for (int i = 0; i < character_idx; i++)
            {
                if (text[i] == '\n')
                {
                    line_number++;
                    line_first_char_idx = i + 1;
                }
            }

            float single_line = (font.pixel_size * font.d_ratio_line_height * font.glyph_size_in_pixels) * size * character_in_line;
            float first_line = (font.pixel_size * font.glyph_size_in_pixels) * size * character_in_line;
            position.Y = (first_line + (line_number * single_line));

            //x axis
            int last_char = character_idx - line_first_char_idx;

            for (int i = 0; i < last_char; i++)
            {
                int g = (int)text[line_first_char_idx + i];
                Ascii_Glyph glyph = font.glyphs[g];
                float glyph_size = glyph.width * character_in_line * size;
                float padding = ((font.pixel_size * font.d_ratio_between_characters * font.glyph_size_in_pixels) * character_in_line * size);
                position.X += glyph_size + padding;
            }

            return position;
        }

        public static float GetLineHeight(Ascii_Font font, float size)
        {
            return font.pixel_size * font.d_ratio_line_height * font.glyph_size_in_pixels * size * character_in_line;
        }

        public static float GetGlyphSize(Ascii_Font font, float size)
        {
            return font.pixel_size * font.glyph_size_in_pixels * size * character_in_line;
        }
    }
}
