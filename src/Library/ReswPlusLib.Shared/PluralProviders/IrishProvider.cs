/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class IrishProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n.IsInt())
            {
                if (n == 1)
                {
                    return PluralTypeEnum.ONE;
                }
                if (n == 2)
                {
                    return PluralTypeEnum.TWO;
                }
                if ((n.IsBetween(3, 6)))
                {
                    return PluralTypeEnum.FEW;
                }
                if ((n.IsBetween(7, 10)))
                {
                    return PluralTypeEnum.MANY;
                }
            }
            return PluralTypeEnum.OTHER;

        }
    }
}
