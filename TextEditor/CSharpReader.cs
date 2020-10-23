using System;
using System.Collections.Generic;

namespace R.TextEditor
{

    public enum TokenType
    {
        NAME = 0, NUMBER = 1, PROTECTED_WORD = 2, TYPE = 3, Operator = 4, TEXT = 5, LINE_COMMENT = 6, MULTI_LINE_COMMANT = 7
    }

    public struct Token
    {
        public int start, end;
        public TokenType type;
    }

    public static class CSharpReader
    {

        static CSharpReader()
        {
            List<string> all_types = new List<string>();
            //var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //for (int i = 0; i < assemblies.Length; i++)
            //{
            //    var types = assemblies[i].GetTypes();
            //    Console.WriteLine(assemblies[i].FullName);

            //    for (int t = 0; t < types.Length; t++)
            //    {
            //        all_types.Add(types[t].Name);
            //    }
            //}

            user_types = all_types.ToArray();
            Console.WriteLine($"found: {user_types.Length} types.");
        }

        static string[] protected_words = new string[]
        {
            "class", "struct", "enum", "namespace",
            "public", "private" , "protected", "internal", "static", "unsafe", "using",
            "var", "new", "stackalloc",
            "if", "else", "switch", "case", "break", "return", "continue",  "for", "while", "do",
        };

        static char string_delimiter = '"';
        static char escape_char = '\\';
        static char char_delimiter = '\'';
        static string single_line_comment = "//";

        static char[] operators = new char[]
        {
            '-', '+', '/', '*', '%', '=', ',', '{', '}', '(', ')', '[', ']', '<', '>', '^', '|', '~', '&', '!', '?', ':', ';', '$'
        };

        static string[] long_operators = new string[]
        {
            "==", "<=", ">=", "||", "&&"
        };

        static string[] types = new string[]
        {
            "void", "bool", "object", "char", "string", "byte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double"
        };

        static string[] user_types;

        static ArrayList<Token> tokens = new ArrayList<Token>(128);

        public unsafe static Token[] Tokenize(string str)
        {
            tokens.count = 0;

            int start = 0;
            int end = 0;

            for (int i = 0; i < str.Length;)
            {
                i = RemoveWhitescape(str, i);

                if (i == str.Length) break;

                // single line comment
                if (Match(str, i, i + (Math.Min(single_line_comment.Length, str.Length - i)) - 1, single_line_comment))
                {
                    tokens.AddItem(new Token()
                    {
                        type = TokenType.LINE_COMMENT,
                        start = i,
                        end = str.Length - 1
                    });

                    break;
                }

                //string or char.
                int string_or_char = IsStringOrChar(str, i);
                if (string_or_char >= 0)
                {
                    tokens.AddItem(new Token()
                    {
                        type = TokenType.TEXT,
                        start = i,
                        end = string_or_char
                    });

                    i = string_or_char + 1;
                }
                // is operator
                else if (IsShortOperator(str[i]))
                {
                    int is_long = IsLongOperator(str, i);

                    if (is_long > 0)
                    {
                        tokens.AddItem(new Token()
                        {
                            type = TokenType.Operator,
                            start = i,
                            end = i + is_long
                        });
                        i += is_long;
                    }
                    else
                    {
                        tokens.AddItem(new Token()
                        {
                            type = TokenType.Operator,
                            start = i,
                            end = i
                        });
                    }

                    i += 1;
                }
                else
                {
                    start = i;

                    while (str.Length > i && !char.IsWhiteSpace(str[i]) && !IsShortOperator(str[i]))
                    {
                        i += 1;
                    }

                    end = i - 1;

                    TokenType token_type = TokenType.NAME;

                    //protected words
                    if (Match(str, start, end, protected_words))
                    {
                        token_type = TokenType.PROTECTED_WORD;

                    }
                    //types
                    else if (Match(str, start, end, types) || Match(str, start, end, user_types))
                    {
                        token_type = TokenType.TYPE;
                    }
                    else
                    {
                        // numbers
                        if (IsNumber(str, start, end))
                        {
                            token_type = TokenType.NUMBER;
                        }
                        // names
                        else
                        {
                            token_type = TokenType.NAME;
                        }
                    }

                    tokens.AddItem(new Token()
                    {
                        type = token_type,
                        start = start,
                        end = end
                    });
                }
            }

            return tokens.ToArray();
        }

