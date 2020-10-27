using R;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public class SDL
{
    const string dll_path = "Dlls/SDL2.dll";
    public static int WINDOW_CENTERED = 0x2FFF0000;

    #region UTF8 Marshaling

    /* Used for stack allocated string marshaling. */
    internal static int Utf8Size(string str)
    {
        Debug.Assert(str != null);
        return (str.Length * 4) + 1;
    }
    internal static int Utf8SizeNullable(string str)
    {
        return str != null ? (str.Length * 4) + 1 : 0;
    }
    internal static unsafe byte* Utf8Encode(string str, byte* buffer, int bufferSize)
    {
        Debug.Assert(str != null);
        fixed (char* strPtr = str)
        {
            Encoding.UTF8.GetBytes(strPtr, str.Length + 1, buffer, bufferSize);
        }
        return buffer;
    }
    internal static unsafe byte* Utf8EncodeNullable(string str, byte* buffer, int bufferSize)
    {
        if (str == null)
        {
            return (byte*)0;
        }
        fixed (char* strPtr = str)
        {
            Encoding.UTF8.GetBytes(strPtr, str.Length + 1, buffer, bufferSize);
        }
        return buffer;
    }

    /* Used for heap allocated string marshaling.
     * Returned byte* must be free'd with FreeHGlobal.
     */
    internal static unsafe byte* Utf8Encode(string str)
    {
        Debug.Assert(str != null);
        int bufferSize = Utf8Size(str);
        byte* buffer = (byte*)Marshal.AllocHGlobal(bufferSize);
        fixed (char* strPtr = str)
        {
            Encoding.UTF8.GetBytes(strPtr, str.Length + 1, buffer, bufferSize);
        }
        return buffer;
    }

    internal static unsafe byte* Utf8EncodeNullable(string str)
    {
        if (str == null)
        {
            return (byte*)0;
        }
        int bufferSize = Utf8Size(str);
        byte* buffer = (byte*)Marshal.AllocHGlobal(bufferSize);
        fixed (char* strPtr = str)
        {
            Encoding.UTF8.GetBytes(
                strPtr,
                (str != null) ? (str.Length + 1) : 0,
                buffer,
                bufferSize
            );
        }
        return buffer;
    }

    /* This is public because SDL_DropEvent needs it! */
    public static unsafe string UTF8_ToManaged(IntPtr s, bool freePtr = false)
    {
        if (s == IntPtr.Zero)
        {
            return null;
        }

        /* We get to do strlen ourselves! */
        byte* ptr = (byte*)s;
        while (*ptr != 0)
        {
            ptr++;
        }

        /* TODO: This #ifdef is only here because the equivalent
         * .NET 2.0 constructor appears to be less efficient?
         * Here's the pretty version, maybe steal this instead:
         *
        string result = new string(
            (sbyte*) s, // Also, why sbyte???
            0,
            (int) (ptr - (byte*) s),
            System.Text.Encoding.UTF8
        );
         * See the CoreCLR source for more info.
         * -flibit
         */

			/* Modern C# lets you just send the byte*, nice! */
			string result = System.Text.Encoding.UTF8.GetString(
				(byte*) s,
				(int) (ptr - (byte*) s)
			);


        /* Some SDL functions will malloc, we have to free! */
        if (freePtr)
        {
            SDL.Free(s);
        }
        return result;
    }

    #endregion

    [DllImport(dll_path, EntryPoint = "SDL_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void Free(IntPtr memblock);

    [DllImport(dll_path, EntryPoint = "SDL_GetError", CallingConvention = CallingConvention.Cdecl)]
    public unsafe static extern IntPtr GetError();

    [DllImport(dll_path, EntryPoint = "SDL_Init", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Init(SDL_INIT_FLAGS prms);

    /* IntPtr refers to an SDL_Window* */
    [DllImport(dll_path, EntryPoint = "SDL_CreateWindow", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe IntPtr INTERNAL_SDL_CreateWindow( byte* title, int x, int y, int w, int h, SDL_WINDOW_FLAGS flags );

    public static unsafe IntPtr CreateWindow( string title, int x, int y, int w, int h, SDL_WINDOW_FLAGS flags )
    {
        int utf8TitleBufSize = Utf8SizeNullable(title);
        byte* utf8Title = stackalloc byte[utf8TitleBufSize];
        return INTERNAL_SDL_CreateWindow( Utf8EncodeNullable(title, utf8Title, utf8TitleBufSize), x, y, w, h, flags );
    }

    [DllImport(dll_path, EntryPoint = "SDL_DestroyWindow", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DestroyWindow(IntPtr window);

    [DllImport(dll_path, EntryPoint = "SDL_GL_CreateContext", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GL_CreateContext(IntPtr window);

    [DllImport(dll_path, EntryPoint = "SDL_GetTicks")]
    public static extern uint GetTicks();

    [DllImport(dll_path, EntryPoint = "SDL_GL_DeleteContext", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GL_DeleteContext(IntPtr gl_context);

    [DllImport(dll_path, EntryPoint = "SDL_Quit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Quit();

    [DllImport(dll_path, EntryPoint = "SDL_GetKeyboardState", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern IntPtr GetKeyboardState(out int numkeys);

    [DllImport(dll_path, EntryPoint = "SDL_PollEvent", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int PollEvent(out SDL_Event envt);

    [DllImport(dll_path, EntryPoint = "SDL_PumpEvents", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern void PumpEvents();

    [DllImport(dll_path, EntryPoint = "SDL_GL_SwapWindow", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int GL_SwapWindow(IntPtr gl_context);

    [DllImport(dll_path, EntryPoint = "SDL_GetMouseState", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern uint GetMouseState(out int x, out int y);

    [DllImport(dll_path, EntryPoint = "SDL_GL_SetAttribute", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int GL_SetAttribute(SDL_GLattr attr, int value);

    public static int GL_SetAttribute(SDL_GLattr attr, SDL_GLprofile profile )
    {
        return GL_SetAttribute(attr, (int)profile);
    }

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_GL_GetProcAddress", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern IntPtr GL_GetProcAddress(IntPtr proc_name);

    public static unsafe T GL_GetProcAddress<T>(string proc) where T : Delegate
    {
        int utf8ProcBufSize = Utf8Size(proc);
        byte* utf8Proc = stackalloc byte[utf8ProcBufSize];
        IntPtr delegatePtr = GL_GetProcAddress((IntPtr)Utf8Encode(proc, utf8Proc, utf8ProcBufSize));
        return (T)Marshal.GetDelegateForFunctionPointer(delegatePtr, typeof(T));
    }

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_ShowCursor", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int ShowCursor(int enabled);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_Delay", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern void Delay(uint ms);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_StartTextInput", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern void StartTextInput();

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_StopTextInput", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern void StopTextInput();

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_SetWindowSize", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern void SetWindowSize(IntPtr window, int w, int h);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_SetWindowBordered", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetWindowBordered(IntPtr window, bool bordered);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_MaximizeWindow", CallingConvention = CallingConvention.Cdecl)]
    public static extern void MaximizeWindow(IntPtr window);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_MinimizeWindow", CallingConvention = CallingConvention.Cdecl)]
    public static extern void MinimizeWindow(IntPtr window);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_SetWindowFullscreen", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetWindowFullscreen(IntPtr window, SDL_WINDOW_FLAGS flags);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_GetNumAudioDrivers", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int GetNumAudioDrivers();

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_GetAudioDriver", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern void* GetAudioDriver(int index);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_AudioInit", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int AudioInit(void* driver_name);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_AudioQuit", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern void AudioQuit();

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_OpenAudioDevice", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern uint OpenAudioDevice(char* device, int iscapture, SDL_AudioSpec* desired, SDL_AudioSpec* obtained, int allowed_changes);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_PauseAudioDevice", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern void PauseAudioDevice(uint dev, int pause_on);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_GetClipboardText", CallingConvention = CallingConvention.Cdecl)]
    static unsafe extern char* inner_GetClipboardText();

    public static unsafe string GetClipboardText()
    {
        var ptr = inner_GetClipboardText();
        var res = UTF8_ToManaged((IntPtr)ptr);
        Free((IntPtr)ptr);
        return res;
    }

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_HasClipboardText", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern bool HasClipboardText();

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_SetClipboardText", CallingConvention = CallingConvention.Cdecl)]
    static unsafe extern int inner_SetClipboardText(byte* text);

    public static unsafe bool SetClipboardText(string text)
    {
        var ptr = text.ToCStr();
        var res = inner_SetClipboardText(ptr) == 0;
        Marshal.FreeHGlobal((IntPtr)ptr);
        return res;
    }

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_RWFromFile", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern IntPtr RWFromFile(byte* file, byte* mode);

    [DllImport(dll_path, CharSet = CharSet.Auto, EntryPoint = "SDL_LoadWAV_RW", CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern SDL_AudioSpec* LoadWAV_RW(IntPtr src, int freesrc, SDL_AudioSpec* spec, byte** audio_buf, uint* audio_len);

    public static unsafe SDL_AudioSpec* LoadWAV(string path, SDL_AudioSpec* spec, byte** audio_buf, uint* audio_len)
    {
        var src = path.ToCStr();
        var rw = RWFromFile(src, "rb".ToCStr());
        return LoadWAV_RW(rw, 0, spec, audio_buf, audio_len);
    }

}

[Flags]
public enum SDL_INIT_FLAGS : uint
{
    TIMER = 0x00000001u,
    AUDIO = 0x00000010u,
    VIDEO = 0x00000020u,
    JOYSTICK = 0x00000200u,
    HAPTIC = 0x00001000u,
    GAMECONTROLLER = 0x00002000u,
    EVENTS = 0x00004000u,
    SENSOR = 0x00008000u,
    NOPARACHUTE = 0x00100000u,
    EVERYTHING = (TIMER | AUDIO | VIDEO | EVENTS | JOYSTICK | HAPTIC | GAMECONTROLLER | SENSOR)
}

[Flags]
public enum SDL_WINDOW_FLAGS
{
    NONE = 0,
    FULLSCREEN = 0x00000001,         /**< fullscreen window */
    OPENGL = 0x00000002,             /**< window usable with OpenGL context */
    SHOWN = 0x00000004,              /**< window is visible */
    HIDDEN = 0x00000008,             /**< window is not visible */
    BORDERLESS = 0x00000010,         /**< no window decoration */
    RESIZABLE = 0x00000020,          /**< window can be resized */
    MINIMIZED = 0x00000040,          /**< window is minimized */
    MAXIMIZED = 0x00000080,          /**< window is maximized */
    INPUT_GRABBED = 0x00000100,      /**< window has grabbed input focus */
    INPUT_FOCUS = 0x00000200,        /**< window has input focus */
    MOUSE_FOCUS = 0x00000400,        /**< window has mouse focus */
    FULLSCREEN_DESKTOP = (FULLSCREEN | 0x00001000),
    FOREIGN = 0x00000800,            /**< window not created by SDL */
    ALLOW_HIGHDPI = 0x00002000,      /**< window should be created in high-DPI mode if supported. On macOS NSHighResolutionCapable must be set true in the application's Info.plist for this to have any effect. */
    MOUSE_CAPTURE = 0x00004000,      /**< window has mouse captured (unrelated to INPUT_GRABBED) */
    ALWAYS_ON_TOP = 0x00008000,      /**< window should always be above others */
    SKIP_TASKBAR = 0x00010000,      /**< window should not be added to the taskbar */
    UTILITY = 0x00020000,      /**< window should be treated as a utility window */
    TOOLTIP = 0x00040000,      /**< window should be treated as a tooltip */
    POPUP_MENU = 0x00080000,      /**< window should be treated as a popup menu */
    VULKAN = 0x10000000       /**< window usable for Vulkan surface */
}

public  enum SDL_EVENT_TYPE : uint
{
    FIRSTEVENT = 0,     /**< Unused (do not remove) */

    /* Application events */
    QUIT = 0x100, /**< User-requested quit */

    /* These application events have special meaning on iOS, see README-ios.md for details */
    APP_TERMINATING,        /**< The application is being terminated by the OS
                                     Called on iOS in applicationWillTerminate()
                                     Called on Android in onDestroy()
                                */
    APP_LOWMEMORY,          /**< The application is low on memory, free memory if possible.
                                     Called on iOS in applicationDidReceiveMemoryWarning()
                                     Called on Android in onLowMemory()
                                */
    APP_WILLENTERBACKGROUND, /**< The application is about to enter the background
                                     Called on iOS in applicationWillResignActive()
                                     Called on Android in onPause()
                                */
    APP_DIDENTERBACKGROUND, /**< The application did enter the background and may not get CPU for some time
                                     Called on iOS in applicationDidEnterBackground()
                                     Called on Android in onPause()
                                */
    APP_WILLENTERFOREGROUND, /**< The application is about to enter the foreground
                                     Called on iOS in applicationWillEnterForeground()
                                     Called on Android in onResume()
                                */
    APP_DIDENTERFOREGROUND, /**< The application is now interactive
                                     Called on iOS in applicationDidBecomeActive()
                                     Called on Android in onResume()
                                */

    /* Display events */
    DISPLAYEVENT = 0x150,  /**< Display state change */

    /* Window events */
    WINDOWEVENT = 0x200, /**< Window state change */
    SYSWMEVENT,             /**< System specific event */

    /* Keyboard events */
    KEYDOWN = 0x300, /**< Key pressed */
    KEYUP,                  /**< Key released */
    TEXTEDITING,            /**< Keyboard text editing (composition) */
    TEXTINPUT,              /**< Keyboard text input */
    KEYMAPCHANGED,          /**< Keymap changed due to a system event such as an
                                     input language or keyboard layout change.
                                */

    /* Mouse events */
    MOUSEMOTION = 0x400, /**< Mouse moved */
    MOUSEBUTTONDOWN,        /**< Mouse button pressed */
    MOUSEBUTTONUP,          /**< Mouse button released */
    MOUSEWHEEL,             /**< Mouse wheel motion */

    /* Joystick events */
    JOYAXISMOTION = 0x600, /**< Joystick axis motion */
    JOYBALLMOTION,          /**< Joystick trackball motion */
    JOYHATMOTION,           /**< Joystick hat position change */
    JOYBUTTONDOWN,          /**< Joystick button pressed */
    JOYBUTTONUP,            /**< Joystick button released */
    JOYDEVICEADDED,         /**< A new joystick has been inserted into the system */
    JOYDEVICEREMOVED,       /**< An opened joystick has been removed */

    /* Game controller events */
    CONTROLLERAXISMOTION = 0x650, /**< Game controller axis motion */
    CONTROLLERBUTTONDOWN,          /**< Game controller button pressed */
    CONTROLLERBUTTONUP,            /**< Game controller button released */
    CONTROLLERDEVICEADDED,         /**< A new Game controller has been inserted into the system */
    CONTROLLERDEVICEREMOVED,       /**< An opened Game controller has been removed */
    CONTROLLERDEVICEREMAPPED,      /**< The controller mapping was updated */

    /* Touch events */
    FINGERDOWN = 0x700,
    FINGERUP,
    FINGERMOTION,

    /* Gesture events */
    DOLLARGESTURE = 0x800,
    DOLLARRECORD,
    MULTIGESTURE,

    /* Clipboard events */
    CLIPBOARDUPDATE = 0x900, /**< The clipboard changed */

    /* Drag and drop events */
    DROPFILE = 0x1000, /**< The system requests a file open */
    DROPTEXT,                 /**< text/plain drag-and-drop event */
    DROPBEGIN,                /**< A new set of drops is beginning (NULL filename) */
    DROPCOMPLETE,             /**< Current set of drops is now complete (NULL filename) */

    /* Audio hotplug events */
    AUDIODEVICEADDED = 0x1100, /**< A new audio device is available */
    AUDIODEVICEREMOVED,        /**< An audio device has been removed. */

    /* Sensor events */
    SENSORUPDATE = 0x1200,     /**< A sensor was updated */

    /* Render events */
    RENDER_TARGETS_RESET = 0x2000, /**< The render targets have been reset and their contents need to be updated */
    RENDER_DEVICE_RESET, /**< The device has been reset and all textures need to be recreated */

    /** Events ::SDL_USEREVENT through ::SDL_LASTEVENT are for your use,
     *  and should be allocated with SDL_RegisterEvents()
     */
    USEREVENT = 0x8000,

    /**
     *  This last event is only for bounding internal arrays
     */
    LASTEVENT = 0xFFFF
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct SDL_Event
{
    [FieldOffset(0)] public SDL_EVENT_TYPE type;
    [FieldOffset(0)] public SDL_WindowEvent window;
    [FieldOffset(0)] public SDL_KeyboardEvent key;
    [FieldOffset(0)] public SDL_TextInputEvent text_input;
    [FieldOffset(0)] private fixed byte padding[56];
}
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SDL_TextInputEvent
{
    public uint type;                              /**< ::SDL_TEXTINPUT */
    public uint timestamp;                         /**< In milliseconds, populated using SDL_GetTicks() */
    public uint windowID;                          /**< The window with keyboard focus, if any */
    public fixed byte text[32];                    /**< The input text */
}

[StructLayout(LayoutKind.Sequential)]
public struct SDL_WindowEvent
{
    public SDL_WindowEventID type;        /**< ::SDL_WINDOWEVENT */
    public uint timestamp;   /**< In milliseconds, populated using SDL_GetTicks() */
    public uint windowID;    /**< The associated window */
    public SDL_WindowEventID _event;        /**< ::SDL_WindowEventID */
    public byte padding1;
    public byte padding2;
    public byte padding3;
    public int data1;       /**< event dependent data */
    public int data2;
}

public enum SDL_WindowEventID : byte
{
    SDL_WINDOWEVENT_NONE,           /**< Never used */
    SDL_WINDOWEVENT_SHOWN,          /**< Window has been shown */
    SDL_WINDOWEVENT_HIDDEN,         /**< Window has been hidden */
    SDL_WINDOWEVENT_EXPOSED,        /**< Window has been exposed and should be
                                         redrawn */
    SDL_WINDOWEVENT_MOVED,          /**< Window has been moved to data1, data2
                                     */
    SDL_WINDOWEVENT_RESIZED,        /**< Window has been resized to data1xdata2 */
    SDL_WINDOWEVENT_SIZE_CHANGED,   /**< The window size has changed, either as
                                         a result of an API call or through the
                                         system or user changing the window size. */
    SDL_WINDOWEVENT_MINIMIZED,      /**< Window has been minimized */
    SDL_WINDOWEVENT_MAXIMIZED,      /**< Window has been maximized */
    SDL_WINDOWEVENT_RESTORED,       /**< Window has been restored to normal size
                                         and position */
    SDL_WINDOWEVENT_ENTER,          /**< Window has gained mouse focus */
    SDL_WINDOWEVENT_LEAVE,          /**< Window has lost mouse focus */
    SDL_WINDOWEVENT_FOCUS_GAINED,   /**< Window has gained keyboard focus */
    SDL_WINDOWEVENT_FOCUS_LOST,     /**< Window has lost keyboard focus */
    SDL_WINDOWEVENT_CLOSE,          /**< The window manager requests that the window be closed */
    SDL_WINDOWEVENT_TAKE_FOCUS,     /**< Window is being offered a focus (should SetWindowInputFocus() on itself or a subwindow, or ignore) */
    SDL_WINDOWEVENT_HIT_TEST        /**< Window had a hit test that wasn't SDL_HITTEST_NORMAL. */
}

[StructLayout(LayoutKind.Sequential)]
public struct SDL_KeyboardEvent
{
    public uint type;        /**< ::SDL_KEYDOWN or ::SDL_KEYUP */
    public uint timestamp;   /**< In milliseconds, populated using SDL_GetTicks() */
    public uint windowID;    /**< The window with keyboard focus, if any */
    public byte state;        /**< ::SDL_PRESSED or ::SDL_RELEASED */
    public byte repeat;       /**< Non-zero if this is a key repeat */
    private byte padding2;
    private byte padding3;
    public SDL_Keysym keysym;  /**< The key that was pressed or released */
}

[StructLayout(LayoutKind.Sequential)]
public struct SDL_Keysym
{
    public SDL_Scancode scancode;      /**< SDL physical key code - see ::SDL_Scancode for details */
    public int sym;            /**< SDL virtual key code - see ::SDL_Keycode for details */
    public ushort mod;                 /**< current key modifiers */
    public uint unused;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SDL_Surface
{
    uint flags;               /**< Read-only */
    void* format;    /**< Read-only */
    int w, h;                   /**< Read-only */
    int pitch;                  /**< Read-only */
    void* pixels;               /**< Read-write */

    /** Application data associated with the surface */
    void* userdata;             /**< Read-write */

    /** information needed for surfaces requiring locks */
    int locked;                 /**< Read-only */
    void* lock_data;            /**< Read-only */

    /** clipping information */
    SDL_Rect clip_rect;         /**< Read-only */

    /** info for fast blit mapping to other surfaces */
    void* map;    /**< Private */

    /** Reference count -- used when freeing surface */
    int refcount;               /**< Read-mostly */
}

public struct SDL_Color
{
    public byte r, g, b, a; 
}

[StructLayout(LayoutKind.Sequential)]
public struct SDL_Rect
{
    public int x, y;
    public int w, h;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SDL_AudioSpec
{
    public int freq;                   /**< DSP frequency -- samples per second */
    public SDL_Audio_Format format;     /**< Audio data format */
    public byte channels;             /**< Number of channels: 1 mono, 2 stereo */
    public byte silence;              /**< Audio buffer silence value (calculated) */
    public ushort samples;             /**< Audio buffer size in sample FRAMES (total samples divided by channel count) */
    public ushort padding;             /**< Necessary for some compile environments */
    public uint size;                /**< Audio buffer size in bytes (calculated) */
    public IntPtr callback; /** this is |SDL_AudioCallback| < Callback that feeds the audio device (NULL to use SDL_QueueAudio()). */
    public void* userdata;             /**< Userdata passed to callback (ignored for NULL callbacks). */
}

public unsafe delegate void SDL_AudioCallback(void* userdata, byte* stream, int len);

public enum SDL_Audio_Format : ushort
{
    AUDIO_U8        = 0x0008,  /**< Unsigned 8-bit samples */
    AUDIO_S8        = 0x8008,  /**< Signed 8-bit samples */
    AUDIO_U16LSB    = 0x0010,  /**< Unsigned 16-bit samples */
    AUDIO_S16LSB    = 0x8010,  /**< Signed 16-bit samples */
    AUDIO_U16MSB    = 0x1010,  /**< As above, but big-endian byte order */
    AUDIO_S16MSB    = 0x9010,  /**< As above, but big-endian byte order */
    AUDIO_U16       = AUDIO_U16LSB,
    AUDIO_S16       = AUDIO_S16LSB,
                      
    AUDIO_S32LSB    = 0x8020, /**< 32-bit integer samples */
    AUDIO_S32MSB    = 0x9020,  /**< As above, but big-endian byte order */
    AUDIO_S32       = AUDIO_S32LSB,
                      
    AUDIO_F32LSB    = 0x8120,  /**< 32-bit floating point samples */
    AUDIO_F32MSB    = 0x9120,  /**< As above, but big-endian byte order */
    AUDIO_F32       = AUDIO_F32LSB
}

public enum SDL_Scancode 
{
    UNKNOWN = 0,

    /**
     *  \name Usage page 0x07
     *
     *  These values are from usage page 0x07 (USB keyboard page).
     */
    /* @{ */

    KEY_A = 4,
    KEY_B = 5,
    KEY_C = 6,
    KEY_D = 7,
    KEY_E = 8,
    KEY_F = 9,
    KEY_G = 10,
    KEY_H = 11,
    KEY_I = 12,
    KEY_J = 13,
    KEY_K = 14,
    KEY_L = 15,
    KEY_M = 16,
    KEY_N = 17,
    KEY_O = 18,
    KEY_P = 19,
    KEY_Q = 20,
    KEY_R = 21,
    KEY_S = 22,
    KEY_T = 23,
    KEY_U = 24,
    KEY_V = 25,
    KEY_W = 26,
    KEY_X = 27,
    KEY_Y = 28,
    KEY_Z = 29,
    KEY_1 = 30,
    KEY_2 = 31,
    KEY_3 = 32,
    KEY_4 = 33,
    KEY_5 = 34,
    KEY_6 = 35,
    KEY_7 = 36,
    KEY_8 = 37,
    KEY_9 = 38,
    KEY_0 = 39,

    KEY_RETURN = 40,
    KEY_ESCAPE = 41,
    KEY_BACKSPACE = 42,
    KEY_TAB = 43,
    KEY_SPACE = 44,

    KEY_MINUS = 45,
    KEY_EQUALS = 46,
    KEY_LEFTBRACKET = 47,
    KEY_RIGHTBRACKET = 48,
    KEY_BACKSLASH = 49, /**< Located at the lower left of the return
                                  *   key on ISO keyboards and at the right end
                                  *   of the QWERTY row on ANSI keyboards.
                                  *   Produces REVERSE SOLIDUS (backslash) and
                                  *   VERTICAL LINE in a US layout, REVERSE
                                  *   SOLIDUS and VERTICAL LINE in a UK Mac
                                  *   layout, NUMBER SIGN and TILDE in a UK
                                  *   Windows layout, DOLLAR SIGN and POUND SIGN
                                  *   in a Swiss German layout, NUMBER SIGN and
                                  *   APOSTROPHE in a German layout, GRAVE
                                  *   ACCENT and POUND SIGN in a French Mac
                                  *   layout, and ASTERISK and MICRO SIGN in a
                                  *   French Windows layout.
                                  */
    KEY_NONUSHASH = 50, /**< ISO USB keyboards actually use this code
                                  *   instead of 49 for the same key, but all
                                  *   OSes I've seen treat the two codes
                                  *   identically. So, as an implementor, unless
                                  *   your keyboard generates both of those
                                  *   codes and your OS treats them differently,
                                  *   you should generate SDL_SCANCODE_BACKSLASH
                                  *   instead of this code. As a user, you
                                  *   should not rely on this code because SDL
                                  *   will never generate it with most (all?)
                                  *   keyboards.
                                  */
    KEY_SEMICOLON = 51,
    KEY_APOSTROPHE = 52,
    KEY_GRAVE = 53, /**< Located in the top left corner (on both ANSI
                              *   and ISO keyboards). Produces GRAVE ACCENT and
                              *   TILDE in a US Windows layout and in US and UK
                              *   Mac layouts on ANSI keyboards, GRAVE ACCENT
                              *   and NOT SIGN in a UK Windows layout, SECTION
                              *   SIGN and PLUS-MINUS SIGN in US and UK Mac
                              *   layouts on ISO keyboards, SECTION SIGN and
                              *   DEGREE SIGN in a Swiss German layout (Mac:
                              *   only on ISO keyboards), CIRCUMFLEX ACCENT and
                              *   DEGREE SIGN in a German layout (Mac: only on
                              *   ISO keyboards), SUPERSCRIPT TWO and TILDE in a
                              *   French Windows layout, COMMERCIAL AT and
                              *   NUMBER SIGN in a French Mac layout on ISO
                              *   keyboards, and LESS-THAN SIGN and GREATER-THAN
                              *   SIGN in a Swiss German, German, or French Mac
                              *   layout on ANSI keyboards.
                              */
    KEY_COMMA = 54,
    KEY_PERIOD = 55,
    KEY_SLASH = 56,

    KEY_CAPSLOCK = 57,

    KEY_F1 = 58,
    KEY_F2 = 59,
    KEY_F3 = 60,
    KEY_F4 = 61,
    KEY_F5 = 62,
    KEY_F6 = 63,
    KEY_F7 = 64,
    KEY_F8 = 65,
    KEY_F9 = 66,
    KEY_F10 = 67,
    KEY_F11 = 68,
    KEY_F12 = 69,

    KEY_PRINTSCREEN = 70,
    KEY_SCROLLLOCK = 71,
    KEY_PAUSE = 72,
    KEY_INSERT = 73, /**< insert on PC, help on some Mac keyboards (but
                                   does send code 73, not 117) */
    KEY_HOME = 74,
    KEY_PAGEUP = 75,
    KEY_DELETE = 76,
    KEY_END = 77,
    KEY_PAGEDOWN = 78,
    KEY_RIGHT = 79,
    KEY_LEFT = 80,
    KEY_DOWN = 81,
    KEY_UP = 82,

    KEY_NUMLOCKCLEAR = 83, /**< num lock on PC, clear on Mac keyboards
                                     */
    KEY_KP_DIVIDE = 84,
    KEY_KP_MULTIPLY = 85,
    KEY_KP_MINUS = 86,
    KEY_KP_PLUS = 87,
    KEY_KP_ENTER = 88,
    KEY_KP_1 = 89,
    KEY_KP_2 = 90,
    KEY_KP_3 = 91,
    KEY_KP_4 = 92,
    KEY_KP_5 = 93,
    KEY_KP_6 = 94,
    KEY_KP_7 = 95,
    KEY_KP_8 = 96,
    KEY_KP_9 = 97,
    KEY_KP_0 = 98,
    KEY_KP_PERIOD = 99,

    KEY_NONUSBACKSLASH = 100, /**< This is the additional key that ISO
                                        *   keyboards have over ANSI ones,
                                        *   located between left shift and Y.
                                        *   Produces GRAVE ACCENT and TILDE in a
                                        *   US or UK Mac layout, REVERSE SOLIDUS
                                        *   (backslash) and VERTICAL LINE in a
                                        *   US or UK Windows layout, and
                                        *   LESS-THAN SIGN and GREATER-THAN SIGN
                                        *   in a Swiss German, German, or French
                                        *   layout. */
    KEY_APPLICATION = 101, /**< windows contextual menu, compose */
    KEY_POWER = 102, /**< The USB document says this is a status flag,
                               *   not a physical key - but some Mac keyboards
                               *   do have a power key. */
    KEY_KP_EQUALS = 103,
    KEY_F13 = 104,
    KEY_F14 = 105,
    KEY_F15 = 106,
    KEY_F16 = 107,
    KEY_F17 = 108,
    KEY_F18 = 109,
    KEY_F19 = 110,
    KEY_F20 = 111,
    KEY_F21 = 112,
    KEY_F22 = 113,
    KEY_F23 = 114,
    KEY_F24 = 115,
    KEY_EXECUTE = 116,
    KEY_HELP = 117,
    KEY_MENU = 118,
    KEY_SELECT = 119,
    KEY_STOP = 120,
    KEY_AGAIN = 121,   /**< redo */
    KEY_UNDO = 122,
    KEY_CUT = 123,
    KEY_COPY = 124,
    KEY_PASTE = 125,
    KEY_FIND = 126,
    KEY_MUTE = 127,
    KEY_VOLUMEUP = 128,
    KEY_VOLUMEDOWN = 129,
    /* not sure whether there's a reason to enable these */
    /*     SDL_SCANCODE_LOCKINGCAPSLOCK = 130,  */
    /*     SDL_SCANCODE_LOCKINGNUMLOCK = 131, */
    /*     SDL_SCANCODE_LOCKINGSCROLLLOCK = 132, */
    KEY_KP_COMMA = 133,
    KEY_KP_EQUALSAS400 = 134,

    KEY_INTERNATIONAL1 = 135, /**< used on Asian keyboards, see
                                            footnotes in USB doc */
    KEY_INTERNATIONAL2 = 136,
    KEY_INTERNATIONAL3 = 137, /**< Yen */
    KEY_INTERNATIONAL4 = 138,
    KEY_INTERNATIONAL5 = 139,
    KEY_INTERNATIONAL6 = 140,
    KEY_INTERNATIONAL7 = 141,
    KEY_INTERNATIONAL8 = 142,
    KEY_INTERNATIONAL9 = 143,
    KEY_LANG1 = 144, /**< Hangul/English toggle */
    KEY_LANG2 = 145, /**< Hanja conversion */
    KEY_LANG3 = 146, /**< Katakana */
    KEY_LANG4 = 147, /**< Hiragana */
    KEY_LANG5 = 148, /**< Zenkaku/Hankaku */
    KEY_LANG6 = 149, /**< reserved */
    KEY_LANG7 = 150, /**< reserved */
    KEY_LANG8 = 151, /**< reserved */
    KEY_LANG9 = 152, /**< reserved */

    KEY_ALTERASE = 153, /**< Erase-Eaze */
    KEY_SYSREQ = 154,
    KEY_CANCEL = 155,
    KEY_CLEAR = 156,
    KEY_PRIOR = 157,
    KEY_RETURN2 = 158,
    KEY_SEPARATOR = 159,
    KEY_OUT = 160,
    KEY_OPER = 161,
    KEY_CLEARAGAIN = 162,
    KEY_CRSEL = 163,
    KEY_EXSEL = 164,

    KEY_KP_00 = 176,
    KEY_KP_000 = 177,
    KEY_THOUSANDSSEPARATOR = 178,
    KEY_DECIMALSEPARATOR = 179,
    KEY_CURRENCYUNIT = 180,
    KEY_CURRENCYSUBUNIT = 181,
    KEY_KP_LEFTPAREN = 182,
    KEY_KP_RIGHTPAREN = 183,
    KEY_KP_LEFTBRACE = 184,
    KEY_KP_RIGHTBRACE = 185,
    KEY_KP_TAB = 186,
    KEY_KP_BACKSPACE = 187,
    KEY_KP_A = 188,
    KEY_KP_B = 189,
    KEY_KP_C = 190,
    KEY_KP_D = 191,
    KEY_KP_E = 192,
    KEY_KP_F = 193,
    KEY_KP_XOR = 194,
    KEY_KP_POWER = 195,
    KEY_KP_PERCENT = 196,
    KEY_KP_LESS = 197,
    KEY_KP_GREATER = 198,
    KEY_KP_AMPERSAND = 199,
    KEY_KP_DBLAMPERSAND = 200,
    KEY_KP_VERTICALBAR = 201,
    KEY_KP_DBLVERTICALBAR = 202,
    KEY_KP_COLON = 203,
    KEY_KP_HASH = 204,
    KEY_KP_SPACE = 205,
    KEY_KP_AT = 206,
    KEY_KP_EXCLAM = 207,
    KEY_KP_MEMSTORE = 208,
    KEY_KP_MEMRECALL = 209,
    KEY_KP_MEMCLEAR = 210,
    KEY_KP_MEMADD = 211,
    KEY_KP_MEMSUBTRACT = 212,
    KEY_KP_MEMMULTIPLY = 213,
    KEY_KP_MEMDIVIDE = 214,
    KEY_KP_PLUSMINUS = 215,
    KEY_KP_CLEAR = 216,
    KEY_KP_CLEARENTRY = 217,
    KEY_KP_BINARY = 218,
    KEY_KP_OCTAL = 219,
    KEY_KP_DECIMAL = 220,
    KEY_KP_HEXADECIMAL = 221,

    KEY_LCTRL = 224,
    KEY_LSHIFT = 225,
    KEY_LALT = 226, /**< alt, option */
    KEY_LGUI = 227, /**< windows, command (apple), meta */
    KEY_RCTRL = 228,
    KEY_RSHIFT = 229,
    KEY_RALT = 230, /**< alt gr, option */
    KEY_RGUI = 231, /**< windows, command (apple), meta */

    KEY_MODE = 257,    /**< I'm not sure if this is really not covered
                                 *   by any of the above, but since there's a
                                 *   special KMOD_MODE for it I'm adding it here
                                 */

    /* @} *//* Usage page 0x07 */

    /**
     *  \name Usage page 0x0C
     *
     *  These values are mapped from usage page 0x0C (USB consumer page).
     */
    /* @{ */

    KEY_AUDIONEXT = 258,
    KEY_AUDIOPREV = 259,
    KEY_AUDIOSTOP = 260,
    KEY_AUDIOPLAY = 261,
    KEY_AUDIOMUTE = 262,
    KEY_MEDIASELECT = 263,
    KEY_WWW = 264,
    KEY_MAIL = 265,
    KEY_CALCULATOR = 266,
    KEY_COMPUTER = 267,
    KEY_AC_SEARCH = 268,
    KEY_AC_HOME = 269,
    KEY_AC_BACK = 270,
    KEY_AC_FORWARD = 271,
    KEY_AC_STOP = 272,
    KEY_AC_REFRESH = 273,
    KEY_AC_BOOKMARKS = 274,

    /* @} *//* Usage page 0x0C */

    /**
     *  \name Walther keys
     *
     *  These are values that Christian Walther added (for mac keyboard?).
     */
    /* @{ */

    KEY_BRIGHTNESSDOWN = 275,
    KEY_BRIGHTNESSUP = 276,
    KEY_DISPLAYSWITCH = 277, /**< display mirroring/dual display
                                           switch, video mode switch */
    KEY_KBDILLUMTOGGLE = 278,
    KEY_KBDILLUMDOWN = 279,
    KEY_KBDILLUMUP = 280,
    KEY_EJECT = 281,
    KEY_SLEEP = 282,

    KEY_APP1 = 283,
    KEY_APP2 = 284,

    /* @} *//* Walther keys */

    /**
     *  \name Usage page 0x0C (additional media keys)
     *
     *  These values are mapped from usage page 0x0C (USB consumer page).
     */
    /* @{ */

    KEY_AUDIOREWIND = 285,
    KEY_AUDIOFASTFORWARD = 286,

    /* @} *//* Usage page 0x0C (additional media keys) */

    /* Add any other keys here. */

    SDL_NUM_SCANCODES = 512 /**< not a key, just marks the number of scancodes
                                 for array bounds */
}

public enum SDL_GLattr
{
    GL_RED_SIZE,
    GL_GREEN_SIZE,
    GL_BLUE_SIZE,
    GL_ALPHA_SIZE,
    GL_BUFFER_SIZE,
    GL_DOUBLEBUFFER,
    GL_DEPTH_SIZE,
    GL_STENCIL_SIZE,
    GL_ACCUM_RED_SIZE,
    GL_ACCUM_GREEN_SIZE,
    GL_ACCUM_BLUE_SIZE,
    GL_ACCUM_ALPHA_SIZE,
    GL_STEREO,
    GL_MULTISAMPLEBUFFERS,
    GL_MULTISAMPLESAMPLES,
    GL_ACCELERATED_VISUAL,
    GL_RETAINED_BACKING,
    GL_CONTEXT_MAJOR_VERSION,
    GL_CONTEXT_MINOR_VERSION,
    GL_CONTEXT_EGL,
    GL_CONTEXT_FLAGS,
    GL_CONTEXT_PROFILE_MASK,
    GL_SHARE_WITH_CURRENT_CONTEXT,
    GL_FRAMEBUFFER_SRGB_CAPABLE,
    GL_CONTEXT_RELEASE_BEHAVIOR,
    GL_CONTEXT_RESET_NOTIFICATION,
    GL_CONTEXT_NO_ERROR
}

public enum SDL_GLprofile : int
{
    GL_CONTEXT_PROFILE_CORE = 0x0001,
    GL_CONTEXT_PROFILE_COMPATIBILITY = 0x0002,
    GL_CONTEXT_PROFILE_ES = 0x0004 /**< GLX_CONTEXT_ES2_PROFILE_BIT_EXT */
}