using EnvDTE;

namespace ReswPlus.Utils
{
    public enum Language { Unknown, CSHARP, VB, CPP }
    public static class ProjectItemExtension
    {
        private const string LANGUAGE_VISUAL_C = "{B5E9BD32-6D3E-4B5D-925E-8A43B79820B4}";
        private const string LANGUAGE_CSHARP = "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}";
        private const string LANGUAGE_VBNET = "{B5E9BD33-6D3E-4B5D-925E-8A43B79820B4}";

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
                    return Language.CPP;
                default:
                    return Language.Unknown;
            }
        }
    }
}
