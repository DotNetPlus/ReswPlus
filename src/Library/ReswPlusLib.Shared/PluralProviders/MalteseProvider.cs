// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class MalteseProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            var isInt = n.IsInt();
            if (isInt)
            {
                var i = (int)n;
                if (i == 1)
                {
                    return PluralTypeEnum.ONE;
                }
                if (i == 0 || (i % 100).IsBetween(2, 10))
                {
                    return PluralTypeEnum.FEW;
                }
                if ((i % 100).IsBetween(11, 19))
                {
                    return PluralTypeEnum.MANY;
                }
            }
         
            return PluralTypeEnum.OTHER;

        }
    }
}
