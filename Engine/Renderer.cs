using System.Numerics;
using static R.GFX;
using static R.ArrayList<R.RenderItem>;
using System.Runtime.InteropServices;

namespace R
{

    public enum RenderItemType : ushort
    {
        Mesh, Quad, TextAscii
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RenderItem
    {
        public Transform transform;
        public Mesh mesh;
        public Material material;
        public bool free_mesh;
    }

    public unsafe static class Renderer
    {

        static float[] quad_vertices_data = new float[]{
         0.5f,  0.5f ,0, 1, 1,
         0.5f, -0.5f ,0, 1, 0,
        -0.5f, -0.5f ,0, 0, 0,
        -0.5f,  0.5f ,0, 0, 1
        };

        static uint[] quad_indicies_data = new uint[]{
        0,1,2,
        0,2,3
    };

        static uint color_shader;
        static uint image_shader;
        static uint tinted_image_shader;
        static uint quad_vert_buffer;
        static uint quad_ind_buffer;

        public static Mesh QuadOne;
        public static bool FlipY = false;
        public static ArrayList<RenderItem> render_item_buffer;

        public static Transform CameraPosition = Transform.Zero;
        public static Matrix4x4 CameraProjection = Matrix4x4.Identity;

        #region shaders

        static string vert =
        Opengl.ShaderVersion + @"
        layout (location = 0) in vec4 position;

        uniform mat4 mat_model; 
        uniform mat4 mat_view_projection; 

        void main() {
        gl_Position = mat_view_projection * mat_model * position;
        }";

        static string frag =
        Opengl.ShaderVersion + @"
        uniform vec4 color_0;
	    out vec4 fragColor;
        void main() {
        fragColor = color_0;
        }";

        static string vert_image =
        Opengl.ShaderVersion + @"
        layout (location = 0) in vec4 position;
        layout (location = 1) in vec2 tex_coord;

        uniform mat4 mat_model; 
        uniform mat4 mat_view_projection; 

        out vec2 _tex_coord;
        void main() {
        _tex_coord = tex_coord;
        gl_Position = mat_view_projection * mat_model * position;
        }";

        static string frag_image =
        Opengl.ShaderVersion + @"
        uniform sampler2D tex_0;
        in vec2 _tex_coord;
	    out vec4 fragColor;

        void main() {
        fragColor = texture(tex_0, _tex_coord);
        }";

        static string frag_tinted_image =
        Opengl.ShaderVersion + @"
        uniform sampler2D tex_0;
        uniform vec4 tint;
        in vec2 _tex_coord;
	    out vec4 fragColor;

        void main() {
        fragColor = texture(tex_0, _tex_coord) * tint;
        }";

        #endregion

        public static void Init()
        {
            color_shader = LoadShader(vert, frag);
            image_shader = LoadShader(vert_image, frag_image);
            tinted_image_shader = LoadShader(vert_image, frag_tinted_image);

            quad_vert_buffer = CreateBuffer();
            BufferFloats(quad_vert_buffer, quad_vertices_data, BufferType.VERTEX);

            quad_ind_buffer = CreateBuffer();
            BufferUints(quad_ind_buffer, quad_indicies_data, BufferType.INDEX);

            QuadOne = new Mesh()
            {
                index_buffer = quad_ind_buffer,
                vertex_buffer = quad_vert_buffer,
                indices_count = 6,
                format = VertexFormat.POSITION | VertexFormat.TEX_COORD
            };

            render_item_buffer = Create();

            GFX.SetBlendMode(BlendMode.Alpha);
            GFX.EnableDepthTest();
            SetCameraSize(1, 1);
        }

        public static void SetCameraSize(float half_width, float half_heigh)
        {
            CameraProjection = Matrix4x4.CreateOrthographicOffCenter(-half_width, half_width, -half_heigh, half_heigh, 0.3f, 1000);
        }

        public static void DrawQuad(Transform transform, Vector4 color)
        {
            Material mat = new Material
            {
                shader = color_shader,
                uniform_params = new UniformParam[]
                {
                    new UniformParam { name = "color_0", vec4 = color, type = UniformType.Vector4 }
                }
            };

            Mesh mesh = new Mesh
            {
                format = VertexFormat.POSITION | VertexFormat.TEX_COORD,
                index_buffer = quad_ind_buffer,
                vertex_buffer = quad_vert_buffer,
                indices_count = 6
            };

            DrawMesh(transform, mat, mesh);
        }

