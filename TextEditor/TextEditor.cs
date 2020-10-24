using R;
using System;
using System.IO;
using System.Numerics;

namespace R.TextEditor
{

    public enum InputHoldKey
    {
        LEFT, RIGHT, UP, DOWN, REMOVE_CHAR, UNDO, REDO
    }

    public static unsafe class AppState
    {
        public static UIContext context;
        public static TextBuffer buffer;
        public static UIE_TextEditor text_editor;
        public static InputHold[] inputs;
        public static int cursor_remembered_x = 0;
        public static float move_speed_modifier = 1;

        static AppState()
        {
            inputs = new InputHold[Enum.GetNames(typeof(InputHoldKey)).Length];

            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = InputHold.Default;
            }
        }

    }

    public static unsafe class TextEditor
    {

        static void Main(string[] args)
        {
            Engine.Init(1280, 720);
            Renderer.Init();
            UI.Init();
            GFX.StencilClearValue(0);

            var loaded = AssetsLoader.LoadAsciiFont("Assets/Fonts/font_8x8.font", out var font);
            //var ttf_font = AssetsLoader.LoadTTFFontAsciiChars("Assets/Fonts/LiberationSans.ttf");

            font.d_pixel_between_characters = 1;
            font.d_pixel_line_height = 10;

            if (loaded)
            {
                UI.state.style.text_font = font;
                UI.state.style.text_size = 16;
            }

            bool running = true;

            string file = File.ReadAllText(@"..\..\..\TextEditor.cs");
            AppState.buffer = TextBuffer.Create(file);

            AppState.text_editor = new UIE_TextEditor(AppState.buffer);
            AppState.text_editor.word_color_supplier = new CSharpTextEditorWordColor();

            AppState.text_editor.rect.horizontal_alignment = RectAlignment.Start;
            AppState.text_editor.rect.horizontal_sizing = RectSizing.Strech;
            AppState.text_editor.rect.horizontal_alignment = RectAlignment.Start;
            AppState.text_editor.rect.vertical_sizing = RectSizing.Strech;

            AppState.context = new UIContext();
            AppState.context.elements.Add(AppState.text_editor);

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

                GFX.ClearColor(new Vector4(0.05f, 0.05f, 0.05f, 1));
                GFX.ClearAll();

                //Renderer.DrawQuad(Transform.Zero, Vector2.One, Renderer.CreateImageMaterail(ttf_font.texture));

                DoTextEditor();

                Engine.SwapBuffers();
            }

        }

        static void PutCursorOnRememberedX()
        {
            var buffer = AppState.buffer;
            if (buffer.cursor.x < AppState.cursor_remembered_x)
            {
                if (buffer.lines[buffer.cursor.y].Length >= AppState.cursor_remembered_x)
                {
                    buffer.cursor.x = AppState.cursor_remembered_x;
                }
                else
                {
                    buffer.cursor.x = buffer.lines[buffer.cursor.y].Length;
                }
            }
        }

        static void DoTextEditor()
        {

            if (Input.KeyPressed(SDL_Scancode.KEY_LALT))
            {
                if (Input.KeyPressed(SDL_Scancode.KEY_LCTRL))
                {
                    if (Input.KeyDown(SDL_Scancode.KEY_UP))
                    {
                        AppState.buffer.ExecuteAction(new TextBufferAction()
                        {
                            type = TextBufferActionType.DuplicateLine,
                            duplicated_line_diraction = -1
                        });
                    }
                    else if (Input.KeyDown(SDL_Scancode.KEY_DOWN))
                    {
                        AppState.buffer.ExecuteAction(new TextBufferAction()
                        {
                            type = TextBufferActionType.DuplicateLine,
                            duplicated_line_diraction = 1
                        });
                    }
                }
                else
                {
                    if (Input.DoInputHolder(SDL_Scancode.KEY_UP, ref AppState.inputs[(int)InputHoldKey.UP]))
                    {
                        AppState.buffer.ExecuteAction(new TextBufferAction()
                        {
                            type = TextBufferActionType.SwapLine,
                            swaped_line_diraction = -1
                        });
                    }
                    else if (Input.DoInputHolder(SDL_Scancode.KEY_DOWN, ref AppState.inputs[(int)InputHoldKey.DOWN]))
                    {
                        AppState.buffer.ExecuteAction(new TextBufferAction()
                        {
                            type = TextBufferActionType.SwapLine,
                            swaped_line_diraction = 1
                        });
                    }
                }
            }
            else
            {

                bool selection_active = Input.KeyPressed(SDL_Scancode.KEY_LSHIFT);

                if (Input.KeyPressed(SDL_Scancode.KEY_LCTRL))
                {
                    AppState.move_speed_modifier = 4;
                }
                else
                {
                    AppState.move_speed_modifier = 1;
                }

                if (Input.DoInputHolder(SDL_Scancode.KEY_LEFT, ref AppState.inputs[(int)InputHoldKey.LEFT], AppState.move_speed_modifier))
                {
                    AppState.buffer.MoveCursor(new Vector2I() { x = -1, y = 0 }, selection_active);
                    AppState.cursor_remembered_x = AppState.buffer.cursor.x;
                    AppState.text_editor.ResetCursorOn();
                }

                if (Input.DoInputHolder(SDL_Scancode.KEY_RIGHT, ref AppState.inputs[(int)InputHoldKey.RIGHT], AppState.move_speed_modifier))
                {
                    AppState.buffer.MoveCursor(new Vector2I() { x = 1, y = 0 }, selection_active);
                    AppState.cursor_remembered_x = AppState.buffer.cursor.x;
                    AppState.text_editor.ResetCursorOn();
                }

                if (Input.DoInputHolder(SDL_Scancode.KEY_UP, ref AppState.inputs[(int)InputHoldKey.UP], AppState.move_speed_modifier))
                {
                    AppState.buffer.MoveCursor(new Vector2I() { x = 0, y = -1 }, selection_active);
                    PutCursorOnRememberedX();
                    AppState.text_editor.ResetCursorOn();
                }

                if (Input.DoInputHolder(SDL_Scancode.KEY_DOWN, ref AppState.inputs[(int)InputHoldKey.DOWN], AppState.move_speed_modifier))
                {
                    AppState.buffer.MoveCursor(new Vector2I() { x = 0, y = 1 }, selection_active);
                    PutCursorOnRememberedX();
                    AppState.text_editor.ResetCursorOn();
                }
            }

            if (Input.KeyPressed(SDL_Scancode.KEY_LCTRL))
            {
                if (Input.DoInputHolder(SDL_Scancode.KEY_Z, ref AppState.inputs[(int)InputHoldKey.UNDO]))
                {
                    AppState.buffer.UndoLastAction();
                }
                if (Input.DoInputHolder(SDL_Scancode.KEY_Y, ref AppState.inputs[(int)InputHoldKey.REDO]))
                {
                    AppState.buffer.RedoLastAction();
                }
            }

            if (Input.KeyDown(SDL_Scancode.KEY_RETURN))
            {
                AppState.buffer.ExecuteAction(new TextBufferAction()
                {
                    type = TextBufferActionType.NextLine
                });

                AppState.text_editor.ResetCursorOn();
            }

            if (Input.DoInputHolder(SDL_Scancode.KEY_BACKSPACE, ref AppState.inputs[(int)InputHoldKey.REMOVE_CHAR]))
            {
                AppState.buffer.ExecuteAction(new TextBufferAction()
                {
                    type = TextBufferActionType.RemoveChar
                });
                AppState.text_editor.ResetCursorOn();
            }

            if (Input.KeyDown(SDL_Scancode.KEY_TAB))
            {
                AppState.buffer.ExecuteAction(new TextBufferAction()
                {
                    type = TextBufferActionType.InsertText,
                    inserted_text = "    "
                });
                AppState.text_editor.ResetCursorOn();
            }

            if (Input.KeyDown(SDL_Scancode.KEY_PAGEUP))
            {
                AppState.buffer.MoveCursor(new Vector2I() { x = 0, y = -1 * AppState.text_editor.number_of_line_to_render });
            }

            if (Input.KeyDown(SDL_Scancode.KEY_PAGEDOWN))
            {
                AppState.buffer.MoveCursor(new Vector2I() { x = 0, y = 1 * AppState.text_editor.number_of_line_to_render });
            }

            if (Input.KeyPressed(SDL_Scancode.KEY_RALT) && Input.KeyDown(SDL_Scancode.KEY_D))
            {
                AppState.text_editor.word_color_supplier = new DefaultTextEditorWordColor();
            }
            if (Input.KeyPressed(SDL_Scancode.KEY_RALT) && Input.KeyDown(SDL_Scancode.KEY_C))
            {
                AppState.text_editor.word_color_supplier = new CSharpTextEditorWordColor();
            }

            if (Input.this_frame_string_input != null)
            {
                string filterd_string = Input.this_frame_string_input;

                for (int i = 0; i < filterd_string.Length; i++)
                {
                    if (filterd_string[i] == '\n' || filterd_string[i] == '\r')
                    {
                        filterd_string = filterd_string.Remove(i, 1);
                    }
                }

                AppState.buffer.ExecuteAction(new TextBufferAction()
                {
                    type = TextBufferActionType.InsertText,
                    inserted_text = filterd_string
                });
                AppState.text_editor.ResetCursorOn();
            }

            UI.Update(Engine.window_size.X, Engine.window_size.Y, AppState.context);

            if (AppState.buffer.cursor.y > AppState.text_editor.top_line + AppState.text_editor.number_of_line_to_render)
            {
                AppState.buffer.cursor.y = AppState.text_editor.top_line + AppState.text_editor.number_of_line_to_render - 1; // -1 cuz we want the index of the line.
            }

            UI.Render(Engine.window_size.X, Engine.window_size.Y, AppState.context);
            //UI.Text(new Vector3(-Engine.window_size.X/2, 0, 0), edited_text);
            //UI.TextCursor(new Vector3(-Engine.window_size.X/2, 0, 0), edited_text, Input.cursor);
        }

    }
}
