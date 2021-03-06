﻿namespace R
{
    public enum InputHoldKey
    {
        LEFT, RIGHT, UP, DOWN, REMOVE_CHAR, UNDO, REDO
    }

    public struct InputHold
    {
        public float input_timer;
        public float rejection_time;
        public float time_between_inputs;
        public float rejection_timer;
        public bool active_last_frame;

        public static InputHold Default = new InputHold
        {
            input_timer = 0,
            rejection_time = 0.25f,
            time_between_inputs = 0.025f,
            active_last_frame = false,
        };

    }

    public unsafe static class Input
    {

        public static string this_frame_string_input;
        public static bool got_user_input_this_frame = false;

        public static bool KeyDown(SDL_Scancode scancode)
        {
            return (Engine.keyboard_state[(int)scancode] & (byte)InputState.DOWN) > 0;
        }

        public static bool KeyPressed(SDL_Scancode scancode)
        {
            return (Engine.keyboard_state[(int)scancode] & (byte)InputState.PRESSED) > 0;
        }

        public static bool KeyUp(SDL_Scancode scancode)
        {
            return (Engine.keyboard_state[(int)scancode] & (byte)InputState.UP) > 0;
        }

        public static bool DoInputHolder(SDL_Scancode scancode, ref InputHold input_holder, float speed_modifier = 1)
        {
            if (KeyPressed(scancode))
            {

                if (!input_holder.active_last_frame)
                {
                    input_holder.active_last_frame = true;
                    return true;
                }
                else
                {
                    input_holder.rejection_timer += Engine.delta_time * speed_modifier;

                    if (input_holder.rejection_timer > input_holder.rejection_time)
                    {
                        input_holder.input_timer += Engine.delta_time * speed_modifier;

                        if (input_holder.input_timer > input_holder.time_between_inputs)
                        {

                            input_holder.input_timer -= input_holder.time_between_inputs;
                            return true;
                        }
                    }

                    return false;
                }
            }
            else
            {
                input_holder.active_last_frame = false;
                input_holder.rejection_timer = 0;
                input_holder.input_timer = 0;

                return false;
            }
        }

        /** Old string input
         * public static void Old_TextEditing_Start(string str)
        {
            editing_text = true;
            edited_text = str;
            cursor = edited_text.Length;
            SDL.StartTextInput();
        }

        public static void Old_TextEditing_Stop()
        {
            editing_text = false;
            edited_text = null;
            SDL.StopTextInput();
        }

        public static void Old_TextEditing_Event(SDL_Event ev)
        {
            if (editing_text)
            {
                if (ev.type == SDL_EVENT_TYPE.TEXTINPUT)
                {
                    var str = SDL.UTF8_ToManaged(new IntPtr(ev.text_input.text));
                    Old_TextEditing_InsertString(str);

                    
                    int chars_in_line = 0;
                    bool moving_left = true;
                    int char_idx = char_cursor - 1;

                    while (chars_in_line < character_per_line && char_idx < edited_text.Length)
                    {
                        if (moving_left)
                        {
                            if (char_idx < 0 || 
                                edited_text[char_idx] == '\n')
                            {
                                moving_left = false;
                                char_idx = char_cursor + 1;
                            }
                            else
                            {
                                char_idx -= 1;
                                chars_in_line += 1;
                            }
                        }
                        else
                        {
                            if (char_idx == edited_text.Length || 
                                edited_text[char_idx] == '\n')
                            {
                                break;
                            }
                            else
                            {
                                char_idx += 1;
                                chars_in_line += 1;
                            }
                        }
                    }

                    if (chars_in_line + str.Length <= character_per_line)
                    {
                        TextEditing_InsertString(str);

                        if (chars_in_line + str.Length == character_per_line &&
                           auto_insert_next_line_at_end_of_the_line)
                        {
                            TextEditing_InsertString("\n");
                        }
                    }
                    else if (chars_in_line != character_per_line)
                    {
                        int idx = character_per_line - chars_in_line;

                        TextEditing_InsertString(str.Substring(0, idx));

                        if (auto_insert_next_line_at_end_of_the_line)
                        {
                            TextEditing_InsertString('\n' + str.Substring(idx));
                        }
                    }
                    
                }
            }
        }

        public static void Old_TextEditing_Update()
        {

            if (KeyDown(SDL_Scancode.KEY_BACKSPACE) && Old_TextEditing_RemoveCharacter())
            {
                start_rejecting_backspace = true;
                start_backspacing = false;
                backspace_timer = 0;
            }

            if (KeyPressed(SDL_Scancode.KEY_BACKSPACE))
            {
                if (start_rejecting_backspace)
                {
                    backspace_timer += Engine.delta_time;

                    if (backspace_timer > backspace_rejection_time)
                    {
                        start_backspacing = true;
                        start_rejecting_backspace = false;
                        backspace_timer = 0;
                    }
                }
                else if (start_backspacing)
                {
                    backspace_timer += Engine.delta_time;

                    if (backspace_timer > backspace_time_between)
                    {
                        Old_TextEditing_RemoveCharacter();
                        backspace_timer -= backspace_time_between;
                    }
                }
                else
                {
                    start_backspacing = false;
                    start_rejecting_backspace = false;
                }
            }

            if (KeyDown(SDL_Scancode.KEY_RETURN))
            {
                Old_TextEditing_InsertString("\n");
            }

            if (KeyDown(SDL_Scancode.KEY_TAB))
            {
                for (int i = 0; i < tab_spaces; i++)
                {
                    Old_TextEditing_InsertString(" ");
                }
            }
        }

        public static void Old_TextEditing_MoveCursorHorizontaly(int chars_to_move)
        {
            cursor += chars_to_move;

            if (cursor <= 0)
            {
                cursor = 0;
            }

            if (cursor > edited_text.Length)
            {
                cursor = edited_text.Length;
            }
        }

        public static void Old_TextEditing_MoveCursorVerticly(int direction)
        {
            if (edited_text.Length == 0) return;

            int offset = Math.Abs(direction) / direction;

            if (offset > 0)
            {
                int new_cursor = cursor;
                int left_chars = Old_TextEditing_CountCharsFromLeft(cursor);

                if (new_cursor == edited_text.Length) new_cursor -= 1;
                if (edited_text[new_cursor] == '\n') new_cursor -= 1;

                while (new_cursor > 0 && edited_text[new_cursor] != '\n')
                {
                    new_cursor -= 1;
                }

                if (new_cursor == 0)
                {
                    cursor = 0;
                    return;
                }

                int left_chars_new = Old_TextEditing_CountCharsFromLeft(new_cursor);
                Console.WriteLine($"{left_chars} | {left_chars_new}");
                int difference = left_chars_new - left_chars;

                if (difference > 0)
                {
                    new_cursor -= difference;
                }

                cursor = new_cursor;
            }
            else if (offset < 0)
            {
                if (cursor == edited_text.Length) return;

                int left_chars = Old_TextEditing_CountCharsFromLeft(cursor);
                int new_cursor = cursor;

                bool started_with_next_line = edited_text[new_cursor] == '\n';

                while (edited_text[new_cursor] != '\n')
                {
                    new_cursor += 1;

                    if (new_cursor >= edited_text.Length)
                    {
                        cursor = edited_text.Length;
                        return;
                    }
                }

                int right_new_cursor = Old_TextEditing_CountCharsFromRight(new_cursor);
                new_cursor += Math.Min(left_chars + 1, right_new_cursor) + (started_with_next_line ? 1 : 0);
                cursor = new_cursor;
            }

        }

        public static int Old_TextEditing_CountCharsInLine(int idx)
        {
            return Old_TextEditing_CountCharsFromLeft(idx) + Old_TextEditing_CountCharsFromRight(idx);
        }

        public static int Old_TextEditing_CountCharsFromLeft(int idx)
        {
            if (idx == 0) return 0;

            int chars_from_left = 0;


            if (idx == edited_text.Length)
            {
                idx -= 1;
            }
            else
            {
                idx -= 1;
            }

            while (idx >= 0)
            {

                if (edited_text[idx] != '\n')
                {
                    chars_from_left += 1;
                }
                else
                {
                    break;
                }

                idx -= 1;
            }
            return chars_from_left;
        }

        public static int Old_TextEditing_CountCharsFromRight(int idx)
        {
            if (idx == edited_text.Length) return 0;
            int chars_from_right = 0;

            if (edited_text[idx] == '\n')
            {
                idx += 1;
            }

            while (idx < edited_text.Length)
            {
                if (edited_text[idx] != '\n')
                {
                    chars_from_right += 1;
                }
                else
                {
                    break;
                }

                idx += 1;
            }

            return chars_from_right;
        }

        //returns true if remove char fro edited_text
        static bool Old_TextEditing_RemoveCharacter()
        {
           if (edited_text.Length > 0 && cursor != 0)
            {
                edited_text = edited_text.Remove(cursor - 1, 1);
                cursor -= 1;
                return true;
            }

            return false;
        }

        static void Old_TextEditing_InsertString(string str)
        {
            edited_text = edited_text.Insert(cursor, str);
            cursor += str.Length;
        }
    **/
    }

    public unsafe static class TextBufferInput
    {
        public static void DoInput(TextBuffer buffer, InputHold[] holders, ref int remembered_x)
        {
            if (Input.KeyPressed(SDL_Scancode.KEY_LALT))
            {
                if (Input.KeyPressed(SDL_Scancode.KEY_LCTRL))
                {
                    if (Input.KeyDown(SDL_Scancode.KEY_UP))
                    {
                        buffer.ExecuteAction(new TextBufferAction()
                        {
                            type = TextBufferActionType.DuplicateLine,
                            duplicated_line_diraction = -1
                        });
                    }
                    else if (Input.KeyDown(SDL_Scancode.KEY_DOWN))
                    {
                        buffer.ExecuteAction(new TextBufferAction()
                        {
                            type = TextBufferActionType.DuplicateLine,
                            duplicated_line_diraction = 1
                        });
                    }
                }
                else
                {
                    if (Input.DoInputHolder(SDL_Scancode.KEY_UP, ref holders[(int)InputHoldKey.UP]))
                    {
                        buffer.ExecuteAction(new TextBufferAction()
                        {
                            type = TextBufferActionType.SwapLine,
                            swaped_line_diraction = -1
                        });
                    }
                    else if (Input.DoInputHolder(SDL_Scancode.KEY_DOWN, ref holders[(int)InputHoldKey.DOWN]))
                    {
                        buffer.ExecuteAction(new TextBufferAction()
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

                var move_speed_modifier = 1;

                if (Input.KeyPressed(SDL_Scancode.KEY_LCTRL))
                {
                    move_speed_modifier = 4;

                    if (Input.KeyDown(SDL_Scancode.KEY_C))
                    {
                        var res = buffer.ToStringSelection();
                        System.Console.WriteLine(res);
                        SDL.SetClipboardText(res);
                    }
                    else if (Input.KeyDown(SDL_Scancode.KEY_V))
                    {
                        if (SDL.HasClipboardText())
                        {
                            buffer.ExecuteAction(new TextBufferAction()
                            {
                                type = TextBufferActionType.InsertText,
                                inserted_text = SDL.GetClipboardText()
                            });
                        }
                    }

                }

                if (Input.DoInputHolder(SDL_Scancode.KEY_LEFT, ref holders[(int)InputHoldKey.LEFT], move_speed_modifier))
                {
                    buffer.MoveCursor(new Vector2I() { x = -1, y = 0 }, selection_active);
                    remembered_x = buffer.cursor.x;
                }

                if (Input.DoInputHolder(SDL_Scancode.KEY_RIGHT, ref holders[(int)InputHoldKey.RIGHT], move_speed_modifier))
                {
                    buffer.MoveCursor(new Vector2I() { x = 1, y = 0 }, selection_active);
                    remembered_x = buffer.cursor.x;
                }

                if (Input.DoInputHolder(SDL_Scancode.KEY_UP, ref holders[(int)InputHoldKey.UP], move_speed_modifier))
                {
                    buffer.MoveCursor(new Vector2I() { x = 0, y = -1 }, selection_active);
                    PutCursorOnRememberedX(buffer, remembered_x);
                }

                if (Input.DoInputHolder(SDL_Scancode.KEY_DOWN, ref holders[(int)InputHoldKey.DOWN], move_speed_modifier))
                {
                    buffer.MoveCursor(new Vector2I() { x = 0, y = 1 }, selection_active);
                    PutCursorOnRememberedX(buffer, remembered_x);
                }
            }

            if (Input.KeyPressed(SDL_Scancode.KEY_LCTRL))
            {
                if (Input.DoInputHolder(SDL_Scancode.KEY_Z, ref holders[(int)InputHoldKey.UNDO]))
                {
                    buffer.UndoLastAction();
                }
                if (Input.DoInputHolder(SDL_Scancode.KEY_Y, ref holders[(int)InputHoldKey.REDO]))
                {
                    buffer.RedoLastAction();
                }
            }

            if (Input.KeyDown(SDL_Scancode.KEY_RETURN))
            {
                buffer.ExecuteAction(new TextBufferAction()
                {
                    type = TextBufferActionType.NextLine
                });
            }

            if (Input.DoInputHolder(SDL_Scancode.KEY_BACKSPACE, ref holders[(int)InputHoldKey.REMOVE_CHAR]))
            {
                buffer.ExecuteAction(new TextBufferAction()
                {
                    type = TextBufferActionType.RemoveChar
                });
            }

            if (Input.KeyDown(SDL_Scancode.KEY_TAB))
            {
                buffer.ExecuteAction(new TextBufferAction()
                {
                    type = TextBufferActionType.InsertText,
                    inserted_text = "    "
                });
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

                buffer.ExecuteAction(new TextBufferAction()
                {
                    type = TextBufferActionType.InsertText,
                    inserted_text = filterd_string
                });
            }

        }

        static void PutCursorOnRememberedX(TextBuffer buffer, int rememberd_x)
        {
            if (buffer.cursor.x < rememberd_x)
            {
                if (buffer.lines[buffer.cursor.y].Length >= rememberd_x)
                {
                    buffer.cursor.x = rememberd_x;
                }
                else
                {
                    buffer.cursor.x = buffer.lines[buffer.cursor.y].Length;
                }
            }
        }
    }
}
