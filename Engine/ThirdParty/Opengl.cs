using System;
using System.Runtime.CompilerServices;

public unsafe delegate void GLDEBUGPROC(uint source, uint type, uint id, uint severity, int length, char* message, void* user_param);
public unsafe delegate void PFNGLDEBUGMESSAGECALLBACKPROC(GLDEBUGPROC proc, void* user_Params);
public delegate void PFNGLVIEWPORTPROC(int x, int y, uint width, uint height);
public delegate void PFNGLCLEARCOLORPROC(float red, float green, float blue, float alpha);
public delegate void PFNGLCLEARPROC(uint mask);
public delegate void PFNGLDRAWARRAYSPROC(uint mode, int first, int count);
public unsafe delegate void PFNGLDRAWELEMENTSPROC(uint mode, uint count, uint type, void* indices);
public unsafe delegate void PFNGLGENBUFFERSPROC(int n, uint* buffers);
public unsafe delegate void PFNGLDELETEBUFFERSPROC(int n, uint* buffers);
public delegate void PFNGLBINDBUFFERPROC(uint target, uint buffer);
public unsafe delegate void PFNGLBUFFERDATAPROC(uint target, long size, void* data, uint usage);
public unsafe delegate void PFNGLVERTEXATTRIBPOINTERPROC(uint index, int size, uint type, byte normalized, int stride, void* pointer);
public delegate void PFNGLENABLEVERTEXATTRIBARRAYPROC(uint index);
public delegate uint PFNGLCREATESHADERPROC(uint type);
public unsafe delegate void PFNGLSHADERSOURCEPROC(uint shader, int count, char** str, int* length);
public delegate void PFNGLCOMPILESHADERPROC(uint shader);
public delegate uint PFNGLCREATEPROGRAMPROC();
public delegate void PFNGLATTACHSHADERPROC(uint program, uint shader);
public delegate void PFNGLLINKPROGRAMPROC(uint program);
public delegate void PFNGLUSEPROGRAMPROC(uint program);
public unsafe delegate void PFNGLGETSHADERIVPROC(uint shader, uint pname, ref int prms);
public unsafe delegate void PFNGLGETSHADERINFOLOGPROC(uint shader, int bufSize, int* length, char* infoLog);
public unsafe delegate int PFNGLGETUNIFORMLOCATIONPROC(uint program, byte* name);
public unsafe delegate int PFNGLGETATTRIBLOCATIONPROC(uint program, char* name);
public unsafe delegate void PFNGLUNIFORMMATRIX4FVPROC(int location, int count, byte transpose, float* value);
public unsafe delegate void PFNGLGENTEXTURESPROC(int n, uint* textures);
public delegate void PFNGLBINDTEXTUREPROC(uint target, uint texture);
public unsafe delegate void PFNGLTEXIMAGE2DPROC(uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, void* pixels);
public delegate void PFNGLTEXPARAMETERIPROC(uint target, uint pname, int param);
public delegate void PFNGLACTIVETEXTUREPROC(uint texture);
public delegate void PFNGLUNIFORM1IPROC(int location, int v0);
public delegate void PFNGLUNIFORM2FPROC(int location, float v0, float v1);
public delegate void PFNGLUNIFORM3FPROC(int location, float v0, float v1, float v2);
public delegate void PFNGLUNIFORM4FPROC(int location, float v0, float v1, float v2, float v3);
public delegate void PFNGLENABLEPROC(uint cap);
public delegate void PFNGLDISABLEPROC(uint cap);
public delegate void PFNGLBLENDFUNCPROC(uint sfactor, uint dfactor);
public delegate uint PFNGLGETERRORPROC();
public unsafe delegate void PFNGLGETINTEGERV(uint pname, int* value);
public delegate void PFNGLSTENCILMASKPROC (uint mask);
public delegate void PFNGLSTENCILFUNCPROC (uint func, int _ref, uint mask);
public delegate void PFNGLSTENCILOPPROC (uint fail, uint zfail, uint zpass);
public delegate void PFNGLCLEARSTENCILPROC (int s);

