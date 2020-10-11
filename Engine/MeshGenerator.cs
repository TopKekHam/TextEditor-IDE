using Microsoft.VisualBasic.CompilerServices;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace R
{

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPosUV
    {
        public Vector3 pos;
        public Vector2 uv;
    }

    public static unsafe class MeshGenerator
    {

        static uint[] quad_indicies_data = new uint[] { 0, 1, 2, 0, 2, 3 };
        static float[] quad_vertices_data_position = new float[]{
         0.5f,  0.5f ,0,
         0.5f, -0.5f ,0,
        -0.5f, -0.5f ,0,
        -0.5f,  0.5f ,0,
        };

        static float[] quad_vertices_data_uvs = new float[]{
         1, 1,
         1, 0,
         0, 0,
         0, 1
        };

        public struct RamTileMapMesh
        {
            public int width, height;
            public float[] vertices;
            public uint[] indices;
        }

        public static Mesh LoadRamTileMapMeshToGPU(RamTileMapMesh ram_mesh)
        {
            Mesh mesh = new Mesh();

            mesh.format = VertexFormat.POSITION | VertexFormat.TEX_COORD;
            mesh.indices_count = (uint)ram_mesh.indices.Length;

            mesh.vertex_buffer = GFX.CreateBuffer();
            GFX.BufferFloats(mesh.vertex_buffer, ram_mesh.vertices, BufferType.VERTEX);

            mesh.index_buffer = GFX.CreateBuffer();
            GFX.BufferUints(mesh.index_buffer, ram_mesh.indices, BufferType.INDEX);

            return mesh;
        }

        public static RamTileMapMesh GenerateTileMapMesh(int width, int height)
        {
            RamTileMapMesh tile_map_mesh = new RamTileMapMesh();

            tile_map_mesh.width = width;
            tile_map_mesh.height = height;

            tile_map_mesh.vertices = new float[(width * height) * 4 * 5]; //w * h tiles * 4 vetices per tile * 5 components in vertex.
            tile_map_mesh.indices = new uint[(width * height) * 6]; //w * h tiles * 6 indices per tile.

            var verts = tile_map_mesh.vertices;
            var inds = tile_map_mesh.indices;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = x + (y * height);

                    //verts
                    {
                        int v_pos = pos * 20;

                        verts[v_pos] = x + 1;
                        verts[v_pos + 1] = -y - 1;
                        verts[v_pos + 2] = 0;
                        verts[v_pos + 3] = 1;
                        verts[v_pos + 4] = 0;

                        verts[v_pos + 5] = x + 1;
                        verts[v_pos + 6] = -y;
                        verts[v_pos + 7] = 0;
                        verts[v_pos + 8] = 1;
                        verts[v_pos + 9] = 1;

                        verts[v_pos + 10] = x;
                        verts[v_pos + 11] = -y;
                        verts[v_pos + 12] = 0;
                        verts[v_pos + 13] = 0;
                        verts[v_pos + 14] = 1;

                        verts[v_pos + 15] = x;
                        verts[v_pos + 16] = -y - 1;
                        verts[v_pos + 17] = 0;
                        verts[v_pos + 18] = 0;
                        verts[v_pos + 19] = 0;
                    }

                    //inds
                    int i_pos = pos * 6;
                    uint s = (uint)pos * 4;

                    inds[i_pos] = s;
                    inds[i_pos + 1] = s + 1;
                    inds[i_pos + 2] = s + 2;
                    inds[i_pos + 3] = s;
                    inds[i_pos + 4] = s + 2;
                    inds[i_pos + 5] = s + 3;

                }
            }

            return tile_map_mesh;
        }

        public static void SetTile(RamTileMapMesh tilemap, int x, int y, int tile_set_width, int tile_set_height, int tile_x, int tile_y)
        {

            var verts = tilemap.vertices;
            int pos = (x + (y * tilemap.height));

            float unit_width = 1.0f / (float)tile_set_width;
            float unit_height = 1.0f / (float)tile_set_height;

            int v_pos = pos * 20;

            verts[v_pos + 3] = unit_width * (float)(tile_x + 1);
            verts[v_pos + 4] = 1 - (unit_height * (float)(tile_y));

            verts[v_pos + 8] = unit_width * (float)(tile_x + 1);
            verts[v_pos + 9] = 1 - (unit_height * (float)(tile_y + 1));

            verts[v_pos + 13] = unit_width * (float)(tile_x);
            verts[v_pos + 14] = 1 - (unit_height * (float)(tile_y + 1));

            verts[v_pos + 18] = unit_width * (float)(tile_x);
            verts[v_pos + 19] = 1 - (unit_height * (float)(tile_y));

        }

        public static Mesh GenerateQuad(Vector2 size, VertexFormat format)
        {
            int stride = GFX.GetVertexSize(format) / sizeof(float);
            float[] vertices = new float[4 * stride];

            for (int i = 0; i < 4; i++)
            {
                int padding = 0;

                if(format.HasFlag(VertexFormat.POSITION))
                {
                    vertices[(stride * i) + padding + 0] = quad_vertices_data_position[(i * 3) + 0] * size.X;
                    vertices[(stride * i) + padding + 1] = quad_vertices_data_position[(i * 3) + 1] * size.Y;
                    vertices[(stride * i) + padding + 2] = quad_vertices_data_position[(i * 3) + 2];
                    padding += 3;
                }

                if (format.HasFlag(VertexFormat.TEX_COORD))
                {
                    vertices[(stride * i) + padding + 0] = quad_vertices_data_uvs[(i * 2) + 0];
                    vertices[(stride * i) + padding + 1] = quad_vertices_data_uvs[(i * 2) + 1];
                    padding += 2;
                }
            }

            Mesh quad = new Mesh();
            
            quad.vertex_buffer = GFX.CreateBuffer();
            GFX.BufferFloats(quad.vertex_buffer, vertices);

            quad.index_buffer = GFX.CreateBuffer();
            GFX.BufferUints(quad.index_buffer, quad_indicies_data, BufferType.INDEX);
            
            quad.format = format;
            quad.indices_count = 6;
            
            return quad;
        }

    }
}
