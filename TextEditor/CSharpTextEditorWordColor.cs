using System.Numerics;

namespace R.TextEditor
{
    public class CSharpTextEditorWordColor : ITextEditorWordColor
    {
        public Vector4[] GenerateLineColorData(string line, UIE_TextEditor_Style style)
        {
            var tokens = CSharpReader.Tokenize(line);

            Vector4[] colors = new Vector4[line.Length];

            for (int i = 0; i < tokens.Length; i++)
            {
                for (int j = tokens[i].start; j <= tokens[i].end; j++)
                {
                    switch (tokens[i].type)
                    {
                        case TokenType.TEXT:
                            colors[j] = new Vector4(0, 0.5f, 1, 1);
                            break;
                        case TokenType.NUMBER:
                            colors[j] = new Vector4(1, 1, 1, 1);
                            break;
                        case TokenType.PROTECTED_WORD:
                            colors[j] = new Vector4(0.3f, 0.3f, 0.85f, 1);
                            break;
                        case TokenType.TYPE:
                            colors[j] = new Vector4(0.85f, 0.3f, 0.3f, 1);
                            break;
                        case TokenType.NAME:
                            colors[j] = style.text_color;
                            break;
                        case TokenType.Operator:
                            colors[j] = new Vector4(0.9f, 0.5f, 0.4f, 1);
                            break;
                        case TokenType.LINE_COMMENT:
                            colors[j] = new Vector4(0.3f, 0.8f, 0.3f, 1);
                            break;
                        case TokenType.MULTI_LINE_COMMANT:
                            colors[j] = new Vector4(0.3f, 0.8f, 0.3f, 1);
                            break;
                    }
                }
            }

            return colors;
        }
    }
}
