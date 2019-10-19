using EnvDTE;
using System;
using Language = ReswPlus.Core.ResourceInfo.Language;

namespace ReswPlus.Utils
{
    public static class ProjectExtension
    {
        private const string LANGUAGE_VISUAL_C = "{B5E9BD32-6D3E-4B5D-925E-8A43B79820B4}";
        private const string LANGUAGE_CSHARP = "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}";
        private const string LANGUAGE_VBNET = "{B5E9BD33-6D3E-4B5D-925E-8A43B79820B4}";

        private static bool IsCppWinRT(this Project project)
        {
            if (project?.Object == null)
            {
                //unknown state
                return false;
            }

            dynamic vcproject = project.Object;
            //Check tools
            try
            {
                if (vcproject.ActiveConfiguration.Tools.Item("CppWinRT") != null)
                {
                    return true;
                }
            }
            catch
            { }

            try
            {
                //check toolFiles
                var toolFiles = vcproject.ToolFiles;
                for (var i = 1; i <= toolFiles.Count; ++i)
                {
                    var value = toolFiles.Item(i).Path as string;
                    if (value != null && value.EndsWith("Microsoft.Windows.CppWinRT.targets", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch { }

            // check Macro
            try
            {

                var iscppWinrtPackage = vcproject.ActiveConfiguration.Evaluate("$(CppWinRTPackage)");
                if (iscppWinrtPackage == "C++/WinRT")
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        public static string GetCppVersion(this Project project)
        {
            try
            {
                //VCProject not available via NuGet, we will use a dynamic type to limit dependencies.
                dynamic vcproject = project?.Object;
                var vcCompilerTool = vcproject.ActiveConfiguration.Tools.Item("VCCLCompilerTool");
                var additionalOptions = vcCompilerTool.AdditionalOptions;
            }
            catch
            {
            }
            return "";
        }

        public static string GetPrecompiledHeader(this Project project)
        {
            try
            {
                //VCProject not available via NuGet, we will use a dynamic type to limit dependencies.
                dynamic vcproject = project?.Object;
                if (vcproject == null)
                {
                    return null;
                }
                var vcCompilerTool = vcproject.ActiveConfiguration.Tools.Item("VCCLCompilerTool");
                if ((int)vcCompilerTool.UsePrecompiledHeader == 0)
                {
                    //pchOption.None, no precompiled header used
                    return null;
                }
                else
                {
                    return (string)vcCompilerTool.PrecompiledHeaderThrough;
                }
            }
            catch { }
            return null;
        }


        public static Language GetLanguage(this Project project)
        {
            if (project == null)
            {
                return Language.Unknown;
            }
            var language = project.CodeModel.Language;
            switch (language)
            {
                case LANGUAGE_CSHARP:
                    return Language.CSHARP;
                case LANGUAGE_VBNET:
                    return Language.VB;
                case LANGUAGE_VISUAL_C:
                    {

                        if (IsCppWinRT(project))
                        {
                            return Language.CPPWINRT;
                        }
                        return Language.CPPCX;
                    }
                default:
                    return Language.Unknown;
            }
        }

        public static bool IsLibrary(this Project project)
        {
            try
            {
                return Convert.ToInt32(project.Properties.Item("OutputTypeEx").Value) == 2;
            }
            catch
            {
                return false;
            }
        }
    }
}
