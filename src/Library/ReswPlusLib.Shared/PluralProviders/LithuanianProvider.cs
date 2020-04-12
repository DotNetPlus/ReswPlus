// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class LithuanianProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if ((n % 10).IsBetween(2, 9) && !(n % 100).IsBetween(11, 19))
            {
                return PluralTypeEnum.FEW;
            }
            if ((n % 10) == 1 && !(n % 100).IsBetween(11, 19))
            {
                return PluralTypeEnum.ONE;
            }
            if (n.GetNumberOfDigitsAfterDecimal() != 0)
                return PluralTypeEnum.MANY;
            return PluralTypeEnum.OTHER;

        }
    }
}
