using EnvDTE;
using ReswPlus.ClassGenerator.Models;
using ReswPlus.Resw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlus.Languages
{
    internal abstract class DotNetGeneratorBase : ICodeGenerator
    {
        internal abstract void AddNewLine();
        internal abstract void GenerateHeaders(bool supportPluralization);

        internal abstract void OpenNamespace(IEnumerable<string> namespaceName);
        internal abstract void CloseNamespace(IEnumerable<string> namespaceName);

        internal abstract void OpenStronglyTypedClass(string resourceFileName, string className);
        internal abstract void CloseStronglyTypedClass();
        internal abstract void OpenRegion(string name);
        internal abstract void CloseRegion();
        internal abstract void CreatePluralizationAccessor(string pluralKey, string summary, bool supportNoneState);

        internal abstract void CreateAccessor(string key, string summary);

        internal abstract void CreateFormatMethod(string key, IEnumerable<FunctionParameter> parameters, string summary = null,
            FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null);

        internal abstract void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys);
        internal abstract IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename);

        public IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem)
        {
            GenerateHeaders(info.SupportPluralization);
            AddNewLine();
            OpenNamespace(info.Namespaces);
            OpenStronglyTypedClass(info.ResoureFile, info.ClassName);

            bool isFirst = true;
            foreach (var item in info.Localizations)
            {
                if(isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    AddNewLine();
                }
                OpenRegion(item.Key);
                if (item is PluralLocalization pluralLocalization)
                {
                    CreatePluralizationAccessor(item.Key, pluralLocalization.TemplateAccessorSummary, pluralLocalization.SupportNoneState);
                    if (pluralLocalization.Parameters != null && pluralLocalization.Parameters.Any())
                    {
                        AddNewLine();
                        CreateFormatMethod(pluralLocalization.Key, pluralLocalization.Parameters, pluralLocalization.FormatSummary, pluralLocalization.ExtraParameterForPluralization, pluralLocalization.ParameterToUseForPluralization);
                    }
                }
                else if (item is Localization localization)
                {
                    CreateAccessor(localization.Key, localization.AccessorSummary);
                    if (localization.Parameters != null && localization.Parameters.Any())
                    {
                        AddNewLine();
                        CreateFormatMethod(localization.Key, localization.Parameters, localization.FormatSummary);
                    }
                }

                CloseRegion();
            }

            CloseStronglyTypedClass();
            AddNewLine();
            CreateMarkupExtension(info.ResoureFile, info.ClassName + "Extension", info.Localizations.Where(i => i is Localization).Select(s => s.Key));
            CloseNamespace(info.Namespaces);
            return GetGeneratedFiles(baseFilename);
        }

    }
}
