using EnvDTE;
using ReswPlus.ClassGenerator.Models;
using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal class CppCodeGenerator : ICodeGenerator
    {
        public CppCodeGenerator()
        {
        }

        public string GetParameterTypeString(ParameterType type)
        {
            switch (type)
            {
                case ParameterType.Byte:
                    return "char";
                case ParameterType.Int:
                    return "int";
                case ParameterType.Uint:
                    return "unsigned int";
                case ParameterType.Long:
                    return "long";
                case ParameterType.String:
                    return "Platform::String^";
                case ParameterType.Double:
                    return "double";
                case ParameterType.Char:
                    return "wchar_t";
                case ParameterType.Ulong:
                    return "unsigned long";
                case ParameterType.Decimal:
                    return "long double";
                //case ParameterType.Object:
                default:
                    return "Platform::Object^";
            }
        }

        private void HeaderFileGenerateHeaders(CodeStringBuilder builderHeader, bool supportPluralization)
        {
            //Header
            builderHeader.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralization)
            {
                builderHeader.AppendLine("// The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }
            builderHeader.AppendLine("#pragma once");
            builderHeader.AppendLine("#include <stdio.h>");
        }

        private void CppFileGenerateHeaders(CodeStringBuilder builderHeader, string precompiledHeader, string headerFilePath, bool supportPluralization)
        {
            //Header
            builderHeader.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralization)
            {
                builderHeader.AppendLine("// The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }
            if (!string.IsNullOrEmpty(precompiledHeader))
            {
                builderHeader.AppendLine($"#include <{precompiledHeader}>");
            }
            builderHeader.AppendLine($"#include \"{headerFilePath}\"");
        }

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

        private void HeaderOpenStronglyTypedClass(CodeStringBuilder builderHeader, string resourceFilename, string className)
        {
            builderHeader.AppendLine($"public ref class {className} sealed");
            builderHeader.AppendLine("{");
            builderHeader.AppendLine("private:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("static Windows::ApplicationModel::Resources::ResourceLoader^ GetResourceLoader();");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"{className}() {{}}");
            builderHeader.RemoveLevel();
        }

        private void CppGenerateStronglyTypedClassStaticFunc(CodeStringBuilder builderHeader, string computedNamespace, string resourceFilename)
        {
            builderHeader.AppendLine($"Windows::ApplicationModel::Resources::ResourceLoader^ {computedNamespace}GetResourceLoader()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("static Windows::ApplicationModel::Resources::ResourceLoader^ _resourceLoader(nullptr);");
            builderHeader.AppendLine("if (_resourceLoader == nullptr)");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"_resourceLoader = Windows::ApplicationModel::Resources::ResourceLoader::GetForViewIndependentUse(L\"{resourceFilename}\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendLine("return _resourceLoader;");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        private void HeaderCloseStronglyTypedClass(CodeStringBuilder builderHeader)
        {
            builderHeader.AppendLine("};");
        }

        public void HeaderOpenRegion(CodeStringBuilder builderHeader, string name)
        {
            builderHeader.AppendLine($"/* Methods and properties for {name} */");
        }

        public void HeaderCloseRegion(CodeStringBuilder builderHeader)
        {
        }

        private void HeaderCreatePluralizationAccessor(CodeStringBuilder builderHeader, string pluralKey, string summary)
        {

            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("/// <summary>");
            builderHeader.AppendLine($"///   {summary}");
            builderHeader.AppendLine("/// </summary>");
            builderHeader.AppendLine($"static Platform::String^ {pluralKey}(double number);");
            builderHeader.RemoveLevel();
        }

        private void CppCreatePluralizationAccessor(CodeStringBuilder builderHeader, string computedNamespaces, string pluralKey, bool supportNoneState)
        {
            builderHeader.AppendLine($"Platform::String^ {computedNamespaces}{pluralKey}(double number)");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            if (supportNoneState)
            {
                builderHeader.AppendLine("if(number == 0)");
                builderHeader.AppendLine("{");
                builderHeader.AddLevel();
                builderHeader.AppendLine($"return GetResourceLoader()->GetString(L\"{pluralKey}_None\");");
                builderHeader.RemoveLevel();
                builderHeader.AppendLine("}");
            }

            builderHeader.AppendLine($"return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L\"{pluralKey}\", number);");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        private void HeaderCreateAccessor(CodeStringBuilder builderHeader, string key, string summary)
        {
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("/// <summary>");
            builderHeader.AppendLine($"///   {summary}");
            builderHeader.AppendLine("/// </summary>");
            builderHeader.AppendLine($"static property Platform::String^ {key}");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("Platform::String^ get();");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.RemoveLevel();
        }

        private void CppCreateAccessor(CodeStringBuilder builderHeader, string computedNamespace, string key)
        {
            builderHeader.AppendLine($"Platform::String^ {computedNamespace}{key}::get()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"return GetResourceLoader()->GetString(L\"{key}\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        private void HeaderCreateFormatMethod(CodeStringBuilder builderHeader, string key, IEnumerable<FunctionParameter> parameters, string summary = null, FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null)
        {
            IEnumerable<FunctionParameter> functionParameters;
            if (extraParameterForFunction != null)
            {
                var list = new List<FunctionParameter>(parameters);
                list.Insert(0, extraParameterForFunction);
                functionParameters = list;
            }
            else
            {
                functionParameters = parameters;
            }
            var parametersStr = functionParameters.Select(p => GetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b);
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("/// <summary>");
            builderHeader.AppendLine($"///   {summary}");
            builderHeader.AppendLine("/// </summary>");
            builderHeader.AppendLine($"static Platform::String^ {key}_Format({parametersStr});");
            builderHeader.RemoveLevel();
        }

        private void CppCreateFormatMethod(CodeStringBuilder builderHeader, string computedNamespace, string key, IEnumerable<FunctionParameter> parameters, FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null)
        {
            IEnumerable<FunctionParameter> functionParameters;
            if (extraParameterForFunction != null)
            {
                var list = new List<FunctionParameter>(parameters);
                list.Insert(0, extraParameterForFunction);
                functionParameters = list;
            }
            else
            {
                functionParameters = parameters;
            }
            var parametersStr = functionParameters.Select(p => GetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b);

            builderHeader.AppendLine($"Platform::String^ {computedNamespace}{key}_Format({parametersStr})");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            var formatParameters = parameters
                .Select(p =>
                {
                    switch (p.Type)
                    {
                        case ParameterType.String:
                            return p.Name + "->Data()";
                        case ParameterType.Object:
                            return p.Name + "->ToString()";
                        default:
                            return p.Name;
                    }
                }).Aggregate((a, b) => a + ", " + b);

            string sourceForFormat;
            if (parameterForPluralization != null)
            {
                var doubleValue = parameterForPluralization.TypeToCast.HasValue ? $"static_cast<{GetParameterTypeString(parameterForPluralization.TypeToCast.Value)}>({parameterForPluralization.Name})" : parameterForPluralization.Name;
                sourceForFormat = $"{key}({doubleValue})";
            }
            else
            {
                sourceForFormat = key;
            }

            builderHeader.AppendLine($"size_t needed = _swprintf_p(nullptr, 0, {sourceForFormat}->Data(), {formatParameters});");
            builderHeader.AppendLine($"wchar_t *buffer = new wchar_t[needed + 1];");
            builderHeader.AppendLine($"_swprintf_p(buffer, needed + 1, {sourceForFormat}->Data(), {formatParameters});");
            builderHeader.AppendLine($"return ref new Platform::String(buffer);");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        private void HeaderCreateMarkupExtension(CodeStringBuilder builderHeader, string resourceFileName, string className, IEnumerable<string> keys)
        {
            builderHeader.AppendLine("public enum class KeyEnum");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("__Undefined = 0,");
            foreach (var key in keys)
            {
                builderHeader.AppendLine($"{key},");
            }
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("};");
            builderHeader.AppendEmptyLine();
            builderHeader.AppendLine($"public ref class {className} sealed: public Windows::UI::Xaml::Markup::MarkupExtension");
            builderHeader.AppendLine("{");
            builderHeader.AppendLine("private:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("Windows::ApplicationModel::Resources::ResourceLoader^ _resourceLoader;");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"{className}();");
            builderHeader.AppendLine("property KeyEnum Key;");
            builderHeader.AppendLine("property Windows::UI::Xaml::Data::IValueConverter^ Converter;");
            builderHeader.AppendLine("property Platform::Object^ ConverterParameter;");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("protected:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("virtual Platform::Object^ ProvideValue() override;");
            builderHeader.RemoveLevel();
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("};");

        }

        private void CppCreateMarkupExtension(CodeStringBuilder builderHeader, string computedNamespace, string resourceFileName, string className, IEnumerable<string> keys)
        {
            builderHeader.AppendLine($"{computedNamespace}{className}()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"_resourceLoader = Windows::ApplicationModel::Resources::ResourceLoader::GetForViewIndependentUse(L\"{resourceFileName}\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendEmptyLine();
            builderHeader.AppendLine($"Platform::Object^ {computedNamespace}ProvideValue()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("Platform::String^ res;");
            builderHeader.AppendLine("if(Key == KeyEnum::__Undefined)");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("res = L\"\";");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendLine("else");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("res = _resourceLoader->GetString(Key.ToString());");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendLine("return Converter == nullptr ? res : Converter->Convert(res, Windows::UI::Xaml::Interop::TypeName(Platform::String::typeid), ConverterParameter, nullptr);");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        public IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem)
        {
            var markupClassName = info.ClassName + "Extension";
            var headerFileName = baseFilename + ".h";
            var cppFileName = baseFilename + ".cpp";
            var baseNamespace = info.Namespace == null || !info.Namespace.Any() ? "" : info.Namespace.Aggregate((a, b) => a + "::" + b) + "::";
            var namespaceAndStronglyTypedClass = $"{baseNamespace}{info.ClassName}::";

            var builderHeader = new CodeStringBuilder("Cpp");
            var builderCpp = new CodeStringBuilder("Cpp");

            var precompiledHeader = GetprecompiledHeader(projectItem);

            HeaderFileGenerateHeaders(builderHeader, info.SupportPluralization);
            builderHeader.AppendEmptyLine();
            HeaderOpenNamespace(builderHeader, info.Namespace);
            HeaderOpenStronglyTypedClass(builderHeader, info.ResoureFile, info.ClassName);
            builderHeader.AppendEmptyLine();

            CppFileGenerateHeaders(builderCpp, precompiledHeader, headerFileName, info.SupportPluralization);
            builderCpp.AppendEmptyLine();
            CppGenerateStronglyTypedClassStaticFunc(builderCpp, namespaceAndStronglyTypedClass, info.ResoureFile);
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
                    CppCreatePluralizationAccessor(builderCpp, namespaceAndStronglyTypedClass, pluralLocalization.Key, pluralLocalization.SupportNoneState);
                    if (pluralLocalization.Parameters != null && pluralLocalization.Parameters.Any())
                    {
                        HeaderCreateFormatMethod(builderHeader, pluralLocalization.Key, pluralLocalization.Parameters, pluralLocalization.FormatSummary, pluralLocalization.ExtraParameterForPluralization, pluralLocalization.ParameterToUseForPluralization);
                        builderCpp.AppendEmptyLine();
                        CppCreateFormatMethod(builderCpp, namespaceAndStronglyTypedClass, pluralLocalization.Key, pluralLocalization.Parameters, pluralLocalization.ExtraParameterForPluralization, pluralLocalization.ParameterToUseForPluralization);
                    }
                }
                else if (item is Localization localization)
                {
                    HeaderCreateAccessor(builderHeader, localization.Key, localization.AccessorSummary);
                    CppCreateAccessor(builderCpp, namespaceAndStronglyTypedClass, localization.Key);
                    if (localization.Parameters != null && localization.Parameters.Any())
                    {
                        HeaderCreateFormatMethod(builderHeader, localization.Key, localization.Parameters, localization.FormatSummary);
                        builderCpp.AppendEmptyLine();
                        CppCreateFormatMethod(builderCpp, namespaceAndStronglyTypedClass, localization.Key, localization.Parameters);
                    }
                }

                HeaderCloseRegion(builderHeader);
            }
            HeaderCloseStronglyTypedClass(builderHeader);
            builderHeader.AppendEmptyLine();
            HeaderCreateMarkupExtension(builderHeader, info.ResoureFile, markupClassName, info.Localizations.Where(i => i is Localization).Select(s => s.Key));
            HeaderCloseNamespace(builderHeader, info.Namespace);
            builderCpp.AppendEmptyLine();
            CppCreateMarkupExtension(builderCpp, baseNamespace + markupClassName + "::", info.ResoureFile, markupClassName, info.Localizations.Where(i => i is Localization).Select(s => s.Key));

            yield return new GeneratedFile() { Filename = headerFileName, Content = builderHeader.GetString() };
            yield return new GeneratedFile() { Filename = cppFileName, Content = builderCpp.GetString() };
        }

        private string GetprecompiledHeader(ProjectItem projectItem)
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
