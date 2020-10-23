using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace R
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2I
    {
        public int x, y;
    }

    public class TextBuffer
    {
        public List<string> lines;
        public Vector2I cursor;

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

        public void NextLine()
        {

            string inserted_string = "";

            if (cursor.x < lines[cursor.y].Length)
            {
                inserted_string = lines[cursor.y].Substring(cursor.x);
                lines[cursor.y] = lines[cursor.y].Remove(cursor.x);
            }

            cursor.y += 1;
            cursor.x = 0;
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

        public void RemoveChar()
        {

            if (cursor.x == 0)
            {
                if (cursor.y > 0)
                {
                    int length = lines[cursor.y - 1].Length;
                    lines[cursor.y - 1] = lines[cursor.y - 1] + lines[cursor.y];
                    lines.RemoveAt(cursor.y);
                    cursor.y -= 1;
                    cursor.x += length;
                }
                else
                {
                    // do nothing...
                }
            }
            else
            {
                if (lines[cursor.y].Length > 0)
                {
                    lines[cursor.y] = lines[cursor.y].Remove(cursor.x - 1, 1);
                    cursor.x -= 1;
                }
                else
                {
                    lines.RemoveAt(cursor.y);
                }
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
}
