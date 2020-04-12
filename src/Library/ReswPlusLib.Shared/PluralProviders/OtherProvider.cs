// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using ReswPlusLib.Interfaces;


namespace ReswPlusLib.Providers
{
    internal class OtherProvider : IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            return PluralTypeEnum.OTHER;
        }
    }
}
