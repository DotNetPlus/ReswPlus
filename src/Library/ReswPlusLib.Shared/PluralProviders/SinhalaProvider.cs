// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class SinhalaProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n == 0 || n == 1 || n.DigitsAfterDecimal() == 1)
                return PluralTypeEnum.ONE;
            return PluralTypeEnum.OTHER;
        }
    }
}
