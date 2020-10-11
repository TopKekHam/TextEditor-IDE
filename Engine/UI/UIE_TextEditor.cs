using System;
using System.Numerics;

namespace R
{
    public struct UIE_TextEditor_Style
    {
        public Vector4 text_color, line_numbers_color, cursor_color;
    }

    public class UIE_TextEditor : UIElement
    {
        public TextBuffer text_buffer;
        public int font_scale;
        public float cursor_blink_speed = 0.5f;
        public UIE_TextEditor_Style style;
        public int number_of_line_to_render = 0;

        public int top_line = 0;
        public int bottom_line = 0;

        public int most_right_char = 0;

        bool cursor_on = true;
        float timer = 0;

        public UIE_TextEditor(TextBuffer _text_buffer)
        {
            style = new UIE_TextEditor_Style()
            {
                text_color = new Vector4(0.55f, 0.45f, 0.33f, 1) * 1.2f,
                line_numbers_color = new Vector4(0.33f, 0.6f, 0.77f, 1),
                cursor_color = new Vector4(1f, 0.75f, 1f, 1)
            };

            text_buffer = _text_buffer;
            font_scale = 2;
        }

        public override void Render(Vector2 canvas_size)
        {
           
            if (text_buffer.lines.Count == 0)
            {
                return;
            }
            else
            {
                FontAscii font = UI.state.style.text_font;
                int font_size = font.glyph_size_in_pixels * font_scale;
                float line_height = Fonts.GetLineHeight(font, font_size);
                Vector2I cursor = text_buffer.cursor;
                float pl = font_size / font.glyph_size_in_pixels; //default padding left
                int first_line = top_line;
                var pos = rect.CalcPosition(canvas_size);

                Transform tran = Transform.Zero;
                tran.position = pos;
                tran.position.X += pl + (rect.width / -2);
                tran.position.Y = pos.Y + (rect.height / 2);

                // draw line numbers

                for (int i = 1; i <= number_of_line_to_render; i++)
                {
                    Renderer.DrawTextAscii(tran, font, $"{first_line + i}", style.line_numbers_color, font_size);
                    tran.position.Y -= line_height;
                }

                float line_number_padding = Fonts.GetTextWidth(font, text_buffer.lines.Count.ToString() + " ", font_size);

                //draw line indicator

                float current_line_y = (Fonts.GetLineHeight(font, font_size) * (cursor.y - top_line));

                Transform line_indicator_tran = Transform.Zero;
                line_indicator_tran.position.X += (line_number_padding / 2) + pos.Y;
                line_indicator_tran.position.Y = (rect.height / 2) + pos.Y - (line_height / 2) - current_line_y;
                Vector4 indicator_color = style.cursor_color;
                indicator_color.W = 0.125f;
                Renderer.DrawQuad(line_indicator_tran, new Vector2(rect.width - line_number_padding, line_height), indicator_color);

                // draw file text

                most_right_char = Math.Min(Math.Max(most_right_char, cursor.x), text_buffer.lines[cursor.y].Length);
                //float cursor_width = Fonts.GetTextWidth(font, "|", font_size);
                float text_width = Fonts.GetTextWidth(font, text_buffer.lines[cursor.y], font_size, most_right_char);

                float text_left_padding = Math.Max(0, text_width - (rect.width - line_number_padding));

                tran.position.Y = pos.Y + (rect.height / 2);
                tran.position.X += line_number_padding - text_left_padding;

                //GFX.EnableStencilTest();
                //GFX.ClearStencil();

                //Transform tran_left_side = Transform.Zero;

                //tran_left_side.position.X = pos.X - (rect.width / 2) + (line_number_padding / 2);
                //tran_left_side.position.Y = pos.Y;
                
                //GFX.StencilWrite();

                //Renderer.DrawQuad(tran_left_side, new Vector2(line_number_padding, rect.height), Vector4.One);

                //GFX.StencilCull();
                for (int i = 0; i < number_of_line_to_render; i++)
                {
                    Renderer.DrawTextAscii(tran, font, text_buffer.lines[first_line + i], style.text_color, font_size);
                    tran.position.Y -= line_height;
                }
                //GFX.DisableStencilTest();

                // draw cursor
                if (cursor_on)
                {
                    //float char_paading = (Fonts.GetTextWidth(font, "_", 16) * 2);
                    float left_padding_cursor = Fonts.GetTextWidth(font, text_buffer.lines[cursor.y], font_size, cursor.x) ;
                    //float char_buttom = Fonts.GlyphHeight(font, '|', font_size); we dont use it rn.
                    bool on_last_char = text_buffer.lines[cursor.y].Length == cursor.x && text_buffer.lines[cursor.y].Length != 0;
                    float char_left = Fonts.GetTextWidth(font, "|", font_size) * (on_last_char ? -1 : 1);
                    tran.position.X += left_padding_cursor - char_left;
                    tran.position.Y = pos.Y + (rect.height / 2) - current_line_y;

                    Renderer.DrawTextAscii(tran, font, "|", style.cursor_color, font_size);
                }

            }
        }

        public void ResetCursorOn()
        {
            timer = 0;
            cursor_on = true;
        }

        public override void Update(Vector2 canvas_size)
        {
            timer += Engine.delta_time;

            if (timer > cursor_blink_speed)
            {
                timer -= cursor_blink_speed;
                cursor_on = !cursor_on;
            }

            rect.width = canvas_size.X;
            rect.height = canvas_size.Y;


            number_of_line_to_render = 0;

            FontAscii font = UI.state.style.text_font;
            int font_size = font.glyph_size_in_pixels * font_scale;
            float drawn_height = 0;
            float line_height = Fonts.GetLineHeight(font, font_size);
            
            while (drawn_height < rect.height &&
               text_buffer.lines.Count > number_of_line_to_render)
            {
                drawn_height += line_height;
                number_of_line_to_render += 1;
            }

            Vector2I cursor = text_buffer.cursor;

            if (cursor.y <= top_line)
            {
                top_line = cursor.y;
                bottom_line = top_line + number_of_line_to_render;
            }
            else if (cursor.y >= bottom_line)
            {
                bottom_line = cursor.y + 1;

                top_line = bottom_line - number_of_line_to_render;
            }

            if (top_line + number_of_line_to_render > text_buffer.lines.Count)
            {
                top_line = cursor.y - number_of_line_to_render;
                bottom_line = text_buffer.lines.Count - 1;
            }
        }
    }

}
