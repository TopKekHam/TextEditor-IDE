using System.Numerics;

namespace R.TextEditor
{
    public class CSharpTextEditorWordColor : ITextEditorWordColor
    {

        public ArrayList<Vector4> colors = new ArrayList<Vector4>(32);

        public Vector4[] GenerateLineColorData(string line, UIE_TextEditor_Style style)
        {
            var tokens = CSharpReader.Tokenize(line);

            colors.count = 0;
            int token_idx = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (tokens.Length > token_idx && tokens[token_idx].end < i)
                {
                    token_idx++;
                }

                if (tokens.Length > token_idx && tokens[token_idx].start <= i)
                {
                    switch (tokens[token_idx].type)
                    {
                        case TokenType.TEXT:
                            colors.AddItem(new Vector4(0, 0.5f, 1, 1));
                            break;
                        case TokenType.NUMBER:
                            colors.AddItem(new Vector4(1, 1, 1, 1));
                            break;
                        case TokenType.PROTECTED_WORD:
                            colors.AddItem(new Vector4(0.3f, 0.3f, 0.85f, 1));
                            break;
                        case TokenType.TYPE:
                            colors.AddItem(new Vector4(0.85f, 0.3f, 0.3f, 1));
                            break;
                        case TokenType.NAME:
                            colors.AddItem(style.text_color);
                            break;
                        case TokenType.Operator:
                            colors.AddItem(new Vector4(0.9f, 0.5f, 0.4f, 1));
                            break;
                        case TokenType.LINE_COMMENT:
                            colors.AddItem(new Vector4(0.3f, 0.8f, 0.3f, 1));
                            break;
                        case TokenType.MULTI_LINE_COMMANT:
                            colors.AddItem(new Vector4(0.3f, 0.8f, 0.3f, 1));
                            break;
                    }
                }
                else
                {
                    colors.AddItem(Vector4.Zero);
                }
            }

            return colors.data;
        }
    }
}
