using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ReswPlus.Core.Interfaces;

namespace ReswPlus.Core.ResourceParser;

public interface IFormatTagParameter
{

}
public sealed class StringRefFormatTagParameter : IFormatTagParameter
{
    public string Id { get; }

    public StringRefFormatTagParameter(string id)
    {
        Id = id;
    }
}
public sealed class LiteralStringFormatTagParameter : IFormatTagParameter
{
    public string Value { get; }

    public LiteralStringFormatTagParameter(string value)
    {
        Value = value;
    }
}

public sealed class MacroFormatTagParameter : IFormatTagParameter
{
    public string Id { get; }

    public MacroFormatTagParameter(string id)
    {
        Id = id;
    }
}

public sealed class FunctionFormatTagParameter : IFormatTagParameter
{
    public ParameterType Type { get; }
    public string Name { get; }
    public ParameterType? TypeToCast { get; }
    public bool IsVariantId { get; }

    public FunctionFormatTagParameter(ParameterType type, string name, ParameterType? typeToCast, bool isVariantId)
    {
        Type = type;
        Name = name;
        TypeToCast = typeToCast;
        IsVariantId = isVariantId;
    }
}

public sealed class FunctionFormatTagParametersInfo
{
    public List<IFormatTagParameter> Parameters { get; set; } = [];
    public FunctionFormatTagParameter? PluralizationParameter { get; set; }
    public FunctionFormatTagParameter? VariantParameter { get; set; }
}

public sealed class FormatTagParameterTypeInfo(ParameterType type, bool canBeQuantifier)
{
    public ParameterType Type { get; } = type;
    public bool CanBeQuantifier { get; } = canBeQuantifier;
}

public sealed class FormatTag
{
    public static readonly Dictionary<string, string> MacrosAvailable = new()
    {
        /* all */
        { "DATE", "LongDate" },
        { "SHORT_DATE", "ShortDate" },
        { "TIME", "LongTime" },
        { "SHORT_TIME", "ShortTime" },
        { "WEEKDAY", "WeekDay" },
        { "SHORT_WEEKDAY", "ShortWeekDay" },
        { "YEAR", "Year" },
        { "SHORT_YEAR", "YearTwoDigits" },
        { "LOCALE_NAME", "LocaleName" },
        { "LOCALE_ID", "LocaleId" },
        { "LOCALE_TWO_LETTERS", "LocaleTwoLetters" },
        /* only UWP and .Net Core*/
        { "VERSION", "AppVersionFull" },
        { "VERSION_XYZ", "AppVersionMajorMinorBuild" },
        { "VERSION_XY", "AppVersionMajorMinor" },
        { "VERSION_X", "AppVersionMajor" },
        { "APP_NAME", "ApplicationName" },
        /* only UWP */
        { "ARCHITECTURE", "Architecture" },
        { "PUBLISHER_NAME", "PublisherName" },
        { "DEVICE_FAMILY", "DeviceFamily" },
        { "DEVICE_MANUFACTURER", "DeviceManufacturer" },
        { "DEVICE_MODEL", "DeviceModel" },
        { "OS_VERSION", "OperatingSystemVersion" }
    };

    public static readonly Dictionary<string, FormatTagParameterTypeInfo> AcceptedTypes = new()
    {
        {"Object", new FormatTagParameterTypeInfo(ParameterType.Object, false)},
        {"Byte", new FormatTagParameterTypeInfo(ParameterType.Byte, false)},
        {"Int", new FormatTagParameterTypeInfo(ParameterType.Int, true)},
        {"Int32", new FormatTagParameterTypeInfo(ParameterType.Int, true)},
        {"UInt32", new FormatTagParameterTypeInfo(ParameterType.Uint, true)},
        {"UInt", new FormatTagParameterTypeInfo(ParameterType.Uint, true)},
        {"Long", new FormatTagParameterTypeInfo(ParameterType.Long, true)},
        {"Int64", new FormatTagParameterTypeInfo(ParameterType.Long, true)},
        {"ULong", new FormatTagParameterTypeInfo(ParameterType.Ulong, true)},
        {"UInt64", new FormatTagParameterTypeInfo(ParameterType.Ulong, true)},
        {"String", new FormatTagParameterTypeInfo(ParameterType.String, false)},
        {"Double", new FormatTagParameterTypeInfo(ParameterType.Double, true)},
        {"Char", new FormatTagParameterTypeInfo(ParameterType.Char, false)},
        {"Decimal", new FormatTagParameterTypeInfo(ParameterType.Decimal, true)}
    };

    private static readonly Regex RegexNamedParameters = new("^(?:(?:\"(?<literalString>(?:\\\\.|[^\\\"])*)\")|(?:(?<localizationRef>\\w+)\\(\\))|(?:(?<quantifier>Plural\\s+)?(?<type>\\w+)\\s*(?<name>\\w+)?))$");

