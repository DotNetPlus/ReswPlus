// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using ReswPlus.Core.Interfaces;
using ReswPlus.Core.ClassGenerator.Models;
using ReswPlus.Core.CodeGenerators;
using ReswPlus.Core.ResourceParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReswPlus.Core.ResourceInfo;

namespace ReswPlus.Core.ClassGenerator
{
    public class ReswClassGenerator
    {
        private const string TagIgnore = "#ReswPlusIgnore";
        private const string Deprecated_TagStrongType = "#ReswPlusTyped";
        private const string TagFormat = "#Format";
        private const string TagFormatDotNet = "#FormatNet";

        private static readonly Regex _regexStringFormat;
        private static readonly Regex _regexRemoveSpace = new Regex("\\s+");
        private static readonly Regex _regexDotNetFormatting = new Regex(@"(?<!{){\d+(,-?\d+)?(:[^}]+)?}");
        private readonly ResourceFileInfo _resourceFileInfo;
        private readonly ICodeGenerator _codeGenerator;
        private readonly IErrorLogger _logger;

        static ReswClassGenerator()
        {
            _regexStringFormat =
                new Regex(
                    $"(?<tag>{TagFormat}|{TagFormatDotNet})\\[" +
                    "(?<formats>(?:" +
                         "[^\\\"]|" +
                         "\"(?:[^\\\"]|\\\\.)*\"" +
                         ")+" +
                       ")" +
                    "\\]");
        }

        private ReswClassGenerator(ResourceFileInfo resourceInfo, ICodeGenerator generator, IErrorLogger logger)
        {
            _resourceFileInfo = resourceInfo;
            _codeGenerator = generator;
            _logger = logger;
        }

        public static ReswClassGenerator CreateGenerator(ResourceFileInfo resourceFileInfo, IErrorLogger logger)
        {
            ICodeGenerator codeGenerator = null;
            switch (resourceFileInfo.ParentProject.Language)
            {
                case ResourceInfo.Language.CSHARP:
                    codeGenerator = new CSharpCodeGenerator();
                    break;
                case ResourceInfo.Language.VB:
                    codeGenerator = new VBCodeGenerator();
                    break;
                case ResourceInfo.Language.CPPCX:
                    codeGenerator = new CppCXCodeGenerator();
                    break;
                case ResourceInfo.Language.CPPWINRT:
                    codeGenerator = new CppWinRTCodeGenerator();
                    break;
            }
            if (codeGenerator != null)
            {
                return new ReswClassGenerator(resourceFileInfo, codeGenerator, logger);
            }
            return null;
        }

        private StronglyTypedClass Parse(string content, string defaultNamespace, bool isAdvanced)
        {
            var namespaceToUse = ExtractNamespace(defaultNamespace);
            var resourceFileName = Path.GetFileName(_resourceFileInfo.Path);
            var className = Path.GetFileNameWithoutExtension(_resourceFileInfo.Path);
            var reswInfo = ReswParser.Parse(content);

            var projectNameIfLibrary = _resourceFileInfo.ParentProject.IsLibrary ? _resourceFileInfo.ParentProject.Name : null;

            //If the resource file is in a library, the resource id in the .pri file
            //will be <library name>/FilenameWithoutExtension
            var resouceNameForResourceLoader = string.IsNullOrEmpty(projectNameIfLibrary) ?
                className : projectNameIfLibrary + "/" + className;


            var result = new StronglyTypedClass()
            {
                IsAdvanced = isAdvanced,
                ClassName = className,
                Namespaces = namespaceToUse,
                ResoureFile = resouceNameForResourceLoader
            };

            var stringItems = reswInfo.Items
                .Where(i => !i.Key.Contains(".") && !(i.Comment?.Contains(TagIgnore) ?? false)).ToArray();

            if (isAdvanced)
            {
                //check Pluralization
                var itemsWithPluralOrVariant = reswInfo.Items.GetItemsWithVariantOrPlural();
                var basicItems = stringItems.Except(itemsWithPluralOrVariant.SelectMany(e => e.Items)).ToArray();

                foreach (var item in itemsWithPluralOrVariant)
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
                                Summary = summary,
                                SupportNoneState = hasNoneForm,
                            };
                        }
                        else
                        {
                            localization = new PluralLocalization()
                            {
                                Key = item.Key,
                                Summary = summary,
                                SupportNoneState = hasNoneForm,
                            };
                        }
                        if (item.Items.Any(i => i.Comment != null && i.Comment.Contains(Deprecated_TagStrongType)))
                        {
                            _logger?.LogError($"{Deprecated_TagStrongType} is no more supported, use {TagFormat} instead. See https://github.com/rudyhuyn/ReswPlus/blob/master/README.md");
                        }
                        var commentToUse =
                            item.Items.FirstOrDefault(i => i.Comment != null && _regexStringFormat.IsMatch(i.Comment));

