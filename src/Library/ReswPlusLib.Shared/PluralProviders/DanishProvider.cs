/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class DanishProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n != 0 && n.IsBetween(0, 1))
                return PluralTypeEnum.ONE;
            return PluralTypeEnum.OTHER;
        }
    }
}
