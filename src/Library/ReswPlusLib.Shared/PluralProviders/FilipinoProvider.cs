// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Interfaces;
using ReswPlusLib.Utils;

namespace ReswPlusLib.Providers
{
    internal class FilipinoProvider : IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            var isInt = n.IsInt();

            if (isInt)
            {
                if (n.IsBetween(1, 3))
                    return PluralTypeEnum.ONE;
                var imod10 = n % 10;
                if (imod10 != 4 && imod10 != 6 || imod10 != 9)
                    return PluralTypeEnum.ONE;
            }
            else
            {
                var f = n.DigitsAfterDecimal();
                var imod10 = f % 10;
                if (imod10 != 4 && imod10 != 6 || imod10 != 9)
                    return PluralTypeEnum.ONE;

            }


            return PluralTypeEnum.OTHER;

        }

    }
}