                        ManageFormattedFunction(localization, commentToUse?.Comment, basicItems, resourceFileName);

                        result.Localizations.Add(localization);
                    }
                    else if (item.SupportVariants)
                    {
                        var singleLineValue = _regexRemoveSpace.Replace(item.Items.FirstOrDefault().Value, " ").Trim();
                        var summary = $"Get the variant version of the string similar to: {singleLineValue}";
                        var commentToUse = item.Items.FirstOrDefault(i => i.Comment != null && _regexStringFormat.IsMatch(i.Comment));

                        var localization = new VariantLocalization()
                        {
                            Key = item.Key,
                            Summary = summary,
                        };

                        ManageFormattedFunction(localization, commentToUse?.Comment, basicItems, resourceFileName);

                        result.Localizations.Add(localization);
                    }
                }

                stringItems = basicItems;
            }

            if (stringItems.Any())
            {
                foreach (var item in stringItems)
                {
                    var singleLineValue = _regexRemoveSpace.Replace(item.Value, " ").Trim();
                    var summary = $"Looks up a localized string similar to: {singleLineValue}";

                    var localization = new RegularLocalization()
                    {
                        Key = item.Key,
                        Summary = summary,
                    };

                    if (isAdvanced)
                    {
                        ManageFormattedFunction(localization, item.Comment, stringItems, resourceFileName);
                    }
                    result.Localizations.Add(localization);
                }
            }

            return result;
        }

        private bool IsDotNetFormatting(string source)
        {
            return _regexDotNetFormatting.IsMatch(source);
        }

        public GenerationResult GenerateCode(string baseFilename, string content, string defaultNamespace, bool isAdvanced)
        {
            var stronglyTypedClassInfo = Parse(content, defaultNamespace, isAdvanced);
            if (stronglyTypedClassInfo == null)
            {
                return null;
            }

            var filesGenerated = _codeGenerator.GetGeneratedFiles(baseFilename, stronglyTypedClassInfo, _resourceFileInfo);
            var result = new GenerationResult()
            {
                Files = filesGenerated
            };

            if (filesGenerated != null && filesGenerated.Any())
            {
                result.MustInstallReswPlusLib = stronglyTypedClassInfo.Localizations.Any(l => l.IsDotNetFormatting || l is PluralLocalization || l.Parameters.Any(p => p is MacroFormatTagParameter));
            }
            return result;
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

        public static (string format, bool isDotNetFormatting) ParseTag(string comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {
                var match = _regexStringFormat.Match(comment);
                if (match.Success)
                {
                    var tag = match.Groups["tag"].Value;
                    return (match.Groups["formats"].Value.Trim(), tag == TagFormatDotNet);
                }
            }
            return (null, false);
        }

        private bool ManageFormattedFunction(Localization localization, string comment, IEnumerable<ReswItem> basicLocalizedItems, string resourceName)
        {
            FunctionFormatTagParametersInfo tagTypedInfo = null;
            var (format, isDotNetFormatting) = ParseTag(comment);
            if (format != null)
            {
                localization.IsDotNetFormatting = isDotNetFormatting;
                var types = FormatTag.SplitParameters(format);
                tagTypedInfo = FormatTag.ParseParameters(localization.Key, types, basicLocalizedItems, resourceName, _logger);
                if (tagTypedInfo != null)
                {
                    localization.Parameters = tagTypedInfo.Parameters;
                }
            }

            if (localization is IVariantLocalization variantLocalization)
            {
                FunctionFormatTagParameter variantParameter = null;
                // Add an extra parameter for variant if necessary
                if (tagTypedInfo?.VariantParameter == null)
                {
                    variantParameter = new FunctionFormatTagParameter
                    { Type = ParameterType.Long, Name = "variantId", IsVariantId = true };
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
                FunctionFormatTagParameter pluralizationQuantifier = null;
                // Add an extra parameter for pluralization if necessary
                if (tagTypedInfo?.PluralizationParameter == null)
                {
                    pluralizationQuantifier = new FunctionFormatTagParameter
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

    }
}
