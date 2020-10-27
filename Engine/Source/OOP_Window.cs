using System.Numerics;

namespace R
{

    public abstract unsafe class OOP_Window
    {
        public static readonly Vector2I back_buffer_size = new Vector2I() { x = 240, y = 160 };

        public byte* back_buffer { get; private set; } = (byte*)0;
        public bool DisableBackBuffer = false;
        public bool running { get; private set; }
        public Vector2I window_size { get; private set; }

        public abstract void Update();
        public abstract void EventCallback(SDL_Event ev);

        private uint back_buffer_texture;

        public OOP_Window()
        {
            Engine.Init("");
            Renderer.Init();
            back_buffer_texture = GFX.CreateTexture();
            back_buffer = Engine.Malloc(back_buffer_size.x * back_buffer_size.y * sizeof(int));

            SetSize(back_buffer_size.x * 3, back_buffer_size.y * 3);
        }

        public void Run()
        {
            running = true;

            while(running)
            {
                Engine.Step();

                while (Engine.PollEvent(out var ev) > 0)
                {
                    if (ev.type == SDL_EVENT_TYPE.WINDOWEVENT)
                    {
                        if (ev.window._event == SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                        {
                            int width = ev.window.data1;
                            int height = ev.window.data2;

                            SetSizeFields(width, height);
                        }
                    }

                    EventCallback(ev);
                }

                Update();

                if(!DisableBackBuffer)
                {
                    Renderer.CameraPosition = Transform.Zero;
                    GFX.BufferTexture(back_buffer, back_buffer_size.x, back_buffer_size.y, TextureType.BIT_32, (int)back_buffer_texture);

                    float ratio = (float)back_buffer_size.x/ (float)back_buffer_size.y;
                    Transform tran = Transform.Zero;
                    tran.scale.Y *= -1;
                    Renderer.DrawQuad(tran, new Vector2(2, 2), back_buffer_texture);
                    Engine.SwapBuffers();
                }
            }
        }

        public void SetSize(int width, int height)
        {
            Engine.SetWindowSize(width, height);
            SetSizeFields(width, height);
        }

        void SetSizeFields(int width, int height)
        {
            window_size = new Vector2I() { x = width, y = height };
        }

        public void Stop()
        {
            running = false;
        }

        public void SwapBuffers()
        {
            Engine.SwapBuffers();
        }

    }

}
