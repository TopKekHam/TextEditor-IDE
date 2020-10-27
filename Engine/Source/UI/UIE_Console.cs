using System;
using System.Numerics;

namespace R
{
    public class UIE_Console : UIElement
    {

        public Vector4 color = new Vector4(0.2f, 0.05f, 0.05f, 1);
        public TextBuffer text_buffer;

        public UIE_Console(TextBuffer buffer)
        {
            text_buffer = buffer;
        }

        public override void Render(Vector2 canvas_size)
        {
            var style = UI.state.style;

            Renderer.DrawQuad(rect.transform, rect.size, Renderer.CreateColorMaterail(color));

            float line_height = Ascii_Font_Utils.GetLineHeight(style.text_font, style.text_size);
            float glyph_size = Ascii_Font_Utils.GetGlyphSize(style.text_font, style.text_size);

            float padding_top = (rect.size.Y - line_height) / 2;

            Transform tran = Transform.Zero;
            tran.position = rect.transform.position;
            tran.position.X += (-rect.size.X / 2) + MathF.Round(glyph_size / 2);
            tran.position.Y += (rect.size.Y / 2) - MathF.Round(padding_top);

            Renderer.DrawTextAscii(tran, style.text_font, text_buffer.ToString(), Vector4.One, style.text_size);

            // draw cursor

            //float char_paading = (Fonts.GetTextWidth(font, "_", 16) * 2);
            var cursor = text_buffer.cursor;
            float left_padding_cursor = Ascii_Font_Utils.GetTextWidth(style.text_font, text_buffer.lines[cursor.y], style.text_size, cursor.x);
            //float char_buttom = Fonts.GlyphHeight(font, '|', font_size); we dont use it rn.
            bool on_last_char = text_buffer.lines[cursor.y].Length == cursor.x && text_buffer.lines[cursor.y].Length != 0;
            float char_left = Ascii_Font_Utils.GetTextWidth(style.text_font, "|", style.text_size) * (on_last_char ? -1 : 1);
            tran.position.X = (-rect.size.X / 2) + MathF.Round(glyph_size / 2) + left_padding_cursor - char_left;

            Renderer.DrawTextAscii(tran, style.text_font, "|", style.text_color, style.text_size);

        }

        public override void Update(Vector2 canvas_size)
        {

        }
    }
}