public unsafe static class Opengl
{

    public static PFNGLDEBUGMESSAGECALLBACKPROC glDebugMessageCallback;
    public static PFNGLVIEWPORTPROC glViewport;
    public static PFNGLCLEARCOLORPROC glClearColor;
    public static PFNGLCLEARPROC glClear;
    public static PFNGLDRAWARRAYSPROC glDrawArrays;
    public static PFNGLDRAWELEMENTSPROC glDrawElements;
    public static PFNGLGENBUFFERSPROC glGenBuffers;
    public static PFNGLDELETEBUFFERSPROC glDeleteBuffers;
    public static PFNGLBINDBUFFERPROC glBindBuffer;
    public static PFNGLBUFFERDATAPROC glBufferData;
    public static PFNGLVERTEXATTRIBPOINTERPROC glVertexAttribPointer;
    public static PFNGLENABLEVERTEXATTRIBARRAYPROC glEnableVertexAttribArray;
    public static PFNGLCREATESHADERPROC glCreateShader;
    public static PFNGLSHADERSOURCEPROC glShaderSource;
    public static PFNGLCOMPILESHADERPROC glCompileShader;
    public static PFNGLCREATEPROGRAMPROC glCreateProgram;
    public static PFNGLATTACHSHADERPROC glAttachShader;
    public static PFNGLLINKPROGRAMPROC glLinkProgram;
    public static PFNGLUSEPROGRAMPROC glUseProgram;
    public static PFNGLGETSHADERIVPROC glGetShaderiv;
    public static PFNGLGETSHADERINFOLOGPROC glGetShaderInfoLog;
    public static PFNGLGETUNIFORMLOCATIONPROC glGetUniformLocation;
    public static PFNGLGETATTRIBLOCATIONPROC glGetAttribLocation;
    public static PFNGLUNIFORMMATRIX4FVPROC glUniformMatrix4fv;
    public static PFNGLGENTEXTURESPROC glGenTextures;
    public static PFNGLBINDTEXTUREPROC glBindTexture;
    public static PFNGLTEXIMAGE2DPROC glTexImage2D;
    public static PFNGLTEXPARAMETERIPROC glTexParameteri;
    public static PFNGLACTIVETEXTUREPROC glActiveTexture;
    public static PFNGLUNIFORM1IPROC glUniform1i;
    public static PFNGLUNIFORM2FPROC glUniform2f;
    public static PFNGLUNIFORM3FPROC glUniform3f;
    public static PFNGLUNIFORM4FPROC glUniform4f;
    public static PFNGLENABLEPROC glEnable;
    public static PFNGLDISABLEPROC glDisable;
    public static PFNGLBLENDFUNCPROC glBlendFunc;
    public static PFNGLGETERRORPROC glGetError;
    public static PFNGLGETINTEGERV glGetIntegerv;
    public static PFNGLSTENCILMASKPROC glStencilMask;
    public static PFNGLSTENCILFUNCPROC glStencilFunc;
    public static PFNGLSTENCILOPPROC glStencilOp;
    public static PFNGLCLEARSTENCILPROC glClearStencil;

