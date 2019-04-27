using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReswPlus.Resw;

namespace ReswPlus.Languages
{
    interface ICodeGenerator
    {
        string GetParameterTypeString(ParameterType type);

        string GetHeaders(bool supportPluralNet);

        string OpenNamespace(string namespaceName);

        string CloseNamespace();

        string OpenStronglyTypedClass(string resourceFileName, string className);
        string CloseStronglyTypedClass();
        string OpenRegion(string name);
        string CloseRegion();
        string CreatePluralNetAccessor(string pluralKey, string summary, string idNone = null);

        string CreateAccessor(string key, string summary);

        string CreateFormatMethod(string key, IEnumerable<FunctionParameter> parameters, string summary = null,
            FunctionParameter extraParameterForFunction = null, string parameterNameForPluralNet = null);

        string CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys);
    }
}
