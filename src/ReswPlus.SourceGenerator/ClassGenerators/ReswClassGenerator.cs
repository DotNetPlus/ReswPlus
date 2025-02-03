using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReswPlus.Core.Interfaces;
using ReswPlus.Core.ResourceParser;
using ReswPlus.SourceGenerator.ClassGenerators.Models;
using ReswPlus.SourceGenerator.CodeGenerators;
using ReswPlus.SourceGenerator.Models;

namespace ReswPlus.SourceGenerator.ClassGenerators;

/// <summary>
/// Generates strongly-typed classes from .resw resource files.
/// </summary>
public sealed class ReswClassGenerator
{
    private const string TagIgnore = "#ReswPlusIgnore";
    private const string Deprecated_TagStrongType = "#ReswPlusTyped";
    private const string TagFormat = "#Format";
    private const string TagFormatDotNet = "#FormatNet";

    private static readonly Regex _regexStringFormat;
    private static readonly Regex _regexRemoveSpace = new("\\s+");

    private readonly ResourceFileInfo _resourceFileInfo;
    private readonly ICodeGenerator _codeGenerator;
    private readonly IErrorLogger? _logger;

    static ReswClassGenerator()
    {
        // Matches either #Format[...] or #FormatNet[...] including escaped quotes inside.
        _regexStringFormat = new Regex(
            $@"(?<tag>{TagFormat}|{TagFormatDotNet})\[(?<formats>(?:""(?:""|[^""])*""|[^\\""])+)\]");
    }

