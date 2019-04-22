using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReswPlus.Resw
{

    internal class FunctionParameter
    {
        public ParameterType Type { get; set; }
        public string Name { get; set; }
    }

    internal class FunctionParametersInfo
    {
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();
        public FunctionParameter PluralNetDecimal { get; set; }
    }
    class ReswTagTyped
    {
        private static readonly Dictionary<string, ParameterType> _acceptedTypes = new Dictionary<string, ParameterType>
        {
            {"o", ParameterType.Object},
            {"b", ParameterType.Byte},
            {"d", ParameterType.Int},
            {"u", ParameterType.Uint},
            {"l", ParameterType.Long},
            {"s", ParameterType.String},
            {"f", ParameterType.Double},
            {"c", ParameterType.Char},
            {"ul", ParameterType.Ulong},
            {"m", ParameterType.Decimal},
            {"Q", ParameterType.Double} //reserved by PluralNet
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
                var paramType = _acceptedTypes[trimmedType];
                if (string.IsNullOrEmpty(paramName))
                {
                    if (trimmedType == "Q")
                    {
                        paramName = "pluralCount";
                    }
                    else
                    {
                        paramName = "param" + paramType + paramIndex;
                    }
                }

                var functionParam = new FunctionParameter { Type = paramType, Name = paramName };
                if (trimmedType == "Q" && result.PluralNetDecimal == null)
                {
                    result.PluralNetDecimal = functionParam;
                }
                result.Parameters.Add(functionParam);
                ++paramIndex;
            }
            return result;
        }

        public static IEnumerable<string> GetParameterSymbols()
        {
            return _acceptedTypes.Keys;
        }
    }
}
