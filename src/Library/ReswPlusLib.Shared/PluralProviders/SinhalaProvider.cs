/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;

namespace ReswPlusLib.Providers
{
    internal class SinhalaProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n == 0 || n == 1 || n.DigitsAfterDecimal() == 1)
                return PluralTypeEnum.ONE;
            return PluralTypeEnum.OTHER;
        }
    }
}
