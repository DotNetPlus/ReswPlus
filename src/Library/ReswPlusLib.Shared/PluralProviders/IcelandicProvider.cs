using ReswPlusLib;
using ReswPlusLib.Interfaces;
using ReswPlusLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlusLib.Providers
{
    internal class IcelandicProvider: IPluralProvider
    {
        public PluralTypeEnum ComputePlural(double n)
        {
            if (n.IsInt())
            {
                var integer = (int)n;
                if (integer % 10 == 1 && integer % 100 != 11)
                {
                    return PluralTypeEnum.ONE;
                }
                return PluralTypeEnum.OTHER;
            }
            else
            {
                return PluralTypeEnum.ONE;
            }


        }
    }
}