        public static int IsStringOrChar(string str, int idx)
        {
            int start = idx;
            bool literal_string = false;
            char delimiter;

            if (str[idx] == '@')
            {
                idx += 1;
                literal_string = true;
            }
            else if (str[idx] == '$')
            {
                idx += 1;
            }

            if (str.Length > idx)
            {

                if (str[idx] == char_delimiter)
                {
                    delimiter = char_delimiter;
                }
                else if (str[idx] == string_delimiter)
                {
                    delimiter = string_delimiter;
                }
                else
                {
                    return -1;
                }

                idx += 1;

                bool escaped = false;

                while (str.Length > idx)
                {
                    if (!escaped || literal_string)
                    {
                        if (str[idx] == delimiter)
                        {
                            if (escaped && !literal_string)
                            {
                                escaped = false;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (str[idx] == escape_char) escaped = true;
                    }

                    idx += 1;
                }

                if (idx == str.Length)
                {
                    idx -= 1;
                }

                return idx;
            }

            return -1;
        }

        public static int DoText(string str, int idx, char delimiter, char esc_char, bool got_escape_char = true)
        {
            int ci = idx;
            bool escaped = false;

            while (str.Length > ci)
            {
                if (!escaped || !got_escape_char)
                {
                    if (str[ci] == delimiter)
                    {
                        if (escaped && got_escape_char)
                        {
                            escaped = false;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (str[ci] == esc_char) escaped = true;
                }

                ci += 1;
            }

            return ci;
        }

        public static int RemoveWhitescape(string src, int c_idx)
        {
            while (src.Length > c_idx && char.IsWhiteSpace(src[c_idx]))
            {
                c_idx += 1;
            }
            return c_idx;
        }

        public static bool IsShortOperator(char c)
        {
            for (int i = 0; i < operators.Length; i++)
            {
                if (c == operators[i]) return true;
            }
            return false;
        }

        public static int IsLongOperator(string str, int idx)
        {

            for (int i = 0; i < long_operators.Length; i++)
            {
                bool found = true;
                int s_idx = idx;

                for (int ci = 0; ci < long_operators[i].Length; ci++)
                {
                    if (s_idx == str.Length || str[s_idx] != long_operators[i][ci])
                    {
                        found = false;
                        break;
                    }

                    s_idx += 1;
                }

                if (found)
                {
                    return long_operators[i].Length;
                }
            }

            return -1;
        }

        public static bool IsNumber(string str, int start, int end)
        {
            bool got_dot_inside = false;

            int word_length = end - start + 1;

            for (int i = 0; i < word_length; i++)
            {
                int idx = start + i;

                if (str[idx] == '.')
                {
                    if (got_dot_inside)
                    {
                        return false;
                    }
                    else
                    {
                        got_dot_inside = true;
                    }
                }
                else
                {
                    if (str[idx] >= '0' && str[idx] <= '9')
                    {
                        continue;
                    }

                    if (char.IsWhiteSpace(str[idx]))
                    {
                        continue;
                    }

                    return false;
                }
            }

            return true;
        }

        public static bool Match(string str, int start, int end, string[] to_match)
        {
            int word_length = end - start + 1;

            for (int i = 0; i < to_match.Length; i++)
            {
                if (to_match[i].Length != word_length) continue;

                bool found = true;

                for (int ch = 0; ch < word_length; ch++)
                {
                    if (str[start + ch] != to_match[i][ch])
                    {
                        found = false;
                        break;
                    }
                }

                if (found) return true;
            }

            return false;
        }

        public static bool Match(string str, int start, int end, string to_match)
        {
            int word_length = end - start + 1;

            if (to_match.Length != word_length) return false;

            for (int ch = 0; ch < word_length; ch++)
            {
                if (str[start + ch] != to_match[ch])
                {
                    return false;
                }
            }

            return true;
        }

    }
}
