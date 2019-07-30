using ReswPlus.Core.ResourceParser;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Core.CodeGenerators
{
    public class CppCXCodeGenerator : CppCodeGeneratorBase
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
                    return "long long";
                case ParameterType.String:
                    return isHeader ? "Platform::String^" : "String^";
                case ParameterType.Double:
                    return "double";
                case ParameterType.Char:
                    return "wchar_t";
                case ParameterType.Ulong:
                    return "unsigned long long";
                case ParameterType.Decimal:
                    return "double";
                //case ParameterType.Object:
                default:
                    return isHeader ? "Platform::Object^" : "Object^";
            }
        }

        protected override bool SupportMultiNamespaceDeclaration()
        {
            //TODO: Must find a way to detect if the project uses C++17 or higher
            return false;
        }


        protected override void HeaderFileGenerateHeaders(CodeStringBuilder builderHeader, string className, IEnumerable<string> namespacesOverride, bool supportPluralization)
        {
            //Header
            builderHeader.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralization)
            {
                builderHeader.AppendLine("// The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }
            builderHeader.AppendLine("#pragma once");
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
            builderHeader.AppendLine("#include <stdio.h>");
            builderHeader.AppendEmptyLine();
            builderHeader.AppendLine("using namespace Platform;");
            builderHeader.AppendLine("using namespace Windows::ApplicationModel::Resources;");
            builderHeader.AppendLine("using namespace Windows::UI::Xaml::Interop;");
            builderHeader.AppendLine($"namespace {LocalNamespaceName} = {localNamespace};");
        }

        protected override void HeaderOpenStronglyTypedClass(CodeStringBuilder builderHeader, string resourceFilename, string className)
        {
            builderHeader.AppendLine($"public ref class {className} sealed");
            builderHeader.AppendLine("{");
            builderHeader.AppendLine("private:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("static Windows::ApplicationModel::Resources::ResourceLoader^ _resourceLoader;");
            builderHeader.AppendLine("static Windows::ApplicationModel::Resources::ResourceLoader^ GetResourceLoader();");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"{className}() {{}}");
            builderHeader.RemoveLevel();
        }

        protected override void CppGenerateStronglyTypedClassStaticFunc(CodeStringBuilder builderHeader, string computedNamespace, string resourceFilename)
        {
            builderHeader.AppendLine($"ResourceLoader^ {computedNamespace}_resourceLoader = nullptr;");
            builderHeader.AppendLine($"ResourceLoader^ {computedNamespace}GetResourceLoader()");
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
            builderHeader.AppendLine($"static property Platform::String^ {key}");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("Platform::String^ get();");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.RemoveLevel();
        }

        protected override void CppCreateAccessor(CodeStringBuilder builderHeader, string computedNamespace, string key)
        {
            builderHeader.AppendLine($"String^ {computedNamespace}{key}::get()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"return GetResourceLoader()->GetString(L\"{key}\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        protected override void HeaderCreateFormatMethod(CodeStringBuilder builderHeader, string key, bool isProperty, IEnumerable<FunctionFormatTagParameter> parameters, string summary = null, IEnumerable<FunctionFormatTagParameter> extraParameters = null)
        {
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

            if (isProperty)
            {
                HeaderCreateAccessor(builderHeader, key, summary);
            }
            else
            {
                builderHeader.AppendLine("public:");
                builderHeader.AddLevel();
                builderHeader.AppendLine("/// <summary>");
                builderHeader.AppendLine($"///   {summary}");
                builderHeader.AppendLine("/// </summary>");

                var parametersStr = functionParameters == null || !functionParameters.Any() ?
                    "" :
                    functionParameters.Select(p => GetParameterTypeString(p.Type, true) + " " + p.Name).Aggregate((a, b) => a + ", " + b); builderHeader.AppendLine($"static Platform::String^ {key}({parametersStr});");
                builderHeader.RemoveLevel();
            }
        }

        protected override void CppCreateFormatMethod(CodeStringBuilder builderCpp, string computedNamespace, string key, bool isProperty, bool isDotNetFormatting, IEnumerable<IFormatTagParameter> parameters, IEnumerable<FunctionFormatTagParameter> extraParameters = null, FunctionFormatTagParameter parameterForPluralization = null, bool supportNoneState = false, FunctionFormatTagParameter parameterForVariant = null)
        {
            if (isProperty)
            {
                builderCpp.AppendLine($"String^ {computedNamespace}{key}::get()");
            }
            else
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

                builderCpp.AppendLine($"String^ {computedNamespace}{key}({parametersStr})");
            }

            builderCpp.AppendLine("{");
            builderCpp.AddLevel();

            string keyToUseStr = parameterForVariant != null ? $"ref new String(L\"{key}_Variant\") + {parameterForVariant.Name}" : $"L\"{key}\"";

            string localizationStr;
            if (parameterForPluralization != null)
            {
                var pluralNumber = parameterForPluralization.TypeToCast.HasValue ? $"static_cast<{GetParameterTypeString(parameterForPluralization.TypeToCast.Value, false)}>({parameterForPluralization.Name})" : parameterForPluralization.Name;

                var supportNoneStateStr = supportNoneState ? "true" : "false";

                localizationStr = $"ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), {keyToUseStr}, {pluralNumber}, {supportNoneStateStr})";
            }
            else
            {
                localizationStr = $"GetResourceLoader()->GetString({keyToUseStr})";
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
                            return isDotNetFormatting ? $"ref new String(L\"{constStringParam.Value}\")" : $"L\"{constStringParam.Value}\"";
                        }
                    case MacroFormatTagParameter macroParam:
                        {
                            return isDotNetFormatting ? $"ReswPlusLib::Macros::{macroParam.Id}" : $"ReswPlusLib::Macros::{macroParam.Id}->Data()";
                        }
                    case LocalizationRefFormatTagParameter localizationStringParameter:
                        {
                            return isDotNetFormatting ? $"{localizationStringParameter.Id}" : $"{localizationStringParameter.Id}->Data()";
                        }
                    case FunctionFormatTagParameter functionParam:
                        {
                            switch (functionParam.Type)
                            {
                                case ParameterType.String:
                                    return isDotNetFormatting ? functionParam.Name : functionParam.Name + "->Data()";
                                case ParameterType.Object:
                                    return isDotNetFormatting ? functionParam.Name + "->ToString()" : functionParam.Name + "->ToString()->Data()";
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
                    builderCpp.AppendLine($"return ReswPlusLib::StringFormatting::FormatDotNet({localizationStr}, ref new Array<Object^>({parameters.Count()}){{{formatParameters}}});");
                }
                else
                {
                    builderCpp.AppendLine($"size_t needed = _swprintf_p(nullptr, 0, {localizationStr}->Data(), {formatParameters});");
                    builderCpp.AppendLine($"wchar_t *buffer = new wchar_t[needed + 1];");
                    builderCpp.AppendLine($"_swprintf_p(buffer, needed + 1, {localizationStr}->Data(), {formatParameters});");
                    builderCpp.AppendLine($"return ref new String(buffer);");
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

        protected override void CppCreateMarkupExtension(CodeStringBuilder builderHeader, string computedNamespace, string resourceFileName, string className, IEnumerable<string> keys)
        {
            builderHeader.AppendLine($"{computedNamespace}{className}()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"_resourceLoader = ResourceLoader::GetForViewIndependentUse(L\"{resourceFileName}\");");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendEmptyLine();
            builderHeader.AppendLine($"Object^ {computedNamespace}ProvideValue()");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("String^ res;");
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
            builderHeader.AppendLine("return Converter == nullptr ? res : Converter->Convert(res, TypeName(String::typeid), ConverterParameter, nullptr);");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

    }
}
