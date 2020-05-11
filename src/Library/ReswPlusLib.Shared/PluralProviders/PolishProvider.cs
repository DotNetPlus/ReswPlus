// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class PolishProvider : IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if ((n % 10).IsBetween(2, 4) && !(n % 100).IsBetween(12, 14))
            {
                return PluralTypeEnum.FEW;
            }
            if (n != 1 && (n % 10).IsBetween(0, 1) ||
                (n % 10).IsBetween(5, 9) ||
                (n % 100).IsBetween(12, 14))
            {
                return PluralTypeEnum.MANY;
            }
            if (n == 1)
            {
                return PluralTypeEnum.ONE;
            }
            return PluralTypeEnum.OTHER;
        }
    }
}
