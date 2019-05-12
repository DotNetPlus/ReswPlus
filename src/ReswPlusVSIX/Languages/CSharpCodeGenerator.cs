using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReswPlus.Languages
{
    internal class CSharpCodeGenerator : ICodeGenerator
    {
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

        public string GetHeaders(bool supportPluralNet)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralNet)
            {
                stringBuilder.AppendLine("// The NuGet package PluralNet is necessary to support Pluralization.");
            }
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using Windows.ApplicationModel.Resources;");
            stringBuilder.AppendLine("using Windows.UI.Xaml.Markup;");
            stringBuilder.Append("using Windows.UI.Xaml.Data;");
            return stringBuilder.ToString();
        }

        public string OpenNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return "";
            }
            else
            {
                return $"namespace {namespaceName} {{";
            }
        }

        public string CloseNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return "";
            }
            else
            {
                return "} //" + namespaceName;
            }
        }

        public string OpenStronglyTypedClass(string resourceFilename, string className)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")]");
            stringBuilder.AppendLine("    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
            stringBuilder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
            stringBuilder.AppendLine($"    public class {className} {{");
            stringBuilder.AppendLine("        private static ResourceLoader _resourceLoader;");

            stringBuilder.AppendLine($"        static {className}()");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine($"            _resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFilename}\");");
            stringBuilder.Append("        }");
            return stringBuilder.ToString();
        }

        public string CloseStronglyTypedClass()
        {
            return "    }";
        }

        public string OpenRegion(string name)
        {
            return $"        #region {name}";
        }

        public string CloseRegion()
        {
            return "        #endregion";
        }

        public string CreatePluralNetAccessor(string pluralKey, string summary, string idNone = null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine($"        ///   {summary}");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.AppendLine($"        public static string {pluralKey}(double number)");
            stringBuilder.AppendLine("        {");
            if (!string.IsNullOrEmpty(idNone))
            {
                stringBuilder.AppendLine("            if(number == 0)");
                stringBuilder.AppendLine("            {");
                stringBuilder.AppendLine($"                return _resourceLoader.GetString(\"{idNone}\");");
                stringBuilder.AppendLine("            }");
            }

            stringBuilder.AppendLine($"            return Huyn.PluralNet.ResourceLoaderExtension.GetPlural(_resourceLoader, \"{pluralKey}\", (decimal)number);");
            stringBuilder.Append("        }");
            return stringBuilder.ToString();
        }

        public string CreateAccessor(string key, string summary)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine($"        ///   {summary}");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.Append(
                $"        public static string {key} => _resourceLoader.GetString(\"{key}\");");

            return stringBuilder.ToString();
        }

        public string CreateFormatMethod(string key, IEnumerable<FunctionParameter> parameters, string summary = null, FunctionParameter extraParameterForFunction = null, string parameterNameForPluralNet = null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine($"        ///   {summary}");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.Append($"        public static string {key}_Format(");

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
            stringBuilder.Append(functionParameters.Select(p => GetParameterTypeString(p.Type) + " " + p.Name).Aggregate((a, b) => a + ", " + b));
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

            stringBuilder.AppendLine(")");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine($"            return string.Format({sourceForFormat}, {formatParameters});");
            stringBuilder.Append("        }");

            return stringBuilder.ToString();
        }

        public string CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")]");
            stringBuilder.AppendLine("    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
            stringBuilder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
            stringBuilder.AppendLine("    [MarkupExtensionReturnType(ReturnType = typeof(string))]");
            stringBuilder.AppendLine($"    public class {className}: MarkupExtension");
            stringBuilder.AppendLine("    {");
            stringBuilder.AppendLine("        public enum KeyEnum");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            __Undefined = 0,");
            foreach (var key in keys)
            {
                stringBuilder.AppendLine($"            {key},");
            }
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("        private static ResourceLoader _resourceLoader;");
            stringBuilder.AppendLine($"        static {className}()");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine($"            _resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFileName}\");");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine("        public KeyEnum Key { get; set;}");
            stringBuilder.AppendLine("        public IValueConverter Converter { get; set;}");
            stringBuilder.AppendLine("        public object ConverterParameter { get; set;}");
            stringBuilder.AppendLine("        protected override object ProvideValue()");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            string res;");
            stringBuilder.AppendLine("            if(Key == KeyEnum.__Undefined)");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                res = \"\";");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine("            else");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                res = _resourceLoader.GetString(Key.ToString());");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine("            return Converter == null ? res : Converter.Convert(res, typeof(String), ConverterParameter, null);");
            stringBuilder.AppendLine("        }");
            stringBuilder.Append("    }");
            return stringBuilder.ToString();
        }
    }
}
