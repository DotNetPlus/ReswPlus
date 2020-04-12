// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class CzechProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n == 1)
                return PluralTypeEnum.ONE;

            if (n.GetNumberOfDigitsAfterDecimal() != 0)
            {
                return PluralTypeEnum.MANY;
            }

            if (n.IsBetween(2, 4))
                return PluralTypeEnum.FEW;

            return PluralTypeEnum.OTHER;
        }
    }
}
