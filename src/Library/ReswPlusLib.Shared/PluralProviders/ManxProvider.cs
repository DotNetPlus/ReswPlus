// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class ManxProvider : IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            var isInt = n.IsInt();
            var i = (int)n;
            if (isInt)
            {
                if (i % 10 == 1)
                    return PluralTypeEnum.ONE;
                if (i % 10 == 2)
                    return PluralTypeEnum.TWO;
                if (i % 20 == 0)
                    return PluralTypeEnum.FEW;
                return PluralTypeEnum.OTHER;
            }
            else
            {
                return PluralTypeEnum.MANY;
            }
        }
    }
}
