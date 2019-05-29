/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */
using ReswPlusLib.Utils;
using ReswPlusLib.Interfaces;


namespace ReswPlusLib.Providers
{
    internal class ScottishGaelicProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n.IsInt())
            {
                var i = (int)n;
                switch (i)
                {
                    case 1:
                    case 11:
                        return PluralTypeEnum.ONE;
                    case 2:
                    case 12:
                        return PluralTypeEnum.TWO;
                }
                if (i <= 19)
                    return PluralTypeEnum.FEW;
            }
            return PluralTypeEnum.OTHER;
        }
    }
}