    private ReswClassGenerator(ResourceFileInfo resourceInfo, ICodeGenerator generator, IErrorLogger? logger)
    {
        _resourceFileInfo = resourceInfo;
        _codeGenerator = generator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ReswClassGenerator"/>.
    /// </summary>
    /// <param name="resourceFileInfo">The resource file information.</param>
    /// <param name="logger">The error logger.</param>
    /// <returns>A new instance of <see cref="ReswClassGenerator"/> or null if the language is not supported.</returns>
    internal static ReswClassGenerator? CreateGenerator(ResourceFileInfo resourceFileInfo, IErrorLogger? logger)
    {
        var codeGenerator = resourceFileInfo.Project.Language switch
        {
            Language.CSharp => new CSharpCodeGenerator(),
            _ => null
        };

        return codeGenerator is not null ? new ReswClassGenerator(resourceFileInfo, codeGenerator, logger) : null;
    }

    /// <summary>
    /// Parses the content of a .resw file and generates a strongly-typed class.
    /// </summary>
    /// <param name="content">The content of the .resw file.</param>
    /// <param name="defaultNamespace">The default namespace to use.</param>
    /// <param name="isAdvanced">Indicates whether advanced features are enabled.</param>
    /// <param name="appType">The type of the application.</param>
    /// <returns>A <see cref="StronglyTypedClass"/> representing the parsed content.</returns>
    private StronglyTypedClass Parse(string content, string defaultNamespace, bool isAdvanced, AppType appType)
    {
        var namespacesToUse = ExtractNamespace(defaultNamespace);
        var resourceFileName = Path.GetFileName(_resourceFileInfo.Path);
        var className = Path.GetFileNameWithoutExtension(_resourceFileInfo.Path);
        var reswInfo = ReswParser.Parse(content);

        // If the resource file is in a library, the resource id in the .pri file is formatted as: "<LibraryName>/FilenameWithoutExtension"
        var projectNameIfLibrary = _resourceFileInfo.Project.IsLibrary ? _resourceFileInfo.Project.Name : null;
        var resourceLoaderName = string.IsNullOrEmpty(projectNameIfLibrary)
            ? className
            : $"{projectNameIfLibrary}/{className}";

        var result = new StronglyTypedClass(
            isAdvanced,
            namespacesToUse,
            resourceLoaderName,
            className,
            appType
        );

        // Only use items with valid keys and that do not contain the ignore tag.
        var stringItems = reswInfo.Items
            .Where(i => IsValidPropertyName(i.Key) && !(i.Comment?.Contains(TagIgnore) ?? false))
            .ToArray();

        if (isAdvanced)
        {
            // Handle pluralization and variant support
            var itemsWithPluralOrVariant = reswInfo.Items.GetItemsWithVariantOrPlural();
            var basicItems = stringItems.Except(itemsWithPluralOrVariant.SelectMany(e => e.Items)).ToArray();

            foreach (var item in itemsWithPluralOrVariant)
            {
                var itemKey = item.Key;
                if (item.SupportPlural)
                {
                    var hasNoneForm = reswInfo.Items.Any(i => i.Key == $"{itemKey}_None");
                    var singleLineValue = _regexRemoveSpace.Replace(item.Items.FirstOrDefault()?.Value ?? string.Empty, " ").Trim();
                    var summary = $"Get the pluralized version of the string similar to: {singleLineValue}";

                    Localization localization = item.SupportVariants
                        ? new PluralVariantLocalization(itemKey, summary) { SupportNoneState = hasNoneForm }
                        : new PluralLocalization(itemKey, summary) { SupportNoneState = hasNoneForm };

                    if (item.Items.Any(i => i.Comment?.Contains(Deprecated_TagStrongType) == true))
                    {
                        _logger?.LogError($"{Deprecated_TagStrongType} is no longer supported. Use {TagFormat} instead. See https://github.com/DotNetPlus/ReswPlus/blob/master/README.md");
                    }

                    // Use the first comment that contains a valid format tag
                    var commentToUse = item.Items.FirstOrDefault(i => !string.IsNullOrEmpty(i.Comment) && _regexStringFormat.IsMatch(i.Comment))?.Comment;
                    ManageFormattedFunction(localization, commentToUse, basicItems, resourceFileName);
                    result.Items.Add(localization);
                }
                else if (item.SupportVariants)
                {
                    var singleLineValue = _regexRemoveSpace.Replace(item.Items.FirstOrDefault()?.Value ?? string.Empty, " ").Trim();
                    var summary = $"Get the variant version of the string similar to: {singleLineValue}";
                    var commentToUse = item.Items.FirstOrDefault(i => !string.IsNullOrEmpty(i.Comment) && _regexStringFormat.IsMatch(i.Comment))?.Comment;

                    var localization = new VariantLocalization(itemKey, summary);
                    ManageFormattedFunction(localization, commentToUse, basicItems, resourceFileName);
                    result.Items.Add(localization);
                }
            }

            stringItems = basicItems;
        }

        // Process the remaining regular strings.
        if (stringItems.Any())
        {
            foreach (var item in stringItems)
            {
                var singleLineValue = _regexRemoveSpace.Replace(item.Value, " ").Trim();
                var summary = $"Looks up a localized string similar to: {singleLineValue}";
                var localization = new RegularLocalization(item.Key, summary);

                if (isAdvanced)
                {
                    ManageFormattedFunction(localization, item.Comment, stringItems, resourceFileName);
                }
                result.Items.Add(localization);
            }
        }

        return result;
    }

    /// <summary>
    /// Validates if the given property name is valid.
    /// </summary>
    /// <param name="propertyName">The property name to validate.</param>
    /// <returns>True if the property name is valid; otherwise, false.</returns>
    private static bool IsValidPropertyName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return false;
        }

        if (!char.IsLetter(propertyName[0]) && propertyName[0] != '_')
        {
            return false;
        }

