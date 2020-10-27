using HI;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using static Opengl;
using static R.Utils;

namespace R
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Mesh
    {
        public uint array_buffer;
        public uint vertex_buffer;
        public uint index_buffer;
        public VertexFormat format;
        public uint indices_count;
    }

    public enum BufferType
    {
        VERTEX, INDEX
    }

    public enum BlendFunction : uint
    {
        BLEND_ZERO = 0,
        BLEND_ONE = 1,
        BLEND_ONE_MINUS_SRC_ALPHA = 0x0303,
        BLEND_SRC_ALPHA = 0x0302,
    }

    public enum BlendMode
    {
        Alpha
    }

    [Flags]
    public enum VertexFormat : uint
    {
        POSITION = 1,
        TEX_COORD = 2,
        NORMAL = 4,
        COLOR = 8,
    }

    public enum VertexComponentPosition : uint
    {
        POSITION = 0,
        TEXCOORD = 1,
        NORMAL = 2,
        COLOR = 3,
    }

    public enum TextureType
    {
        BIT_32, BIT_8
    }

    public enum UniformType : short
    {
        Vector2, Vector3, Vector4, Texture
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct UniformParam
    {
        [FieldOffset(0)] public string name;
        [FieldOffset(8)] public UniformType type;

        [FieldOffset(10)] public Vector2 vec2;
        [FieldOffset(10)] public Vector3 vec3;
        [FieldOffset(10)] public Vector4 vec4;
        [FieldOffset(10)] public uint texture;

        [FieldOffset(10)] private fixed byte padding[4 * 16];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Material
    {
        public uint shader;
        public UniformParam[] uniform_params;
    }

    public unsafe class GFX
    {
        public static void ClearColor(Vector4 color)
        {
            glClearColor(color.X, color.Y, color.Z, color.W);
        }

        public static void ClearColorBuffer()
        {
            glClear(GL_Enum.COLOR_BUFFER_BIT);
        }

        public static void ClearDepth()
        {
            glClear(GL_Enum.DEPTH_BUFFER_BIT);
        }

        public static void ClearStencil()
        {
            glClear(GL_Enum.STENCIL_BUFFER_BIT);
        }

        public static void StencilClearValue(int value)
        {
            glClearStencil(value);
        }

        public static void ClearAll()
        {
            glClear(GL_Enum.COLOR_BUFFER_BIT | GL_Enum.DEPTH_BUFFER_BIT | GL_Enum.STENCIL_BUFFER_BIT);
        }

        public static void EnableDepthTest()
        {
            glEnable(GL_Enum.DEPTH_TEST);
        }

        public static void DisableDepthTest()
        {
            glDisable(GL_Enum.DEPTH_TEST);
        }

        public static void EnableStencilTest()
        {
            glEnable(GL_Enum.STENCIL_TEST);
        }

        public static void DisableStencilTest()
        {
            glDisable(GL_Enum.STENCIL_TEST);
        }

        public static void StencilWrite()
        {
            glStencilMask(int.MaxValue); // Writing = ON
            glStencilFunc(GL_Enum.ALWAYS, 1, int.MaxValue); // Always "add" to frame
            glStencilOp(GL_Enum.REPLACE, GL_Enum.REPLACE, GL_Enum.REPLACE); // Replace on success
            //Anything rendered here becomes "cut" frame.
        }

        public static void StencilCull(bool inside)
        {
            glStencilMask(0); // Writing = OFF
            glStencilFunc(inside ? GL_Enum.NOTEQUAL : GL_Enum.EQUAL, 1, int.MaxValue); // Anything that wasn't defined above will not be rendered.
            glStencilOp(GL_Enum.KEEP, GL_Enum.KEEP, GL_Enum.KEEP);
            //Anything rendered here will be cut if goes beyond frame defined before.
        }

        public static uint CreateBuffer()
        {
            uint buffer;
            glGenBuffers(1, &buffer);
            return buffer;
        }

        public static void BindBuffer(uint buffer, BufferType type)
        {
            if (type == BufferType.VERTEX)
            {
                glBindBuffer(GL_Enum.ARRAY_BUFFER, (uint)buffer);
            }
            else if (type == BufferType.INDEX)
            {
                glBindBuffer(GL_Enum.ELEMENT_ARRAY_BUFFER, (uint)buffer);
            }
        }

        public static void BufferBytes(uint buffer, byte[] data, BufferType type)
        {
            fixed (void* ptr = data) { BufferData(buffer, ptr, data.Length, type); }
        }

        public static void BufferFloats(uint buffer, float[] data, BufferType type = BufferType.VERTEX)
        {
            fixed (void* ptr = data) { BufferData(buffer, ptr, data.Length * 4, type); }
        }

        public static void BufferUints(uint buffer, uint[] data, BufferType type = BufferType.INDEX)
        {
            fixed (void* ptr = data) { BufferData(buffer, ptr, data.Length * 4, type); }
        }

        public static void BufferData(uint buffer, void* data, int length, BufferType type)
        {

            BindBuffer(buffer, type);

            if (type == BufferType.VERTEX)
            {
                glBufferData(GL_Enum.ARRAY_BUFFER, length, data, GL_Enum.STATIC_DRAW);
            }
            else if (type == BufferType.INDEX)
            {
                glBufferData(GL_Enum.ELEMENT_ARRAY_BUFFER, length, data, GL_Enum.STATIC_DRAW);
            }

        }

        public static void DeleteBuffer(uint buffer)
        {
            glDeleteBuffers(1, &buffer);
        }

        public static void BindShader(uint shader)
        {
            glUseProgram((uint)shader);
        }

        public static void BufferMesh(ref Mesh mesh)
        {
            uint array_buffer;
            glGenVertexArrays(1, &array_buffer);
            glBindVertexArray(array_buffer);

            mesh.array_buffer = array_buffer;
            BindBuffer(mesh.vertex_buffer, BufferType.VERTEX);
            SetupVertexAttrib(mesh.format);
            BindBuffer(mesh.index_buffer, BufferType.INDEX);
        }

        public static void BindMesh(Mesh mesh)
        {
            glBindVertexArray(mesh.array_buffer);
        }

        public static void DeleteMesh(Mesh mesh)
        {
            glDeleteBuffers(1, &mesh.vertex_buffer);
            glDeleteBuffers(1, &mesh.index_buffer);
            glDeleteVertexArrays(1, &mesh.array_buffer);
        }

        public static void BindTexture(uint buffer)
        {
            glBindTexture(GL_Enum.TEXTURE_2D, buffer);
        }

        public static uint BufferTexture(byte[] data, int width, int height, TextureType type)
        {
            unsafe
            {
                fixed (void* ptr = data)
                {
                    return BufferTexture(ptr, width, height, type);
                }
            }
        }

        public static uint CreateTexture()
        {
            uint buffer;
            glGenTextures(1, &buffer);
            return buffer;
        }

        public static uint BufferTexture(void* data, int width, int height, TextureType type, int texture_buffer = -1, bool nearest = true)
        {
            uint buffer;

            if (texture_buffer == -1)
            {
                glGenTextures(1, &buffer);
                glBindTexture(GL_Enum.TEXTURE_2D, (uint)buffer);
            }
            else
            {
                buffer = (uint)texture_buffer;
            }

            GL_Enum mode = GL_Enum.NEAREST;

            if(!nearest)
            {
                mode = GL_Enum.LINEAR;
            }

            glTexParameteri(GL_Enum.TEXTURE_2D, GL_Enum.TEXTURE_MIN_FILTER, mode);
            glTexParameteri(GL_Enum.TEXTURE_2D, GL_Enum.TEXTURE_MAG_FILTER, mode);
            glTexParameteri(GL_Enum.TEXTURE_2D, GL_Enum.TEXTURE_WRAP_S, GL_Enum.MIRRORED_REPEAT);
            glTexParameteri(GL_Enum.TEXTURE_2D, GL_Enum.TEXTURE_WRAP_T, GL_Enum.MIRRORED_REPEAT);

            if (type == TextureType.BIT_8)
            {
                glTexImage2D(GL_Enum.TEXTURE_2D, 0, GL_Enum.ALPHA, width, height, 0, GL_Enum.ALPHA, GL_Enum.UNSIGNED_BYTE, data);
            }
            else
            {
                glTexImage2D(GL_Enum.TEXTURE_2D, 0, GL_Enum.RGBA, width, height, 0, GL_Enum.BGRA, GL_Enum.UNSIGNED_BYTE, data);
            }

            return buffer;
        }

        public static void ActivateTexture(int index)
        {
            glActiveTexture(GL_Enum.TEXTURE0 + (uint)index);
        }

        public static void SetViewport(uint width, uint height)
        {
            glViewport(0, 0, width, height);
        }

        static byte[] message = new byte[1024];

        static void CheckShaderCompileError(uint shader)
        {
            unsafe
            {
                GL_Enum compiled = 0;
                glGetShaderiv(shader, GL_Enum.COMPILE_STATUS, ref compiled);

                if (compiled != GL_Enum.TRUE)
                {
                    int log_length = 0;

                    fixed (byte* ptr = message)
                    {
                        glGetShaderInfoLog(shader, 1024, &log_length, (char*)ptr);
                        Console.WriteLine(Utils.FromUtf8(message));
                    }
                }
            }
        }

        static uint CompileShader(string src, GL_Enum type)
        {
            uint shader = glCreateShader(type);

            int size = SDL.Utf8Size(src);
            byte* utf8Proc = stackalloc byte[size];
            char* s = (char*)SDL.Utf8Encode(src, utf8Proc, size);

            glShaderSource(shader, 1, &s, null);
            glCompileShader(shader);

            CheckShaderCompileError(shader);

            return shader;
        }

        public static uint LoadShader(string vert_src, string frag_src)
        {
            unsafe
            {
                uint vert_shader = CompileShader(vert_src, GL_Enum.VERTEX_SHADER);
                uint frag_shader = CompileShader(frag_src, GL_Enum.FRAGMENT_SHADER);

                uint program = glCreateProgram();

                glAttachShader(program, vert_shader);
                glAttachShader(program, frag_shader);
                glLinkProgram(program);

                return program;
            }
        }

        public static int GetVertexSize(VertexFormat format)
        {
            int stride = 0;

            if (format.HasFlag(VertexFormat.POSITION))
            {
                stride += 12;
            }

            if (format.HasFlag(VertexFormat.TEX_COORD))
            {
                stride += 8;
            }

            if (format.HasFlag(VertexFormat.NORMAL))
            {
                stride += 12;
            }

            if(format.HasFlag(VertexFormat.COLOR))
            {
                stride += 16;
            }

            return stride;
        }

        public static void SetupVertexAttrib(VertexFormat format)
        {
            int stride = GetVertexSize(format);
            int next_ptr = 0;

            if (format.HasFlag(VertexFormat.POSITION))
            {
                glEnableVertexAttribArray((uint)VertexComponentPosition.POSITION);
                glVertexAttribPointer((uint)VertexComponentPosition.POSITION, 3, GL_Enum.FLOAT, 0, stride, (void*)0);
                next_ptr += 12;
            }

            if (format.HasFlag(VertexFormat.TEX_COORD))
            {
                glEnableVertexAttribArray((uint)VertexComponentPosition.TEXCOORD);
                glVertexAttribPointer((uint)VertexComponentPosition.TEXCOORD, 2, GL_Enum.FLOAT, 0, stride, (void*)(next_ptr));
                next_ptr += 8;
            }

            if (format.HasFlag(VertexFormat.NORMAL))
            {
                glEnableVertexAttribArray((uint)VertexComponentPosition.NORMAL);
                glVertexAttribPointer((uint)VertexComponentPosition.NORMAL, 2, GL_Enum.FLOAT, 0, stride, (void*)(next_ptr));
                next_ptr += 12;
            }

            if (format.HasFlag(VertexFormat.COLOR))
            {
                glEnableVertexAttribArray((uint)VertexComponentPosition.COLOR);
                glVertexAttribPointer((uint)VertexComponentPosition.COLOR, 4, GL_Enum.FLOAT, 0, stride, (void*)(next_ptr));
                next_ptr += 16;
            }

        }

        public static void BindCameraView(uint shader, Transform transform, Transform CameraPosition, Matrix4x4 CameraProjection)
        {
            Matrix4x4 mat_model = Mathf.ModelMatrixFromTransfrom(transform);
            SetUniformMat4(GetUniformLocation(shader, "mat_model"), mat_model);
            Matrix4x4 mat_view_projection = Mathf.ModelMatrixFromTransfrom(CameraPosition) * CameraProjection;
            SetUniformMat4(GetUniformLocation(shader, "mat_view_projection"), mat_view_projection);
        }

        public static void BindMaterial(Material material)
        {
            BindShader(material.shader);

            if (material.uniform_params != null)
            {
                var unis = material.uniform_params;
                int tex_slot = 0;

                for (int i = 0; i < material.uniform_params.Length; i++)
                {
                    switch (unis[i].type)
                    {
                        case UniformType.Vector2:
                            SetUniformV2(GetUniformLocation(material.shader, unis[i].name), unis[i].vec2);
                            break;
                        case UniformType.Vector3:
                            SetUniformV3(GetUniformLocation(material.shader, unis[i].name), unis[i].vec3);
                            break;
                        case UniformType.Vector4:
                            SetUniformV4(GetUniformLocation(material.shader, unis[i].name), unis[i].vec4);
                            break;
                        case UniformType.Texture:
                            ActivateTexture(tex_slot);
                            BindTexture(unis[i].texture);
                            SetUniformTexture(GetUniformLocation(material.shader, unis[i].name), tex_slot);
                            tex_slot++;
                            break;
                    }
                }
            }
        }

        public static void Draw(uint indices_count)
        {
            glDrawElements(GL_Enum.TRIANGLES, indices_count, GL_Enum.UNSIGNED_INT, (void*)0);
        }

        public static void DrawLine(uint indices_count)
        {
            glDrawElements(GL_Enum.LINES, indices_count, GL_Enum.UNSIGNED_INT, (void*)0);
        }

        public static int GetUniformLocation(uint shader, string uniform_name)
        {
            var location = glGetUniformLocation(shader, uniform_name.ToCStr());
            return location;
        }

        public static void SetBlendMode(BlendFunction source, BlendFunction destination)
        {
            glBlendFunc((uint)source, (uint)destination);
        }

        public static void SetBlendMode(BlendMode mode)
        {
            glEnable(GL_Enum.BLEND);

            switch (mode)
            {
                case BlendMode.Alpha:
                    glBlendFunc((uint)BlendFunction.BLEND_SRC_ALPHA, (uint)BlendFunction.BLEND_ONE_MINUS_SRC_ALPHA);
                    break;
                default:
                    glBlendFunc((uint)BlendFunction.BLEND_ONE, (uint)BlendFunction.BLEND_ZERO);
                    break;
            }
        }

        public static void SetUniformV2(int uniform, Vector2 vec)
        {
            glUniform2f(uniform, vec.X, vec.Y);
        }

        public static void SetUniformV3(int uniform, Vector3 vec)
        {
            glUniform3f(uniform, vec.X, vec.Y, vec.Z);
        }

        public static void SetUniformV4(int uniform, Vector4 vec)
        {
            glUniform4f(uniform, vec.X, vec.Y, vec.Z, vec.W);
        }

        public static void SetUniformTexture(int uniform, int slot)
        {
            glUniform1i(uniform, slot);
        }

        public static void SetUniformMat4(int uniform, float[] vec)
        {
            fixed (float* ptr = vec)
            {
                glUniformMatrix4fv(uniform, 1, 0, ptr);
            }
        }

        public static void SetUniformMat4(int uniform, Matrix4x4 mat)
        {
            glUniformMatrix4fv(uniform, 1, 0, &mat.M11);
        }

        public static void SetUniform(int uniform, int data)
        {
            glUniform1i(uniform, data);
        }

    }

}