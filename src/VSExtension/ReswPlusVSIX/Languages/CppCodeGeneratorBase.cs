using EnvDTE;
using ReswPlus.ClassGenerator.Models;
using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal abstract class CppCodeGeneratorBase : ICodeGenerator
    {
        protected const string LocalNamespaceName = "local";

        protected abstract string GetParameterTypeString(ParameterType type, bool isHeader);
        protected abstract void HeaderFileGenerateHeaders(CodeStringBuilder builderHeader, bool supportPluralization);
        protected abstract void CppFileGenerateHeaders(CodeStringBuilder builderHeader, string precompiledHeader, string headerFilePath, string localNamespace, bool supportPluralization);
        protected abstract void HeaderOpenStronglyTypedClass(CodeStringBuilder builderHeader, string resourceFilename, string className);
        protected abstract void CppGenerateStronglyTypedClassStaticFunc(CodeStringBuilder builderHeader, string computedNamespace, string resourceFilename);
        protected abstract void HeaderCloseStronglyTypedClass(CodeStringBuilder builderHeader);

        protected abstract void HeaderCreatePluralizationAccessor(CodeStringBuilder builderHeader, string pluralKey, string summary);
        protected abstract void CppCreatePluralizationAccessor(CodeStringBuilder builderHeader, string computedNamespaces, string pluralKey, bool supportNoneState);
        protected abstract void HeaderCreateAccessor(CodeStringBuilder builderHeader, string key, string summary);
        protected abstract void CppCreateAccessor(CodeStringBuilder builderHeader, string computedNamespace, string key);
        protected abstract void HeaderCreateFormatMethod(CodeStringBuilder builderHeader, string key, IEnumerable<FunctionParameter> parameters, string summary = null, FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null);
        protected abstract void CppCreateFormatMethod(CodeStringBuilder builderHeader, string computedNamespace, string key, IEnumerable<FunctionParameter> parameters, FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null);
        protected abstract void HeaderCreateMarkupExtension(CodeStringBuilder builderHeader, string resourceFileName, string className, IEnumerable<string> keys);
        protected abstract void CppCreateMarkupExtension(CodeStringBuilder builderHeader, string computedNamespace, string resourceFileName, string className, IEnumerable<string> keys);

        public void HeaderOpenNamespace(CodeStringBuilder builderHeader, string[] namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                foreach (var subNamespace in namespaces)
                {
                    builderHeader.AppendLine($"namespace {subNamespace}");
                    builderHeader.AppendLine("{");
                    builderHeader.AddLevel();
                }
            }
        }

        public void HeaderCloseNamespace(CodeStringBuilder builderHeader, string[] namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                foreach (var n in namespaces.Reverse())
                {
                    builderHeader.RemoveLevel();
                    builderHeader.AppendLine($"}} // namespace {n}");
                }
            }
        }

        public void HeaderOpenRegion(CodeStringBuilder builderHeader, string name)
        {
            builderHeader.AppendLine($"/* Methods and properties for {name} */");
        }

        public void HeaderCloseRegion(CodeStringBuilder builderHeader)
        {
        }

        public IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem)
        {
            var markupClassName = info.ClassName + "Extension";
            var headerFileName = baseFilename + ".h";
            var cppFileName = baseFilename + ".cpp";
            var baseNamespace = info.Namespace == null || !info.Namespace.Any() ? "" : info.Namespace.Aggregate((a, b) => a + "::" + b);

            var builderHeader = new CodeStringBuilder("Cpp");
            var builderCpp = new CodeStringBuilder("Cpp");

            var precompiledHeader = GetPrecompiledHeader(projectItem);
            var localNamespace = baseNamespace == "" ? "" : $"{LocalNamespaceName}::";
            var namespaceResourceClass = $"{localNamespace}{info.ClassName}::";
            var namespaceMarkupExtensionClass = $"{localNamespace}{markupClassName}::";

            HeaderFileGenerateHeaders(builderHeader, info.SupportPluralization);
            builderHeader.AppendEmptyLine();
            HeaderOpenNamespace(builderHeader, info.Namespace);
            HeaderOpenStronglyTypedClass(builderHeader, info.ResoureFile, info.ClassName);
            builderHeader.AppendEmptyLine();

            CppFileGenerateHeaders(builderCpp, precompiledHeader, headerFileName, baseNamespace, info.SupportPluralization);
            builderCpp.AppendEmptyLine();
            CppGenerateStronglyTypedClassStaticFunc(builderCpp, namespaceResourceClass, info.ResoureFile);
            builderCpp.AppendEmptyLine();

            var firstLocalization = true;
            foreach (var item in info.Localizations)
            {
                if (firstLocalization)
                {
                    firstLocalization = false;
                }
                else
                {
                    builderHeader.AppendEmptyLine();
                    builderCpp.AppendEmptyLine();
                }

                HeaderOpenRegion(builderHeader, item.Key);
                if (item is PluralLocalization pluralLocalization)
                {
                    HeaderCreatePluralizationAccessor(builderHeader, item.Key, pluralLocalization.TemplateAccessorSummary);
                    CppCreatePluralizationAccessor(builderCpp, namespaceResourceClass, pluralLocalization.Key, pluralLocalization.SupportNoneState);
                    if (pluralLocalization.Parameters != null && pluralLocalization.Parameters.Any())
                    {
                        HeaderCreateFormatMethod(builderHeader, pluralLocalization.Key, pluralLocalization.Parameters, pluralLocalization.FormatSummary, pluralLocalization.ExtraParameterForPluralization, pluralLocalization.ParameterToUseForPluralization);
                        builderCpp.AppendEmptyLine();
                        CppCreateFormatMethod(builderCpp, namespaceResourceClass, pluralLocalization.Key, pluralLocalization.Parameters, pluralLocalization.ExtraParameterForPluralization, pluralLocalization.ParameterToUseForPluralization);
                    }
                }
                else if (item is Localization localization)
                {
                    HeaderCreateAccessor(builderHeader, localization.Key, localization.AccessorSummary);
                    CppCreateAccessor(builderCpp, namespaceResourceClass, localization.Key);
                    if (localization.Parameters != null && localization.Parameters.Any())
                    {
                        HeaderCreateFormatMethod(builderHeader, localization.Key, localization.Parameters, localization.FormatSummary);
                        builderCpp.AppendEmptyLine();
                        CppCreateFormatMethod(builderCpp, namespaceResourceClass, localization.Key, localization.Parameters);
                    }
                }

                HeaderCloseRegion(builderHeader);
            }
            HeaderCloseStronglyTypedClass(builderHeader);
            builderHeader.AppendEmptyLine();
            HeaderCreateMarkupExtension(builderHeader, info.ResoureFile, markupClassName, info.Localizations.Where(i => i is Localization).Select(s => s.Key));
            HeaderCloseNamespace(builderHeader, info.Namespace);
            builderCpp.AppendEmptyLine();
            CppCreateMarkupExtension(builderCpp, namespaceMarkupExtensionClass, info.ResoureFile, markupClassName, info.Localizations.Where(i => i is Localization).Select(s => s.Key));

            yield return new GeneratedFile() { Filename = headerFileName, Content = builderHeader.GetString() };
            yield return new GeneratedFile() { Filename = cppFileName, Content = builderCpp.GetString() };
        }

        private string GetPrecompiledHeader(ProjectItem projectItem)
        {
            try
            {
                //VCProject not available via NuGet, we will use a dynamic type to limit dependencies.
                dynamic vcproject = projectItem?.ContainingProject?.Object;
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
    }
}
