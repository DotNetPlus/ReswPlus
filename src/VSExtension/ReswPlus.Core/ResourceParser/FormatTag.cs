using ReswPlus.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReswPlus.Core.ResourceParser
{
    public interface IFormatTagParameter
    {

    }
    public class StringRefFormatTagParameter : IFormatTagParameter
    {
        public string Id { get; set; }
    }
    public class LiteralStringFormatTagParameter : IFormatTagParameter
    {
        public string Value { get; set; }
    }

    public class MacroFormatTagParameter : IFormatTagParameter
    {
        public string Id { get; set; }
    }

    public class FunctionFormatTagParameter : IFormatTagParameter
    {
        public ParameterType Type { get; set; }
        public string Name { get; set; }
        public ParameterType? TypeToCast { get; set; }
        public bool IsVariantId { get; set; }
    }

    public class FunctionFormatTagParametersInfo
    {
        public List<IFormatTagParameter> Parameters { get; set; } = new List<IFormatTagParameter>();
        public FunctionFormatTagParameter PluralizationParameter { get; set; }
        public FunctionFormatTagParameter VariantParameter { get; set; }
    }

    public class FormatTagParameterTypeInfo
    {
        public ParameterType Type { get; }
        public bool CanBeQuantifier { get; }

        public FormatTagParameterTypeInfo(ParameterType type, bool canBeQuantifier)
        {
            Type = type;
            CanBeQuantifier = canBeQuantifier;
        }
    }

    public class FormatTag
    {
        public static readonly Dictionary<string, string> MacrosAvailable = new Dictionary<string, string>()
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

        public static readonly Dictionary<string, FormatTagParameterTypeInfo> AcceptedTypes = new Dictionary<string, FormatTagParameterTypeInfo>
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

        private static readonly Regex RegexNamedParameters = new Regex("^(?:(?:\"(?<constStrings>[^\"]*)\")|(?:(?<localizationRef>\\w+)\\(\\))|(?:(?<quantifier>Plural\\s+)?(?<type>\\w+)\\s*(?<name>\\w+)?))$");

        public static FunctionFormatTagParametersInfo ParseParameters(string key, IEnumerable<string> types, IEnumerable<ReswItem> basicLocalizedItems, string resourceFilename, IErrorLogger logger)
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
                if (matchNamedParameters.Groups["constStrings"].Success)
                {
                    var param = new LiteralStringFormatTagParameter()
                    {
                        Value = matchNamedParameters.Groups["constStrings"].Value
                    };

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
                        var param = new StringRefFormatTagParameter()
                        {
                            Id = localizationRef
                        };

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
                            result.Parameters.Add(new MacroFormatTagParameter() { Id = macroID });
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
                            if (paramTypeId == "Variant")
                            {
                                paramName = "variantId";
                            }
                            else if (isQuantifier)
                            {
                                paramName = "pluralCount";
                            }
                            else
                            {
                                paramName = "param" + paramType.type + paramIndex;
                            }
                        }

                        var functionParam = new FunctionFormatTagParameter { Type = paramType.type.Value, Name = paramName, TypeToCast = paramType.typeToCast, IsVariantId = paramType.isVariantId };
                        if (isQuantifier && result.PluralizationParameter == null)
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
            if (AcceptedTypes.TryGetValue(key, out var info))
            {
                return (info.Type, (isQuantifier && info.CanBeQuantifier && info.Type != ParameterType.Double ? (ParameterType?)ParameterType.Double : null), false);
            }
            return (null, null, false);
        }
    }
}
