using ReswPlus.Core.ClassGenerator.Models;
using ReswPlus.Core.ResourceParser;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Core.CodeGenerators
{
    public class CppWinRTCodeGenerator : CppCodeGeneratorBase
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
                    return isHeader ? "Windows::Foundation::IStringable const&" : "IStringable const&";
            }
        }
        protected override bool SupportMultiNamespaceDeclaration()
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

            var baseGeneratedFileName = (namespacesOverride == null || namespacesOverride.Count() <= 1 ? "" : namespacesOverride.Skip(1).Aggregate((a, b) => a + "." + b) + ".");

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

            var baseNamespace = namespaces == null || namespaces.Count() <= 1 ? "" : namespaces.Skip(1).Aggregate((a, b) => a + "." + b) + ".";
            var gheader = baseNamespace + className;

            builderHeader.AppendLine($"#include \"{gheader}.g.cpp\"");
            builderHeader.AppendLine($"#include \"{gheader}Extension.g.cpp\"");
            builderHeader.AppendLine("#include <stdio.h>");
            builderHeader.AppendLine("#include <winrt/Windows.Foundation.h>");
            builderHeader.AppendLine("#include <winrt/Windows.UI.Xaml.Interop.h>");
            builderHeader.AppendLine("using namespace winrt;");
            builderHeader.AppendLine("using namespace std;");
            if (namespaces != null && namespaces.Any())
            {
                builderHeader.AppendLine($"using namespace winrt::{namespaces.Aggregate((a, b) => a + "::" + b)};");
            }
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

        protected override void HeaderCreateFormatMethod(CodeStringBuilder builderHeader, string key, bool isProperty, IEnumerable<FunctionFormatTagParameter> parameters, string summary = null, IEnumerable<FunctionFormatTagParameter> extraParameters = null)
        {
            if (isProperty)
            {
                HeaderCreateAccessor(builderHeader, key, summary);
                return;
            }

            IEnumerable<FunctionFormatTagParameter> functionParameters;
            if (extraParameters != null)
            {
                var list = new List<FunctionFormatTagParameter>(parameters);
                list.InsertRange(0, extraParameters);
                functionParameters = list;
            }
            else
            {
                functionParameters = parameters;
            }
            var parametersStr = functionParameters.Any() ?
                functionParameters.Select(p => GetParameterTypeString(p.Type, true) + " " + p.Name).Aggregate((a, b) => a + ", " + b)
                : "";
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("/// <summary>");
            builderHeader.AppendLine($"///   {summary}");
            builderHeader.AppendLine("/// </summary>");
            builderHeader.AppendLine($"static hstring {key}({parametersStr});");
            builderHeader.RemoveLevel();
        }

        protected override void CppCreateFormatMethod(CodeStringBuilder builderCpp, string computedNamespace, string key, bool isProperty, bool isDotNetFormatting, IEnumerable<IFormatTagParameter> parameters, IEnumerable<FunctionFormatTagParameter> extraParameters = null, FunctionFormatTagParameter parameterForPluralization = null, bool supportNoneState = false, FunctionFormatTagParameter parameterForVariant = null)
        {
            var functionParameters = parameters != null ? parameters.OfType<FunctionFormatTagParameter>().ToList() :
                               new List<FunctionFormatTagParameter>();
            if (extraParameters != null && extraParameters.Any())
            {
                functionParameters.InsertRange(0, extraParameters);
            }

            var parametersStr = functionParameters.Any() ?
            functionParameters.Select(p => GetParameterTypeString(p.Type, false) + " " + p.Name).Aggregate((a, b) => a + ", " + b)
            : "";

            builderCpp.AppendLine($"hstring {computedNamespace}{key}({parametersStr})");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();

            var keyToUseStr = parameterForVariant != null ? $"hstring(L\"{key}_Variant\") + to_wstring({parameterForVariant.Name})" : $"L\"{key}\"";


            string localizationStr;
            if (parameterForPluralization != null)
            {
                var pluralNumber = parameterForPluralization.TypeToCast.HasValue ? $"static_cast<{GetParameterTypeString(parameterForPluralization.TypeToCast.Value, false)}>({parameterForPluralization.Name})" : parameterForPluralization.Name;

                var supportNoneStateStr = supportNoneState ? "true" : "false";
                localizationStr = $"ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), {keyToUseStr}, {pluralNumber}, {supportNoneStateStr})";
            }
            else
            {
                localizationStr = $"GetResourceLoader().GetString({keyToUseStr})";
            }

            if (parameters != null && parameters.Any())
            {
                var formatParameters = parameters
                    .Select(p =>
                    {
                        switch (p)
                        {
                            case LiteralStringFormatTagParameter constStringParam:
                                {
                                    return isDotNetFormatting ? $"box_value(L\"{constStringParam.Value}\")" : $"L\"{constStringParam.Value}\"";
                                }
                            case MacroFormatTagParameter macroParam:
                                {
                                    return isDotNetFormatting ? $"box_value(ReswPlusLib::Macros::{macroParam.Id}())" : $"ReswPlusLib::Macros::{macroParam.Id}()->c_str()";
                                }
                            case StringRefFormatTagParameter localizationStringParameter:
                                {
                                    return isDotNetFormatting ? $"box_value({localizationStringParameter.Id}())" : $"{localizationStringParameter.Id}().c_str()";
                                }
                            case FunctionFormatTagParameter functionParam:
                                {
                                    if (isDotNetFormatting)
                                    {
                                        return $"box_value({functionParam.Name})";
                                    }
                                    switch (functionParam.Type)
                                    {
                                        case ParameterType.String:
                                            return functionParam.Name + ".c_str()";
                                        case ParameterType.Object:
                                            return $"_{functionParam.Name}_string";
                                        default:
                                            return functionParam.Name;
                                    }
                                }
                            default:
                                //should not happen
                                return "";
                        }

                    }).Aggregate((a, b) => a + ", " + b);

                if (isDotNetFormatting)
                {
                    builderCpp.AppendLine($"array<IInspectable const, {parameters.Count()}> _string_parameters = {{{formatParameters}}};");
                    builderCpp.AppendLine($"return ReswPlusLib::StringFormatting::FormatDotNet({localizationStr}, _string_parameters);");
                }
                else
                {
                    foreach (var param in parameters.OfType<FunctionFormatTagParameter>().Where(p => p.Type == ParameterType.Object))
                    {
                        builderCpp.AppendLine($"auto _{param.Name}_string = {param.Name} == nullptr ? L\"\" : {param.Name}.ToString().c_str();");
                    }

                    builderCpp.AppendLine($"size_t needed = _swprintf_p(nullptr, 0, {localizationStr}.c_str(), {formatParameters});");
                    builderCpp.AppendLine($"wchar_t *buffer = new wchar_t[needed + 1];");
                    builderCpp.AppendLine($"_swprintf_p(buffer, needed + 1, {localizationStr}.c_str(), {formatParameters});");
                    builderCpp.AppendLine($"return hstring(buffer);");
                }
            }
            else
            {
                builderCpp.AppendLine($"return {localizationStr};");
            }
            builderCpp.RemoveLevel();
            builderCpp.AppendLine("}");
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
            builderHeader.AppendLine("private:");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"static hstring KeyEnumToString(KeyEnum key);");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("};");
        }

        protected override void CppCreateMarkupExtension(CodeStringBuilder builderCpp, string computedNamespace, string resourceFileName, string className, IEnumerable<string> keys)
        {
            builderCpp.AppendLine($"{computedNamespace}{className}()");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();
            builderCpp.AppendLine($"_resourceLoader = ResourceLoader::GetForViewIndependentUse(L\"{resourceFileName}\");");
            builderCpp.RemoveLevel();
            builderCpp.AppendLine("}");
            builderCpp.AppendEmptyLine();
            builderCpp.AppendLine($"IInspectable {computedNamespace}ProvideValue()");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();
            builderCpp.AppendLine("hstring res;");
            builderCpp.AppendLine("if(Key() == KeyEnum::__Undefined)");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();
            builderCpp.AppendLine("res = L\"\";");
            builderCpp.RemoveLevel();
            builderCpp.AppendLine("}");
            builderCpp.AppendLine("else");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();

            builderCpp.AppendLine("auto keyStr = KeyEnumToString(Key());");
            builderCpp.AppendLine("if(keyStr == L\"\")");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();
            builderCpp.AppendLine("return box_value(L\"\");");
            builderCpp.RemoveLevel();
            builderCpp.AppendLine("}");
            builderCpp.AppendLine("res = _resourceLoader.GetString(keyStr);");
            builderCpp.RemoveLevel();
            builderCpp.AppendLine("}");
            builderCpp.AppendLine("return Converter() == nullptr ? box_value(res) : Converter().Convert(box_value(res), xaml_typename<hstring>(), ConverterParameter(), L\"\");");
            builderCpp.RemoveLevel();
            builderCpp.AppendLine("}");

            builderCpp.AppendEmptyLine();

            builderCpp.AppendLine($"hstring {computedNamespace}KeyEnumToString(KeyEnum key)");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();
            builderCpp.AppendLine("switch(key)");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();
            foreach (var key in keys)
            {
                builderCpp.AppendLine($"case KeyEnum::{key}:");
                builderCpp.AddLevel();
                builderCpp.AppendLine($"return hstring(L\"{key}\");");
                builderCpp.RemoveLevel();
            }
            builderCpp.AppendLine("default:");
            builderCpp.AddLevel();
            builderCpp.AppendLine("return hstring(L\"\");");
            builderCpp.RemoveLevel();
            builderCpp.RemoveLevel();
            builderCpp.AppendLine("}");
            builderCpp.RemoveLevel();
            builderCpp.AppendLine("}");
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
                    return "Windows.Foundation.IStringable";
            }
        }

        private void IdlCreateFormatMethod(CodeStringBuilder builderHeader, string key, bool isProperty, IEnumerable<FunctionFormatTagParameter> parameters, string summary = null, IEnumerable<FunctionFormatTagParameter> extraParameters = null)
        {
            if (isProperty)
            {
                IdlCreateAccessor(builderHeader, key, summary);
                return;
            }

            IEnumerable<FunctionFormatTagParameter> functionParameters;
            if (extraParameters != null)
            {
                var list = new List<FunctionFormatTagParameter>(parameters);
                list.InsertRange(0, extraParameters);
                functionParameters = list;
            }
            else
            {
                functionParameters = parameters;
            }

            var parametersStr = functionParameters.Any() ?
                functionParameters.Select(p => IdlGetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b) :
                "";
            builderHeader.AppendLine($"static String {key}({parametersStr});");
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

            builderHeader.AppendEmptyLine();

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

        private GeneratedFile GenerateIdlFile(CodeStringBuilder builderIdl, string baseFilename, StronglyTypedClass info)
        {
            HeaderOpenNamespace(builderIdl, info.Namespaces, false);
            IdlOpenStronglyTypedClass(builderIdl, info.ClassName);

            foreach (var item in info.Localizations)
            {
                IdlCreateFormatMethod(builderIdl, item.Key, item.IsProperty, item.Parameters.OfType<FunctionFormatTagParameter>(), item.Summary, item.ExtraParameters);
            }
            IdlCloseStronglyTypedClass(builderIdl);
            builderIdl.AppendEmptyLine();

            IdlCreateMarkupExtension(builderIdl, info.ClassName + "Extension", info.Localizations.Where(i => i is Localization).Select(s => s.Key));
            HeaderCloseNamespace(builderIdl, info.Namespaces, false);

            return new GeneratedFile() { Filename = baseFilename + ".idl", Content = builderIdl.GetString() };
        }

        #endregion

        public override IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ResourceInfo.ResourceFileInfo resourceFileInfo)
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

            var generatedFiles = GeneratedFiles(baseFilename, info, resourceFileInfo, namespaces);
            foreach (var file in generatedFiles)
            {
                yield return file;
            }
            var builderIdl = new CodeStringBuilder(resourceFileInfo.ParentProject.GetIndentString());
            // Generate .idl file
            yield return GenerateIdlFile(builderIdl, baseFilename, info);
        }
    }
}
