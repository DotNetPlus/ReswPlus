using ReswPlus.Core.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Core.CodeGenerators
{
    public class CSharpCodeGenerator : DotNetGeneratorBase
    {
        private readonly CodeStringBuilder _builder;

        public CSharpCodeGenerator()
        {
            _builder = new CodeStringBuilder("CSharp");
        }

        public string GetParameterTypeString(ParameterType type)
        {
            switch (type)
            {
                case ParameterType.Byte:
                    return "byte";
                case ParameterType.Int:
                    return "int";
                case ParameterType.Uint:
                    return "uint";
                case ParameterType.Long:
                    return "long";
                case ParameterType.String:
                    return "string";
                case ParameterType.Double:
                    return "double";
                case ParameterType.Char:
                    return "char";
                case ParameterType.Ulong:
                    return "ulong";
                case ParameterType.Decimal:
                    return "decimal";
                //case ParameterType.Object:
                default:
                    return "object";
            }
        }

        public override IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename)
        {
            yield return new GeneratedFile() { Filename = baseFilename + ".cs", Content = _builder.GetString() };
        }

        public override void GenerateHeaders(bool supportPluralization)
        {
            _builder.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralization)
            {
                _builder.AppendLine("// The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }
            _builder.AppendLine("using System;");
            _builder.AppendLine("using Windows.ApplicationModel.Resources;");
            _builder.AppendLine("using Windows.UI.Xaml.Markup;");
            _builder.AppendLine("using Windows.UI.Xaml.Data;");
        }

        public override void OpenNamespace(IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                _builder.AppendLine($"namespace {namespaces.Aggregate((a, b) => a + "." + b)}{{");
                _builder.AddLevel();
            }
        }

        public override void CloseNamespace(IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                _builder.RemoveLevel();
                _builder.AppendLine($"}} //{namespaces.Aggregate((a, b) => a + "." + b)}");
            }
        }

        public override void OpenStronglyTypedClass(string resourceFilename, string className)
        {

            _builder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"{Constants.ReswPlusExtensionVersion}\")]");
            _builder.AppendLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
            _builder.AppendLine("[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
            _builder.AppendLine($"public class {className} {{");
            _builder.AddLevel();
            _builder.AppendLine("private static ResourceLoader _resourceLoader;");
            _builder.AppendLine($"static {className}()");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine($"_resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFilename}\");");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
        }

        public override void CloseStronglyTypedClass()
        {
            _builder.RemoveLevel();
            _builder.AppendLine("}");
        }

        public override void OpenRegion(string name)
        {
            _builder.AppendLine($"#region {name}");
        }

        public override void CloseRegion(string name)
        {
            _builder.AppendLine($"#endregion");
        }

        public override void CreateFormatMethod(string key, bool isProperty, IEnumerable<FormatTagParameter> parameters, string summary = null, IEnumerable<FunctionFormatTagParameter> extraParameters = null, FunctionFormatTagParameter parameterForPluralization = null, bool supportNoneState = false, FunctionFormatTagParameter parameterForVariant = null)
        {
            _builder.AppendLine("/// <summary>");
            _builder.AppendLine($"///   {summary}");
            _builder.AppendLine("/// </summary>");

            if (isProperty)
            {
                _builder.AppendLine($"public static string {key}");
                _builder.AppendLine("{");
                _builder.AddLevel();
                _builder.AppendLine("get");
            }
            else
            {
                var functionParameters = parameters != null ? parameters.OfType<FunctionFormatTagParameter>().ToList() :
                    new List<FunctionFormatTagParameter>();
                if (extraParameters != null && extraParameters.Any())
                {
                    functionParameters.InsertRange(0, extraParameters);
                }

                if (parameters.Any(p => p is FunctionFormatTagParameter functionParam && functionParam.IsVariantId) || extraParameters.Any(p=>p.IsVariantId))
                {
                    // one of the parameter is a variantId, we must create a second method with object as the variantId type.
                    var genericParametersStr = functionParameters.Select(p => (p.IsVariantId ? "object" : GetParameterTypeString(p.Type)) + " " + p.Name).Aggregate((a, b) => a + ", " + b);
                    _builder.AppendLine($"public static string {key}({genericParametersStr})");
                    _builder.AppendLine("{");
                    _builder.AddLevel();
                    _builder.AppendLine("try");
                    _builder.AppendLine("{");
                    _builder.AddLevel();
                    _builder.AppendLine($"return {key}({functionParameters.Select(p => p.IsVariantId ? $"Convert.ToInt64({p.Name})" : p.Name).Aggregate((a, b) => a + ", " + b)});");
                    _builder.RemoveLevel();
                    _builder.AppendLine("}");
                    _builder.AppendLine("catch");
                    _builder.AppendLine("{");
                    _builder.AddLevel();
                    _builder.AppendLine("return \"\";");
                    _builder.RemoveLevel();
                    _builder.AppendLine("}");
                    _builder.RemoveLevel();
                    _builder.AppendLine("}");
                    _builder.AppendEmptyLine();
                    _builder.AppendLine("/// <summary>");
                    _builder.AppendLine($"///   {summary}");
                    _builder.AppendLine("/// </summary>");
                }

                var parametersStr = functionParameters.Select(p => GetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b);
                _builder.AppendLine($"public static string {key}({parametersStr})");
            }
            _builder.AppendLine("{");
            _builder.AddLevel();

            string keyToUseStr = $"\"{key}\"";
            if (parameterForVariant != null)
            {
                keyToUseStr = $"\"{key}_Variant\" + {parameterForVariant.Name}";
            }

            string localizationStr;
            if (parameterForPluralization != null)
            {
                var pluralNumber = parameterForPluralization.TypeToCast.HasValue ? $"({GetParameterTypeString(parameterForPluralization.TypeToCast.Value)}){parameterForPluralization.Name}" : parameterForPluralization.Name;

                var supportNoneStateStr = supportNoneState ? "true" : "false";
                localizationStr = $"ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, {keyToUseStr}, {pluralNumber}, {supportNoneStateStr})";

            }
            else
            {
                localizationStr = $"_resourceLoader.GetString({keyToUseStr})";
            }

            if (parameters != null && parameters.Any())
            {
                var formatParameters = parameters.Select(p =>
                {
                    switch (p)
                    {
                        case FunctionFormatTagParameter functionParam:
                            return functionParam.Name;
                        case MacroFormatTagParameter macroParam:
                            return $"ReswPlusLib.Macros.{macroParam.Id}";
                        case ConstStringFormatTagParameter constStringParameter:
                            return $"\"{constStringParameter.Value}\"";
                        case LocalizationRefFormatTagParameter localizationStringParameter:
                            return localizationStringParameter.Id;
                        default:
                            //should not happen
                            return "";
                    }
                }).Aggregate((a, b) => a + ", " + b);
                _builder.AppendLine($"return string.Format({localizationStr}, {formatParameters});");
            }
            else
            {
                _builder.AppendLine($"return {localizationStr};");
            }

            if (isProperty)
            {
                _builder.RemoveLevel();
                _builder.AppendLine("}");
            }
            _builder.RemoveLevel();
            _builder.AppendLine("}");

        }

        public override void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys)
        {
            _builder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"{Constants.ReswPlusExtensionVersion}\")]");
            _builder.AppendLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
            _builder.AppendLine("[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
            _builder.AppendLine("[MarkupExtensionReturnType(ReturnType = typeof(string))]");
            _builder.AppendLine($"public class {className}: MarkupExtension");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine("public enum KeyEnum");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine("__Undefined = 0,");
            foreach (var key in keys)
            {
                _builder.AppendLine($"{key},");
            }
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.AppendEmptyLine();
            _builder.AppendLine("private static ResourceLoader _resourceLoader;");
            _builder.AppendLine($"static {className}()");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine($"_resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFileName}\");");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.AppendLine("public KeyEnum Key { get; set;}");
            _builder.AppendLine("public IValueConverter Converter { get; set;}");
            _builder.AppendLine("public object ConverterParameter { get; set;}");
            _builder.AppendLine("protected override object ProvideValue()");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine("string res;");
            _builder.AppendLine("if(Key == KeyEnum.__Undefined)");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine("res = \"\";");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.AppendLine("else");
            _builder.AppendLine("{");
            _builder.AddLevel();
            _builder.AppendLine("res = _resourceLoader.GetString(Key.ToString());");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.AppendLine("return Converter == null ? res : Converter.Convert(res, typeof(String), ConverterParameter, null);");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
        }

        public override void AddNewLine()
        {
            _builder.AppendEmptyLine();
        }
    }
}
