using EnvDTE;

namespace ReswPlus.Utils
{
    public enum Language { Unknown, CSHARP, VB, CPPCX, CPPWINRT }
    public static class ProjectItemExtension
    {
        private const string LANGUAGE_VISUAL_C = "{B5E9BD32-6D3E-4B5D-925E-8A43B79820B4}";
        private const string LANGUAGE_CSHARP = "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}";
        private const string LANGUAGE_VBNET = "{B5E9BD33-6D3E-4B5D-925E-8A43B79820B4}";


        private static bool IsCppWinRT(Project project)
        {
            try
            {
                dynamic vcproject = project.Object;
                var toolFiles = vcproject.ToolFiles;
                for (var i = 1; i <= toolFiles.Count; ++i)
                {
                    var value = toolFiles.Item(i).Path as string;
                    if (value != null && value.EndsWith("Microsoft.Windows.CppWinRT.targets", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        public static Language GetLanguage(this ProjectItem projectItem)
        {
            var language = projectItem.ContainingProject.CodeModel.Language;
            switch (language)
            {
                case LANGUAGE_CSHARP:
                    return Language.CSHARP;
                case LANGUAGE_VBNET:
                    return Language.VB;
                case LANGUAGE_VISUAL_C:
                    {

                        if (IsCppWinRT(projectItem.ContainingProject))
                        {
                            return Language.CPPWINRT;
                        }
                        return Language.CPPCX;
                    }
                default:
                    return Language.Unknown;
            }
        }
    }
}