    public static FunctionFormatTagParametersInfo? ParseParameters(string key, IEnumerable<string> types, IEnumerable<ReswItem> basicLocalizedItems, string resourceFilename, IErrorLogger? logger)
    {
        var result = new FunctionFormatTagParametersInfo();
        var paramIndex = 1;
        foreach (var type in types)
        {
            var matchNamedParameters = RegexNamedParameters.Match(type.Trim());
            if (!matchNamedParameters.Success)
            {
                logger?.LogError($"[ReswPlus] Incorrect tag for the key '{key}': incorrect formatting", resourceFilename);
                return null;
            }
            if (matchNamedParameters.Groups["literalString"].Success)
            {
                var param = new LiteralStringFormatTagParameter(matchNamedParameters.Groups["literalString"].Value);
                result.Parameters.Add(param);
            }
            else
            {
                if (matchNamedParameters.Groups["localizationRef"].Success)
                {
                    var localizationRef = matchNamedParameters.Groups["localizationRef"].Value;
                    // Localization Identifier
                    if (!basicLocalizedItems.Any(i => i.Key == localizationRef))
                    {
                        //Identifier not found
                        logger?.LogError($"ReswPlus: Incorrect tag for the key '{key}': '{localizationRef}' doesn't exist in the resw file.", resourceFilename);
                        return null;
                    }
                    var param = new StringRefFormatTagParameter(localizationRef);

                    result.Parameters.Add(param);
                }
                else
                {
                    var paramTypeId = matchNamedParameters.Groups["type"].Value;
                    var isQuantifier = matchNamedParameters.Groups["quantifier"].Success;
                    var paramName = matchNamedParameters.Groups["name"].Value;

                    if (isQuantifier && !string.IsNullOrEmpty(paramTypeId) && string.IsNullOrEmpty(paramName))
                    {
                        if (!AcceptedTypes.ContainsKey(paramTypeId))
                        {
                            paramName = paramTypeId;
                            paramTypeId = "";
                        }
                    }
                    else if (!isQuantifier && paramTypeId == "Plural" && string.IsNullOrEmpty(paramName))
                    {
                        isQuantifier = true;
                        paramTypeId = paramName = "";
                    }
                    else if (!isQuantifier && MacrosAvailable.TryGetValue(paramTypeId, out var macroID) && string.IsNullOrEmpty(paramName))
                    {
                        result.Parameters.Add(new MacroFormatTagParameter(macroID));
                        continue;
                    }

                    var paramType = GetParameterType(paramTypeId, isQuantifier);
                    if (!paramType.type.HasValue)
                    {
                        logger?.LogError($"ReswPlus: Incorrect tag for the key '{key}': '{paramTypeId}' is not a correct type.", resourceFilename);
                        return null;
                    }
                    if (string.IsNullOrEmpty(paramName))
                    {
                        paramName = paramTypeId == "Variant" ? "variantId" : isQuantifier ? "pluralCount" : "param" + paramType.type + paramIndex;
                    }

                    var functionParam = new FunctionFormatTagParameter(paramType.type.Value, paramName, paramType.typeToCast, paramType.isVariantId);
                    if (isQuantifier && result.PluralizationParameter is null)
                    {
                        result.PluralizationParameter = functionParam;
                    }
                    else if (paramTypeId == "Variant")
                    {
                        if (result.VariantParameter != null)
                        {
                            logger?.LogError($"ReswPlus: The key '{key}' has more than 1 Variant parameter", resourceFilename);
                            return null;
                        }
                        result.VariantParameter = functionParam;
                    }
                    result.Parameters.Add(functionParam);
                    ++paramIndex;
                }
            }
        }
        return result;
    }

    public static (ParameterType? type, ParameterType? typeToCast, bool isVariantId) GetParameterType(string key, bool isQuantifier)
    {
        if (key == "Variant")
        {
            // VariantId
            return (ParameterType.Long, null, true);
        }
        else if (isQuantifier)
        {
            //Quantifier for plural
            if (string.IsNullOrEmpty(key))
            {
                return (ParameterType.Double, null, false);
            }
        }
        return AcceptedTypes.TryGetValue(key, out var info)
            ? ((ParameterType? type, ParameterType? typeToCast, bool isVariantId))(info.Type, isQuantifier && info.CanBeQuantifier && info.Type != ParameterType.Double ? ParameterType.Double : null, false)
            : ((ParameterType? type, ParameterType? typeToCast, bool isVariantId))(null, null, false);
    }

    /// <summary>
    /// Extract parameters but ignore \" and , in literal strings.
    /// </summary>
    public static IEnumerable<string> SplitParameters(string source)
    {
        source = source.Trim();
        var regex = new Regex(@"(?:(?<param>(?:\x22(?:\\.|[^\x22])*\x22)|(?:\w[^\x22,]*?))\s*(?:,|$)\s*)+");
        var match = regex.Match(source);
        if (match.Success && match.Length == source.Length)
        {
            foreach (Capture capture in match.Groups["param"].Captures)
            {
                yield return capture.Value;
            }
        }
    }
}
