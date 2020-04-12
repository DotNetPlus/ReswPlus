// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class WelshProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            var isInt = n.IsInt();
            if (isInt)
            {
                var i = (int)n;
                switch (i)
                {
                    case 0:
                        return PluralTypeEnum.ZERO;
                    case 1:
                        return PluralTypeEnum.ONE;
                    case 2:
                        return PluralTypeEnum.TWO;
                    case 3:
                        return PluralTypeEnum.FEW;
                    case 6:
                        return PluralTypeEnum.MANY;
                }
            }
            return PluralTypeEnum.OTHER;
        }
    }
}
