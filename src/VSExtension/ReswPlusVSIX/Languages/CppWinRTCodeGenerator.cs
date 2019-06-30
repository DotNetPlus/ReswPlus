using EnvDTE;
using ReswPlus.ClassGenerator.Models;
using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal class CppWinRTCodeGenerator : CppCodeGeneratorBase
    {
        protected override string GetParameterTypeString(ParameterType type, bool isHeader)
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
                    return "hstring const&";
                case ParameterType.Decimal:
                case ParameterType.Double:
                    return "double";
                case ParameterType.Char:
                    return "wchar_t";
                case ParameterType.Ulong:
                    return "unsigned long";
                //case ParameterType.Object:
                default:
                    return isHeader ? "Windows::Foundation::IInspectable const&" : "IInspectable const&";
            }
        }
        protected override bool SupportMultiNamespaceDeclaration(Project project)
        {
            return true;
        }

        protected override void HeaderFileGenerateHeaders(CodeStringBuilder builderHeader, string className, IEnumerable<string> namespacesOverride, bool supportPluralization)
        {
            //Header
            builderHeader.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralization)
            {
                builderHeader.AppendLine("// The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }

            var baseGeneratedFileName = (namespacesOverride == null || namespacesOverride.Count() < 1 ? "" : namespacesOverride.Skip(1).Aggregate((a, b) => a + "." + b) + ".");

            builderHeader.AppendLine("#pragma once");
            builderHeader.AppendLine($"#include \"{baseGeneratedFileName}{className}.g.h\"");
            builderHeader.AppendLine($"#include \"{baseGeneratedFileName}{className}Extension.g.h\"");
            builderHeader.AppendLine("#include <winrt/Windows.ApplicationModel.Resources.h>");
            builderHeader.AppendLine("#include <winrt/Windows.UI.Xaml.Markup.h>");
            if (supportPluralization)
            {
                builderHeader.AppendLine("#include <winrt/ReswPlusLib.h>");
            }
        }

        protected override void CppFileGenerateHeaders(CodeStringBuilder builderHeader, string precompiledHeader, string headerFilePath, string localNamespace, string className, IEnumerable<string> namespaces, bool supportPluralization)
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

            var baseNamespace = namespaces == null || namespaces.Count() < 1 ? "" : namespaces.Skip(1).Aggregate((a, b) => a + "." + b) + ".";
            var gheader = baseNamespace + className;

            builderHeader.AppendLine($"#include \"{gheader}.g.cpp\"");
            builderHeader.AppendLine($"#include \"{gheader}Extension.g.cpp\"");
            builderHeader.AppendLine("#include <stdio.h>");
            builderHeader.AppendLine("#include <winrt/Windows.Foundation.h>");
            builderHeader.AppendLine("#include <winrt/Windows.UI.Xaml.Interop.h>");
            builderHeader.AppendLine("using namespace winrt;");
            builderHeader.AppendLine("using namespace winrt::Windows::Foundation;");
            builderHeader.AppendLine("using namespace winrt::Windows::ApplicationModel::Resources;");
            builderHeader.AppendLine("using namespace winrt::Windows::UI::Xaml::Interop;");
            builderHeader.AppendLine($"namespace {LocalNamespaceName} = {localNamespace};");
        }

        protected override void HeaderOpenStronglyTypedClass(CodeStringBuilder builderHeader, string resourceFilename, string className)
        {
            builderHeader.AppendLine($"struct {className}");
            builderHeader.AppendLine("{");
            builderHeader.AppendLine("private:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("static Windows::ApplicationModel::Resources::ResourceLoader _resourceLoader;");
            builderHeader.AppendLine("static Windows::ApplicationModel::Resources::ResourceLoader GetResourceLoader();");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"{className}() {{}}");
            builderHeader.RemoveLevel();
        }

        protected override void CppGenerateStronglyTypedClassStaticFunc(CodeStringBuilder builderHeader, string computedNamespace, string resourceFilename)
        {
            builderHeader.AppendLine($"ResourceLoader {computedNamespace}_resourceLoader = nullptr;");
            builderHeader.AppendLine($"ResourceLoader {computedNamespace}GetResourceLoader()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("if (_resourceLoader == nullptr)");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"_resourceLoader = ResourceLoader::GetForViewIndependentUse(L\"{resourceFilename}\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendLine("return _resourceLoader;");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        protected override void HeaderCloseStronglyTypedClass(CodeStringBuilder builderHeader)
        {
            builderHeader.AppendLine("};");
        }

        protected override void HeaderCreatePluralizationAccessor(CodeStringBuilder builderHeader, string pluralKey, string summary)
        {
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("/// <summary>");
            builderHeader.AppendLine($"///   {summary}");
            builderHeader.AppendLine("/// </summary>");
            builderHeader.AppendLine($"static hstring {pluralKey}(double number);");
            builderHeader.RemoveLevel();
        }
        protected override void CppCreatePluralizationAccessor(CodeStringBuilder builderHeader, string computedNamespaces, string pluralKey, bool supportNoneState)
        {
            builderHeader.AppendLine($"hstring {computedNamespaces}{pluralKey}(double number)");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            if (supportNoneState)
            {
                builderHeader.AppendLine("if(number == 0)");
                builderHeader.AppendLine("{");
                builderHeader.AddLevel();
                builderHeader.AppendLine($"return GetResourceLoader().GetString(L\"{pluralKey}_None\");");
                builderHeader.RemoveLevel();
                builderHeader.AppendLine("}");
            }

            builderHeader.AppendLine($"return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L\"{pluralKey}\", number);");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        protected override void HeaderCreateAccessor(CodeStringBuilder builderHeader, string key, string summary)
        {
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("/// <summary>");
            builderHeader.AppendLine($"///   {summary}");
            builderHeader.AppendLine("/// </summary>");
            builderHeader.AppendLine($"static hstring {key}();");
            builderHeader.RemoveLevel();
        }

        protected override void CppCreateAccessor(CodeStringBuilder builderHeader, string computedNamespace, string key)
        {
            builderHeader.AppendLine($"hstring {computedNamespace}{key}()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"return GetResourceLoader().GetString(L\"{key}\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        protected override void HeaderCreateFormatMethod(CodeStringBuilder builderHeader, string key, IEnumerable<FunctionParameter> parameters, string summary = null, FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null)
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
            var parametersStr = functionParameters.Select(p => GetParameterTypeString(p.Type, true) + " " + p.Name).Aggregate((a, b) => a + ", " + b);
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("/// <summary>");
            builderHeader.AppendLine($"///   {summary}");
            builderHeader.AppendLine("/// </summary>");
            builderHeader.AppendLine($"static hstring {key}_Format({parametersStr});");
            builderHeader.RemoveLevel();
        }

        protected override void CppCreateFormatMethod(CodeStringBuilder builderHeader, string computedNamespace, string key, IEnumerable<FunctionParameter> parameters, FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null)
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
            var parametersStr = functionParameters.Select(p => GetParameterTypeString(p.Type, false) + " " + p.Name).Aggregate((a, b) => a + ", " + b);

            builderHeader.AppendLine($"hstring {computedNamespace}{key}_Format({parametersStr})");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            foreach (var param in parameters.Where(p => p.Type == ParameterType.Object))
            {
                builderHeader.AppendLine($"auto _{param.Name}_istringable = {param.Name}.try_as<IStringable>();");
                builderHeader.AppendLine($"auto _{param.Name}_string = _{param.Name}_istringable == nullptr ? L\"\" : _{param.Name}_istringable.ToString().c_str();");
            }

            var formatParameters = parameters
                .Select(p =>
                {
                    switch (p.Type)
                    {
                        case ParameterType.String:
                            return p.Name + ".c_str()";
                        case ParameterType.Object:
                            return $"_{p.Name}_string";
                        default:
                            return p.Name;
                    }
                }).Aggregate((a, b) => a + ", " + b);

            string sourceForFormat;
            if (parameterForPluralization != null)
            {
                var doubleValue = parameterForPluralization.TypeToCast.HasValue ? $"static_cast<{GetParameterTypeString(parameterForPluralization.TypeToCast.Value, false)}>({parameterForPluralization.Name})" : parameterForPluralization.Name;
                sourceForFormat = $"{key}({doubleValue})";
            }
            else
            {
                sourceForFormat = $"{key}()";
            }

            builderHeader.AppendLine($"size_t needed = _swprintf_p(nullptr, 0, {sourceForFormat}.c_str(), {formatParameters});");
            builderHeader.AppendLine($"wchar_t *buffer = new wchar_t[needed + 1];");
            builderHeader.AppendLine($"_swprintf_p(buffer, needed + 1, {sourceForFormat}.c_str(), {formatParameters});");
            builderHeader.AppendLine($"return hstring(buffer);");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        protected override void HeaderCreateMarkupExtension(CodeStringBuilder builderHeader, string resourceFileName, string className, IEnumerable<string> keys, IEnumerable<string> namespaces)
        {
            var namespacesToUse = namespaces == null || !namespaces.Any() ? "" : namespaces.Aggregate((a, b) => a + "::" + b) + "::";

            builderHeader.AppendLine($"struct {className}: {className}T<{className}>");
            builderHeader.AppendLine("{");
            builderHeader.AppendLine("private:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("Windows::ApplicationModel::Resources::ResourceLoader _resourceLoader;");
            builderHeader.AppendLine($"{namespacesToUse}KeyEnum _key;");
            builderHeader.AppendLine("Windows::UI::Xaml::Data::IValueConverter _converter;");
            builderHeader.AppendLine("Windows::Foundation::IInspectable _converterParameter;");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"{className}();");
            builderHeader.AppendLine($"{namespacesToUse}KeyEnum Key(){{ return _key; }}");
            builderHeader.AppendLine($"void Key({namespacesToUse}KeyEnum value){{ _key = value; }}");
            builderHeader.AppendLine("Windows::UI::Xaml::Data::IValueConverter Converter(){{ return _converter; }}");
            builderHeader.AppendLine("void Converter(Windows::UI::Xaml::Data::IValueConverter value){{ _converter = value; }}");
            builderHeader.AppendLine("Windows::Foundation::IInspectable ConverterParameter(){{ return _converterParameter; }}");
            builderHeader.AppendLine("void ConverterParameter(Windows::Foundation::IInspectable value){{ _converterParameter = value; }}");
            builderHeader.AppendLine("Windows::Foundation::IInspectable ProvideValue();");
            builderHeader.RemoveLevel();
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("};");
        }

        protected override void CppCreateMarkupExtension(CodeStringBuilder builderHeader, string computedNamespace, string resourceFileName, string className, IEnumerable<string> keys)
        {
            builderHeader.AppendLine($"{computedNamespace}{className}()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"_resourceLoader = ResourceLoader::GetForViewIndependentUse(L\"{resourceFileName}\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendEmptyLine();
            builderHeader.AppendLine($"IInspectable {computedNamespace}ProvideValue()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("hstring res;");
            builderHeader.AppendLine("if(Key() == KeyEnum::__Undefined)");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("res = L\"\";");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendLine("else");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("res = _resourceLoader.GetString(L\"TODO\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendLine("return Converter() == nullptr ? box_value(res) : Converter().Convert(box_value(res), xaml_typename<hstring>(), ConverterParameter(), L\"\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        #region IDL

        private void IdlOpenStronglyTypedClass(CodeStringBuilder builderHeader, string className)
        {
            builderHeader.AppendLine($"static runtimeclass {className}");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
        }

        private void IdlCloseStronglyTypedClass(CodeStringBuilder builderHeader)
        {
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("};");
        }

        private void IdlCreatePluralizationAccessor(CodeStringBuilder builderHeader, string pluralKey)
        {
            builderHeader.AppendLine($"static String {pluralKey}(Double number);");
        }

        private void IdlCreateAccessor(CodeStringBuilder builderHeader, string key, string summary)
        {
            builderHeader.AppendLine($"static String {key}{{ get; }};");
        }

        private string IdlGetParameterTypeString(ParameterType type)
        {
            switch (type)
            {
                case ParameterType.Byte:
                    return "Char";
                case ParameterType.Int:
                    return "Int32";
                case ParameterType.Uint:
                    return "UInt32";
                case ParameterType.Long:
                    return "Int64";
                case ParameterType.Decimal:
                case ParameterType.String:
                    return "String";
                case ParameterType.Double:
                    return "Double";
                case ParameterType.Char:
                    return "Char";
                case ParameterType.Ulong:
                    return "UInt64";
                //case ParameterType.Object:
                default:
                    return "Object";
            }
        }

        private void IdlCreateFormatMethod(CodeStringBuilder builderHeader, string key, IEnumerable<FunctionParameter> parameters, string summary = null, FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null)
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
            var parametersStr = functionParameters.Select(p => IdlGetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b);
            builderHeader.AppendLine($"static String {key}_Format({parametersStr});");
        }

        private void IdlCreateMarkupExtension(CodeStringBuilder builderHeader, string className, IEnumerable<string> keys)
        {
            builderHeader.AppendLine("enum KeyEnum");
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
            builderHeader.AppendLine($"runtimeclass {className}: Windows.UI.Xaml.Markup.MarkupExtension");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"{className}();");
            builderHeader.AppendLine("KeyEnum Key;");
            builderHeader.AppendLine("Windows.UI.Xaml.Data.IValueConverter Converter;");
            builderHeader.AppendLine("Object ConverterParameter;");
            builderHeader.AppendLine("overridable Object ProvideValue();");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("};");
        }

        protected override void HeaderAddExtra(CodeStringBuilder builderHeader, StronglyTypedClass info)
        {
            var namespaces = new List<string>
            {
                "winrt"
            };
            if (info.Namespaces != null)
            {
                namespaces.AddRange(info.Namespaces);
            }
            namespaces.Add("factory_implementation");

            HeaderOpenNamespace(builderHeader, namespaces, true);
            builderHeader.AppendLine($"struct {info.ClassName} : {info.ClassName}T<{info.ClassName}, implementation::{info.ClassName}>");
            builderHeader.AppendLine("{");
            builderHeader.AppendLine("};");
            builderHeader.AppendEmptyLine();
            builderHeader.AppendLine($"struct {info.ClassName}Extension : {info.ClassName}ExtensionT<{info.ClassName}Extension, implementation::{info.ClassName}Extension>");
            builderHeader.AppendLine("{");
            builderHeader.AppendLine("};");
            HeaderCloseNamespace(builderHeader, namespaces, true);
        }

        private GeneratedFile GenerateIdlFile(string baseFilename, StronglyTypedClass info, ProjectItem projectItem)
        {
            var builderIdl = new CodeStringBuilder("Cpp");
            HeaderOpenNamespace(builderIdl, info.Namespaces, false);
            IdlOpenStronglyTypedClass(builderIdl, info.ClassName);

            foreach (var item in info.Localizations)
            {
                if (item is PluralLocalization pluralLocalization)
                {
                    IdlCreatePluralizationAccessor(builderIdl, item.Key);
                    if (pluralLocalization.Parameters != null && pluralLocalization.Parameters.Any())
                    {
                        IdlCreateFormatMethod(builderIdl, pluralLocalization.Key, pluralLocalization.Parameters, pluralLocalization.FormatSummary, pluralLocalization.ExtraParameterForPluralization, pluralLocalization.ParameterToUseForPluralization);
                    }
                }
                else if (item is Localization localization)
                {
                    IdlCreateAccessor(builderIdl, localization.Key, localization.AccessorSummary);
                    if (localization.Parameters != null && localization.Parameters.Any())
                    {
                        IdlCreateFormatMethod(builderIdl, localization.Key, localization.Parameters, localization.FormatSummary);
                    }
                }
            }
            IdlCloseStronglyTypedClass(builderIdl);
            builderIdl.AppendEmptyLine();

            IdlCreateMarkupExtension(builderIdl, info.ClassName + "Extension", info.Localizations.Where(i => i is Localization).Select(s => s.Key));
            HeaderCloseNamespace(builderIdl, info.Namespaces, false);

            return new GeneratedFile() { Filename = baseFilename + ".idl", Content = builderIdl.GetString() };
        }

        #endregion

        public override IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem)
        {
            // Generate .cpp and .h files
            var namespaces = new List<string>
            {
                "winrt"
            };
            if (info.Namespaces != null)
            {
                namespaces.AddRange(info.Namespaces);
            }
            namespaces.Add("implementation");

            var generatedFiles = GeneratedFiles(baseFilename, info, projectItem, namespaces);
            foreach (var file in generatedFiles)
            {
                yield return file;
            }

            // Generate .idl file
            yield return GenerateIdlFile(baseFilename, info, projectItem);

        }
    }
}
