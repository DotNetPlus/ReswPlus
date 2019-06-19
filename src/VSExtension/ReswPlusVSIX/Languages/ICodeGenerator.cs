using ReswPlus.Resw;
using System.Collections.Generic;

namespace ReswPlus.Languages
{
    internal interface ICodeGenerator
    {
        string GetParameterTypeString(ParameterType type);

        void NewLine();
        void GetHeaders(bool supportPluralization);

        void OpenNamespace(string[] namespaceName);
        void CloseNamespace(string[] namespaceName);

        void OpenStronglyTypedClass(string resourceFileName, string className);
        void CloseStronglyTypedClass();
        void OpenRegion(string name);
        void CloseRegion();
        void CreatePluralizationAccessor(string pluralKey, string summary, string idNone = null);

        void CreateAccessor(string key, string summary);

        void CreateFormatMethod(string key, IEnumerable<FunctionParameter> parameters, string summary = null,
            FunctionParameter extraParameterForFunction = null, FunctionParameter parameterForPluralization = null);

        void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys);
        string GetString();
    }
}
