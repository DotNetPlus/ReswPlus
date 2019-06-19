using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal class CppCodeGenerator : ICodeGenerator
    {
        private readonly CodeStringBuilder _builder;
        private readonly string _precompiledFileName;
        public CppCodeGenerator()
        {
            _builder = new CodeStringBuilder("Cpp");
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

        public void NewLine()
        {
            _builder.AppendEmptyLine();
        }

        public string GetString()
        {
            return _builder.GetString();
        }

        public void GetHeaders(bool supportPluralization)
        {
            _builder.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralization)
            {
                _builder.AppendLine("// The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }
            _builder.AppendLine("#pragma once");
            _builder.AppendLine("#include <stdio.h>");
        }

        public void OpenNamespace(string[] namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                var line = "";
                foreach (var subNamespace in namespaces)
                {
                    line += $"namespace {subNamespace}{{";
                }
                _builder.AppendLine(line);
                _builder.AddLevel();
            }
        }

        public void CloseNamespace(string[] namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                _builder.RemoveLevel();
                var endNamespaces = "";
                foreach (var n in namespaces)
                {
                    endNamespaces += "}";
                }
                _builder.AppendLine($"{endNamespaces} //{namespaces.Aggregate((a, b) => a + "::" + b)}");
            }
        }

        public void OpenStronglyTypedClass(string resourceFilename, string className)
        {
            _builder.AppendLine($"public ref class {className} sealed{{");
            _builder.AppendLine("private:");
            _builder.AddLevel();
            _builder.AppendLine("static Windows::ApplicationModel::Resources::ResourceLoader^ GetResourceLoader() {");
            _builder.AddLevel();
            _builder.AppendLine("static Windows::ApplicationModel::Resources::ResourceLoader^ _resourceLoader(nullptr);");
            _builder.AppendLine("if (_resourceLoader == nullptr)");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine($"_resourceLoader = Windows::ApplicationModel::Resources::ResourceLoader::GetForViewIndependentUse(L\"{resourceFilename}\");");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.AppendLine("return _resourceLoader;");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.RemoveLevel();
            _builder.AppendLine("public:");
            _builder.AddLevel();
            _builder.AppendLine($"{className}() {{}}");
            _builder.RemoveLevel();
        }

        public void CloseStronglyTypedClass()
        {
            _builder.AppendLine("};");
        }

        public void OpenRegion(string name)
        {
            _builder.AppendLine($"#pragma region {name}");
        }

        public void CloseRegion()
        {
            _builder.AppendLine("#pragma endregion");
        }

        public void CreatePluralizationAccessor(string pluralKey, string summary, string idNone = null)
        {

            _builder.AppendLine("public:");
            _builder.AddLevel();
            _builder.AppendLine("/// <summary>");
            _builder.AppendLine($"///   {summary}");
            _builder.AppendLine("/// </summary>");
            _builder.AppendLine($"static Platform::String^ {pluralKey}(double number)");
            _builder.AppendLine("{");
            _builder.AddLevel();
            if (!string.IsNullOrEmpty(idNone))
            {
                _builder.AppendLine("if(number == 0)");
                _builder.AppendLine("{");
                _builder.AddLevel();
                _builder.AppendLine($"return GetResourceLoader()->GetString(\"{idNone}\");");
                _builder.RemoveLevel();
                _builder.AppendLine("}");
            }

            _builder.AppendLine($"return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), \"{pluralKey}\", number);");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.RemoveLevel();

        }

        public void CreateAccessor(string key, string summary)
        {
            _builder.AppendLine("public:");
            _builder.AddLevel();
            _builder.AppendLine("/// <summary>");
            _builder.AppendLine($"///   {summary}");
            _builder.AppendLine("/// </summary>");
            _builder.AppendLine($"static property Platform::String^ {key}");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine($"Platform::String^ get() {{ return GetResourceLoader()->GetString(\"{key}\"); }}");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.RemoveLevel();
        }

        public void CreateFormatMethod(string key, IEnumerable<FunctionParameter> parameters, string summary = null, FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null)
        {
            _builder.AppendLine("/// <summary>");
            _builder.AppendLine($"///   {summary}");
            _builder.AppendLine("/// </summary>");

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
            _builder.AppendLine("public:");
            _builder.AddLevel();
            _builder.AppendLine($"static Platform::String^ {key}_Format({parametersStr})");
            _builder.AppendLine("{");
            _builder.AddLevel();
            var formatParameters = parameters
                .Select(p =>
                {
                    string value = null;
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

            _builder.AppendLine($"size_t needed = _swprintf_p(nullptr, 0, {sourceForFormat}->Data(), {formatParameters});");
            _builder.AppendLine($"wchar_t *buffer = new wchar_t[needed + 1];");
            _builder.AppendLine($"_swprintf_p(buffer, needed + 1, {sourceForFormat}->Data(), {formatParameters});");
            _builder.AppendLine($"return ref new Platform::String(buffer);");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.RemoveLevel();

        }

        public void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys)
        {
            _builder.AppendLine("public enum class KeyEnum");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine("__Undefined = 0,");
            foreach (var key in keys)
            {
                _builder.AppendLine($"{key},");
            }
            _builder.RemoveLevel();
            _builder.AppendLine("};");
            _builder.AppendEmptyLine();
            _builder.AppendLine($"public ref class {className} sealed: public Windows::UI::Xaml::Markup::MarkupExtension");
            _builder.AppendLine("{");
            _builder.AppendLine("private:");
            _builder.AddLevel();
            _builder.AppendLine("Windows::ApplicationModel::Resources::ResourceLoader^ _resourceLoader;");
            _builder.RemoveLevel();
            _builder.AppendLine("public:");
            _builder.AddLevel();
            _builder.AppendLine($"{className}()");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine($"_resourceLoader = Windows::ApplicationModel::Resources::ResourceLoader::GetForViewIndependentUse(L\"{resourceFileName}\");");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.AppendLine("property KeyEnum Key;");
            _builder.AppendLine("property Windows::UI::Xaml::Data::IValueConverter^ Converter;");
            _builder.AppendLine("property Platform::Object^ ConverterParameter;");
            _builder.RemoveLevel();
            _builder.AppendLine("protected:");
            _builder.AddLevel();
            _builder.AppendLine("virtual Platform::Object^ ProvideValue() override");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine("Platform::String^ res;");
            _builder.AppendLine("if(Key == KeyEnum::__Undefined)");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine("res = \"\";");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.AppendLine("else");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine("res = _resourceLoader->GetString(Key.ToString());");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.AppendLine("return Converter == nullptr ? res : Converter->Convert(res, Windows::UI::Xaml::Interop::TypeName(Platform::String::typeid), ConverterParameter, nullptr);");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.RemoveLevel();
            _builder.AppendLine("};");
        }
    }
}
