using ReswPlus.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Languages
{
    internal class VBCodeGenerator : DotNetGeneratorBase
    {
        private readonly CodeStringBuilder _builder;

        public VBCodeGenerator()
        {
            _builder = new CodeStringBuilder("Basic");
        }

        public string GetParameterString(FunctionParameter info)
        {
            return GetParameterTypeString(info.Type) +
                " " +
                (info.TypeToCast != null ? "(" + GetParameterTypeString(info.TypeToCast.Value) + ")" : null) +
                info.Name;
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

        internal override IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename)
        {
            yield return new GeneratedFile() { Filename = baseFilename + ".vb", Content = _builder.GetString() };
        }

        internal override void GenerateHeaders(bool supportPluralization)
        {
            _builder.AppendLine("' File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralization)
            {
                _builder.AppendLine("' The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }
            _builder.AppendLine("Imports System");
            _builder.AppendLine("Imports Windows.ApplicationModel.Resources");
            _builder.AppendLine("Imports Windows.UI.Xaml.Markup");
            _builder.AppendLine("Imports Windows.UI.Xaml.Data");
        }

        internal override void OpenNamespace(IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                _builder.AppendLine($"Namespace {namespaces.Aggregate((a, b) => a + "." + b)}");
                _builder.AddLevel();
            }
        }

        internal override void CloseNamespace(IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                _builder.RemoveLevel();
                _builder.AppendLine($"End Namespace '{namespaces.Aggregate((a, b) => a + "." + b)}");
            }
        }

        internal override void OpenStronglyTypedClass(string resourceFilename, string className)
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

        internal override void CloseStronglyTypedClass()
        {
            _builder.RemoveLevel();
            _builder.AppendLine("End Class");
        }

        internal override void OpenRegion(string name)
        {
            _builder.AppendLine($"#Region \"{name}\"");
        }

        internal override void CloseRegion()
        {
            _builder.AppendLine("#End Region");
        }

        internal override void CreateTemplateAccessor(string key, string summary, bool supportPlural, bool pluralSupportNoneState, bool supportVariants)
        {
            if (!supportPlural && !supportVariants)
            {
                return;
            }
            _builder.AppendLine("' <summary>");
            _builder.AppendLine($"'   {summary}");
            _builder.AppendLine("' </summary>");

            var parameters = new List<string>();
            if (supportPlural)
            {
                parameters.Add("pluralNumber As Double");
            }
            if (supportVariants)
            {
                parameters.Add("variantId As Integer");
            }
            _builder.AppendLine($"Public Shared Function {key}({parameters.Aggregate((a, b) => a + ", " + b)}) As String");
            _builder.AddLevel();
            if (supportPlural && pluralSupportNoneState)
            {
                _builder.AppendLine("If pluralNumber = 0 Then");
                _builder.AddLevel();
                var noneKey = supportVariants ? $"\"{key}_Variant\" & variantId & \"_None\"" : $"\"{key}_None\"";
                _builder.AppendLine($"Return _resourceLoader.GetString({noneKey})");
                _builder.RemoveLevel();
                _builder.AppendLine("End If");
            }
            var stringKey = supportVariants ? $"\"{key}_Variant\" & variantId" : $"\"{key}\"";
            if (supportPlural)
            {
                _builder.AppendLine($"Return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, {stringKey}, CDec(pluralNumber))");
            }
            else
            {
                _builder.AppendLine($"Return _resourceLoader.GetString({stringKey})");
            }
            _builder.RemoveLevel();
            _builder.AppendLine("End Function");
        }

        internal override void CreateAccessor(string key, string summary)
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

        internal override void CreateFormatMethod(string key, IEnumerable<FunctionParameter> parameters, string summary = null, IEnumerable<FunctionParameter> extraParameters = null, FunctionParameter parameterForPluralization = null, FunctionParameter parameterForVariant = null)
        {
            _builder.AppendLine("' <summary>");
            _builder.AppendLine($"'   {summary}");
            _builder.AppendLine("' </summary>");

            IEnumerable<FunctionParameter> functionParameters;
            if (extraParameters != null && extraParameters.Any())
            {
                var list = new List<FunctionParameter>(parameters);
                list.InsertRange(0, extraParameters);
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
            if (parameterForPluralization != null)
            {
                var doubleValue = parameterForPluralization.TypeToCast.HasValue ? $"CType({parameterForPluralization.Name}, {GetParameterTypeString(parameterForPluralization.TypeToCast.Value)})" : parameterForPluralization.Name;
                if (parameterForVariant != null)
                {
                    sourceForFormat = $"{key}({doubleValue}, {parameterForVariant.Name})";
                }
                else
                {
                    sourceForFormat = $"{key}({doubleValue})";
                }
            }
            else
            {
                if (parameterForVariant != null)
                {
                    sourceForFormat = $"{key}({parameterForVariant.Name})";
                }
                else
                {
                    sourceForFormat = key;
                }
            }
            _builder.AddLevel();
            _builder.AppendLine($"Return String.Format({sourceForFormat}, {formatParameters})");
            _builder.RemoveLevel();
            _builder.AppendLine("End Function");
        }

        internal override void CreateMarkupExtension(string resourceFileName, string className, IEnumerable<string> keys)
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

        internal override void AddNewLine()
        {
            _builder.AppendEmptyLine();
        }
    }
}
