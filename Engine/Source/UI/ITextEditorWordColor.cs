using System.Numerics;

namespace R
{
    public interface ITextEditorWordColor
    {
        Vector4[] GenerateLineColorData(string line, UIE_TextEditor_Style style);
    }

    public class DefaultTextEditorWordColor : ITextEditorWordColor
    {
        public Vector4[] GenerateLineColorData(string line, UIE_TextEditor_Style style)
        {
            Vector4[] colors = new Vector4[line.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = style.text_color;
            }

            return colors;
        }
    }
}
