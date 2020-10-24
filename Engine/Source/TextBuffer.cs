using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Transactions;

namespace R
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2I
    {
        public int x, y;
    }

    public enum TextBufferActionType : int
    {
        InsertText, SwapLine, DuplicateLine, RemoveChar, NextLine
    }

    public enum TextBufferCharRemoveOperationType : int
    {
        NONE, CHAR, LINE
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct TextBufferCharRemoveOperation
    {
        [FieldOffset(0)] public TextBufferCharRemoveOperationType type;
        [FieldOffset(4)] public char removed_char;
        [FieldOffset(8)] public int prev_line_length;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct TextBufferAction
    {
        [FieldOffset(0)] public TextBufferActionType type;
        [FieldOffset(4)] public Vector2I cursor_pos;

        //inserted text
        [FieldOffset(32)] public string inserted_text;

        //removed char
        [FieldOffset(12)] public TextBufferCharRemoveOperation remove_operation;

        //spawed line
        [FieldOffset(12)] public int swaped_line_diraction;

        //duplicated line
        [FieldOffset(12)] public int duplicated_line;
        [FieldOffset(16)] public int duplicated_line_diraction;

        //next line
        [FieldOffset(12)] public Vector2I new_cursor_pos;
    }

    public class TextBuffer
    {
        public List<string> lines;
        public Vector2I cursor;
        public TextBufferActionList actions = new TextBufferActionList();

        public static TextBuffer Create()
        {
            TextBuffer buffer = new TextBuffer();

            buffer.lines = new List<string>();
            buffer.cursor = new Vector2I() { x = 0, y = 0 };

            return buffer;
        }

        public static TextBuffer Create(string src)
        {
            TextBuffer buffer = new TextBuffer();

            buffer.lines = new List<string>();
            buffer.cursor = new Vector2I() { x = 0, y = 0 };

            int start = 0;

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == '\r')
                {
                    buffer.lines.Add(src.Substring(start, i - start));
                    start = i + 1;
                }
                else if (src[i] == '\n')
                {
                    start = i + 1;

                    if (i - 1 > 0 && src[i - 1] != '\r')
                    {
                        buffer.lines.Add(src.Substring(start, i - start));
                    }
                }
            }

            if (start != src.Length)
            {
                buffer.lines.Add(src.Substring(start, src.Length - start));
            }

            return buffer;
        }

        public void InsertText(string str)
        {
            lines[cursor.y] = lines[cursor.y].Insert(cursor.x, str);
            cursor.x += str.Length;
        }

        public bool SwapLine(int dir)
        {
            if (lines.Count > dir + cursor.y && 0 <= dir + cursor.y)
            {
                string temp = lines[cursor.y + dir];
                lines[cursor.y + dir] = lines[cursor.y];
                lines[cursor.y] = temp;
                cursor.y += dir;
                return true;
            }

            return false;
        }

        public void DuplicateLine(int dir)
        {
            if (dir < 0)
            {
                dir += 1;
            }

            lines.Insert(cursor.y + dir, lines[cursor.y]);
            cursor.y += dir;
        }

        public void NextLine(bool add_indentation)
        {

            string inserted_string = "";
            int spaces = 0;

            if (add_indentation)
            {

                while (lines[cursor.y].Length > spaces && lines[cursor.y][spaces] == ' ')
                {
                    spaces++;
                }

                inserted_string = new string(' ', spaces);
            }

            if (cursor.x < lines[cursor.y].Length)
            {
                inserted_string += lines[cursor.y].Substring(cursor.x);
                lines[cursor.y] = lines[cursor.y].Remove(cursor.x);
            }

            cursor.y += 1;
            cursor.x = spaces;
            lines.Insert(cursor.y, inserted_string);
        }

        public void MoveCursor(Vector2I move_vec)
        {
            if (move_vec.y != 0)
            {
                var height = lines.Count;
                cursor.y = Math.Clamp(cursor.y + move_vec.y, 0, height - 1);

                var width = lines[cursor.y].Length;
                cursor.x = Math.Clamp(cursor.x + move_vec.x, 0, width);
            }

            if (move_vec.x != 0)
            {
                var height = lines.Count;
                var width = lines[cursor.y].Length;

                cursor.x += move_vec.x;

                if (cursor.x > width)
                {
                    cursor.y += 1;
                    cursor.x -= width + 1;
                }

                if (cursor.x < 0)
                {
                    int y = cursor.y - 1;
                    cursor.y = Math.Max(cursor.y - 1, 0);

                    if (y < 0)
                    {
                        cursor.x = 0;
                    }
                    else
                    {
                        cursor.x = Math.Max(lines[cursor.y].Length + 1 + cursor.x, 0);
                    }
                }

                cursor.y = Math.Clamp(cursor.y, 0, height);
            }

        }

        public TextBufferCharRemoveOperation RemoveChar()
        {
            TextBufferCharRemoveOperation operation = new TextBufferCharRemoveOperation();
            if (cursor.x == 0)
            {
                if (cursor.y > 0)
                {
                    operation.type = TextBufferCharRemoveOperationType.LINE;

                    int length = lines[cursor.y - 1].Length;
                    operation.prev_line_length = length;

                    lines[cursor.y - 1] = lines[cursor.y - 1] + lines[cursor.y];
                    lines.RemoveAt(cursor.y);
                    cursor.y -= 1;
                    cursor.x += length;
                }
                else
                {
                    // do nothing...
                    operation.type = TextBufferCharRemoveOperationType.NONE;
                }
            }
            else
            {
                if (lines[cursor.y].Length > 0)
                {
                    operation.removed_char = lines[cursor.y][cursor.x - 1];
                    operation.type = TextBufferCharRemoveOperationType.CHAR;

                    lines[cursor.y] = lines[cursor.y].Remove(cursor.x - 1, 1);
                    cursor.x -= 1;
                }
            }

            return operation;
        }

        public void ExecuteAction(TextBufferAction action, bool save_action_to_buffer = true)
        {
            action.cursor_pos = cursor;

            switch (action.type)
            {
                case TextBufferActionType.InsertText:
                    {
                        action.inserted_text = new string(action.inserted_text);
                        InsertText(action.inserted_text);
                    }
                    break;
                case TextBufferActionType.SwapLine:
                    {
                        action.type = TextBufferActionType.SwapLine;
                        var swaped = SwapLine(action.swaped_line_diraction);
                        if (!swaped) return;
                    }
                    break;
                case TextBufferActionType.DuplicateLine:
                    {
                        action.type = TextBufferActionType.DuplicateLine;
                        action.duplicated_line = cursor.y;
                        DuplicateLine(action.duplicated_line_diraction);
                    }
                    break;
                case TextBufferActionType.RemoveChar:
                    {
                        var operation = RemoveChar();
                        if (operation.type == TextBufferCharRemoveOperationType.NONE) return;

                        action.remove_operation = operation;
                    }
                    break;
                case TextBufferActionType.NextLine:
                    {
                        action.type = TextBufferActionType.NextLine;
                        NextLine(true);
                        action.new_cursor_pos = cursor;
                    }
                    break;
            }

            if(save_action_to_buffer)
            {
                actions.AddAction(action);
            }
        }

        public void UndoLastAction()
        {
            if (!actions.Empty())
            {
                var action = actions.Current();

                switch (action.type)
                {
                    case TextBufferActionType.InsertText:
                        {
                            var cr = action.cursor_pos;
                            lines[cr.y] = lines[cr.y].Remove(cr.x, action.inserted_text.Length);
                            cursor = cr;
                        }
                        break;
                    case TextBufferActionType.SwapLine:
                        {
                            cursor = action.cursor_pos;
                            cursor.y += action.swaped_line_diraction;
                            SwapLine(action.swaped_line_diraction * -1);
                        }
                        break;
                    case TextBufferActionType.DuplicateLine:
                        {
                            lines.RemoveAt(action.duplicated_line);
                            cursor = action.cursor_pos;
                        }
                        break;
                    case TextBufferActionType.RemoveChar:
                        {
                            var cr = action.cursor_pos;
                            var ch = action.remove_operation.removed_char;

                            switch (action.remove_operation.type)
                            {
                                case TextBufferCharRemoveOperationType.NONE:
                                    break;
                                case TextBufferCharRemoveOperationType.CHAR:
                                    lines[cr.y] = lines[cr.y].Insert(cr.x - 1, $"{ch}");
                                    break;
                                case TextBufferCharRemoveOperationType.LINE:
                                    int len = action.remove_operation.prev_line_length;
                                    var concat_str = "";

                                    if (lines[cr.y - 1].Length > len)
                                    {
                                        concat_str = lines[cr.y - 1].Substring(len);
                                        lines[cr.y - 1] = lines[cr.y - 1].Remove(len);
                                    }

                                    lines.Insert(cr.y, concat_str);

                                    break;
                            }
                            cursor = cr;
                        }
                        break;
                    case TextBufferActionType.NextLine:
                        {
                            cursor = action.new_cursor_pos;
                            RemoveChar();
                        }
                        break;
                }

                actions.MovePrev();
            }
        }

        public void RedoLastAction()
        {
            if(actions.MoveNext())
            {
                ExecuteAction(actions.Current(), false);
            }
        }

        public string ToString()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < lines.Count; i++)
            {
                builder.Append(lines[i]);

                if (lines.Count - 1 != i)
                {
                    builder.Append("\r\n");
                }
            }

            return builder.ToString();
        }

    }


    public class TextBufferActionList
    {
        public ArrayList<TextBufferAction> actions = new ArrayList<TextBufferAction>(128);

        int current = -1;
        int max = -1;

        public void AddAction(TextBufferAction action)
        {
            current += 1;

            if (current == actions.count)
            {
                actions.AddItem(action);
            }
            else
            {
                actions[current] = action;
            }

            max = current;
        }

        public void MovePrev()
        {
            if (current >= 0)
            {
                current -= 1;
            }
        }

        public bool MoveNext()
        {
            if (current < max)
            {
                current += 1;
                return true;
            }

            return false;
        }

        public TextBufferAction Current()
        {
            return actions[current];
        }

        public bool Empty()
        {
            return (current < 0);
        }
    }
}


