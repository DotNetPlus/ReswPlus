using ReswPlus.Core.ResourceInfo;
using ReswPlus.Utils;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;

namespace ReswPlus.ResourceInfo
{
    public class ProjectInfo : IProject
    {
        private readonly ProjectItem _projectItem;

        public bool IsLibrary { get; }
        public string Name { get; }
        public Core.ResourceInfo.Language Language { get; }

        public ProjectInfo(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _projectItem = projectItem;
            var project = projectItem.ContainingProject;
            IsLibrary = project.IsLibrary();
            Name = project.Name;
            Language = project.GetLanguage();
        }

        public string GetPrecompiledHeader()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (Language == Core.ResourceInfo.Language.CPPWINRT || Language == Core.ResourceInfo.Language.CPPCX)
            {
                return _projectItem.ContainingProject.GetPrecompiledHeader();
            }
            return null;
        }

        private string GetVSLanguageId()
        {
            switch (Language)
            {
                case Core.ResourceInfo.Language.CSHARP:
                    return "CSharp";
                case Core.ResourceInfo.Language.VB:
                    return "Basic";
                case Core.ResourceInfo.Language.CPPCX:
                case Core.ResourceInfo.Language.CPPWINRT:
                    return "Cpp";
            }
            return "";
        }

        public string GetIndentString()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) is DTE dte)
                {
                    var textEditorSetting = dte.Properties["TextEditor", GetVSLanguageId()];
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
            return "    "; // 4 spaces
        }

    }
}
