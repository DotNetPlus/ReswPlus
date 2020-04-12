// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

namespace ReswPlusLib.Interfaces
{
    public interface IPluralProvider
    {
        PluralTypeEnum ComputePlural(double n);
    }
}
