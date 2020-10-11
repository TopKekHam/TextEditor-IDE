using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace R
{
    public class Fonts
    {
        public static readonly float character_in_line = 16;

        public static Mesh GenerateTextMesh(FontAscii font, string text, float size = 16)
        {
            Mesh mesh = new Mesh();

            float[] vertices = new float[text.Length * 4 * 5];
            uint[] indices = new uint[text.Length * 6];

            float padding_x = 0;
            float padding_y = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    padding_y -= (font.pixel_size * font.d_pixel_line_height) * size * character_in_line;
                    padding_x = 0;
                    continue;
                }

                int g = (int)text[i];
                if (g > 255) g = 0;
                Glyph glyph = font.glyphs[g];

                //verts
                {
                    int v_pos = i * 4 * 5;

                    float glyph_top = (-glyph.height * character_in_line * size) + padding_y;
                    float glyph_right_side = (glyph.width * character_in_line * size) + padding_x;

                    vertices[v_pos] = glyph_right_side;
                    vertices[v_pos + 1] = glyph_top;
                    vertices[v_pos + 2] = 0;
                    vertices[v_pos + 3] = glyph.uv_top_right.X;
                    vertices[v_pos + 4] = glyph.uv_bottom_left.Y;

                    vertices[v_pos + 5] = glyph_right_side;
                    vertices[v_pos + 6] = padding_y;
                    vertices[v_pos + 7] = 0;
                    vertices[v_pos + 8] = glyph.uv_top_right.X;
                    vertices[v_pos + 9] = glyph.uv_top_right.Y;

                    vertices[v_pos + 10] = padding_x;
                    vertices[v_pos + 11] = padding_y;
                    vertices[v_pos + 12] = 0;
                    vertices[v_pos + 13] = glyph.uv_bottom_left.X;
                    vertices[v_pos + 14] = glyph.uv_top_right.Y;

                    vertices[v_pos + 15] = padding_x;
                    vertices[v_pos + 16] = glyph_top;
                    vertices[v_pos + 17] = 0;
                    vertices[v_pos + 18] = glyph.uv_bottom_left.X;
                    vertices[v_pos + 19] = glyph.uv_bottom_left.Y;

                    padding_x = glyph_right_side + (font.pixel_size * character_in_line * size * font.d_pixel_between_characters);
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

            mesh.format = VertexFormat.POSITION | VertexFormat.TEX_COORD;
            mesh.indices_count = (uint)indices.Length;

            return mesh;
        }

        //return the maximum length of the text in pixels.
        public static float GetTextWidth(FontAscii font, string text, float size, int number_of_chars = -1)
        {
            float text_width = 0;
            float max_width = 0;

            if(number_of_chars == -1)
            {
                number_of_chars = text.Length;
            }

            for (int i = 0; i < number_of_chars; i++)
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
                Glyph glyph = font.glyphs[g];
                text_width += (glyph.width + (font.d_pixel_between_characters * (i == text.Length - 1 ? 0 : font.pixel_size))) * character_in_line * size;
            }

            if (max_width < text_width)
            {
                max_width = text_width;
            }

            return max_width;
        }

        //return the height of the text in .
        public static float GetTextHeight(FontAscii font, string text, float size)
        {
            
            float line_count = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    line_count += 1;
                }
            }

            float single_line = (font.pixel_size * font.d_pixel_line_height) * size * character_in_line;
            float first_line = (font.pixel_size * font.glyph_size_in_pixels) * size * character_in_line;
            return first_line + (line_count * single_line);
        }

        public static float GlyphHeight(FontAscii font, char c, float size)
        {
            return font.glyphs[(int)c].height * character_in_line * (size / font.glyph_size_in_pixels);
           // return font.pixel_size * font.glyph_size_in_pixels * size  * glyph_height;
        }

        public static Vector3 GetCharPosition(FontAscii font, string text, float size, int character_idx)
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

            float single_line = (font.pixel_size * font.d_pixel_line_height) * size * character_in_line;
            float first_line = (font.pixel_size * font.glyph_size_in_pixels) * size * character_in_line;
            position.Y = (first_line + (line_number * single_line));

            //x axis
            int last_char = character_idx - line_first_char_idx;

            for (int i = 0; i < last_char; i++)
            {
                int g = (int)text[line_first_char_idx + i];
                Glyph glyph = font.glyphs[g];
                float glyph_size = glyph.width * character_in_line * size;
                float padding = ((font.pixel_size * font.d_pixel_between_characters) * character_in_line * size);
                position.X += glyph_size + padding;
            }

            return position;
        }

        public static float GetLineHeight(FontAscii font, float size)
        {
            return font.pixel_size * font.d_pixel_line_height * size * character_in_line;
        }

        public static float GetGlyphSize(FontAscii font, float size)
        {
            return font.pixel_size * font.glyph_size_in_pixels * size * character_in_line;
        }

    }
}
