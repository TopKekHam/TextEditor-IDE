using System.Numerics;

namespace R
{
    public class UIE_TextEditor_LineNumbers : UIElement
    {

        public Vector4 text_color = Vector4.One;

        public UIE_TextEditor text_editor_refrence;

        public UIE_TextEditor_LineNumbers(UIE_TextEditor editor)
        {
            text_editor_refrence = editor;
        }

        public override void Render(Vector2 canvas_size)
        {
            var pos = rect.transform.position;
            var size = rect.size;

            var font = UI.state.style.text_font;
            int font_size = UI.state.style.text_size;
            float line_height = Ascii_Font_Utils.GetLineHeight(font, font_size);

            int number_of_line_to_render = text_editor_refrence.number_of_line_to_render;
            int first_line = text_editor_refrence.top_line;

            float pl = font_size / font.glyph_size_in_pixels; //default padding left

            Transform tran = Transform.Zero;
            tran.position = pos;
            tran.position.X += pl + (size.X / -2);
            tran.position.Y += (size.Y / 2);

            // draw line numbers

            for (int i = 1; i <= number_of_line_to_render; i++)
            {
                Renderer.DrawTextAscii(tran, font, $"{first_line + i}", text_color, font_size);
                tran.position.Y -= line_height;
            }
        }

        public override void Update(Vector2 canvas_size)
        {
            
        }
    }
}
