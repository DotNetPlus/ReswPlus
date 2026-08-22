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
    internal const string TagIgnore = "#ReswPlusIgnore";
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
        // Matches either #Format[...] or #FormatNet[...], where the content may hold quoted literals that
        // escape a quote with a backslash.
        //
        // The run inside a literal is written so that it can only be read one way: characters that are
        // neither a quote nor a backslash, then any number of escapes each followed by more of the same. Read
        // as "anything up to a quote", a backslash could be taken either on its own or as the start of an
        // escape, and the expression has to try every combination of a run of them before it can fail — which
        // is exponential, and this runs over the comment of a resource on every keystroke.
        _regexStringFormat = new Regex(
            $@"(?<tag>{TagFormat}|{TagFormatDotNet})\[(?<formats>(?:""[^""\\]*(?:\\.[^""\\]*)*""|[^\\""])+)\]",
            RegexOptions.None,
            RegexTimeout);
    }

    /// <summary>
    /// How long a match is allowed to take before it is abandoned.
    /// </summary>
    /// <remarks>
    /// The expressions here are written not to backtrack pathologically, and this is the net under them: a
    /// resource file is written by hand, and a generator that never returns takes the whole build with it.
    /// </remarks>
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

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
    private StronglyTypedClass Parse(
        string content,
        string defaultNamespace,
        bool isAdvanced,
        AppType appType,
        bool generateResourceInterface)
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
            appType,
            generateResourceInterface
        );

        // Only use items with valid keys, that do not carry the ignore tag, and whose name the generated types
        // don't already declare.
        var stringItems = reswInfo.Items
            .Where(i => IsValidPropertyName(i.Key) && !(i.Comment?.Contains(TagIgnore) ?? false))
            .Where(i => !GeneratedIdentifier.ConflictsWithGeneratedMember(i.Key, className, generateResourceInterface))
            .ToArray();

        if (isAdvanced)
        {
            // Handle pluralization and variant support
            var itemsWithPluralOrVariant = reswInfo.Items.GetItemsWithVariantOrPlural().ToArray();
            var basicItems = stringItems.Except(itemsWithPluralOrVariant.SelectMany(e => e.Items)).ToArray();

            foreach (var item in itemsWithPluralOrVariant)
            {
                var itemKey = item.Key;

                // The forms of the resource are already out of the plain items, so a conflicting group is
                // dropped here rather than declined into members the generated types already declare.
                if (GeneratedIdentifier.ConflictsWithGeneratedMember(itemKey, className, generateResourceInterface))
                {
                    continue;
                }

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
                    _ = ManageFormattedFunction(localization, commentToUse, basicItems, resourceFileName);
                    result.Items.Add(localization);
                }
                else if (item.SupportVariants)
                {
                    var singleLineValue = _regexRemoveSpace.Replace(item.Items.FirstOrDefault()?.Value ?? string.Empty, " ").Trim();
                    var summary = $"Get the variant version of the string similar to: {singleLineValue}";
                    var commentToUse = item.Items.FirstOrDefault(i => !string.IsNullOrEmpty(i.Comment) && _regexStringFormat.IsMatch(i.Comment))?.Comment;

                    var localization = new VariantLocalization(itemKey, summary);
                    _ = ManageFormattedFunction(localization, commentToUse, basicItems, resourceFileName);
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
                    _ = ManageFormattedFunction(localization, item.Comment, stringItems, resourceFileName);
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
    internal static bool IsValidPropertyName(string propertyName)
    {
        return
            !string.IsNullOrWhiteSpace(propertyName) &&
            (char.IsLetter(propertyName[0]) || propertyName[0] == '_') &&
            propertyName.Skip(1).All(c => char.IsLetterOrDigit(c) || c == '_');
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
    internal GenerationResult? GenerateCode(
        string baseFilename,
        string content,
        string defaultNamespace,
        bool isAdvanced,
        AppType appType,
        bool generateResourceInterface)
    {
        var stronglyTypedClassInfo = Parse(content, defaultNamespace, isAdvanced, appType, generateResourceInterface);
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

        return CultureNames.Contains(lastSegment) ? splitted.Take(splitted.Length - 1).ToArray() : splitted;
    }

    /// <summary>
    /// The names of every culture the machine knows, to recognize a namespace segment that is a language tag.
    /// </summary>
    /// <remarks>
    /// Built once rather than enumerated per resource: there are some hundreds of cultures, and this is reached
    /// while a developer types in a resource file.
    /// </remarks>
    private static readonly HashSet<string> CultureNames = BuildCultureNames();

    private static HashSet<string> BuildCultureNames()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
        {
            names.Add(culture.Name);
        }

        return names;
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

        // The generated member declares its extra parameters first, and every parameter it declares needs a name
        // of its own: the tag can name a parameter after one the generator adds, or name two of them alike.
        GeneratedIdentifier.MakeNamesUnique(
        [
            .. localization.ExtraParameters,
            .. localization.Parameters.OfType<FunctionFormatTagParameter>(),
        ]);

        return true;
    }
}
