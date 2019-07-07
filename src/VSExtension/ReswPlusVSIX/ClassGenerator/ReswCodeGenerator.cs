// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using EnvDTE;
using ReswPlus.ClassGenerator.Models;
using ReswPlus.Languages;
using ReswPlus.Resw;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReswPlus.CodeGenerator
{

    internal class ReswCodeGenerator
    {
        private const string TagIgnore = "#ReswPlusIgnore";
        private const string Deprecated_TagStrongType = "#ReswPlusTyped";
        private const string TagFormat = "#ReswPlusFormat";
        private const string TagFormatDotNet = "#ReswPlusFormatNet";

        private static readonly Regex _regexStringFormat;
        private static readonly Regex _regexRemoveSpace = new Regex("\\s");
        private static readonly Regex _regexDotNetFormatting = new Regex(@"(?<!{){\d+(,-?\d+)?(:[^}]+)?}");
        private readonly ProjectItem _projectItem;
        private readonly ICodeGenerator _codeGenerator;

        static ReswCodeGenerator()
        {
            var listFormats = "(?:" + ReswTagTyped.GetParameterSymbols().Aggregate((a, b) => a + "|" + b) + ")";
            var listFormatsWithName = listFormats + "(?:\\(\\w+\\))?";
            _regexStringFormat =
                new Regex(
                    $"(?<tag>{Deprecated_TagStrongType}|{TagFormat}|{TagFormatDotNet})\\[\\s*(?<formats>{listFormatsWithName}(?:\\s*,\\s*{listFormatsWithName}\\s*)*)\\]");
        }

        private ReswCodeGenerator(ProjectItem item, ICodeGenerator generator)
        {
            _projectItem = item;
            _codeGenerator = generator;
        }

        public static ReswCodeGenerator CreateGenerator(ProjectItem item, Utils.Language language)
        {
            ICodeGenerator codeGenerator = null;
            switch (language)
            {
                case Utils.Language.CSHARP:
                    codeGenerator = new CSharpCodeGenerator();
                    break;
                case Utils.Language.VB:
                    codeGenerator = new VBCodeGenerator();
                    break;
                case Utils.Language.CPPCX:
                    codeGenerator = new CppCXCodeGenerator();
                    break;
                case Utils.Language.CPPWINRT:
                    codeGenerator = new CppWinRTCodeGenerator();
                    break;
            }
            if (codeGenerator != null)
            {
                return new ReswCodeGenerator(item, codeGenerator);
            }
            return null;
        }


        private StronglyTypedClass Parse(string resourcePath, string content, string defaultNamespace, bool isAdvanced)
        {
            var namespaceToUse = ExtractNamespace(defaultNamespace);

            var filename = Path.GetFileNameWithoutExtension(resourcePath);
            var reswInfo = ReswParser.Parse(content);

            var projectNameIfLibrary = GetProjectNameIfLibrary(resourcePath);

            //If the resource file is in a library, the resource id in the .pri file
            //will be <library name>/FilenameWithoutExtension
            var resouceNameForResourceLoader = string.IsNullOrEmpty(projectNameIfLibrary) ?
                filename : projectNameIfLibrary + "/" + filename;


            var result = new StronglyTypedClass()
            {
                SupportPluralization = isAdvanced,
                ClassName = filename,
                Namespaces = namespaceToUse,
                ResoureFile = resouceNameForResourceLoader
            };

            var stringItems = reswInfo.Items
                .Where(i => !i.Key.Contains(".") && !(i.Comment?.Contains(TagIgnore) ?? false)).ToArray();

            if (isAdvanced)
            {
                //check Pluralization
                var itemsVariantedWithPluralOrVariant = reswInfo.Items.VariantWithPluralAndVariant();
                foreach (var item in itemsVariantedWithPluralOrVariant)
                {
                    if (item.SupportPlural)
                    {
                        var idNone = item.Key + "_None";
                        var hasNoneForm = reswInfo.Items.Any(i => i.Key == idNone);

                        var singleLineValue = _regexRemoveSpace.Replace(item.Items.FirstOrDefault().Value, " ").Trim();

                        var summary = $"Get the pluralized version of the string similar to: {singleLineValue}";

                        PluralLocalization localization;
                        if (item.SupportVariants)
                        {
                            localization = new PluralVariantLocalization()
                            {
                                Key = item.Key,
                                TemplateAccessorSummary = summary,
                                SupportNoneState = hasNoneForm,
                            };
                        }
                        else
                        {
                            localization = new PluralLocalization()
                            {
                                Key = item.Key,
                                TemplateAccessorSummary = summary,
                                SupportNoneState = hasNoneForm,
                            };
                        }
                        var commentToUse =
                            item.Items.FirstOrDefault(i => i.Comment != null && _regexStringFormat.IsMatch(i.Comment));
                        if (commentToUse != null)
                        {
                            ManageFormattedFunction(localization, item.Key, item.Items.FirstOrDefault().Value, commentToUse.Comment);
                        }

                        result.Localizations.Add(localization);
                    }
                    else if (item.SupportVariants)
                    {
                        var singleLineValue = _regexRemoveSpace.Replace(item.Items.FirstOrDefault().Key, " ").Trim();
                        var summary = $"Get the variant version of the string similar to: {singleLineValue}";
                        var commentToUse = item.Items.FirstOrDefault(i => i.Comment != null && _regexStringFormat.IsMatch(i.Comment));

                        var localization = new VariantLocalization()
                        {
                            Key = item.Key,
                            TemplateAccessorSummary = summary,
                        };

                        if (!string.IsNullOrEmpty(commentToUse?.Comment))
                        {
                            ManageFormattedFunction(localization, item.Key, commentToUse.Value, commentToUse.Comment);
                        }

                        result.Localizations.Add(localization);
                    }
                }

                stringItems = stringItems.Except(itemsVariantedWithPluralOrVariant.SelectMany(e => e.Items)).ToArray();
            }

            if (stringItems.Any())
            {
                if (stringItems.Any())
                {
                    foreach (var item in stringItems)
                    {
                        var isFormattedFunction = ParseTag(item.Comment).format != null;
                        var singleLineValue = _regexRemoveSpace.Replace(item.Value, " ").Trim();
                        var summary = $"Looks up a localized string similar to: {singleLineValue}";

                        var localization = new Localization()
                        {
                            Key = item.Key,
                            AccessorSummary = summary,
                            IsDotNetFormatting = IsDotNetFormatting(item.Value)
                        };

                        if (isFormattedFunction)
                        {
                            ManageFormattedFunction(localization, item.Key, item.Value, item.Comment);
                        }

                        result.Localizations.Add(localization);
                    }
                }
            }

            return result;
        }

        private bool IsDotNetFormatting(string source)
        {
            return _regexDotNetFormatting.IsMatch(source);
        }

        public IEnumerable<GeneratedFile> GenerateCode(string resourcePath, string baseFilename, string content, string defaultNamespace, bool supportPluralization, ProjectItem projectItem)
        {
            var stronglyTypedClass = Parse(resourcePath, content, defaultNamespace, supportPluralization);
            if (stronglyTypedClass == null)
            {
                return null;
            }

            return _codeGenerator.GetGeneratedFiles(baseFilename, stronglyTypedClass, projectItem);
        }

        private string[] ExtractNamespace(string defaultNamespace)
        {
            if (string.IsNullOrEmpty(defaultNamespace))
            {
                return new string[0];
            }

            // remove bcp47 tag from the namespace
            var regexNamespace =
                new Regex("\\.Strings\\.[a-z]{2}(?:[-_](?:Latn|Cyrl|Hant|Hans))?(?:[-_](?:\\d{3}|[A-Z]{2,3}))?$");
            var match = regexNamespace.Match(defaultNamespace);
            if (match.Success)
            {
                return defaultNamespace.Substring(0, match.Index + 8).Split('.');
            }

            return defaultNamespace.Split('.');
        }

        private (string format, bool isDotNetFormatting) ParseTag(string comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {

                var match = _regexStringFormat.Match(comment);
                if (match.Success)
                {
                    return (match.Groups["formats"].Value, match.Groups["tag"].Value == TagFormatDotNet);
                }
            }
            return (null, false);
        }

        private bool ManageFormattedFunction(LocalizationBase localization, string key, string exampleValue, string comment)
        {
            var (format, isDotNetFormatting) = ParseTag(comment);
            if (format == null)
            {
                return false;
            }
            localization.IsDotNetFormatting = isDotNetFormatting;
            var singleLineValue = _regexRemoveSpace.Replace(exampleValue, " ").Trim();
            var types = format.Split(',');
            var tagTypedInfo = ReswTagTyped.ParseParameters(types);

            var summary = $"Format the string similar to: {singleLineValue}";
            localization.FormatSummary = summary;
            localization.Parameters = tagTypedInfo.Parameters;

            if (localization is IVariantLocalization variantLocalization)
            {
                FunctionParameter variantParameter = null;
                // Add an extra parameter for variant if necessary
                if (tagTypedInfo.VariantParameter == null)
                {
                    variantParameter = new FunctionParameter
                    { Type = ParameterType.Int, Name = "variantId" };
                    localization.ExtraParameters.Add(variantParameter);
                }
                else
                {
                    variantParameter = tagTypedInfo.VariantParameter;
                }

                variantLocalization.ParameterToUseForVariant = variantParameter;

            }

            if (localization is PluralLocalization pluralLocalization)
            {
                FunctionParameter pluralizationQuantifier = null;
                // Add an extra parameter for pluralization if necessary
                if (tagTypedInfo.PluralizationParameter == null)
                {
                    pluralizationQuantifier = new FunctionParameter
                    { Type = ParameterType.Double, Name = "pluralizationReferenceNumber" };
                    pluralLocalization.ExtraParameters.Add(pluralizationQuantifier);
                }
                else
                {
                    pluralizationQuantifier = tagTypedInfo.PluralizationParameter;
                }

                pluralLocalization.ParameterToUseForPluralization = pluralizationQuantifier;
            }

            return true;
        }

        private string GetProjectNameIfLibrary(string filepath)
        {
            var project = _projectItem?.ContainingProject;
            if (project != null)
            {
                try
                {
                    var isLibrary = (int)project.Properties.Item("OutputTypeEx").Value == 2;
                    if (isLibrary)
                    {
                        return project.Name;
                    }
                }
                catch { }
            }

            return null;
        }
    }
}
