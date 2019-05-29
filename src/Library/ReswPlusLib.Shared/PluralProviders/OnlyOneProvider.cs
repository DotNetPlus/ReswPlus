/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class OnlyOneProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n == 1)
            {
                return PluralTypeEnum.ONE;
            }
            else
            {
                return PluralTypeEnum.OTHER;
            }
        }

    }
}
