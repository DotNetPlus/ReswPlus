// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;


namespace ReswPlusLib.Providers
{
    internal class ArabicProvider : IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n.IsInt())
            {
                if (n == 0)
                {
                    return PluralTypeEnum.ZERO;
                }
                if (n == 1)
                {
                    return PluralTypeEnum.ONE;
                }
                if (n == 2)
                {
                    return PluralTypeEnum.TWO;
                }
                if ((n % 100).IsBetween(3, 10))
                {
                    return PluralTypeEnum.FEW;
                }
                if ((n % 100).IsBetween(11, 99))
                {
                    return PluralTypeEnum.MANY;
                }
            }
            return PluralTypeEnum.OTHER;

        }
    }
}