        public static void DrawQuad(Transform transform, uint texture)
        {
            Material mat = CreateImageMaterail(texture);

            Mesh mesh = new Mesh
            {
                format = VertexFormat.POSITION | VertexFormat.TEX_COORD,
                index_buffer = quad_ind_buffer,
                vertex_buffer = quad_vert_buffer,
                indices_count = 6
            };

            DrawMesh(transform, mat, mesh);
        }

        public static void DrawQuad(Transform transform, Vector2 size, Vector4 color)
        {
            Material mat = new Material
            {
                shader = color_shader,
                uniform_params = new UniformParam[]
                {
                    new UniformParam { name = "color_0", vec4 = color, type = UniformType.Vector4 }
                }
            };

            Mesh mesh = MeshGenerator.GenerateQuad(size, VertexFormat.POSITION);

            DrawMesh(transform, mat, mesh);

            FreeMesh(mesh);
        }

        public static void DrawQuad(Transform transform, Vector2 size, uint texture)
        {
            Material mat = new Material
            {
                shader = image_shader,
                uniform_params = new UniformParam[]
                {
                    new UniformParam { name = "tex_0", texture = texture, type = UniformType.Texture }
                }
            };

            Mesh mesh = MeshGenerator.GenerateQuad(size, VertexFormat.POSITION | VertexFormat.TEX_COORD);

            DrawMesh(transform, mat, mesh);

            FreeMesh(mesh);
        }

        public static void DrawQuad(Transform transform, Vector2 size, Material mat)
        {

            Mesh mesh = MeshGenerator.GenerateQuad(size, VertexFormat.POSITION | VertexFormat.TEX_COORD);

            DrawMesh(transform, mat, mesh);

            FreeMesh(mesh);
        }

        public static void DrawMesh(Transform transform, Material material, Mesh mesh)
        {
            if (FlipY)
            {
                transform.position.Y *= -1;
            }

            BindMesh(mesh);
            BindMaterial(material);
            BindCameraView(material.shader, transform, CameraPosition, CameraProjection);
            Draw(mesh.indices_count);
        }

        public static uint BufferTexture(RamTexture texture)
        {
            return GFX.BufferTexture(texture.data, texture.width, texture.height, TextureType.BIT_32);
        }

        public static Material CreateImageMaterail(uint texture)
        {
            Material mat = new Material();

            mat.shader = image_shader;
            mat.uniform_params = new UniformParam[] { new UniformParam { name = "tex_0", texture = texture, type = UniformType.Texture } };

            return mat;
        }

        public static Material CreateColorMaterail(Vector4 color)
        {
            Material mat = new Material();

            mat.shader = color_shader;
            mat.uniform_params = new UniformParam[] { new UniformParam { name = "color_0", vec4 = color, type = UniformType.Vector4 } };

            return mat;
        }

        public static void DrawTextAscii(Transform transform, FontAscii font, string text, Vector4 color, float size)
        {
            var text_mesh = Fonts.GenerateTextMesh(font, text, size);

            Material mat = new Material();
            mat.shader = tinted_image_shader;
            mat.uniform_params = new UniformParam[] {
                new UniformParam { name = "tex_0", texture = font.texture, type = UniformType.Texture },
                new UniformParam { name = "tint", vec4 = color, type = UniformType.Vector4}
            };

            DrawMesh(transform, mat, text_mesh);

            FreeMesh(text_mesh);
        }

        public static void AddRenderItem(RenderItem item)
        {
            render_item_buffer = AddItem(render_item_buffer, item);
        }

        public static void AddQuadRenderItem(Transform transform, Vector2 size, Material material)
        {
            render_item_buffer = AddItem(render_item_buffer, new RenderItem()
            {
                mesh = MeshGenerator.GenerateQuad(size, VertexFormat.TEX_COORD | VertexFormat.TEX_COORD),
                free_mesh= true,
                material = material,
                transform = transform
            });
        }

        public static void AddTextAsciiRenderItem(Transform transform, FontAscii font, string text, Vector4 color, float size)
        {
            render_item_buffer = AddItem(render_item_buffer, new RenderItem()
            {
                mesh = Fonts.GenerateTextMesh(font, text, size),
                free_mesh = true,
                material = CreateColorMaterail(color),
                transform = transform
            });
        }

        public static void FlushRenderItemBuffer()
        {
            for (int i = 0; i < render_item_buffer.count; i++)
            {
                var item = render_item_buffer[i];
                DrawMesh(item.transform, item.material, item.mesh);

                if(item.free_mesh)
                {
                    GFX.FreeMesh(item.mesh);
                }
            }

            render_item_buffer.count = 0;
        }

    }

}