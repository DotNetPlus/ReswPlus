using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal class VBCodeGenerator : ICodeGenerator
    {
        private readonly CodeStringBuilder _builder;

        public VBCodeGenerator()
        {
            _builder = new CodeStringBuilder();
        }

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

        public void NewLine()
        {
            _builder.AppendEmptyLine();
        }

        public string GetString()
        {
            return _builder.GetString();
        }

        public void GetHeaders(bool supportPluralNet)
        {
            _builder.AppendLine("' File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralNet)
            {
                _builder.AppendLine("' The NuGet package PluralNet is necessary to support Pluralization.");
            }
            _builder.AppendLine("Imports System");
            _builder.AppendLine("Imports Windows.ApplicationModel.Resources");
            _builder.AppendLine("Imports Windows.UI.Xaml.Markup");
            _builder.AppendLine("Imports Windows.UI.Xaml.Data");
        }

        public void OpenNamespace(string namespaceName)
        {
            if (!string.IsNullOrEmpty(namespaceName))
            {
                _builder.AppendLine($"Namespace {namespaceName}");
                _builder.AddLevel();
            }
        }

        public void CloseNamespace(string namespaceName)
        {
            if (!string.IsNullOrEmpty(namespaceName))
            {
                _builder.RemoveLevel();
                _builder.AppendLine("End Namespace");
            }
        }

        public void OpenStronglyTypedClass(string resourceFilename, string className)
        {

            _builder.AppendLine("<System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")>");
            _builder.AppendLine("<System.Diagnostics.DebuggerNonUserCodeAttribute()>");
            _builder.AppendLine("<System.Runtime.CompilerServices.CompilerGeneratedAttribute()>");
            _builder.AppendLine($"Public Class {className}");
            _builder.AddLevel();
            _builder.AppendLine("Private Shared _resourceLoader as ResourceLoader");
            _builder.AppendEmptyLine();
            _builder.AppendLine($"Shared Sub New()");
            _builder.AddLevel();
            _builder.AppendLine($"_resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFilename}\")");
            _builder.RemoveLevel();
            _builder.AppendLine("End Sub");
        }

        public void CloseStronglyTypedClass()
        {
            _builder.RemoveLevel();
            _builder.AppendLine("End Class");
        }

        public void OpenRegion(string name)
        {
            _builder.AppendLine($"#Region \"{name}\"");
        }

        public void CloseRegion()
        {
            _builder.AppendLine("#End Region");
        }

        public void CreatePluralNetAccessor(string pluralKey, string summary, string idNone = null)
        {
            _builder.AppendLine("' <summary>");
            _builder.AppendLine($"'   {summary}");
            _builder.AppendLine("' </summary>");
            _builder.AppendLine($"Public Shared Function {pluralKey}(number As Double) As String");
            _builder.AddLevel();
            if (!string.IsNullOrEmpty(idNone))
            {
                _builder.AppendLine("If number = 0 Then");
                _builder.AddLevel();
                _builder.AppendLine($"Return _resourceLoader.GetString(\"{idNone}\")");
                _builder.RemoveLevel();
                _builder.AppendLine("End If");
            }

            _builder.AppendLine($"Return Huyn.PluralNet.ResourceLoaderExtension.GetPlural(_resourceLoader, \"{pluralKey}\", CDec(number))");
            _builder.RemoveLevel();
            _builder.AppendLine("End Function");
        }

        public void CreateAccessor(string key, string summary)
        {
            _builder.AppendLine("' <summary>");
            _builder.AppendLine($"'   {summary}");
            _builder.AppendLine("' </summary>");
            _builder.AppendLine($"Public Shared ReadOnly Property {key} As String");
            _builder.AddLevel();
            _builder.AppendLine("Get");
            _builder.AddLevel();
            _builder.AppendLine($"Return _resourceLoader.GetString(\"{key}\")");
            _builder.RemoveLevel();
            _builder.AppendLine("End Get");
            _builder.RemoveLevel();
            _builder.AppendLine("End Property");
        }

        public void CreateFormatMethod(string key, IEnumerable<FunctionParameter> parameters, string summary = null, FunctionParameter extraParameterForFunction = null, string parameterNameForPluralNet = null)
        {
            _builder.AppendLine("' <summary>");
            _builder.AppendLine($"'   {summary}");
            _builder.AppendLine("' </summary>");

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
            var parametersStr = functionParameters.Select(p => "ByVal " + p.Name + " As " + GetParameterTypeString(p.Type)).Aggregate((a, b) => a + ", " + b);
            _builder.AppendLine($"Public Shared Function {key}_Format({parametersStr}) As String");
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
            _builder.AddLevel();
            _builder.AppendLine($"Return String.Format({sourceForFormat}, {formatParameters})");
            _builder.RemoveLevel();
            _builder.AppendLine("End Function");
        }

        public void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys)
        {

            _builder.AppendLine("<System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"0.1.0.0\")>");
            _builder.AppendLine("<System.Diagnostics.DebuggerNonUserCodeAttribute()>");
            _builder.AppendLine("<System.Runtime.CompilerServices.CompilerGeneratedAttribute()>");
            _builder.AppendLine("<MarkupExtensionReturnType(ReturnType:=GetType(String))>");
            _builder.AppendLine($"Public Class {className}");
            _builder.AddLevel();
            _builder.AppendLine("Inherits MarkupExtension");
            _builder.AppendLine("Public Enum KeyEnum");
            _builder.AddLevel();
            _builder.AppendLine("__Undefined = 0");
            foreach (var key in keys)
            {
                _builder.AppendLine(key);
            }
            _builder.RemoveLevel();
            _builder.AppendLine("End Enum");
            _builder.AppendEmptyLine();
            _builder.AppendLine("Private Shared _resourceLoader as ResourceLoader");
            _builder.AppendLine("Shared Sub New()");
            _builder.AddLevel();
            _builder.AppendLine($"_resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFileName}\")");
            _builder.RemoveLevel();
            _builder.AppendLine("End Sub");
            _builder.AppendEmptyLine();
            _builder.AppendLine("Public Property Key As KeyEnum");
            _builder.AppendLine("Public Property Converter As IValueConverter");
            _builder.AppendLine("Public Property ConverterParameter As Object");
            _builder.AppendLine("Protected Overrides Function ProvideValue() As Object");
            _builder.AddLevel();
            _builder.AppendLine("Dim res As String");
            _builder.AppendLine("If Key = KeyEnum.__Undefined Then");
            _builder.AddLevel();
            _builder.AppendLine("res = \"\"");
            _builder.RemoveLevel();
            _builder.AppendLine("Else");
            _builder.AddLevel();
            _builder.AppendLine("res = _resourceLoader.GetString(Key.ToString())");
            _builder.RemoveLevel();
            _builder.AppendLine("End If");
            _builder.AppendLine("Return If(Converter Is Nothing, res, Converter.Convert(res, GetType(String), ConverterParameter, Nothing))");
            _builder.RemoveLevel();
            _builder.AppendLine("End Function");
            _builder.RemoveLevel();
            _builder.AppendLine("End Class");
        }
    }
}
