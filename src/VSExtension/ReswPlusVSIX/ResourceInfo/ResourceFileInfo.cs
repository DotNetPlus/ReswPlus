using ReswPlus.Core.ResourceInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            _projectItem = projectItem;
            var project = projectItem.ContainingProject;
            IsLibrary = project.IsLibrary();
            Name = project.Name;
            Language = project.GetLanguage();
        }

        public string GetPrecompiledHeader()
        {
            if (Language == Core.ResourceInfo.Language.CPPWINRT || Language == Core.ResourceInfo.Language.CPPCX)
            {
                return _projectItem.ContainingProject.GetPrecompiledHeader();
            }
            return null;
        }

        public string GetLanguageId()
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
            try
            {
                var dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE;
                if (dte != null)
                {
                    var textEditorSetting = dte.Properties["TextEditor", GetLanguageId()];
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

    public class ResourceFileInfo : IResourceFileInfo
    {

        public string Path { get; }
        public IProject ContainingProject { get; }

        public ResourceFileInfo(ProjectItem projectItem)
        {
            Path = projectItem.Properties.Item("FullPath").Value as string;
            ContainingProject = new ProjectInfo(projectItem);
        }
    }
}
