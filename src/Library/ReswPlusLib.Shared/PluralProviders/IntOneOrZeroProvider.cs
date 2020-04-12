// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class IntOneOrZeroProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n == 0 || n == 1)
            {
                return PluralTypeEnum.ONE;
            }
            else
            {
                return PluralTypeEnum.OTHER;
            }
        }

    }
}
