using System.Numerics;

namespace R
{
    public class UIE_Text : UIElement
    {

        public string text;
        public int font_size;

        public UIE_Text(string _text)
        {
            text = _text;
            
            font_size = UI.state.style.text_size;

            rect.width = Ascii_Font_Utils.GetTextWidth(UI.state.style.text_font, text, font_size);
            rect.height = Ascii_Font_Utils.GetTextHeight(UI.state.style.text_font, text, font_size);
        }

        public override void Render(Vector2 canvas_size)
        {
            var pos = rect.CalcPosition(canvas_size);

            var text_width = Ascii_Font_Utils.GetTextWidth(UI.state.style.text_font, text, font_size);
            var text_height = Ascii_Font_Utils.GetTextHeight(UI.state.style.text_font, text, font_size);

            Transform tran = Transform.Zero;
            tran.position = pos;
            tran.position.X -= text_width / 2;
            tran.position.Y -= text_height / 2;

            Renderer.DrawTextAscii(tran, UI.state.style.text_font, text, UI.state.style.text_color, font_size);
        }

        public override void Update(Vector2 canvas_size)
        {
            rect.width = Ascii_Font_Utils.GetTextWidth(UI.state.style.text_font, text, font_size);
            rect.height = Ascii_Font_Utils.GetTextHeight(UI.state.style.text_font, text, font_size);
        }
    }
}
