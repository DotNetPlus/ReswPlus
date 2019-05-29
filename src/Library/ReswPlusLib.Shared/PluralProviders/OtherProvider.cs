/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Interfaces;


namespace ReswPlusLib.Providers
{
    internal class OtherProvider : IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            return PluralTypeEnum.OTHER;
        }
    }
}
