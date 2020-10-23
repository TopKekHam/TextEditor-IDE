using System.Runtime.InteropServices;

namespace R
{

    // SDL_mixer.
    public static class Mixer
    {
        const string dll_path = "SDL2_mixer.dll";

        [DllImport(dll_path, EntryPoint = "Mix_Init", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Init(MIX_InitFlags flags);

        [DllImport(dll_path, EntryPoint = "Mix_Quit", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Quit();

        [DllImport(dll_path, EntryPoint = "Mix_OpenAudio", CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenAudio(int frequency, ushort format, int channels, int chunksize);

    }

    public enum MIX_InitFlags
    {
        MIX_INIT_FLAC = 0x00000001,
        MIX_INIT_MOD = 0x00000002,
        MIX_INIT_MP3 = 0x00000008,
        MIX_INIT_OGG = 0x00000010,
        MIX_INIT_MID = 0x00000020,
        MIX_INIT_OPUS = 0x00000040
    }

}
