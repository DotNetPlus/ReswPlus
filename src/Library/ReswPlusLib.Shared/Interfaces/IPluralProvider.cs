/* ReswPlusLib
 * Author Rudy Huyn
 * License MIT / http://bit.ly/mit-license
 */

namespace ReswPlusLib.Interfaces
{
    public interface IPluralProvider
    {
        PluralTypeEnum ComputePlural(double n);
    }
}
