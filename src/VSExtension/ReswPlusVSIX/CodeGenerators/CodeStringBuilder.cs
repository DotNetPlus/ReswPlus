using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text;

namespace ReswPlus.CodeGenerators
{
    public class CodeStringBuilder
    {
        private readonly StringBuilder _stringBuilder;
        private readonly string _indentString;
        private uint _level;

        public CodeStringBuilder(string language)
        {
            _level = 0;
            _stringBuilder = new StringBuilder();
            _indentString = GetIndentString(language);
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

        public string GetIndentString(string language)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE;
                if (dte != null)
                {
                    var textEditorSetting = dte.Properties["TextEditor", language];
                    if ((bool)textEditorSetting.Item("InsertTabs").Value)
                    {
                        return "\t";
                    }
                    else
                    {
                        var res = "";
                        var numberCharacters = (int)textEditorSetting.Item("IndentSize").Value;
                        for (var i = 0; i < numberCharacters; ++i)
                        {
                            res += " ";
                        }
                        return res;
                    }
                }
            }
            catch
            {
            }
            return "    ";
        }

    }
}
