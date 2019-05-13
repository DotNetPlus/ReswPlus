using System;
using System.Text;

namespace ReswPlus.Languages
{
    public static class CodeStringBuilderExtension
    {
        public static StringBuilder AddSpace(this StringBuilder builder, uint level)
        {
            for (var i = 0; i < level; ++i)
            {
                builder.Append("    ");
            }
            return builder;
        }
    }

    public class CodeStringBuilder
    {
        private readonly StringBuilder _stringBuilder;
        private uint _level;

        public CodeStringBuilder()
        {
            _level = 0;
            _stringBuilder = new StringBuilder();
        }

        public StringBuilder AppendLine(string value)
        {
            return _stringBuilder.AddSpace(_level).AppendLine(value);
        }

        public void AddLevel()
        {
            ++_level;
        }

        public void RemoveLevel()
        {
            if (_level > 0)
            {
                --_level;
            }
        }

        public void AppendEmptyLine()
        {
            _stringBuilder.AppendLine("");
        }

        public string GetString()
        {
            return _stringBuilder.ToString();
        }
    }
}
