using ReswPlus.Core.ClassGenerator.Models;
using ReswPlus.Core.ResourceParser;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Core.CodeGenerators
{
    public abstract class DotNetGeneratorBase : ICodeGenerator
    {
        protected abstract void AddNewLine(CodeStringBuilder builder);
        protected abstract void GenerateHeaders(CodeStringBuilder builder, bool supportPluralization);

        protected abstract void OpenNamespace(CodeStringBuilder builder, IEnumerable<string> namespaceName);
        protected abstract void CloseNamespace(CodeStringBuilder builder, IEnumerable<string> namespaceName);

        protected abstract void OpenStronglyTypedClass(CodeStringBuilder builder, string resourceFileName, string className);
        protected abstract void CloseStronglyTypedClass(CodeStringBuilder builder);
        protected abstract void OpenRegion(CodeStringBuilder builder, string name);
        protected abstract void CloseRegion(CodeStringBuilder builder, string name);

        protected abstract void CreateFormatMethod(CodeStringBuilder builder, string key, bool isProperty, IEnumerable<IFormatTagParameter> parameters, string summary = null,
            IEnumerable<FunctionFormatTagParameter> extraParameters = null, FunctionFormatTagParameter parameterForPluralization = null, bool supportNoneState = false, FunctionFormatTagParameter parameterForVariant = null);

        protected abstract void CreateMarkupExtension(CodeStringBuilder builder, string resourceFileName, string className, IEnumerable<string> keys);
        protected abstract IEnumerable<GeneratedFile> GetGeneratedFiles(CodeStringBuilder builder, string baseFilename);

        public IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ResourceInfo.ResourceFileInfo resourceFileInfo)
        {
            var builder = new CodeStringBuilder(resourceFileInfo.ParentProject.GetIndentString());
            GenerateHeaders(builder, info.IsAdvanced);
            AddNewLine(builder);
            OpenNamespace(builder, info.Namespaces);
            OpenStronglyTypedClass(builder, info.ResoureFile, info.ClassName);

            foreach (var item in info.Localizations)
            {
                AddNewLine(builder);

                OpenRegion(builder, item.Key);

                CreateFormatMethod(
                    builder,
                    item.Key,
                    item.IsProperty,
                    item.Parameters,
                    item.Summary,
                    item.ExtraParameters,
                    (item as PluralLocalization)?.ParameterToUseForPluralization,
                    (item as PluralLocalization)?.SupportNoneState ?? false,
                    (item as IVariantLocalization)?.ParameterToUseForVariant);

                CloseRegion(builder, item.Key);
            }

            CloseStronglyTypedClass(builder);
            AddNewLine(builder);
            CreateMarkupExtension(builder, info.ResoureFile, info.ClassName + "Extension", info.Localizations.Where(i => i is Localization).Select(s => s.Key));
            CloseNamespace(builder, info.Namespaces);
            return GetGeneratedFiles(builder, baseFilename);
        }
    }
}
