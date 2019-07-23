using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal class CSharpCodeGenerator : DotNetGeneratorBase
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

        internal override IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename)
        {
            yield return new GeneratedFile() { Filename = baseFilename + ".cs", Content = _builder.GetString() };
        }

        internal override void GenerateHeaders(bool supportPluralization)
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

        internal override void OpenNamespace(IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                _builder.AppendLine($"namespace {namespaces.Aggregate((a, b) => a + "." + b)}{{");
                _builder.AddLevel();
            }
        }

        internal override void CloseNamespace(IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                _builder.RemoveLevel();
                _builder.AppendLine($"}} //{namespaces.Aggregate((a, b) => a + "." + b)}");
            }
        }

        internal override void OpenStronglyTypedClass(string resourceFilename, string className)
        {

            _builder.AppendLine("[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")]");
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

        internal override void CloseStronglyTypedClass()
        {
            _builder.RemoveLevel();
            _builder.AppendLine("}");
        }

        internal override void OpenRegion(string name)
        {
            _builder.AppendLine($"#region {name}");
        }

        internal override void CloseRegion()
        {
            _builder.AppendLine("#endregion");
        }

        internal override void CreateTemplateAccessor(string key, string summary, bool supportPlural, bool pluralSupportNoneState, bool supportVariants)
        {
            if (!supportPlural && !supportVariants)
            {
                return;
            }
            var parameters = new List<string>();
            var genericParameters = new List<string>();
            if (supportVariants)
            {
                parameters.Add("long variantId");
                genericParameters.Add("object variantId");
            }
            if (supportPlural)
            {
                parameters.Add("double pluralNumber");
                genericParameters.Add("double pluralNumber");
            }

            _builder.AppendLine("/// <summary>");
            _builder.AppendLine($"///   {summary}");
            _builder.AppendLine("/// </summary>");
            _builder.AppendLine($"public static string {key}({parameters.Aggregate((a, b) => a + ", " + b)})");
            _builder.AppendLine("{");
            _builder.AddLevel();
            if (supportPlural && pluralSupportNoneState)
            {
                _builder.AppendLine("if(pluralNumber == 0)");
                _builder.AppendLine("{");
                _builder.AddLevel();
                var noneKey = supportVariants ? $"\"{key}_Variant\" + variantId + \"_None\"" : $"\"{key}_None\"";
                _builder.AppendLine($"return _resourceLoader.GetString({noneKey});");
                _builder.RemoveLevel();
                _builder.AppendLine("}");
            }

            var stringKey = supportVariants ? $"\"{key}_Variant\" + variantId" : $"\"{key}\"";
            if (supportPlural)
            {
                _builder.AppendLine($"return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, {stringKey}, pluralNumber);");
            }
            else
            {
                _builder.AppendLine($"return _resourceLoader.GetString({stringKey});");
            }
            _builder.RemoveLevel();
            _builder.AppendLine("}");

            if (supportVariants)
            {
                _builder.AppendEmptyLine();
                _builder.AppendLine("/// <summary>");
                _builder.AppendLine($"///   {summary}");
                _builder.AppendLine("/// </summary>");
                _builder.AppendLine($"public static string {key}({genericParameters.Aggregate((a, b) => a + ", " + b)})");
                _builder.AppendLine("{");
                _builder.AddLevel();
                _builder.AppendLine("try");
                _builder.AppendLine("{");
                _builder.AddLevel();
                if (supportPlural)
                {
                    _builder.AppendLine($"return {key}(Convert.ToInt64(variantId), pluralNumber);");
                }
                else
                {
                    _builder.AppendLine($"return {key}(Convert.ToInt64(variantId));");
                }
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
            }
        }

        internal override void CreateAccessor(string key, string summary)
        {

            _builder.AppendLine("/// <summary>");
            _builder.AppendLine($"///   {summary}");
            _builder.AppendLine("/// </summary>");
            _builder.AppendLine($"public static string {key} => _resourceLoader.GetString(\"{key}\");");
        }

        internal override void CreateFormatMethod(string key, IEnumerable<Parameter> parameters, string summary = null, IEnumerable<FunctionParameter> extraParameters = null, FunctionParameter parameterForPluralization = null, FunctionParameter parameterForVariant = null)
        {
            _builder.AppendLine("/// <summary>");
            _builder.AppendLine($"///   {summary}");
            _builder.AppendLine("/// </summary>");

            IEnumerable<FunctionParameter> functionParameters;
            if (extraParameters != null && extraParameters.Any())
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
                functionParameters.Select(p => GetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b)
                : "";
            _builder.AppendLine($"public static string {key}_Format({parametersStr})");
            _builder.AppendLine("{");
            _builder.AddLevel();
            var formatParameters = parameters.Select(p =>
            {
                switch (p)
                {
                    case FunctionParameter functionParam:
                        return functionParam.Name;
                    case MacroParameter macroParam:
                        return $"ReswPlusLib.Macros.{macroParam.Id}";
                    case ConstStringParameter constStringParameter:
                        return $"\"{constStringParameter.Value}\"";
                    case LocalizationRefParameter localizationStringParameter:
                        return localizationStringParameter.Id;
                    default:
                        //should not happen
                        return "";
                }
            }).Aggregate((a, b) => a + ", " + b);

            string sourceForFormat;
            if (parameterForPluralization != null)
            {
                var doubleValue = parameterForPluralization.TypeToCast.HasValue ? $"({GetParameterTypeString(parameterForPluralization.TypeToCast.Value)}){parameterForPluralization.Name}" : parameterForPluralization.Name;
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

            _builder.AppendLine($"return string.Format({sourceForFormat}, {formatParameters});");
            _builder.RemoveLevel();
            _builder.AppendLine("}");

            if (parameters.Any(p => p is FunctionParameter functionParam && functionParam.IsVariantId))
            {
                // one of the parameter is a variantId, we must create a second method with object as the variantId type.
                _builder.AppendEmptyLine();
                _builder.AppendLine("/// <summary>");
                _builder.AppendLine($"///   {summary}");
                _builder.AppendLine("/// </summary>");
                var genericParametersStr = functionParameters.Select(p => (p.IsVariantId ? "object" : GetParameterTypeString(p.Type)) + " " + p.Name).Aggregate((a, b) => a + ", " + b);
                _builder.AppendLine($"public static string {key}_Format({genericParametersStr})");
                _builder.AppendLine("{");
                _builder.AddLevel();
                _builder.AppendLine("try");
                _builder.AppendLine("{");
                _builder.AddLevel();
                _builder.AppendLine($"return {key}_Format({functionParameters.Select(p => p.IsVariantId ? $"Convert.ToInt64({p.Name})" : p.Name).Aggregate((a, b) => a + ", " + b)});");
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
            }
        }

        internal override void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys)
        {
            _builder.AppendLine("[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")]");
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

        internal override void AddNewLine()
        {
            _builder.AppendEmptyLine();
        }
    }
}
