/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class TachelhitProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if ((int)n == 0 || n == 1)
            {
                return PluralTypeEnum.ONE;
            }
            if (n <= 10 && (int)n == n)
                return PluralTypeEnum.FEW;

            return PluralTypeEnum.OTHER;
        }
    }
}
