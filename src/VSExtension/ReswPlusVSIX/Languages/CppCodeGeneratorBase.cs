using EnvDTE;
using ReswPlus.ClassGenerator.Models;
using ReswPlus.Resw;
using ReswPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal abstract class CppCodeGeneratorBase : ICodeGenerator
    {
        protected const string LocalNamespaceName = "local";

        protected abstract bool SupportMultiNamespaceDeclaration(Project project);
        protected abstract string GetParameterTypeString(ParameterType type, bool isHeader);
        protected abstract void HeaderFileGenerateHeaders(CodeStringBuilder builderHeader, string className, IEnumerable<string> namespacesOverride, bool supportPluralization);
        protected abstract void CppFileGenerateHeaders(CodeStringBuilder builderHeader, string precompiledHeader, string headerFilePath, string localNamespace, string className, IEnumerable<string> namespaces, bool supportPluralization);
        protected abstract void HeaderOpenStronglyTypedClass(CodeStringBuilder builderHeader, string resourceFilename, string className);
        protected abstract void CppGenerateStronglyTypedClassStaticFunc(CodeStringBuilder builderHeader, string computedNamespace, string resourceFilename);
        protected abstract void HeaderCloseStronglyTypedClass(CodeStringBuilder builderHeader);

        protected abstract void HeaderCreateTemplateAccessor(CodeStringBuilder builderHeader, string key, string summary, bool supportPlural, bool supportVariants);
        protected abstract void CppCreateTemplateAccessor(CodeStringBuilder builderHeader, string computedNamespaces, string key, bool supportPlural, bool pluralSupportNoneState, bool supportVariants);
        protected abstract void HeaderCreateAccessor(CodeStringBuilder builderHeader, string key, string summary);
        protected abstract void CppCreateAccessor(CodeStringBuilder builderHeader, string computedNamespace, string key);
        protected abstract void HeaderCreateFormatMethod(CodeStringBuilder builderHeader, string key, IEnumerable<FunctionParameter> parameters, string summary = null, IEnumerable<FunctionParameter> extraParameters = null);
        protected abstract void CppCreateFormatMethod(CodeStringBuilder builderHeader, string computedNamespace, string key, bool isDotNetFormatting, IEnumerable<Parameter> parameters, IEnumerable<FunctionParameter> extraParameters = null, FunctionParameter parameterForPluralization = null, FunctionParameter parameterForVariant = null);
        protected abstract void HeaderCreateMarkupExtension(CodeStringBuilder builderHeader, string resourceFileName, string className, IEnumerable<string> keys, IEnumerable<string> namespaces);
        protected abstract void CppCreateMarkupExtension(CodeStringBuilder builderHeader, string computedNamespace, string resourceFileName, string className, IEnumerable<string> keys);

        public void HeaderOpenNamespace(CodeStringBuilder builderHeader, IEnumerable<string> namespaces, bool supportNestedMamespacesAtOnce)
        {
            if (namespaces != null && namespaces.Any())
            {
                if (supportNestedMamespacesAtOnce)
                {
                    builderHeader.AppendLine($"namespace {namespaces.Aggregate((a, b) => a + "::" + b)}");
                    builderHeader.AppendLine("{");
                    builderHeader.AddLevel();
                }
                else
                {
                    foreach (var subNamespace in namespaces)
                    {
                        builderHeader.AppendLine($"namespace {subNamespace}");
                        builderHeader.AppendLine("{");
                        builderHeader.AddLevel();
                    }
                }
            }
        }

        public void HeaderCloseNamespace(CodeStringBuilder builderHeader, IEnumerable<string> namespaces, bool supportNestedMamespacesAtOnce)
        {

            if (namespaces != null && namespaces.Any())
            {
                if (supportNestedMamespacesAtOnce)
                {
                    builderHeader.RemoveLevel();
                    builderHeader.AppendLine($"}} // namespace {namespaces.Aggregate((a, b) => a + "::" + b)}");
                }
                else
                {
                    foreach (var n in namespaces.Reverse())
                    {
                        builderHeader.RemoveLevel();
                        builderHeader.AppendLine($"}} // namespace {n}");
                    }
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

        public virtual IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem)
        {
            return GeneratedFiles(baseFilename, info, projectItem, null);
        }

        protected virtual void HeaderAddExtra(CodeStringBuilder builderHeader, StronglyTypedClass info)
        {

        }

        public virtual IEnumerable<GeneratedFile> GeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem, IEnumerable<string> namespaceOverride)
        {
            var supportMultiNamespaceDeclaration = SupportMultiNamespaceDeclaration(projectItem.ContainingProject);
            var namespacesToUse = namespaceOverride ?? info.Namespaces;
            var markupClassName = info.ClassName + "Extension";
            var headerFileName = baseFilename + ".h";
            var cppFileName = baseFilename + ".cpp";
            var baseNamespace = namespacesToUse == null || !namespacesToUse.Any() ? "" : namespacesToUse.Aggregate((a, b) => a + "::" + b);

            var builderHeader = new CodeStringBuilder("Cpp");
            var builderCpp = new CodeStringBuilder("Cpp");

            var precompiledHeader = projectItem.GetPrecompiledHeader();
            var localNamespace = baseNamespace == "" ? "" : $"{LocalNamespaceName}::";
            var namespaceResourceClass = $"{localNamespace}{info.ClassName}::";
            var namespaceMarkupExtensionClass = $"{localNamespace}{markupClassName}::";

            HeaderFileGenerateHeaders(builderHeader, info.ClassName, info.Namespaces, info.SupportPluralization);
            builderHeader.AppendEmptyLine();
            HeaderOpenNamespace(builderHeader, namespacesToUse, supportMultiNamespaceDeclaration);
            HeaderOpenStronglyTypedClass(builderHeader, info.ResoureFile, info.ClassName);
            builderHeader.AppendEmptyLine();

            CppFileGenerateHeaders(builderCpp, precompiledHeader, headerFileName, baseNamespace, info.ClassName, info.Namespaces, info.SupportPluralization);
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
                    HeaderCreateTemplateAccessor(builderHeader, item.Key, pluralLocalization.TemplateAccessorSummary, true, pluralLocalization is IVariantLocalization);
                    CppCreateTemplateAccessor(builderCpp, namespaceResourceClass, pluralLocalization.Key, true, pluralLocalization.SupportNoneState, pluralLocalization is IVariantLocalization);
                    if (pluralLocalization.Parameters != null && pluralLocalization.Parameters.Any())
                    {
                        HeaderCreateFormatMethod(builderHeader, pluralLocalization.Key, pluralLocalization.Parameters.OfType<FunctionParameter>(), pluralLocalization.FormatSummary, pluralLocalization.ExtraParameters);
                        builderCpp.AppendEmptyLine();
                        CppCreateFormatMethod(builderCpp, namespaceResourceClass, pluralLocalization.Key, pluralLocalization.IsDotNetFormatting, pluralLocalization.Parameters, pluralLocalization.ExtraParameters, pluralLocalization.ParameterToUseForPluralization, (pluralLocalization as IVariantLocalization)?.ParameterToUseForVariant);
                    }
                }
                else if (item is Localization localization)
                {
                    HeaderCreateAccessor(builderHeader, localization.Key, localization.AccessorSummary);
                    CppCreateAccessor(builderCpp, namespaceResourceClass, localization.Key);
                    if (localization.Parameters != null && localization.Parameters.Any())
                    {
                        HeaderCreateFormatMethod(builderHeader, localization.Key, localization.Parameters.OfType<FunctionParameter>(), localization.FormatSummary);
                        builderCpp.AppendEmptyLine();
                        CppCreateFormatMethod(builderCpp, namespaceResourceClass, localization.Key, localization.IsDotNetFormatting, localization.Parameters);
                    }
                }

                HeaderCloseRegion(builderHeader);
            }
            HeaderCloseStronglyTypedClass(builderHeader);
            builderHeader.AppendEmptyLine();
            HeaderCreateMarkupExtension(builderHeader, info.ResoureFile, markupClassName, info.Localizations.Where(i => i is Localization).Select(s => s.Key), info.Namespaces);
            HeaderCloseNamespace(builderHeader, namespacesToUse, supportMultiNamespaceDeclaration);
            HeaderAddExtra(builderHeader, info);
            builderCpp.AppendEmptyLine();
            CppCreateMarkupExtension(builderCpp, namespaceMarkupExtensionClass, info.ResoureFile, markupClassName, info.Localizations.Where(i => i is Localization).Select(s => s.Key));

            yield return new GeneratedFile() { Filename = headerFileName, Content = builderHeader.GetString() };
            yield return new GeneratedFile() { Filename = cppFileName, Content = builderCpp.GetString() };
        }
    }
}
