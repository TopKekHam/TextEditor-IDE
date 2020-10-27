using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace R.TextEditor
{

    [Flags]
    public enum VisibleUIElements
    {
        LINE_NUMBERS = 1, CONSOLE
    }

    public enum ActiveBuffer
    {
        FILE = 1, CONSOLE = 2
    }

    public static unsafe class AppState
    {
        public static UIContext context;
        public static TextBuffer buffer;
        public static InputHold[] input_holders;
        public static int cursor_remembered_x = 0;
        public static float move_speed_modifier = 1;
        public static VisibleUIElements visible_elements = VisibleUIElements.LINE_NUMBERS;
        public static UIE_TextEditor text_editor;
        public static UIE_TextEditor_LineNumbers text_editor_line_numbers;
        public static UIE_Console console;
        public static ActiveBuffer active_buffer_type = ActiveBuffer.FILE;

        public static List<TextBuffer> text_buffers;
        public static TextBuffer console_text_buffer;
        public static int need_to_draw_frame;

        static AppState()
        {
            console_text_buffer = new TextBuffer();
            console_text_buffer.single_line_mode = true;
            text_buffers = new List<TextBuffer>();

            input_holders = new InputHold[Enum.GetNames(typeof(InputHoldKey)).Length];
            for (int i = 0; i < input_holders.Length; i++)
            {
                input_holders[i] = InputHold.Default;
            }
        }

    }

    public static unsafe class TextEditor
    {

        static void Main(string[] args)
        {
            Engine.Init("", 1280, 720);
            Renderer.Init();
            UI.Init();

            Ascii_Font font = new Ascii_Font();
            Ascii_Font font2 = new Ascii_Font();

            var loaded = AssetsLoader.LoadAsciiFont("Assets/Fonts/font_8x8.font", ref font);

            UI.state.style.text_font = font;
            UI.state.style.text_size = 16;

            bool running = true;

            string file = File.ReadAllText(@"..\..\..\TextEditor.cs");
            var file_buffer = new TextBuffer(file);
            AppState.buffer = file_buffer;
            AppState.text_buffers.Add(file_buffer);

            AppState.text_editor = new UIE_TextEditor(AppState.buffer);
            AppState.console = new UIE_Console(AppState.console_text_buffer);
            AppState.text_editor.word_color_supplier = new CSharpTextEditorWordColor();
            AppState.context = new UIContext();

            AppState.text_editor_line_numbers = new UIE_TextEditor_LineNumbers(AppState.text_editor);

            AppState.context.elements.Add(AppState.text_editor);
            AppState.context.elements.Add(AppState.text_editor_line_numbers);
            AppState.context.elements.Add(AppState.console);

            AppState.need_to_draw_frame = 3;

            while (running)
            {
                Engine.Step();

                while (Engine.PollEvent(out var ev) > 0)
                {
                    if (ev.type == SDL_EVENT_TYPE.WINDOWEVENT && ev.window._event == SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
                    {
                        running = false;
                        break;
                    }
                }

                DoInputs();

                if (AppState.need_to_draw_frame > 0 || Input.got_user_input_this_frame)
                {
                    UpdateLayout();
                    UI.Update(Engine.window_size.X, Engine.window_size.Y, AppState.context);

                    if (AppState.text_buffers[0].cursor.y > AppState.text_editor.top_line + AppState.text_editor.number_of_line_to_render)
                    {
                        AppState.text_buffers[0].cursor.y = AppState.text_editor.top_line + AppState.text_editor.number_of_line_to_render - 1; // -1 cuz we want the index of the line.
                    }

                    GFX.ClearColor(new Vector4(0.05f, 0.05f, 0.05f, 1));
                    GFX.ClearAll();
                    UI.Render(Engine.window_size.X, Engine.window_size.Y, AppState.context);

                    //Renderer.DrawQuad(Transform.Zero, new Vector2(font_ttf.texture_width, font_ttf.texture_height), Renderer.CreateImageMaterail(font_ttf.texture));

                    Engine.SwapBuffers();
                    AppState.need_to_draw_frame -= 1;
                }
            }

        }

        static void DoInputs()
        {
            if (Input.KeyDown(SDL_Scancode.KEY_F1))
            {
                AppState.visible_elements ^= VisibleUIElements.LINE_NUMBERS;
            }

            if (Input.KeyDown(SDL_Scancode.KEY_ESCAPE))
            {
                AppState.visible_elements ^= VisibleUIElements.CONSOLE;

                if ((int)(AppState.visible_elements & VisibleUIElements.CONSOLE) > 0)
                {
                    AppState.active_buffer_type = ActiveBuffer.CONSOLE;
                }
                else
                {
                    AppState.active_buffer_type = ActiveBuffer.FILE;
                }
            }

            if (AppState.active_buffer_type == ActiveBuffer.FILE)
            {
                AppState.buffer = AppState.text_buffers[0];
            }
            else if (AppState.active_buffer_type == ActiveBuffer.CONSOLE)
            {
                AppState.buffer = AppState.console_text_buffer;
            }

            if (AppState.active_buffer_type == ActiveBuffer.CONSOLE)
            {
                if (Input.KeyDown(SDL_Scancode.KEY_RETURN))
                {
                    ExecuteCommand(AppState.buffer.ToString());
                    AppState.buffer.Clear();
                }
            }
            else if (AppState.active_buffer_type == ActiveBuffer.FILE)
            {
                if (Input.KeyPressed(SDL_Scancode.KEY_RALT) && Input.KeyDown(SDL_Scancode.KEY_D))
                {
                    AppState.text_editor.word_color_supplier = new DefaultTextEditorWordColor();
                }

                if (Input.KeyPressed(SDL_Scancode.KEY_RALT) && Input.KeyDown(SDL_Scancode.KEY_C))
                {
                    AppState.text_editor.word_color_supplier = new CSharpTextEditorWordColor();
                }

                if (Input.KeyDown(SDL_Scancode.KEY_PAGEUP))
                {
                    AppState.buffer.MoveCursor(new Vector2I() { x = 0, y = -1 * AppState.text_editor.number_of_line_to_render });
                }

                if (Input.KeyDown(SDL_Scancode.KEY_PAGEDOWN))
                {
                    AppState.buffer.MoveCursor(new Vector2I() { x = 0, y = 1 * AppState.text_editor.number_of_line_to_render });
                }

            }

            TextBufferInput.DoInput(AppState.buffer, AppState.input_holders, ref AppState.cursor_remembered_x);
        }

        static void ExecuteCommand(string command)
        {
            var command_params = command.Split(' ');
        }

        static void UpdateLayout()
        {
            float text_editor_start_x = 0;
            float text_editor_end_x = UI.state.canvas_size.X;
            float text_editor_start_y = 0;
            float text_editor_end_y = UI.state.canvas_size.Y;

            var show_line_numbers = AppState.visible_elements.HasFlag(VisibleUIElements.LINE_NUMBERS);
            AppState.text_editor_line_numbers.enabled = show_line_numbers;

            if (show_line_numbers)
            {
                int digits = AppState.text_buffers[0].lines.Count.DigitNumber() + 1;
                float line_numbers_width = Ascii_Font_Utils.GetTextWidth(UI.state.style.text_font, new string('#', digits), UI.state.style.text_size);
                text_editor_start_x += line_numbers_width;
                var ln = AppState.text_editor_line_numbers;
                ln.rect.size.X = line_numbers_width;
                ln.rect.size.Y = Engine.window_size.Y;
                ln.rect.Align(UI.state.canvas_size, RectAlignment.Start, RectAlignment.Start);
            }

            var show_console = AppState.visible_elements.HasFlag(VisibleUIElements.CONSOLE);
            AppState.console.enabled = show_console;

            if (show_console)
            {
                var console = AppState.console;
                float line_height = Ascii_Font_Utils.GetLineHeight(UI.state.style.text_font, UI.state.style.text_size);
                console.rect.size.X = UI.state.canvas_size.X;
                console.rect.size.Y = line_height * 1.25f;
                console.rect.Align(UI.state.canvas_size, RectAlignment.Center, RectAlignment.End);
                text_editor_end_y -= console.rect.size.Y;
            }

            var editor = AppState.text_editor;

            editor.rect.size.X = text_editor_end_x - text_editor_start_x;
            editor.rect.size.Y = text_editor_end_y - text_editor_start_y;
            editor.rect.Align(UI.state.canvas_size, RectAlignment.Start, RectAlignment.Start);
            editor.rect.transform.position.X += text_editor_start_x;
            editor.rect.transform.position.Y += text_editor_start_y;
        }

    }
}
