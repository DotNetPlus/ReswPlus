using EnvDTE;
using ReswPlus.Core.ClassGenerator.Models;
using ReswPlus.Core.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Core.CodeGenerators
{
    public abstract class DotNetGeneratorBase : ICodeGenerator
    {
        public abstract void AddNewLine();
        public abstract void GenerateHeaders(bool supportPluralization);

        public abstract void OpenNamespace(IEnumerable<string> namespaceName);
        public abstract void CloseNamespace(IEnumerable<string> namespaceName);

        public abstract void OpenStronglyTypedClass(string resourceFileName, string className);
        public abstract void CloseStronglyTypedClass();
        public abstract void OpenRegion(string name);
        public abstract void CloseRegion(string name);

        public abstract void CreateFormatMethod(string key, bool isProperty, IEnumerable<FormatTagParameter> parameters, string summary = null,
            IEnumerable<FunctionFormatTagParameter> extraParameters = null, FunctionFormatTagParameter parameterForPluralization = null, bool supportNoneState = false, FunctionFormatTagParameter parameterForVariant = null);

        public abstract void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys);
        public abstract IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename);

        public IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem)
        {
            GenerateHeaders(info.IsAdvanced);
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
