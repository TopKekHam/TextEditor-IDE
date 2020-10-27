using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace R
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Transform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public static Transform Zero = new Transform() { position = Vector3.Zero, rotation = Quaternion.Identity, scale = Vector3.One };
    }

    [Flags]
    public enum InputState : byte
    {
        IDLE = 0, PRESSED = 1, DOWN = 2, UP = 4
    }

    public enum MouseButton
    {
        SDL_BUTTON_LEFT = 1,
        SDL_BUTTON_MIDDLE = 2,
        SDL_BUTTON_RIGHT = 3
    }

    public struct MouseState
    {
        public InputState left;
        public InputState right;
        public Vector2 mouse_position;
    }

    public unsafe class Engine
    {

        public static IntPtr window;
        public static Vector2 window_size;
        public static IntPtr opengl_context;
        public static long prev_ticks;
        public static byte[] keyboard_state;
        public static float delta_time;
        public static MouseState mouse_state;
        public static int frame_cap = -1; // if frame cap less equils to 0, frame cap disabled.
        public static byte* sdl_keyboard_state;

        public static void Init(string window_title, int width = 800, int height = 600)
        {

            if (SDL.Init(SDL_INIT_FLAGS.VIDEO | SDL_INIT_FLAGS.EVENTS | SDL_INIT_FLAGS.AUDIO) > 0)
            {
                Console.WriteLine("something bad happend");
            }

            sdl_keyboard_state = (byte*)SDL.GetKeyboardState(out int keynums);

            Opengl.SetGLAtrribs();

            window = SDL.CreateWindow(window_title, SDL.WINDOW_CENTERED, SDL.WINDOW_CENTERED, width, height, 
                SDL_WINDOW_FLAGS.OPENGL | SDL_WINDOW_FLAGS.RESIZABLE | 
                (width == -1 ? SDL_WINDOW_FLAGS.MAXIMIZED : SDL_WINDOW_FLAGS.NONE));

            window_size = new Vector2(width, height);
            opengl_context = SDL.GL_CreateContext(window);
            Opengl.LoadGLProcs();

            Console.WriteLine($"Max texture size: {Opengl.MAX_TEXTURE_SIZE}");
            GFX.SetViewport((uint)window_size.X, (uint)window_size.Y);

            prev_ticks = DateTime.UtcNow.Ticks;
            keyboard_state = new byte[512];


            //Audio.Init(); 
            AssetsLoader.Init();

            SDL.StartTextInput();
        }

        public static void DestroyEngine()
        {
            SDL.GL_DeleteContext(opengl_context);
            SDL.DestroyWindow(window);
            SDL.Quit();
        }

        public static void Step()
        {
            long ticks_now = DateTime.UtcNow.Ticks;
            delta_time = (ticks_now - prev_ticks) / (float)TimeSpan.TicksPerSecond;
            prev_ticks = ticks_now;

            Input.this_frame_string_input = null;
            Input.got_user_input_this_frame = false;

            for (int i = 0; i < keyboard_state.Length; i++)
            {
                if (*(sdl_keyboard_state + i) > 0)
                {
                    if ((keyboard_state[i] & (byte)InputState.PRESSED) == 0)
                    {
                        keyboard_state[i] |= (byte)InputState.DOWN;
                    }
                    else
                    {
                        keyboard_state[i] = Utils.ClearBits(keyboard_state[i], (byte)InputState.DOWN);
                    }

                    Input.got_user_input_this_frame = true;

                    keyboard_state[i] |= (byte)InputState.PRESSED;
                }
                else
                {
                    if ((keyboard_state[i] & (byte)InputState.PRESSED) > 0)
                    {
                        keyboard_state[i] |= (byte)InputState.UP;
                    }
                    else
                    {
                        keyboard_state[i] = Utils.ClearBits(keyboard_state[i], (byte)InputState.UP);
                    }

                    keyboard_state[i] = Utils.ClearBits(keyboard_state[i], (byte)InputState.PRESSED | (byte)InputState.DOWN);

                }
            }

            HandleMouseEvents();
        }

        public static int PollEvent(out SDL_Event evnt)
        {
            var new_event = new SDL_Event();

            int r_value = SDL.PollEvent(out new_event);

            if (r_value > 0)
            {

                if (new_event.type == SDL_EVENT_TYPE.WINDOWEVENT)
                {
                    if (new_event.window._event == SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                    {
                        window_size = new Vector2(new_event.window.data1, new_event.window.data2);
                        GFX.SetViewport((uint)window_size.X, (uint)window_size.Y);
                    }
                }

                if (new_event.type == SDL_EVENT_TYPE.TEXTINPUT)
                {
                    var str = SDL.UTF8_ToManaged(new IntPtr(new_event.text_input.text));
                    Input.this_frame_string_input += str;
                }

            }

            evnt = new_event;

            return r_value;

        }

        static int SDL_BUTTON(MouseButton x)
        {
            return (1 << (((int)x) - 1));
        }

        static InputState HandleMouseEvent(MouseButton button, InputState mouse_button_state, uint mouse_state)
        {

            if ((SDL_BUTTON(button) & mouse_state) > 0)
            {
                if (mouse_button_state.HasFlag(InputState.PRESSED))
                {
                    mouse_button_state ^= InputState.DOWN;

                }
                else
                {
                    mouse_button_state |= InputState.DOWN;
                    mouse_button_state |= InputState.PRESSED;
                }
            }
            else
            {
                if (mouse_button_state.HasFlag(InputState.UP))
                {
                    mouse_button_state ^= InputState.UP;
                }

                if (mouse_button_state.HasFlag(InputState.PRESSED))
                {
                    mouse_button_state ^= InputState.UP;
                }

                mouse_button_state &= ~InputState.DOWN;
                mouse_button_state &= ~InputState.PRESSED;
            }

            return mouse_button_state;
        }

        static void HandleMouseEvents()
        {
            uint state = SDL.GetMouseState(out int x, out int y);
            mouse_state.mouse_position = new Vector2(x, y);

            mouse_state.left = HandleMouseEvent(MouseButton.SDL_BUTTON_LEFT, mouse_state.left, state);
            mouse_state.right = HandleMouseEvent(MouseButton.SDL_BUTTON_RIGHT, mouse_state.right, state);

        }

        public static void WaitForNextFrame()
        {
            if (frame_cap > 0)
            {
                uint ms = (uint)(Engine.delta_time * 1000);
                uint fc = (uint)MathF.Ceiling(1000.0f / frame_cap);

                if (ms < fc)
                {
                    SDL.Delay(fc - ms);
                }
            }
        }

        public static void SwapBuffers()
        {
            SDL.GL_SwapWindow(window);
        }

        public static uint TimeStamp()
        {
            return SDL.GetTicks();
        }

        public static T* Malloc<T>() where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal(sizeof(T));
        }

        public static byte* Malloc(int size)
        {
            return (byte*)Marshal.AllocHGlobal(size);
        }

        public static void Memcopy(void* dest, void* src, int size)
        {
            Buffer.MemoryCopy(src, dest, size, size);
        }

        public static void Free(void* ptr)
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
        }

        public static void SetWindowSize(int width, int height)
        {
            SDL.SetWindowSize(window, width, height);
            GFX.SetViewport((uint)width, (uint)height);
            window_size = new Vector2(width, height);
        }

        public static void SetWindowBordered(bool value)
        {
            SDL.SetWindowBordered(window, value);
        }

        public static void MaximizeWindow()
        {
            SDL.MaximizeWindow(window);
        }

        public static void MinimizeWindow()
        {
            SDL.MinimizeWindow(window);
        }

        public static void SetWindowFullscreen(SDL_WINDOW_FLAGS flags)
        {
            SDL.SetWindowFullscreen(window, flags);
        }

        public static void LogError(string text)
        {
#if DEBUG
            Console.WriteLine(text);
#endif
            File.AppendAllText("debug_log.txt", "-" + text);
        }
    }

}

