// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ReswPlus.ReswGen
{
    internal class FunctionParameter
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }

    internal class FunctionParametersInfo
    {
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();
        public FunctionParameter PluralNetDecimal { get; set; }
    }

    internal class ReswCodeGenerator
    {
        private const string TagIgnore = "#ReswPlusIgnore";
        private const string TagStrongType = "#ReswPlusTyped";

        private static readonly Dictionary<string, string> AcceptedTypes = new Dictionary<string, string>
        {
          {"o", "object"},
          {"b", "byte"},
          {"d", "int"},
          {"u", "uint"},
          {"l", "long"},
          {"s", "string"},
          {"f", "double"},
          {"c", "char"},
          {"ul", "ulong"},
          {"m", "decimal"},
          {"Q", "double"} //reserved by PluralNet
        };
        private static readonly Regex RegexStringFormat;
        private static readonly Regex RegexNamedParameters = new Regex("(?<type>\\w+)(?:\\((?<name>\\w+)\\))?");
        private static readonly Regex RegexRemoveSpace = new Regex("\\s");

        static ReswCodeGenerator()
        {
            var listFormats = "(?:" + (from t in AcceptedTypes select t.Key).Aggregate((a, b) => a + "|" + b) + ")";
            var listFormatsWithName = listFormats + "(?:\\(\\w+\\))?";
            RegexStringFormat = new Regex($"\\{TagStrongType}\\[\\s*(?<formats>{listFormatsWithName}(?:\\s*,\\s*{listFormatsWithName}\\s*)*)\\]");
        }

        public static string GenerateCode(string resourcePath, string content, string defaultNamespace, bool supportPluralNet)
        {
            var namespaceToUse = ExtractNamespace(defaultNamespace);

            var filename = Path.GetFileNameWithoutExtension(resourcePath);
            var reswInfo = CreateResW(content);

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralNet)
            {
                stringBuilder.AppendLine("// The NuGet package PluralNet is necessary to support Pluralization.");
            }
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using Windows.ApplicationModel.Resources;");
            stringBuilder.AppendLine("using Windows.UI.Xaml.Markup;");
            stringBuilder.AppendLine("using Windows.UI.Xaml.Data;");

            stringBuilder.AppendLine("");
            stringBuilder.AppendLine($"namespace {namespaceToUse} {{");
            stringBuilder.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")]");
            stringBuilder.AppendLine("    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
            stringBuilder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
            stringBuilder.AppendLine($"    public class {filename} {{");
            stringBuilder.AppendLine("        private static ResourceLoader  _resourceLoader;");

            stringBuilder.AppendLine($"        static {filename}()");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            _resourceLoader = ResourceLoader.GetForViewIndependentUse();");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine("");

            var stringItems = reswInfo.Items.Where(i => !i.Key.Contains(".") && !(i.Comment?.ToLower()?.Contains(TagIgnore) ?? false)).ToArray();

            if (supportPluralNet)
            {
                //check Pluralization
                const string regexPluralItem = "_(Zero|One|Other|Many|Few|None)$";
                var itemsWithPlural = reswInfo.Items.Where(c => c.Key.Contains("_") && Regex.IsMatch(c.Key, regexPluralItem)).ToArray();

                foreach (var item in (from item in itemsWithPlural group item by item.Key.Substring(0, item.Key.LastIndexOf('_'))))
                {
                    var idNone = item.Key + "_None";
                    var hasNoneForm = reswInfo.Items.Any(i => i.Key == idNone);

                    var singleLineValue = RegexRemoveSpace.Replace(item.FirstOrDefault().Value, " ").Trim();

                    stringBuilder.AppendLine($"        #region {item.Key}");
                    stringBuilder.AppendLine("        /// <summary>");
                    stringBuilder.AppendLine($"        ///   Get the pluralized version of the string similar to: {singleLineValue}");
                    stringBuilder.AppendLine("        /// </summary>");
                    stringBuilder.AppendLine($"        public static string {item.Key}(double number)");
                    stringBuilder.AppendLine("        {");
                    if (hasNoneForm)
                    {
                        stringBuilder.AppendLine("            if(number == 0)");
                        stringBuilder.AppendLine("            {");
                        stringBuilder.AppendLine($"                return _resourceLoader.GetString(\"{idNone}\");");
                        stringBuilder.AppendLine("            }");
                    }

                    stringBuilder.AppendLine($"            return Huyn.PluralNet.ResourceLoaderExtension.GetPlural(_resourceLoader, \"{item.Key}\", (decimal)number);");
                    stringBuilder.AppendLine("        }");

                    var commentToUse = item.FirstOrDefault(i => i.Comment != null && RegexStringFormat.IsMatch(i.Comment));
                    if (commentToUse != null)
                    {
                        var formattedFunction = GetFormattedFunction(item.Key, item.FirstOrDefault().Value, commentToUse.Comment, true);
                        if (formattedFunction != null)
                        {
                            stringBuilder.Append(formattedFunction);
                        }
                    }

                    stringBuilder.AppendLine("        #endregion");
                    stringBuilder.AppendLine("");
                }

                stringItems = stringItems.Except(itemsWithPlural).ToArray();
            }

            if (stringItems.Any())
            {
                foreach (var item in stringItems)
                {
                    var formattedFunction = GetFormattedFunction(item.Key, item.Value, item.Comment, false);
                    if (formattedFunction != null)
                    {
                        stringBuilder.AppendLine("        #region " + item.Key);
                    }

                    var singleLineValue = RegexRemoveSpace.Replace(item.Value, " ").Trim();
                    stringBuilder.AppendLine("        /// <summary>");
                    stringBuilder.AppendLine($"        ///   Looks up a localized string similar to: {singleLineValue}");
                    stringBuilder.AppendLine("        /// </summary>");
                    stringBuilder.AppendLine(
                        $"        public static string {item.Key} => _resourceLoader.GetString(\"{item.Key}\");");
                    if (formattedFunction != null)
                    {
                        stringBuilder.Append(formattedFunction);
                        stringBuilder.AppendLine("        #endregion");
                    }

                    stringBuilder.AppendLine("");
                }

                stringBuilder.AppendLine("    }");
                stringBuilder.AppendLine("");
                GenerateMarkupExtension(filename, stringBuilder, stringItems);
                stringBuilder.AppendLine("");
            }

            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("");
            return stringBuilder.ToString();
        }

        private static void GenerateMarkupExtension(string filename, StringBuilder stringBuilder, IEnumerable<ReswItem> items)
        {
            var classname = filename + "Extension";
            stringBuilder.AppendLine("    public enum ReswPlusKeyExtension");
            stringBuilder.AppendLine("    {");
            foreach (var item in items)
            {
                stringBuilder.AppendLine($"        {item.Key},");
            }
            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")]");
            stringBuilder.AppendLine("    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]");
            stringBuilder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]");
            stringBuilder.AppendLine("    [MarkupExtensionReturnType(ReturnType = typeof(string))]");
            stringBuilder.AppendLine($"    public class {classname}: MarkupExtension");
            stringBuilder.AppendLine("    {");
            stringBuilder.AppendLine("        private static ResourceLoader  _resourceLoader;");
            stringBuilder.AppendLine($"        static {classname}()");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            _resourceLoader = ResourceLoader.GetForViewIndependentUse();");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine("        public ReswPlusKeyExtension? Key { get; set;}");
            stringBuilder.AppendLine("        public IValueConverter Converter { get; set;}");
            stringBuilder.AppendLine("        public object ConverterParameter { get; set;}");
            stringBuilder.AppendLine("        protected override object ProvideValue()");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            string res;");
            stringBuilder.AppendLine("            if(!Key.HasValue)");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                res = \"\";");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine("            else");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                res = _resourceLoader.GetString(Key.Value.ToString());");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine("            return Converter == null ? res : Converter.Convert(res, typeof(String), ConverterParameter, null);");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine("    }");

        }

        private static string ExtractNamespace(string defaultNamespace)
        {
            // remove bcp47 tag from the namespace
            var regexNamespace = new Regex("\\.Strings\\.[a-z]{2}(?:[-_](?:Latn|Cyrl|Hant|Hans))?(?:[-_](?:\\d{3}|[A-Z]{2,3}))?$");
            var match = regexNamespace.Match(defaultNamespace);
            if (match.Success)
            {
                return defaultNamespace.Substring(0, match.Index + 8);
            }

            return defaultNamespace;
        }

        private static string GetFormattedFunction(string key, string exampleValue, string comment, bool isPluralNet)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return null;
            }
            var matches = RegexStringFormat.Match(comment);
            if (!matches.Success)
            {
                return null;
            }

            var stringBuilder = new StringBuilder();
            var types = matches.Groups["formats"].Value.Split(',');
            var singleLineValue = RegexRemoveSpace.Replace(exampleValue, " ").Trim();

            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine($"        ///   Format the string similar to: {singleLineValue}");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.Append($"        public static string {key}_Format(");
            var parametersInfo = ParseParameters(types);


            var functionParameters = new List<FunctionParameter>();
            string referenceString;
            if (isPluralNet)
            {
                string pluralNetParameterName;
                if (parametersInfo.PluralNetDecimal == null)
                {
                    pluralNetParameterName = "pluralNetReferenceNumber";
                    functionParameters.Add(new FunctionParameter { Type = "double", Name = pluralNetParameterName });
                }
                else
                {
                    pluralNetParameterName = parametersInfo.PluralNetDecimal.Name;
                }
                referenceString = key + "(" + pluralNetParameterName + ")";
            }
            else
            {
                referenceString = key;
            }

            functionParameters.AddRange(parametersInfo.Parameters);
            stringBuilder.Append(functionParameters.Select(p => p.Type + " " + p.Name).Aggregate((a, b) => a + ", " + b));
            var formatParameters = parametersInfo.Parameters.Select(p => p.Name).Aggregate((a, b) => a + ", " + b);

            stringBuilder.AppendLine(")");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine($"            return string.Format({referenceString}, {formatParameters});");
            stringBuilder.AppendLine("        }");

            return stringBuilder.ToString();
        }

        private static FunctionParametersInfo ParseParameters(IEnumerable<string> types)
        {
            var result = new FunctionParametersInfo();
            var paramIndex = 1;
            foreach (var type in types)
            {
                var matchNamedParameters = RegexNamedParameters.Match(type);
                if (!matchNamedParameters.Success)
                {
                    continue;
                }

                var trimmedType = matchNamedParameters.Groups["type"].Value;
                var paramName = matchNamedParameters.Groups["name"].Value;
                var paramType = AcceptedTypes[trimmedType];
                if (string.IsNullOrEmpty(paramName))
                {
                    if (trimmedType == "Q")
                    {
                        paramName = "pluralCount";
                    }
                    else
                    {
                        paramName = "param" + paramType.Substring(0, 1).ToUpper() + paramType.Substring(1) + paramIndex;
                    }
                }

                var functionParam = new FunctionParameter { Type = paramType, Name = paramName };
                if (trimmedType == "Q" && result.PluralNetDecimal == null)
                {
                    result.PluralNetDecimal = functionParam;
                }
                result.Parameters.Add(functionParam);
                ++paramIndex;
            }
            return result;
        }

        private static ReswInfo CreateResW(string content)
        {
            var res = new ReswInfo
            {
                Items = new List<ReswItem>()
            };

            var xml = new XmlDocument();
            xml.LoadXml(content);

            var nodes = xml.DocumentElement?.SelectNodes("//data");
            if (nodes == null)
            {
                return res;
            }

            foreach (XmlElement element in nodes)
            {
                string key = null, value = null, comment = null;
                var elementKey = element.Attributes.GetNamedItem("name");
                if (elementKey != null)
                {
                    key = elementKey.Value ?? string.Empty;
                }
                else
                {
                    continue;
                }
                var elementValue = element.SelectSingleNode("value");
                if (elementValue != null)
                {
                    value = elementValue.InnerText;
                }
                else
                {
                    continue;
                }

                var elementComment = element.SelectSingleNode("comment");
                if (elementComment != null)
                {
                    comment = elementComment.InnerText;
                }

                res.Items.Add(new ReswItem(key, value, comment));
            }
            return res;
        }
    }
}
