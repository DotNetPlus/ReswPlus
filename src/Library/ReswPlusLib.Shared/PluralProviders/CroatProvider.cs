// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class CroatProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n.IsInt())
            {
                var integer = (int)n;
                if (integer % 10 == 1 && integer % 100 != 11)
                {
                    return PluralTypeEnum.ONE;
                }
                if ((integer % 10).IsBetween(2, 4) && !(integer % 100).IsBetween(12, 14))
                {
                    return PluralTypeEnum.FEW;
                }
            }
            var f = n.DigitsAfterDecimal();
            if (f % 10 == 1 && f % 100 != 11)
            {
                return PluralTypeEnum.ONE;
            }

            if ((f % 10).IsBetween(2, 4) && !(f % 100).IsBetween(12, 14))
            {
                return PluralTypeEnum.FEW;
            }

            return PluralTypeEnum.OTHER;
        }
    }
}
