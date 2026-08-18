using System;
using System.Collections.Generic;

namespace ReswPlusSamples.SyntaxHighlighting
{
    internal enum SyntaxTokenKind
    {
        Comment,
        Keyword,
        Number,
        String,
        XmlAttribute,
        XmlName,
    }

    internal readonly struct SyntaxToken
    {
        public SyntaxToken(int start, int length, SyntaxTokenKind kind)
        {
            Start = start;
            Length = length;
            Kind = kind;
        }

        public int Start { get; }

        public int Length { get; }

        public SyntaxTokenKind Kind { get; }
    }

    internal static class SyntaxTokenizer
    {
        private static readonly HashSet<string> CSharpKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "abstract", "as", "async", "await", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do",
            "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed",
            "float", "for", "foreach", "get", "global", "goto", "if", "implicit", "in", "init", "int",
            "interface", "internal", "is", "lock", "long", "namespace", "new", "not", "null", "object",
            "operator", "or", "out", "override", "params", "partial", "private", "protected", "public",
            "readonly", "record", "ref", "required", "return", "sbyte", "sealed", "set", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "value", "var",
            "virtual", "void", "volatile", "when", "where", "while", "with", "yield",
        };

        public static void Tokenize(string sourceCode, Action<SyntaxToken> addToken)
        {
            if (sourceCode.TrimStart().StartsWith("<", StringComparison.Ordinal))
            {
                TokenizeXml(sourceCode, addToken);
            }
            else
            {
                TokenizeCSharp(sourceCode, addToken);
            }
        }

        private static void TokenizeCSharp(string sourceCode, Action<SyntaxToken> addToken)
        {
            var index = 0;

            while (index < sourceCode.Length)
            {
                if (StartsWith(sourceCode, index, "//"))
                {
                    var end = sourceCode.IndexOf('\n', index);
                    end = end < 0 ? sourceCode.Length : end;
                    AddToken(addToken, index, end, SyntaxTokenKind.Comment);
                    index = end;
                }
                else if (StartsWith(sourceCode, index, "/*"))
                {
                    var end = sourceCode.IndexOf("*/", index + 2, StringComparison.Ordinal);
                    end = end < 0 ? sourceCode.Length : end + 2;
                    AddToken(addToken, index, end, SyntaxTokenKind.Comment);
                    index = end;
                }
                else if (TryReadString(sourceCode, index, out var stringEnd))
                {
                    AddToken(addToken, index, stringEnd, SyntaxTokenKind.String);
                    index = stringEnd;
                }
                else if (char.IsDigit(sourceCode[index]))
                {
                    var end = index + 1;
                    while (end < sourceCode.Length && IsNumberCharacter(sourceCode[end]))
                    {
                        end++;
                    }

                    AddToken(addToken, index, end, SyntaxTokenKind.Number);
                    index = end;
                }
                else if (IsIdentifierStart(sourceCode[index]))
                {
                    var end = index + 1;
                    while (end < sourceCode.Length && IsIdentifierPart(sourceCode[end]))
                    {
                        end++;
                    }

                    if (CSharpKeywords.Contains(sourceCode.Substring(index, end - index)))
                    {
                        AddToken(addToken, index, end, SyntaxTokenKind.Keyword);
                    }

                    index = end;
                }
                else if (sourceCode[index] == '#')
                {
                    var end = index + 1;
                    while (end < sourceCode.Length && IsIdentifierPart(sourceCode[end]))
                    {
                        end++;
                    }

                    AddToken(addToken, index, end, SyntaxTokenKind.Keyword);
                    index = end;
                }
                else
                {
                    index++;
                }
            }
        }

