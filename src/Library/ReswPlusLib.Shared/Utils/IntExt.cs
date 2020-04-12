// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

namespace ReswPlusLib.Utils
{
    internal static class IntExtension
    {
        public static bool IsBetween(this int number, int start, int end)
        {
            return start <= number && number <= end;
        }
        public static bool IsBetweenEndNotIncluded(this int number, int start, int end)
        {
            return start <= number && number < end;
        }

        public static bool IsBetween(this long number, long start, long end)
        {
            return start <= number && number <= end;
        }
        public static bool IsBetweenEndNotIncluded(this long number, long start, long end)
        {
            return start <= number && number < end;
        }


    }
}
