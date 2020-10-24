﻿using System;
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
        public ITextEditorWordColor word_color_supplier;
        public int top_line = 0;
        public int bottom_line = 0;
        public float x_padding = 0;

        bool cursor_on = true;
        float timer = 0;
        float line_number_padding;

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
            word_color_supplier = new DefaultTextEditorWordColor();
        }

        public override void Render(Vector2 canvas_size)
        {

            if (text_buffer.lines.Count == 0)
            {
                return;
            }
            else
            {
                Ascii_Font font = UI.state.style.text_font;
                int font_size = font.glyph_size_in_pixels * font_scale;
                float line_height = Ascii_Font_Utils.GetLineHeight(font, font_size);
                Vector2I cursor = text_buffer.cursor;
                float pl = font_size / font.glyph_size_in_pixels; //default padding left
                int first_line = top_line;
                var pos = rect.CalcPosition(canvas_size);

                //Transform tran = Transform.Zero;
                //tran.position = pos;
                //tran.position.X += pl + (rect.width / -2);
                //tran.position.Y = pos.Y + (rect.height / 2);

                //// draw line numbers

                //for (int i = 1; i <= number_of_line_to_render; i++)
                //{
                //    Renderer.DrawTextAscii(tran, font, $"{first_line + i}", style.line_numbers_color, font_size);
                //    tran.position.Y -= line_height;
                //}

                // write writing drawable area to stencil buffer 
                {
                    GFX.EnableStencilTest();
                    GFX.ClearStencil();

                    Transform stencil_tran = Transform.Zero;

                    stencil_tran.position.X = pos.X;
                    stencil_tran.position.Y = pos.Y;

                    GFX.StencilWrite();
                    Renderer.DrawQuad(stencil_tran, new Vector2(rect.width, rect.height), Vector4.Zero);
                    GFX.StencilCull(false);
                }


                //draw line indicator
                float current_line_y = (line_height * (cursor.y - top_line));

                Transform line_indicator_tran = Transform.Zero;
                line_indicator_tran.position.X = pos.X;
                line_indicator_tran.position.Y = (rect.height / 2) + pos.Y - (line_height / 2) - current_line_y;
                Vector4 indicator_color = style.cursor_color;
                indicator_color.W = 0.125f;
                var line_indicator_size = new Vector2(rect.width, line_height);
                Renderer.DrawQuad(line_indicator_tran, line_indicator_size, indicator_color);


                // draw buffer selection

                if (text_buffer.selection_active)
                {

                    Vector2I ancor = text_buffer.selection_ancor;
                    int start_line = Math.Min(ancor.y, cursor.y);
                    int end_line = Math.Max(ancor.y, cursor.y);

                    for (int i = start_line; i <= end_line; i++)
                    {
                        int start = 0;
                        int end = text_buffer.lines[i].Length;

                        if (i == ancor.y || i == cursor.y)
                        {
                            if (ancor.y == cursor.y)
                            {
                                start = Math.Min(ancor.x, cursor.x);
                                end = Math.Max(ancor.x, cursor.x);
                            }
                            else if (ancor.y == i)
                            {
                                if (ancor.y > cursor.y)
                                {
                                    start = 0;
                                    end = ancor.x;
                                }
                                else
                                {
                                    start = ancor.x;
                                    end = text_buffer.lines[ancor.y].Length;
                                }
                            }
                            else
                            {
                                if (ancor.y < cursor.y)
                                {
                                    start = 0;
                                    end = cursor.x;
                                }
                                else
                                {
                                    start = cursor.x;
                                    end = text_buffer.lines[cursor.y].Length;
                                }
                            }
                        }

                        DrawSelection(font, pos, i, start, end, new Vector4(0.6f, 0.6f, 1f, 0.25f));
                    }
                }

                // draw file text
                Transform tran = Transform.Zero;
                tran.position = pos;
                tran.position.Y = pos.Y + (rect.height / 2);
                tran.position.X = pos.X + (rect.width / -2) - x_padding + pl;

                // draw lines of text.
                for (int i = 0; i < number_of_line_to_render; i++)
                {
                    var text = text_buffer.lines[first_line + i];
                    var text_mesh = Ascii_Font_Utils.GenerateTextMesh(font, text, font_size, word_color_supplier.GenerateLineColorData(text, style));

                    Material mat = new Material();
                    mat.shader = Ascii_Font_Utils.shader;
                    mat.uniform_params = new UniformParam[] {
                        new UniformParam { name = "tex_0", texture = font.texture, type = UniformType.Texture },
                        new UniformParam { name = "tint", vec4 = Vector4.One, type = UniformType.Vector4}
                    };

                    Renderer.DrawMesh(tran, mat, text_mesh);
                    GFX.DeleteMesh(text_mesh);

                    tran.position.Y -= line_height;
                }

                // draw cursor
                if (cursor_on)
                {
                    //float char_paading = (Fonts.GetTextWidth(font, "_", 16) * 2);
                    float left_padding_cursor = Ascii_Font_Utils.GetTextWidth(font, text_buffer.lines[cursor.y], font_size, cursor.x);
                    //float char_buttom = Fonts.GlyphHeight(font, '|', font_size); we dont use it rn.
                    bool on_last_char = text_buffer.lines[cursor.y].Length == cursor.x && text_buffer.lines[cursor.y].Length != 0;
                    float char_left = Ascii_Font_Utils.GetTextWidth(font, "|", font_size) * (on_last_char ? -1 : 1);
                    tran.position.X += left_padding_cursor - char_left;
                    tran.position.Y = pos.Y + (rect.height / 2) - current_line_y;

                    Renderer.DrawTextAscii(tran, font, "|", style.cursor_color, font_size);
                }


                GFX.DisableStencilTest();


            }
        }

        void DrawSelection(Ascii_Font font, Vector3 pos, int line, int start, int end, Vector4 color)
        {
            int font_size = font.glyph_size_in_pixels * font_scale;
            float line_height = Ascii_Font_Utils.GetLineHeight(font, font_size);
            float pl = font_size / font.glyph_size_in_pixels; //default padding left;

            float sl_padding = Ascii_Font_Utils.GetTextWidth(font, text_buffer.lines[line], font_size, start);
            float sl_width = Ascii_Font_Utils.GetTextWidth(font, text_buffer.lines[line], font_size, end - start, start);
            
            if(sl_width == 0)
            {
                sl_width = Ascii_Font_Utils.GetGlyphSize(font, font_size);
            }

            float sl_line_y = (line_height * (line - top_line));

            Transform tran_selection = Transform.Zero;

            tran_selection.position.X = pl + pos.X + (rect.width / -2.0f) + (sl_width / 2) + sl_padding - x_padding;
            tran_selection.position.Y = pos.Y + (rect.height / 2) - (line_height / 2) - sl_line_y;
            Renderer.DrawQuad(tran_selection, new Vector2(sl_width, line_height), color);
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

            Ascii_Font font = UI.state.style.text_font;
            int font_size = font.glyph_size_in_pixels * font_scale;
            float drawn_height = 0;
            float line_height = Ascii_Font_Utils.GetLineHeight(font, font_size);
            line_number_padding = Ascii_Font_Utils.GetTextWidth(font, text_buffer.lines.Count.ToString() + " ", font_size);

            while (drawn_height < rect.height &&
               text_buffer.lines.Count > number_of_line_to_render)
            {
                drawn_height += line_height;

                if (drawn_height < rect.height)
                {
                    number_of_line_to_render += 1;
                }
            }

            Vector2I cursor = text_buffer.cursor;

            // determine what lines to draw 

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

            // determine from with char start to draw

            float left_side_text_width = Ascii_Font_Utils.GetTextWidth(font, text_buffer.lines[text_buffer.cursor.y], font_size, text_buffer.cursor.x);
            float padding_threshold = 50;

            if (left_side_text_width - x_padding < padding_threshold)
            {
                x_padding = left_side_text_width - padding_threshold;
                x_padding = Math.Max(x_padding, 0);
            }
            else if (left_side_text_width - x_padding > rect.width - line_number_padding - padding_threshold)
            {
                x_padding += (left_side_text_width - x_padding) - (rect.width - line_number_padding - padding_threshold);
            }

        }
    }

}
