using EnvDTE;
using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal class CppCXCodeGenerator : CppCodeGeneratorBase
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

        protected override bool SupportMultiNamespaceDeclaration(Project project)
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

        protected override void HeaderCreateTemplateAccessor(CodeStringBuilder builderHeader, string pluralKey, string summary, bool supportPlural, bool supportVariants)
        {
            var parameters = new List<string>();
            if (supportVariants)
            {
                parameters.Add("long long variantId");
            }
            if (supportPlural)
            {
                parameters.Add("double pluralNumber");
            }

            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("/// <summary>");
            builderHeader.AppendLine($"///   {summary}");
            builderHeader.AppendLine("/// </summary>");
            builderHeader.AppendLine($"static Platform::String^ {pluralKey}({parameters.Aggregate((a, b) => a + ", " + b)});");
            builderHeader.RemoveLevel();
        }
        protected override void CppCreateTemplateAccessor(CodeStringBuilder builderCpp, string computedNamespaces, string key, bool supportPlural, bool pluralSupportNoneState, bool supportVariants)
        {
            var parameters = new List<string>();
            if (supportVariants)
            {
                parameters.Add("long long variantId");
            }
            if (supportPlural)
            {
                parameters.Add("double pluralNumber");
            }

            builderCpp.AppendLine($"String^ {computedNamespaces}{key}({parameters.Aggregate((a, b) => a + ", " + b)})");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();
            if (supportPlural && pluralSupportNoneState)
            {
                builderCpp.AppendLine("if(pluralNumber == 0)");
                builderCpp.AppendLine("{");
                builderCpp.AddLevel();
                builderCpp.AppendLine($"return GetResourceLoader()->GetString(L\"{key}_None\");");
                builderCpp.RemoveLevel();
                builderCpp.AppendLine("}");
            }

            var stringKey = supportVariants ? $"ref new String(L\"{key}_Variant\") + variantId" : $"L\"{key}\"";
            if (supportPlural)
            {
                builderCpp.AppendLine($"return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), {stringKey}, pluralNumber);");
            }
            else
            {
                builderCpp.AppendLine($"return GetResourceLoader()->GetString({stringKey});");
            }
            builderCpp.RemoveLevel();
            builderCpp.AppendLine("}");
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

        protected override void HeaderCreateFormatMethod(CodeStringBuilder builderHeader, string key, IEnumerable<FunctionParameter> parameters, string summary = null, IEnumerable<FunctionParameter> extraParameters = null)
        {
            IEnumerable<FunctionParameter> functionParameters;
            if (extraParameters != null)
            {
                var list = new List<FunctionParameter>(parameters);
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
            builderHeader.AppendLine($"static Platform::String^ {key}_Format({parametersStr});");
            builderHeader.RemoveLevel();
        }

        protected override void CppCreateFormatMethod(CodeStringBuilder builderCpp, string computedNamespace, string key, bool isDotNetFormatting, IEnumerable<Parameter> parameters, IEnumerable<FunctionParameter> extraParameters = null, FunctionParameter parameterForPluralization = null, FunctionParameter parameterForVariant = null)
        {
            IEnumerable<FunctionParameter> functionParameters;
            if (extraParameters != null)
            {
                var list = new List<FunctionParameter>(parameters.OfType<FunctionParameter>());
                list.InsertRange(0, extraParameters);
                functionParameters = list;
            }
            else
            {
                functionParameters = parameters.OfType<FunctionParameter>();
            }
            var parametersStr = functionParameters.Any() ?
                functionParameters.Select(p => GetParameterTypeString(p.Type, false) + " " + p.Name).Aggregate((a, b) => a + ", " + b)
                : "";

            builderCpp.AppendLine($"String^ {computedNamespace}{key}_Format({parametersStr})");
            builderCpp.AppendLine("{");
            builderCpp.AddLevel();
            var formatParameters = parameters
                .Select(p =>
                {
                    switch (p)
                    {
                        case ConstStringParameter constStringParam:
                            {
                                return $"L\"{constStringParam.Value}\"";
                            }
                        case LocalizationRefParameter localizationStringParameter:
                            {
                                return $"{localizationStringParameter.Id}->Data()";
                            }
                        case FunctionParameter functionParam:
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

            string sourceForFormat;
            if (parameterForPluralization != null)
            {
                var doubleValue = parameterForPluralization.TypeToCast.HasValue ? $"static_cast<{GetParameterTypeString(parameterForPluralization.TypeToCast.Value, false)}>({parameterForPluralization.Name})" : parameterForPluralization.Name;
                if (parameterForVariant != null)
                {
                    sourceForFormat = $"{key}({parameterForVariant.Name}, {doubleValue})";
                }
                else
                {
                    sourceForFormat = $"{key}({doubleValue})";
                }
            }
            else
            {
                if (parameterForVariant != null)
                {
                    sourceForFormat = $"{key}({parameterForVariant.Name})";
                }
                else
                {
                    sourceForFormat = key;
                }
            }
            if (isDotNetFormatting)
            {
                builderCpp.AppendLine($"return ReswPlusLib::StringFormatting::FormatDotNet({sourceForFormat}, ref new Array<Object^>({parameters.Count()}){{{formatParameters}}});");
            }
            else
            {
                builderCpp.AppendLine($"size_t needed = _swprintf_p(nullptr, 0, {sourceForFormat}->Data(), {formatParameters});");
                builderCpp.AppendLine($"wchar_t *buffer = new wchar_t[needed + 1];");
                builderCpp.AppendLine($"_swprintf_p(buffer, needed + 1, {sourceForFormat}->Data(), {formatParameters});");
                builderCpp.AppendLine($"return ref new String(buffer);");
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
