using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReswPlus.Languages
{
    internal class VBCodeGenerator : ICodeGenerator
    {
        public string GetParameterTypeString(ParameterType type)
        {
            switch (type)
            {
                case ParameterType.Byte:
                    return "Byte";
                case ParameterType.Int:
                    return "Integer";
                case ParameterType.Uint:
                    return "UInteger";
                case ParameterType.Long:
                    return "Long";
                case ParameterType.String:
                    return "String";
                case ParameterType.Double:
                    return "Double";
                case ParameterType.Char:
                    return "Char";
                case ParameterType.Ulong:
                    return "ULong";
                case ParameterType.Decimal:
                    return "Decimal";
                //case ParameterType.Object:
                default:
                    return "object";
            }
        }

        public string GetHeaders(bool supportPluralNet)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("' File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralNet)
            {
                stringBuilder.AppendLine("' The NuGet package PluralNet is necessary to support Pluralization.");
            }
            stringBuilder.AppendLine("Imports System");
            stringBuilder.AppendLine("Imports Windows.ApplicationModel.Resources");
            stringBuilder.AppendLine("Imports Windows.UI.Xaml.Markup");
            stringBuilder.Append("Imports Windows.UI.Xaml.Data");
            return stringBuilder.ToString();
        }

        public string OpenNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return "";
            }
            else
            {
                return $"Namespace {namespaceName}";
            }
        }

        public string CloseNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                return "";
            }
            else
            {
                return "End Namespace";
            }
        }

        public string OpenStronglyTypedClass(string resourceFilename, string className)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("    <System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")>");
            stringBuilder.AppendLine("    <System.Diagnostics.DebuggerNonUserCodeAttribute()>");
            stringBuilder.AppendLine("    <System.Runtime.CompilerServices.CompilerGeneratedAttribute()>");
            stringBuilder.AppendLine($"    Public Class {className}");
            stringBuilder.AppendLine("        Private Shared _resourceLoader as ResourceLoader");

            stringBuilder.AppendLine($"        Shared Sub New()");
            stringBuilder.AppendLine($"            _resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFilename}\")");
            stringBuilder.Append("        End Sub");
            return stringBuilder.ToString();
        }

        public string CloseStronglyTypedClass()
        {
            return "    End Class";
        }

        public string OpenRegion(string name)
        {
            return $"        #Region \"{name}\"";
        }

        public string CloseRegion()
        {
            return "        #End Region";
        }

        public string CreatePluralNetAccessor(string pluralKey, string summary, string idNone = null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("        ' <summary>");
            stringBuilder.AppendLine($"        '   {summary}");
            stringBuilder.AppendLine("        ' </summary>");
            stringBuilder.AppendLine($"        Public Shared Function {pluralKey}(number As Double) As String");
            if (!string.IsNullOrEmpty(idNone))
            {
                stringBuilder.AppendLine("            If number == 0");
                stringBuilder.AppendLine($"                Return _resourceLoader.GetString(\"{idNone}\")");
                stringBuilder.AppendLine("            End If");
            }

            stringBuilder.AppendLine($"            Return Huyn.PluralNet.ResourceLoaderExtension.GetPlural(_resourceLoader, \"{pluralKey}\", CDec(number))");
            stringBuilder.Append("        End Function");
            return stringBuilder.ToString();
        }

        public string CreateAccessor(string key, string summary)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("        ' <summary>");
            stringBuilder.AppendLine($"        '   {summary}");
            stringBuilder.AppendLine("        ' </summary>");
            stringBuilder.AppendLine($"        Public Shared ReadOnly Property {key} As String");
            stringBuilder.AppendLine("            Get");
            stringBuilder.AppendLine($"                Return _resourceLoader.GetString(\"{key}\")");
            stringBuilder.AppendLine("            End Get");
            stringBuilder.AppendLine("        End Property");

            return stringBuilder.ToString();
        }

        public string CreateFormatMethod(string key, IEnumerable<FunctionParameter> parameters, string summary = null, FunctionParameter extraParameterForFunction = null, string parameterNameForPluralNet = null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("        ' <summary>");
            stringBuilder.AppendLine($"        '   {summary}");
            stringBuilder.AppendLine("        ' </summary>");
            stringBuilder.Append($"        Public Shared Function {key}_Format(");

            IEnumerable<FunctionParameter> functionParameters;
            if (extraParameterForFunction != null)
            {
                var list = new List<FunctionParameter>(parameters);
                list.Insert(0, extraParameterForFunction);
                functionParameters = list;
            }
            else
            {
                functionParameters = parameters;
            }
            stringBuilder.Append(functionParameters.Select(p => "ByVal " + p.Name + " As " + GetParameterTypeString(p.Type)).Aggregate((a, b) => a + ", " + b));
            var formatParameters = parameters.Select(p => p.Name).Aggregate((a, b) => a + ", " + b);

            string sourceForFormat;
            if (!string.IsNullOrEmpty(parameterNameForPluralNet))
            {
                sourceForFormat = key + "(" + parameterNameForPluralNet + ")";
            }
            else
            {
                sourceForFormat = key;
            }

            stringBuilder.AppendLine(") As String");
            stringBuilder.AppendLine($"            Return String.Format({sourceForFormat}, {formatParameters})");
            stringBuilder.Append("        End Function");

            return stringBuilder.ToString();
        }

        public string CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("    <System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")>");
            stringBuilder.AppendLine("    <System.Diagnostics.DebuggerNonUserCodeAttribute()>");
            stringBuilder.AppendLine("    <System.Runtime.CompilerServices.CompilerGeneratedAttribute()>");
            stringBuilder.AppendLine("    <MarkupExtensionReturnType(ReturnType:=GetType(String))>");
            stringBuilder.AppendLine($"    Public Class {className}");
            stringBuilder.AppendLine("        Inherits MarkupExtension");
            stringBuilder.AppendLine("        Public Enum KeyEnum");
            stringBuilder.AppendLine("            __Undefined = 0");
            foreach (var key in keys)
            {
                stringBuilder.AppendLine($"            {key}");
            }
            stringBuilder.AppendLine("        End Enum");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("        Private Shared _resourceLoader as ResourceLoader");
            stringBuilder.AppendLine("        Shared Sub New()");
            stringBuilder.AppendLine($"            _resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFileName}\")");
            stringBuilder.AppendLine("        End Sub");
            stringBuilder.AppendLine("        Public Property Key As KeyEnum");
            stringBuilder.AppendLine("        Public Property Converter As IValueConverter");
            stringBuilder.AppendLine("        Public Property ConverterParameter As Object");
            stringBuilder.AppendLine("        Protected Overrides Function ProvideValue() As Object");
            stringBuilder.AppendLine("            Dim res As String");
            stringBuilder.AppendLine("            If Key = KeyEnum.__Undefined Then");
            stringBuilder.AppendLine("                res = \"\"");
            stringBuilder.AppendLine("            Else");
            stringBuilder.AppendLine("                res = _resourceLoader.GetString(Key.ToString())");
            stringBuilder.AppendLine("            End If");
            stringBuilder.AppendLine("                        Return If(Converter Is Nothing, res, Converter.Convert(res, GetType(String), ConverterParameter, Nothing))");
            stringBuilder.AppendLine("        End Function");
            stringBuilder.Append("    End Class");
            return stringBuilder.ToString();
        }
    }
}
