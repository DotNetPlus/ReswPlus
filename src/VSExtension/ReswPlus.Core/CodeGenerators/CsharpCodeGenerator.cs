using ReswPlus.Core.ResourceParser;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Core.CodeGenerators
{
    public class CSharpCodeGenerator : DotNetGeneratorBase
    {
        protected string GetParameterTypeString(ParameterType type)
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

        protected override IEnumerable<GeneratedFile> GetGeneratedFiles(CodeStringBuilder builder, string baseFilename)
        {
            yield return new GeneratedFile() { Filename = baseFilename + ".cs", Content = builder.GetString() };
        }

        protected override void GenerateHeaders(CodeStringBuilder builder, bool supportPluralization)
        {
            builder.AppendLine("// File generated automatically by ReswPlus. https://github.com/DotNetPlus/ReswPlus");
            if (supportPluralization)
            {
                builder.AppendLine("// The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }
            builder.AppendLine("using System;");
            builder.AppendLine("using Windows.ApplicationModel.Resources;");
            builder.AppendLine("using Windows.UI.Xaml.Markup;");
            builder.AppendLine("using Windows.UI.Xaml.Data;");
        }

        protected override void OpenNamespace(CodeStringBuilder builder, IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                builder.AppendLine($"namespace {namespaces.Aggregate((a, b) => a + "." + b)}{{");
                builder.AddLevel();
            }
        }

        protected override void CloseNamespace(CodeStringBuilder builder, IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                builder.RemoveLevel();
                builder.AppendLine($"}} //{namespaces.Aggregate((a, b) => a + "." + b)}");
            }
        }

        protected override void OpenStronglyTypedClass(CodeStringBuilder builder, string resourceFilename, string className)
        {

            builder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{Constants.ReswPlusName}\", \"{Constants.ReswPlusExtensionVersion}\")]");
            builder.AppendLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
            builder.AppendLine("[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
            builder.AppendLine($"public static class {className} {{");
            builder.AddLevel();
            builder.AppendLine("private static ResourceLoader _resourceLoader;");
            builder.AppendLine($"static {className}()");
            builder.AppendLine("{");
            builder.AddLevel();
            builder.AppendLine($"_resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFilename}\");");
            builder.RemoveLevel();
            builder.AppendLine("}");
        }

        protected override void CloseStronglyTypedClass(CodeStringBuilder builder)
        {
            builder.RemoveLevel();
            builder.AppendLine("}");
        }

        protected override void OpenRegion(CodeStringBuilder builder, string name)
        {
            builder.AppendLine($"#region {name}");
        }

        protected override void CloseRegion(CodeStringBuilder builder, string name)
        {
            builder.AppendLine($"#endregion");
        }

        protected override void CreateFormatMethod(CodeStringBuilder builder, string key, bool isProperty, IEnumerable<IFormatTagParameter> parameters, string summary = null, IEnumerable<FunctionFormatTagParameter> extraParameters = null, FunctionFormatTagParameter parameterForPluralization = null, bool supportNoneState = false, FunctionFormatTagParameter parameterForVariant = null)
        {
            builder.AppendLine("/// <summary>");
            builder.AppendLine($"///   {summary}");
            builder.AppendLine("/// </summary>");

            if (isProperty)
            {
                builder.AppendLine($"public static string {key}");
                builder.AppendLine("{");
                builder.AddLevel();
                builder.AppendLine("get");
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
                    builder.AppendLine($"public static string {key}({genericParametersStr})");
                    builder.AppendLine("{");
                    builder.AddLevel();
                    builder.AppendLine("try");
                    builder.AppendLine("{");
                    builder.AddLevel();
                    builder.AppendLine($"return {key}({functionParameters.Select(p => p.IsVariantId ? $"Convert.ToInt64({p.Name})" : p.Name).Aggregate((a, b) => a + ", " + b)});");
                    builder.RemoveLevel();
                    builder.AppendLine("}");
                    builder.AppendLine("catch");
                    builder.AppendLine("{");
                    builder.AddLevel();
                    builder.AppendLine("return \"\";");
                    builder.RemoveLevel();
                    builder.AppendLine("}");
                    builder.RemoveLevel();
                    builder.AppendLine("}");
                    builder.AppendEmptyLine();
                    builder.AppendLine("/// <summary>");
                    builder.AppendLine($"///   {summary}");
                    builder.AppendLine("/// </summary>");
                }

                var parametersStr = functionParameters.Select(p => GetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b);
                builder.AppendLine($"public static string {key}({parametersStr})");
            }
            builder.AppendLine("{");
            builder.AddLevel();

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
                        case LiteralStringFormatTagParameter constStringParameter:
                            return $"\"{constStringParameter.Value}\"";
                        case StringRefFormatTagParameter localizationStringParameter:
                            return localizationStringParameter.Id;
                        default:
                            //should not happen
                            return "";
                    }
                }).Aggregate((a, b) => a + ", " + b);
                builder.AppendLine($"return string.Format({localizationStr}, {formatParameters});");
            }
            else
            {
                builder.AppendLine($"return {localizationStr};");
            }

            if (isProperty)
            {
                builder.RemoveLevel();
                builder.AppendLine("}");
            }
            builder.RemoveLevel();
            builder.AppendLine("}");

        }

        protected override void CreateMarkupExtension(CodeStringBuilder builder, string resourceFileName, string className, IEnumerable<string> keys)
        {
            builder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{Constants.ReswPlusName}\", \"{Constants.ReswPlusExtensionVersion}\")]");
            builder.AppendLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
            builder.AppendLine("[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
            builder.AppendLine("[MarkupExtensionReturnType(ReturnType = typeof(string))]");
            builder.AppendLine($"public class {className}: MarkupExtension");
            builder.AppendLine("{");
            builder.AddLevel();
            builder.AppendLine("public enum KeyEnum");
            builder.AppendLine("{");
            builder.AddLevel();
            builder.AppendLine("__Undefined = 0,");
            foreach (var key in keys)
            {
                builder.AppendLine($"{key},");
            }
            builder.RemoveLevel();
            builder.AppendLine("}");
            builder.AppendEmptyLine();
            builder.AppendLine("private static ResourceLoader _resourceLoader;");
            builder.AppendLine($"static {className}()");
            builder.AppendLine("{");
            builder.AddLevel();
            builder.AppendLine($"_resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFileName}\");");
            builder.RemoveLevel();
            builder.AppendLine("}");
            builder.AppendLine("public KeyEnum Key { get; set;}");
            builder.AppendLine("public IValueConverter Converter { get; set;}");
            builder.AppendLine("public object ConverterParameter { get; set;}");
            builder.AppendLine("protected override object ProvideValue()");
            builder.AppendLine("{");
            builder.AddLevel();
            builder.AppendLine("string res;");
            builder.AppendLine("if(Key == KeyEnum.__Undefined)");
            builder.AppendLine("{");
            builder.AddLevel();
            builder.AppendLine("res = \"\";");
            builder.RemoveLevel();
            builder.AppendLine("}");
            builder.AppendLine("else");
            builder.AppendLine("{");
            builder.AddLevel();
            builder.AppendLine("res = _resourceLoader.GetString(Key.ToString());");
            builder.RemoveLevel();
            builder.AppendLine("}");
            builder.AppendLine("return Converter == null ? res : Converter.Convert(res, typeof(String), ConverterParameter, null);");
            builder.RemoveLevel();
            builder.AppendLine("}");
            builder.RemoveLevel();
            builder.AppendLine("}");
        }

        protected override void AddNewLine(CodeStringBuilder builder)
        {
            builder.AppendEmptyLine();
        }
    }
}
