using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ReswPlus.Resw
{

    internal class FunctionParameter
    {
        public ParameterType Type { get; set; }
        public string Name { get; set; }
        public ParameterType? TypeToCast { get; set; }
        public bool IsVariantId { get; internal set; }
    }

    internal class FunctionParametersInfo
    {
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();
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
            {"o", new ParameterTypeInfo(ParameterType.Object, false)},
            {"b", new ParameterTypeInfo(ParameterType.Byte, false)},
            {"d", new ParameterTypeInfo(ParameterType.Int, true)},
            {"u", new ParameterTypeInfo(ParameterType.Uint, true)},
            {"l", new ParameterTypeInfo(ParameterType.Long, true)},
            {"s", new ParameterTypeInfo(ParameterType.String, false)},
            {"f", new ParameterTypeInfo(ParameterType.Double, true)},
            {"c", new ParameterTypeInfo(ParameterType.Char, false)},
            {"ul", new ParameterTypeInfo(ParameterType.Ulong, true)},
            {"m", new ParameterTypeInfo(ParameterType.Decimal, true)}
        };

        private static readonly Regex RegexNamedParameters = new Regex("(?<type>\\w+)(?:\\((?<name>\\w+)\\))?");

        public static FunctionParametersInfo ParseParameters(IEnumerable<string> types)
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
                var paramType = GetParameterType(trimmedType);
                if (string.IsNullOrEmpty(paramName))
                {
                    if(trimmedType == "V")
                    {
                        paramName = "variantId";
                    }
                    else if (trimmedType.StartsWith("Q"))
                    {
                        paramName = "pluralCount";
                    }
                    else
                    {
                        paramName = "param" + paramType.type + paramIndex;
                    }
                }

                var functionParam = new FunctionParameter { Type = paramType.type, Name = paramName, TypeToCast = paramType.typeToCast, IsVariantId = paramType.isVariantId };
                if (trimmedType.StartsWith("Q") && result.PluralizationParameter == null)
                {
                    result.PluralizationParameter = functionParam;
                }
                else if(trimmedType == "V" && result.VariantParameter == null)
                {
                    result.VariantParameter = functionParam;
                }

                result.Parameters.Add(functionParam);
                ++paramIndex;
            }
            return result;
        }

        public static (ParameterType type, ParameterType? typeToCast, bool isVariantId) GetParameterType(string key)
        {
            if(key == "V")
            {
                // VariantId
                return (ParameterType.Long, null, true);
            }
            else if (key.StartsWith("Q"))
            {
                //Quantifier for plural
                if (key == "Q")
                {
                    return (ParameterType.Double, null, false);
                }
                key = key.Substring(1);
            }
            var info = _acceptedTypes[key];
            return (info.Type, (info.CanBeQuantifier && info.Type != ParameterType.Double ? (ParameterType?)ParameterType.Double : null), false);
        }

        public static IEnumerable<string> GetParameterSymbols()
        {
            yield return "V"; // variant
            yield return "Q"; // plural
            foreach (var type in _acceptedTypes)
            {
                yield return type.Key;
                if (type.Value.CanBeQuantifier)
                {
                    yield return "Q" + type.Key;
                }
            }
        }
    }
}
