using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal class CSharpCodeGenerator : ICodeGenerator
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

        public void NewLine()
        {
            _builder.AppendEmptyLine();
        }

        public string GetString()
        {
            return _builder.GetString();
        }

        public void GetHeaders(bool supportPluralNet)
        {
            _builder.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralNet)
            {
                _builder.AppendLine("// The NuGet package PluralNet is necessary to support Pluralization.");
            }
            _builder.AppendLine("using System;");
            _builder.AppendLine("using Windows.ApplicationModel.Resources;");
            _builder.AppendLine("using Windows.UI.Xaml.Markup;");
            _builder.AppendLine("using Windows.UI.Xaml.Data;");
        }

        public void OpenNamespace(string namespaceName)
        {
            if (!string.IsNullOrEmpty(namespaceName))
            {
                _builder.AppendLine($"namespace {namespaceName}{{");
                _builder.AddLevel();
            }
        }

        public void CloseNamespace(string namespaceName)
        {
            if (!string.IsNullOrEmpty(namespaceName))
            {
                _builder.RemoveLevel();
                _builder.AppendLine($"}} //{namespaceName}");
            }
        }

        public void OpenStronglyTypedClass(string resourceFilename, string className)
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

        public void CloseStronglyTypedClass()
        {
            _builder.RemoveLevel();
            _builder.AppendLine("}");
        }

        public void OpenRegion(string name)
        {
            _builder.AppendLine($"#region {name}");
        }

        public void CloseRegion()
        {
            _builder.AppendLine("#endregion");
        }

        public void CreatePluralNetAccessor(string pluralKey, string summary, string idNone = null)
        {

            _builder.AppendLine("/// <summary>");
            _builder.AppendLine($"///   {summary}");
            _builder.AppendLine("/// </summary>");
            _builder.AppendLine($"public static string {pluralKey}(double number)");
            _builder.AppendLine("{");
            _builder.AddLevel();
            if (!string.IsNullOrEmpty(idNone))
            {
                _builder.AppendLine("if(number == 0)");
                _builder.AppendLine("{");
                _builder.AddLevel();
                _builder.AppendLine($"return _resourceLoader.GetString(\"{idNone}\");");
                _builder.RemoveLevel();
                _builder.AppendLine("}");
            }

            _builder.AppendLine($"return Huyn.PluralNet.ResourceLoaderExtension.GetPlural(_resourceLoader, \"{pluralKey}\", (decimal)number);");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
        }

        public void CreateAccessor(string key, string summary)
        {

            _builder.AppendLine("/// <summary>");
            _builder.AppendLine($"///   {summary}");
            _builder.AppendLine("/// </summary>");
            _builder.AppendLine($"public static string {key} => _resourceLoader.GetString(\"{key}\");");
        }

        public void CreateFormatMethod(string key, IEnumerable<FunctionParameter> parameters, string summary = null, FunctionParameter extraParameterForFunction = null, string parameterNameForPluralNet = null)
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
            _builder.AppendLine($"public static string {key}_Format({parametersStr})");
            _builder.AppendLine("{");
            _builder.AddLevel();
            var formatParameters = parameters.Select(p => p.Name).Aggregate((a, b) => a + ", " + b);

            string sourceForFormat;
            if (!string.IsNullOrEmpty(parameterNameForPluralNet))
            {
                sourceForFormat = key + "(" + parameterNameForPluralNet + ")";
            }
            else
            {
                sourceForFormat = key;
            }

            _builder.AppendLine($"return string.Format({sourceForFormat}, {formatParameters});");
            _builder.RemoveLevel();
            _builder.AppendLine("}");
        }

        public void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys)
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
    }
}
