// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class OneOrTwoProvider : IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n == 1)
            {
                return PluralTypeEnum.ONE;
            }
            else if (n == 2)
            {
                return PluralTypeEnum.TWO;
            }
            else
            {
                return PluralTypeEnum.OTHER;
            }
        }

    }
}
