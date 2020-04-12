// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Interfaces;
using ReswPlusLib.Utils;

namespace ReswPlusLib.Providers
{
    internal class MacedonianProvider : IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n.IsInt())
            {
                if (n % 10 == 1)
                {
                    return PluralTypeEnum.ONE;
                }
            }
            else
            {
                var f = n.DigitsAfterDecimal();
                if (f % 10 == 1)
                {
                    return PluralTypeEnum.ONE;
                }
            }
            return PluralTypeEnum.OTHER;
        }
    }
}
