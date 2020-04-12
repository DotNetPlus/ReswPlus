// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Interfaces;
using ReswPlusLib.Utils;

namespace ReswPlusLib.Providers
{
    internal class HebrewProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n.IsInt())
            {
                switch ((int)n)
                {
                    case 1:
                        return PluralTypeEnum.ONE;
                    case 2:
                        return PluralTypeEnum.TWO;
                }

                if (n != 0 && (n % 10) == 0)
                {
                    return PluralTypeEnum.MANY;
                }
            }
            return PluralTypeEnum.OTHER;

        }
    }
}
