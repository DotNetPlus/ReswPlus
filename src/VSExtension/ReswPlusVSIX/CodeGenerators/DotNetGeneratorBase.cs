using EnvDTE;
using ReswPlus.ClassGenerator.Models;
using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.CodeGenerators
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
        internal abstract void CloseRegion(string name);

        internal abstract void CreateFormatMethod(string key, bool isProperty, IEnumerable<FormatTagParameter> parameters, string summary = null,
            IEnumerable<FunctionFormatTagParameter> extraParameters = null, FunctionFormatTagParameter parameterForPluralization = null, bool supportNoneState = false, FunctionFormatTagParameter parameterForVariant = null);

        internal abstract void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys);
        internal abstract IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename);

        public IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem)
        {
            GenerateHeaders(info.SupportPluralization);
            AddNewLine();
            OpenNamespace(info.Namespaces);
            OpenStronglyTypedClass(info.ResoureFile, info.ClassName);

            foreach (var item in info.Localizations)
            {
                AddNewLine();

                OpenRegion(item.Key);

                CreateFormatMethod(
                    item.Key,
                    item.IsProperty,
                    item.Parameters,
                    item.Summary,
                    item.ExtraParameters,
                    (item as PluralLocalization)?.ParameterToUseForPluralization,
                    (item as PluralLocalization)?.SupportNoneState ?? false,
                    (item as IVariantLocalization)?.ParameterToUseForVariant);

                CloseRegion(item.Key);
            }

            CloseStronglyTypedClass();
            AddNewLine();
            CreateMarkupExtension(info.ResoureFile, info.ClassName + "Extension", info.Localizations.Where(i => i is Localization).Select(s => s.Key));
            CloseNamespace(info.Namespaces);
            return GetGeneratedFiles(baseFilename);
        }

    }
}
