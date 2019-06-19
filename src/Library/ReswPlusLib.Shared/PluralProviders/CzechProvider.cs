/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class CzechProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n == 1)
                return PluralTypeEnum.ONE;

            if (n.GetNumberOfDigitsAfterDecimal() != 0)
            {
                return PluralTypeEnum.MANY;
            }

            if (n.IsBetween(2, 4))
                return PluralTypeEnum.FEW;

            return PluralTypeEnum.OTHER;
        }
    }
}