        private static void TokenizeXml(string sourceCode, Action<SyntaxToken> addToken)
        {
            var index = 0;
            var insideTag = false;
            var expectingElementName = false;

            while (index < sourceCode.Length)
            {
                if (StartsWith(sourceCode, index, "<!--"))
                {
                    var end = sourceCode.IndexOf("-->", index + 4, StringComparison.Ordinal);
                    end = end < 0 ? sourceCode.Length : end + 3;
                    AddToken(addToken, index, end, SyntaxTokenKind.Comment);
                    index = end;
                }
                else if (!insideTag && sourceCode[index] == '<')
                {
                    insideTag = true;
                    expectingElementName = true;
                    index++;
                    if (index < sourceCode.Length && (sourceCode[index] == '/' || sourceCode[index] == '?' || sourceCode[index] == '!'))
                    {
                        index++;
                    }
                }
                else if (insideTag && sourceCode[index] == '>')
                {
                    insideTag = false;
                    expectingElementName = false;
                    index++;
                }
                else if (insideTag && (sourceCode[index] == '"' || sourceCode[index] == '\''))
                {
                    var quote = sourceCode[index];
                    var end = index + 1;
                    while (end < sourceCode.Length && sourceCode[end] != quote)
                    {
                        end++;
                    }

                    end = Math.Min(end + 1, sourceCode.Length);
                    AddToken(addToken, index, end, SyntaxTokenKind.String);
                    index = end;
                }
                else if (insideTag && IsXmlNameStart(sourceCode[index]))
                {
                    var end = index + 1;
                    while (end < sourceCode.Length && IsXmlNamePart(sourceCode[end]))
                    {
                        end++;
                    }

                    var kind = expectingElementName ? SyntaxTokenKind.XmlName : SyntaxTokenKind.XmlAttribute;
                    AddToken(addToken, index, end, kind);
                    expectingElementName = false;
                    index = end;
                }
                else
                {
                    index++;
                }
            }
        }

        private static bool TryReadString(string sourceCode, int start, out int end)
        {
            var quoteIndex = start;
            var verbatim = false;

            if (sourceCode[start] == '@' && start + 1 < sourceCode.Length && sourceCode[start + 1] == '"')
            {
                verbatim = true;
                quoteIndex++;
            }
            else if (sourceCode[start] == '$')
            {
                quoteIndex++;
                if (quoteIndex < sourceCode.Length && sourceCode[quoteIndex] == '@')
                {
                    verbatim = true;
                    quoteIndex++;
                }
            }
            else if (sourceCode[start] == '@' && start + 2 < sourceCode.Length && sourceCode[start + 1] == '$')
            {
                verbatim = true;
                quoteIndex += 2;
            }

            if (quoteIndex >= sourceCode.Length || (sourceCode[quoteIndex] != '"' && sourceCode[quoteIndex] != '\''))
            {
                end = start;
                return false;
            }

            var quote = sourceCode[quoteIndex];
            end = quoteIndex + 1;
            while (end < sourceCode.Length)
            {
                if (sourceCode[end] == quote)
                {
                    if (verbatim && quote == '"' && end + 1 < sourceCode.Length && sourceCode[end + 1] == '"')
                    {
                        end += 2;
                        continue;
                    }

                    end++;
                    return true;
                }

                if (!verbatim && sourceCode[end] == '\\' && end + 1 < sourceCode.Length)
                {
                    end += 2;
                }
                else
                {
                    end++;
                }
            }

            return true;
        }

        private static bool StartsWith(string source, int index, string value)
        {
            return index + value.Length <= source.Length
                && string.CompareOrdinal(source, index, value, 0, value.Length) == 0;
        }

        private static void AddToken(Action<SyntaxToken> addToken, int start, int end, SyntaxTokenKind kind)
        {
            addToken(new SyntaxToken(start, end - start, kind));
        }

        private static bool IsIdentifierStart(char value)
        {
            return value == '_' || char.IsLetter(value);
        }

        private static bool IsIdentifierPart(char value)
        {
            return value == '_' || char.IsLetterOrDigit(value);
        }

        private static bool IsNumberCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value == '.' || value == '_';
        }

        private static bool IsXmlNameStart(char value)
        {
            return char.IsLetter(value) || value == '_' || value == ':';
        }

        private static bool IsXmlNamePart(char value)
        {
            return IsXmlNameStart(value) || char.IsDigit(value) || value == '-' || value == '.';
        }
    }
}
