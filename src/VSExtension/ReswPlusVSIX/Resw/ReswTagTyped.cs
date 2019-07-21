using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ReswPlus.Resw
{
    internal interface Parameter
    {

    }

    internal class ConstStringParameter : Parameter
    {
        public string Value { get; set; }
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

        private static readonly Regex RegexNamedParameters = new Regex("^(?:(?:\"(?<constStrings>[^\"]*)\")|(?<soloQuantifier>Plural)|(?:(?<quantifier>Plural\\s+)?(?<type>\\w+)\\s*(?<name>\\w+)?))$");

        public static FunctionParametersInfo ParseParameters(IEnumerable<string> types)
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
                    var isQuantifier = matchNamedParameters.Groups["soloQuantifier"].Success || matchNamedParameters.Groups["quantifier"].Success;
                    var trimmedType = matchNamedParameters.Groups["type"].Value?.Trim() ?? "";
                    var paramName = matchNamedParameters.Groups["name"].Value;
                    var paramType = GetParameterType(trimmedType, isQuantifier);
                    if (!paramType.type.HasValue)
                    {
                        return null;
                    }
                    if (string.IsNullOrEmpty(paramName))
                    {
                        if (trimmedType == "Variant")
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
                    else if (trimmedType == "Variant" && result.VariantParameter == null)
                    {
                        result.VariantParameter = functionParam;
                    }
                    result.Parameters.Add(functionParam);
                    ++paramIndex;
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
