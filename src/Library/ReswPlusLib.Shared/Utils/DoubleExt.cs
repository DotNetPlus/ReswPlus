/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */

namespace ReswPlusLib.Utils
{
    internal static class doubleExtension
    {
        public static bool IsBetween(this double number, double start, double end)
        {
            return start <= number && number <= end;
        }
        public static bool IsBetweenEndNotIncluded(this double number, double start, double end)
        {
            return start <= number && number < end;
        }

        public static bool IsInt(this double number)
        {
            return (int)number == number;
        }

        public static uint GetNumberOfDigitsAfterDecimal(this double number)
        {
            return (uint)((number - (int)number).ToString()).Length - 2;
        }


        public static long DigitsAfterDecimal(this double number)
        {
            try
            {
                //need optimization
                var str = ((number - (int)number).ToString()).Substring(2);
                if (str == "")
                    return 0;
                return long.Parse(str);
            }
            catch
            {
                return 0;
            }
        }
    }
}