        return propertyName.Skip(1).All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    /// <summary>
    /// Generates code from the given .resw file content.
    /// </summary>
    /// <param name="baseFilename">The base filename for the generated files.</param>
    /// <param name="content">The content of the .resw file.</param>
    /// <param name="defaultNamespace">The default namespace to use.</param>
    /// <param name="isAdvanced">Indicates whether advanced features are enabled.</param>
    /// <param name="appType">The type of the application.</param>
    /// <returns>A <see cref="GenerationResult"/> containing the generated files.</returns>
    internal GenerationResult? GenerateCode(string baseFilename, string content, string defaultNamespace, bool isAdvanced, AppType appType)
    {
        var stronglyTypedClassInfo = Parse(content, defaultNamespace, isAdvanced, appType);
        if (stronglyTypedClassInfo is null)
        {
            return null;
        }

        var filesGenerated = _codeGenerator.GetGeneratedFiles(baseFilename, stronglyTypedClassInfo, _resourceFileInfo);
        var result = new GenerationResult(filesGenerated);

        if (filesGenerated?.Any() == true)
        {
            result.ContainsPlural = stronglyTypedClassInfo.Items.Any(l => l is PluralLocalization);
            result.ContainsMacro = stronglyTypedClassInfo.Items.Any(l => l.Parameters.Any(p => p is MacroFormatTagParameter));
        }
        return result;
    }

    /// <summary>
    /// Extracts the namespace segments from the given default namespace.
    /// </summary>
    /// <param name="defaultNamespace">The default namespace.</param>
    /// <returns>An array of namespace segments.</returns>
    private string[] ExtractNamespace(string defaultNamespace)
    {
        if (string.IsNullOrEmpty(defaultNamespace))
        {
            return Array.Empty<string>();
        }

        // Remove bcp47 tag from the namespace if present.
        var splitted = defaultNamespace.Split('.');
        var lastSegment = splitted.Last().Replace('_', '-');
        var culture = CultureInfo.GetCultures(CultureTypes.AllCultures)
                                 .FirstOrDefault(c => string.Equals(c.Name, lastSegment, StringComparison.OrdinalIgnoreCase));
        return culture != null ? splitted.Take(splitted.Length - 1).ToArray() : splitted;
    }

    /// <summary>
    /// Parses the format tag from the given comment.
    /// </summary>
    /// <param name="comment">The comment containing the format tag.</param>
    /// <returns>A tuple containing the format string and a boolean indicating if it is .NET formatting.</returns>
    public static (string? format, bool isDotNetFormatting) ParseTag(string? comment)
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

    /// <summary>
    /// Manages the formatted function for the given localization.
    /// </summary>
    /// <param name="localization">The localization to manage.</param>
    /// <param name="comment">The comment containing the format tag.</param>
    /// <param name="basicLocalizedItems">The basic localized items.</param>
    /// <param name="resourceName">The name of the resource.</param>
    /// <returns>True if the function was managed successfully; otherwise, false.</returns>
    private bool ManageFormattedFunction(Localization localization, string? comment, IEnumerable<ReswItem> basicLocalizedItems, string resourceName)
    {
        FunctionFormatTagParametersInfo? tagTypedInfo = null;
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
            // If a variant parameter was not provided via the format tag, add a default.
            var variantParameter = tagTypedInfo?.VariantParameter ?? new FunctionFormatTagParameter(ParameterType.Long, "variantId", null, true);
            if (tagTypedInfo?.VariantParameter is null)
            {
                localization.ExtraParameters.Add(variantParameter);
            }
            variantLocalization.ParameterToUseForVariant = variantParameter;
        }

        if (localization is PluralLocalization pluralLocalization)
        {
            // If pluralization parameter was not provided via the format tag, add a default.
            var pluralizationParameter = tagTypedInfo?.PluralizationParameter ?? new FunctionFormatTagParameter(
                ParameterType.Double,
                "pluralizationReferenceNumber",
                null,
                false);

            if (tagTypedInfo?.PluralizationParameter is null)
            {
                pluralLocalization.ExtraParameters.Add(pluralizationParameter);
            }
            pluralLocalization.ParameterToUseForPluralization = pluralizationParameter;
        }

        return true;
    }
}
