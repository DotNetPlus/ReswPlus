/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Interfaces;
using ReswPlusLib.Utils;

namespace ReswPlusLib.Providers
{
    internal class CentralAtlasTamazightProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n == 0 || n == 1 || (n.IsInt() && n.IsBetween(11, 99)))
            {
                return PluralTypeEnum.ONE;
            }
            return PluralTypeEnum.OTHER;
        }
    }
}
