using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReswPlus.Resw
{
    internal interface Parameter
    {

    }
    internal class LocalizationRefParameter : Parameter
    {
        public string Id { get; set; }
    }
    internal class ConstStringParameter : Parameter
    {
        public string Value { get; set; }
    }

    internal class MacroParameter : Parameter
    {
        public string Id { get; set; }
    }

    internal class FunctionParameter : Parameter
    {
        public ParameterType Type { get; set; }
        public string Name { get; set; }
        public ParameterType? TypeToCast { get; set; }
        public bool IsVariantId { get; internal set; }
    }

    internal class FunctionParametersInfo
    {
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public FunctionParameter PluralizationParameter { get; set; }
        public FunctionParameter VariantParameter { get; set; }
    }

    internal class ParameterTypeInfo
    {
        public ParameterType Type { get; }
        public bool CanBeQuantifier { get; }

        public ParameterTypeInfo(ParameterType type, bool canBeQuantifier)
        {
            Type = type;
            CanBeQuantifier = canBeQuantifier;
        }
    }

    internal class ReswTagTyped
    {
        private static readonly Dictionary<string, string> _macroAvailable = new Dictionary<string, string>()
        {
            { "SHORT_DATE", "ShortDate" },
            { "LONG_DATE", "LongDate" },
            { "SHORT_TIME", "ShortTime" },
            { "LONG_TIME", "LongTime" },
            { "WEEK_DAY", "WeekDay" },
            { "YEAR", "Year" },
            { "SHORT_YEAR", "YearTwoDigits" },
            { "LOCALE_NAME", "LocaleName" },
            { "LOCALE_ID", "LocaleId" },
            { "LOCALE_TWO_LETTERS", "LocaleTwoLetters" },
            { "VERSION", "AppVersionFull" },
            { "VERSION_XYZ", "AppVersionMajorMinorBuild" },
            { "VERSION_XY", "AppVersionMajorMinor" },
            { "VERSION_X", "AppVersionMajor" },
            { "ARCHITECTURE", "Architecture" },
            { "APP_NAME", "ApplicationName" },
            { "PUBLISHER_NAME", "PublisherName" },
            { "DEVICE_FAMILY", "DeviceFamily" },
            { "DEVICE_MANUFACTURER", "DeviceManufacturer" },
            { "DEVICE_MODEL", "DeviceModel" },
            { "OS_VERSION", "OperatingSystemVersion" }
        };

        private static readonly Dictionary<string, ParameterTypeInfo> _acceptedTypes = new Dictionary<string, ParameterTypeInfo>
        {
            {"Object", new ParameterTypeInfo(ParameterType.Object, false)},
            {"Byte", new ParameterTypeInfo(ParameterType.Byte, false)},
            {"Int", new ParameterTypeInfo(ParameterType.Int, true)},
            {"Int32", new ParameterTypeInfo(ParameterType.Int, true)},
            {"UInt32", new ParameterTypeInfo(ParameterType.Uint, true)},
            {"UInt", new ParameterTypeInfo(ParameterType.Uint, true)},
            {"Long", new ParameterTypeInfo(ParameterType.Long, true)},
            {"Int64", new ParameterTypeInfo(ParameterType.Long, true)},
            {"ULong", new ParameterTypeInfo(ParameterType.Ulong, true)},
            {"UInt64", new ParameterTypeInfo(ParameterType.Ulong, true)},
            {"String", new ParameterTypeInfo(ParameterType.String, false)},
            {"Double", new ParameterTypeInfo(ParameterType.Double, true)},
            {"Char", new ParameterTypeInfo(ParameterType.Char, false)},
            {"Decimal", new ParameterTypeInfo(ParameterType.Decimal, true)}
        };

        private static readonly Regex RegexNamedParameters = new Regex("^(?:(?:\"(?<constStrings>[^\"]*)\")|(?:(?<localizationRef>\\w+)\\(\\))|(?:(?<quantifier>Plural\\s+)?(?<type>\\w+)\\s*(?<name>\\w+)?))$");

        public static FunctionParametersInfo ParseParameters(IEnumerable<string> types, IEnumerable<ReswItem> basicLocalizedItems)
        {
            var result = new FunctionParametersInfo();
            var paramIndex = 1;
            foreach (var type in types)
            {
                var matchNamedParameters = RegexNamedParameters.Match(type.Trim());
                if (!matchNamedParameters.Success)
                {
                    return null;
                }
                if (matchNamedParameters.Groups["constStrings"].Success)
                {
                    var param = new ConstStringParameter()
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
                            return null;
                        }
                        var param = new LocalizationRefParameter()
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
                        if (!isQuantifier && paramTypeId == "Plural" && string.IsNullOrEmpty(paramName))
                        {
                            isQuantifier = true;
                            paramTypeId = paramName = "";
                        }

                        if (!isQuantifier && _macroAvailable.TryGetValue(paramTypeId, out var macroID) && string.IsNullOrEmpty(paramName))
                        {
                            result.Parameters.Add(new MacroParameter() { Id = macroID });
                            continue;
                        }

                        var paramType = GetParameterType(paramTypeId, isQuantifier);
                        if (!paramType.type.HasValue)
                        {
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

                        var functionParam = new FunctionParameter { Type = paramType.type.Value, Name = paramName, TypeToCast = paramType.typeToCast, IsVariantId = paramType.isVariantId };
                        if (isQuantifier && result.PluralizationParameter == null)
                        {
                            result.PluralizationParameter = functionParam;
                        }
                        else if (paramTypeId == "Variant" && result.VariantParameter == null)
                        {
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
            if (_acceptedTypes.TryGetValue(key, out var info))
            {
                return (info.Type, (info.CanBeQuantifier && info.Type != ParameterType.Double ? (ParameterType?)ParameterType.Double : null), false);
            }
            return (null, null, false);
        }
    }
}
