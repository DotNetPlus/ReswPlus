using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlus.Core.Interfaces
{
    public interface IErrorLogger
    {
        void LogError(string message, string document = null);
        void LogWarning(string message, string document = null);
    }
}
