using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal class CppWinRTCodeGenerator : CppCodeGeneratorBase
    {
        protected override string GetParameterTypeString(ParameterType type)
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
                    return "winrt::hstring";
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
                    return "winrt::Windows::Foundation::IInspectable";
            }
        }

        protected override void HeaderFileGenerateHeaders(CodeStringBuilder builderHeader, bool supportPluralization)
        {
            //Header
            builderHeader.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralization)
            {
                builderHeader.AppendLine("// The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }
            builderHeader.AppendLine("#pragma once");
            builderHeader.AppendLine("#include <winrt/Windows.ApplicationModel.Resources.h>");
            builderHeader.AppendLine("#include <winrt/Windows.UI.Xaml.Markup.h>");
        }

        protected override void CppFileGenerateHeaders(CodeStringBuilder builderHeader, string precompiledHeader, string headerFilePath, bool supportPluralization)
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
            builderHeader.AppendLine("#include <winrt/Windows.Foundation.h>");
            builderHeader.AppendLine("#include <winrt/Windows.UI.Xaml.Interop.h>");
            builderHeader.AppendLine("using namespace winrt::Windows::Foundation;");
            builderHeader.AppendLine("using namespace winrt::Windows::ApplicationModel::Resources;");
            builderHeader.AppendLine("using namespace winrt::Windows::UI::Xaml::Interop;");
        }

        protected override void HeaderOpenStronglyTypedClass(CodeStringBuilder builderHeader, string resourceFilename, string className)
        {
            builderHeader.AppendLine($"public class {className} sealed");
            builderHeader.AppendLine("{");
            builderHeader.AppendLine("private:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("static winrt::Windows::ApplicationModel::Resources::ResourceLoader _resourceLoader;");
            builderHeader.AppendLine("static winrt::Windows::ApplicationModel::Resources::ResourceLoader GetResourceLoader();");
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
            builderHeader.AppendLine($"static winrt::hstring {pluralKey}(double number);");
            builderHeader.RemoveLevel();
        }
        protected override void CppCreatePluralizationAccessor(CodeStringBuilder builderHeader, string computedNamespaces, string pluralKey, bool supportNoneState)
        {
            builderHeader.AppendLine($"winrt::hstring {computedNamespaces}{pluralKey}(double number)");
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
            builderHeader.AppendLine($"static winrt::hstring Get{key}();");
            builderHeader.RemoveLevel();
        }

        protected override void CppCreateAccessor(CodeStringBuilder builderHeader, string computedNamespace, string key)
        {
            builderHeader.AppendLine($"winrt::hstring {computedNamespace}Get{key}()");
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
            var parametersStr = functionParameters.Select(p => GetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b);
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("/// <summary>");
            builderHeader.AppendLine($"///   {summary}");
            builderHeader.AppendLine("/// </summary>");
            builderHeader.AppendLine($"static winrt::hstring {key}_Format({parametersStr});");
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
            var parametersStr = functionParameters.Select(p => GetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b);

            builderHeader.AppendLine($"winrt::hstring {computedNamespace}{key}_Format({parametersStr})");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            var formatParameters = parameters
                .Select(p =>
                {
                    switch (p.Type)
                    {
                        case ParameterType.String:
                            return p.Name + ".c_str()";
                        case ParameterType.Object:
                            return p.Name + ".ToString()";
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

            builderHeader.AppendLine($"size_t needed = _swprintf_p(nullptr, 0, {sourceForFormat}.c_str(), {formatParameters});");
            builderHeader.AppendLine($"wchar_t *buffer = new wchar_t[needed + 1];");
            builderHeader.AppendLine($"_swprintf_p(buffer, needed + 1, {sourceForFormat}.c_str(), {formatParameters});");
            builderHeader.AppendLine($"return winrt::box_value(buffer);");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

        protected override void HeaderCreateMarkupExtension(CodeStringBuilder builderHeader, string resourceFileName, string className, IEnumerable<string> keys)
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
            builderHeader.AppendLine($"public class {className} sealed: public winrt::Windows::UI::Xaml::Markup::MarkupExtension");
            builderHeader.AppendLine("{");
            builderHeader.AppendLine("private:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("winrt::Windows::ApplicationModel::Resources::ResourceLoader _resourceLoader;");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("public:");
            builderHeader.AddLevel();
            builderHeader.AppendLine($"{className}();");
            builderHeader.AppendLine("KeyEnum Key;");
            builderHeader.AppendLine("winrt::Windows::UI::Xaml::Data::IValueConverter Converter;");
            builderHeader.AppendLine("winrt::Windows::Foundation::IInspectable ConverterParameter;");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("protected:");
            builderHeader.AddLevel();
            builderHeader.AppendLine("virtual winrt::Windows::Foundation::IInspectable ProvideValue() const override;");
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
            builderHeader.AppendLine("winrt::hstring res;");
            builderHeader.AppendLine("if(Key == KeyEnum::__Undefined)");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("res = L\"\";");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendLine("else");
            builderHeader.AppendLine("{");
            builderHeader.AddLevel();
            builderHeader.AppendLine("res = _resourceLoader.GetString(Key.ToString());");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
            builderHeader.AppendLine("return Converter == nullptr ? res : Converter.Convert(res, TypeName(winrt::hstring::typeid), ConverterParameter, nullptr);");
            builderHeader.RemoveLevel();
            builderHeader.AppendLine("}");
        }

    }
}
