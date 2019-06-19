/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class SlavicProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n.IsInt())
            {
                if ((n % 10) == 1 && (n % 100) != 11)
                {
                    return PluralTypeEnum.ONE;
                }
                if ((n % 10).IsBetween(2, 4) && !(n % 100).IsBetween(12, 14))
                {
                    return PluralTypeEnum.FEW;
                }
                if ((n % 10) == 0 ||
                    (n % 10).IsBetween(5, 9) ||
                    (n % 100).IsBetween(11, 14))
                {
                    return PluralTypeEnum.MANY;
                }

            }
            return PluralTypeEnum.OTHER;

        }
    }
}