    public const uint GL_TRUE = 1;
    public const uint GL_DEPTH_BUFFER_BIT = 0x00000100;
    public const uint GL_STENCIL_BUFFER_BIT = 0x00000400;
    public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
    public const uint GL_DEPTH_TEST = 0x0B71;
    public const uint GL_STENCIL_TEST = 0x0B90;
    public const uint GL_ARRAY_BUFFER = 0x8892;
    public const uint GL_ELEMENT_ARRAY_BUFFER = 0x8893;
    public const uint GL_STATIC_DRAW = 0x88E4;
    public const uint GL_TEXTURE_2D = 0x0DE1;
    public const uint GL_TEXTURE_MIN_FILTER = 0x2801;
    public const uint GL_TEXTURE_MAG_FILTER = 0x2800;
    public const uint GL_TEXTURE_WRAP_S = 0x2802;
    public const uint GL_TEXTURE_WRAP_T = 0x2803;
    public const int GL_LINEAR = 0x2601;
    public const int GL_NEAREST = 0x2600;
    public const int GL_MIRRORED_REPEAT = 0x8370;
    public const int GL_ALPHA = 0x1906;
    public const int GL_RGBA = 0x1908;
    public const uint GL_UNSIGNED_BYTE = 0x1401;
    public const uint GL_BGRA = 0x80E1;
    public const uint GL_TEXTURE0 = 0x84C0;
    public const uint GL_FRAGMENT_SHADER = 0x8B30;
    public const uint GL_VERTEX_SHADER = 0x8B31;
    public const uint GL_COMPILE_STATUS = 0x8B81;
    public const uint GL_SHADER_SOURCE_LENGTH = 0x8B88;
    public const uint GL_FLOAT = 0x1406;
    public const uint GL_LINES = 0x0001;
    public const uint GL_TRIANGLES = 0x0004;
    public const uint GL_UNSIGNED_INT = 0x1405;
    public const uint GL_BLEND = 0x0BE2;
    public const uint GL_EQUAL = 0x0202;
    public const uint GL_NOTEQUAL = 0x0205;
    public const uint GL_ALWAYS = 0x0207;
    public const uint GL_KEEP = 0x1E00;
    public const uint GL_REPLACE = 0x1E01;

    public const uint GL_MAX_TEXTURE_SIZE = 0x0D33;

    public static int gl_major_version = 3;
    public static int gl_minor_version = 3;
    public static string ShaderVersion => $"#version {gl_major_version}{gl_minor_version}0";

    public static int MAX_TEXTURE_SIZE = 0;

