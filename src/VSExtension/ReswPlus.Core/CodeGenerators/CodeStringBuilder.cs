using System.Text;

namespace ReswPlus.Core.CodeGenerators
{
    public class CodeStringBuilder
    {
        private readonly StringBuilder _stringBuilder;
        private readonly string _indentString;
        private uint _level;

        public CodeStringBuilder(string indentString)
        {
            _level = 0;
            _stringBuilder = new StringBuilder();
            _indentString = indentString;
        }

        public void AppendLine(string value)
        {
            AddSpace(_level);
            _stringBuilder.AppendLine(value);
        }

        public void AddSpace(uint level)
        {
            for (var i = 0; i < level; ++i)
            {
                _stringBuilder.Append(_indentString);
            }
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
