// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ReswPlus.Languages;

namespace ReswPlus.Resw
{

    internal class ReswCodeGenerator
    {
        private const string TagIgnore = "#ReswPlusIgnore";
        private const string TagStrongType = "#ReswPlusTyped";

        private static readonly Regex RegexStringFormat;
        private static readonly Regex RegexRemoveSpace = new Regex("\\s");
        private readonly ICodeGenerator _codeGenerator;

        static ReswCodeGenerator()
        {
            var listFormats = "(?:" + ReswTagTyped.GetParameterSymbols().Aggregate((a, b) => a + "|" + b) + ")";
            var listFormatsWithName = listFormats + "(?:\\(\\w+\\))?";
            RegexStringFormat = new Regex($"\\{TagStrongType}\\[\\s*(?<formats>{listFormatsWithName}(?:\\s*,\\s*{listFormatsWithName}\\s*)*)\\]");
        }

        public ReswCodeGenerator(ICodeGenerator codeGenerator)
        {
            this._codeGenerator = codeGenerator;
        }
        public string GenerateCode(string resourcePath, string content, string defaultNamespace, bool supportPluralNet)
        {
            var namespaceToUse = ExtractNamespace(defaultNamespace);

            var filename = Path.GetFileNameWithoutExtension(resourcePath);
            var reswInfo = ReswParser.Parse(content);

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(_codeGenerator.GetHeaders(supportPluralNet));

            stringBuilder.AppendLine("");
            stringBuilder.AppendLine(_codeGenerator.OpenNamespace(namespaceToUse));
            stringBuilder.AppendLine(_codeGenerator.OpenStronglyTypedClass(filename));
            stringBuilder.AppendLine("");

            var stringItems = reswInfo.Items.Where(i => !i.Key.Contains(".") && !(i.Comment?.Contains(TagIgnore) ?? false)).ToArray();

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

                    var summary = $"Get the pluralized version of the string similar to: {singleLineValue}";
                    stringBuilder.AppendLine(_codeGenerator.OpenRegion(item.Key));
                    stringBuilder.AppendLine(
                        _codeGenerator.CreatePluralNetAccessor(item.Key, summary, hasNoneForm ? idNone : null));
              
                    var commentToUse = item.FirstOrDefault(i => i.Comment != null && RegexStringFormat.IsMatch(i.Comment));
                    if (commentToUse != null)
                    {
                        var formattedFunction = GetFormattedFunction(item.Key, item.FirstOrDefault().Value, commentToUse.Comment, true);
                        if (formattedFunction != null)
                        {
                            stringBuilder.AppendLine(formattedFunction);
                            stringBuilder.AppendLine("");
                        }
                    }

                    stringBuilder.AppendLine(_codeGenerator.CloseRegion());
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
                        stringBuilder.AppendLine(_codeGenerator.OpenRegion(item.Key));
                        stringBuilder.AppendLine("");
                    }

                    var singleLineValue = RegexRemoveSpace.Replace(item.Value, " ").Trim();
                    var summary = $"Looks up a localized string similar to: {singleLineValue}";
                    stringBuilder.AppendLine(_codeGenerator.CreateAccessor(item.Key, summary));
   
                    if (formattedFunction != null)
                    {
                        stringBuilder.AppendLine(formattedFunction);
                        stringBuilder.AppendLine(_codeGenerator.CloseRegion());
                    }

                    stringBuilder.AppendLine("");
                }

                stringBuilder.AppendLine(_codeGenerator.CloseStronglyTypedClass());
                stringBuilder.AppendLine("");
                var markupExtensionStr = _codeGenerator.CreateMarkupExtension(filename + "Extension", stringItems.Select(s=>s.Key));
                if (!string.IsNullOrEmpty(markupExtensionStr))
                {
                    stringBuilder.AppendLine(markupExtensionStr);
                    stringBuilder.AppendLine("");
                }
            }
            else
            {
                stringBuilder.AppendLine(_codeGenerator.CloseStronglyTypedClass());
            }
            stringBuilder.AppendLine(_codeGenerator.CloseNamespace());
            stringBuilder.AppendLine("");
            return stringBuilder.ToString();
        }


        private string ExtractNamespace(string defaultNamespace)
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

        private string GetFormattedFunction(string key, string exampleValue, string comment, bool isPluralNet)
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

            var singleLineValue = RegexRemoveSpace.Replace(exampleValue, " ").Trim();
            var types = matches.Groups["formats"].Value.Split(',');
            var tagTypedInfo = ReswTagTyped.ParseParameters(types);


            FunctionParameter extraParameterForPluralNet = null;
            string pluralNetParameterName = null;
            if (isPluralNet)
            {
                // Add an extra parameter for pluralization if necessary
                if (tagTypedInfo.PluralNetDecimal == null)
                {
                    pluralNetParameterName = "pluralNetReferenceNumber";
                    extraParameterForPluralNet = new FunctionParameter { Type = ParameterType.Double, Name = pluralNetParameterName };
                }
                else
                {
                    pluralNetParameterName = tagTypedInfo.PluralNetDecimal.Name;
                }
            }

            var summary = $"Format the string similar to: {singleLineValue}";

            return _codeGenerator.CreateFormatMethod(key, tagTypedInfo.Parameters, summary, extraParameterForPluralNet, pluralNetParameterName);
        }
    }
}