    public unsafe static void Init()
    {
        SDL.GL_SetAttribute(SDL_GLattr.GL_CONTEXT_MAJOR_VERSION, gl_major_version);
        SDL.GL_SetAttribute(SDL_GLattr.GL_CONTEXT_MINOR_VERSION, gl_minor_version);
        SDL.GL_SetAttribute(SDL_GLattr.GL_CONTEXT_PROFILE_MASK, SDL_GLprofile.GL_CONTEXT_PROFILE_CORE);

        glDebugMessageCallback = SDL.GL_GetProcAddress<PFNGLDEBUGMESSAGECALLBACKPROC>("glDebugMessageCallback");
        glDrawArrays = SDL.GL_GetProcAddress<PFNGLDRAWARRAYSPROC>("glDrawArrays");
        glDrawElements = SDL.GL_GetProcAddress<PFNGLDRAWELEMENTSPROC>("glDrawElements");
        glGenBuffers = SDL.GL_GetProcAddress<PFNGLGENBUFFERSPROC>("glGenBuffers");
        glDeleteBuffers = SDL.GL_GetProcAddress<PFNGLDELETEBUFFERSPROC>("glDeleteBuffers");
        glBindBuffer = SDL.GL_GetProcAddress<PFNGLBINDBUFFERPROC>("glBindBuffer");
        glBufferData = SDL.GL_GetProcAddress<PFNGLBUFFERDATAPROC>("glBufferData");
        glVertexAttribPointer = SDL.GL_GetProcAddress<PFNGLVERTEXATTRIBPOINTERPROC>("glVertexAttribPointer");
        glEnableVertexAttribArray = SDL.GL_GetProcAddress<PFNGLENABLEVERTEXATTRIBARRAYPROC>("glEnableVertexAttribArray");
        glGetAttribLocation = SDL.GL_GetProcAddress<PFNGLGETATTRIBLOCATIONPROC>("glGetAttribLocation");
        glCreateShader = SDL.GL_GetProcAddress<PFNGLCREATESHADERPROC>("glCreateShader");
        glShaderSource = SDL.GL_GetProcAddress<PFNGLSHADERSOURCEPROC>("glShaderSource");
        glCompileShader = SDL.GL_GetProcAddress<PFNGLCOMPILESHADERPROC>("glCompileShader");
        glCreateProgram = SDL.GL_GetProcAddress<PFNGLCREATEPROGRAMPROC>("glCreateProgram");
        glAttachShader = SDL.GL_GetProcAddress<PFNGLATTACHSHADERPROC>("glAttachShader");
        glLinkProgram = SDL.GL_GetProcAddress<PFNGLLINKPROGRAMPROC>("glLinkProgram");
        glUseProgram = SDL.GL_GetProcAddress<PFNGLUSEPROGRAMPROC>("glUseProgram");
        glGetShaderiv = SDL.GL_GetProcAddress<PFNGLGETSHADERIVPROC>("glGetShaderiv");
        glGetShaderInfoLog = SDL.GL_GetProcAddress<PFNGLGETSHADERINFOLOGPROC>("glGetShaderInfoLog");
        glGetUniformLocation = SDL.GL_GetProcAddress<PFNGLGETUNIFORMLOCATIONPROC>("glGetUniformLocation");
        glUniformMatrix4fv = SDL.GL_GetProcAddress<PFNGLUNIFORMMATRIX4FVPROC>("glUniformMatrix4fv");
        glViewport = SDL.GL_GetProcAddress<PFNGLVIEWPORTPROC>("glViewport");
        glClearColor = SDL.GL_GetProcAddress<PFNGLCLEARCOLORPROC>("glClearColor");
        glClear = SDL.GL_GetProcAddress<PFNGLCLEARPROC>("glClear");
        glGenTextures = SDL.GL_GetProcAddress<PFNGLGENTEXTURESPROC>("glGenTextures");
        glBindTexture = SDL.GL_GetProcAddress<PFNGLBINDTEXTUREPROC>("glBindTexture");
        glTexImage2D = SDL.GL_GetProcAddress<PFNGLTEXIMAGE2DPROC>("glTexImage2D");
        glTexParameteri = SDL.GL_GetProcAddress<PFNGLTEXPARAMETERIPROC>("glTexParameteri");
        glActiveTexture = SDL.GL_GetProcAddress<PFNGLACTIVETEXTUREPROC>("glActiveTexture");
        glUniform1i = SDL.GL_GetProcAddress<PFNGLUNIFORM1IPROC>("glUniform1i");
        glUniform2f = SDL.GL_GetProcAddress<PFNGLUNIFORM2FPROC>("glUniform2f");
        glUniform3f = SDL.GL_GetProcAddress<PFNGLUNIFORM3FPROC>("glUniform3f");
        glUniform4f = SDL.GL_GetProcAddress<PFNGLUNIFORM4FPROC>("glUniform4f");
        glEnable = SDL.GL_GetProcAddress<PFNGLENABLEPROC>("glEnable");
        glDisable = SDL.GL_GetProcAddress<PFNGLDISABLEPROC>("glDisable");
        glBlendFunc = SDL.GL_GetProcAddress<PFNGLBLENDFUNCPROC>("glBlendFunc");
        glGetError = SDL.GL_GetProcAddress<PFNGLGETERRORPROC>("glGetError");
        glGetIntegerv = SDL.GL_GetProcAddress<PFNGLGETINTEGERV>("glGetIntegerv");
        glStencilMask = SDL.GL_GetProcAddress<PFNGLSTENCILMASKPROC>("glStencilMask");
        glStencilFunc = SDL.GL_GetProcAddress< PFNGLSTENCILFUNCPROC> ("glStencilFunc");
        glStencilOp = SDL.GL_GetProcAddress<PFNGLSTENCILOPPROC> ("glStencilOp");
        glClearStencil = SDL.GL_GetProcAddress< PFNGLCLEARSTENCILPROC> ("glClearStencil");

        glDebugMessageCallback(DebugOpengl, (void*)0);

        fixed(int* ptr = &MAX_TEXTURE_SIZE) { glGetIntegerv(GL_MAX_TEXTURE_SIZE, ptr); }
    }

    static void DebugOpengl(uint source, uint type, uint id, uint severity, int length, char* message, void* user_param)
    {
        Console.WriteLine("Error in Opengl.");

        for (int i = 0; i < length; i++)
        {
            Console.Write(*(message + i));
        }
    }

}

