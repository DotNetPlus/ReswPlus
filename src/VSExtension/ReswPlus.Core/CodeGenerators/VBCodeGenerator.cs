using ReswPlus.Core.ResourceParser;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Core.CodeGenerators
{
    public class VBCodeGenerator : DotNetGeneratorBase
    {
        public string GetParameterString(FunctionFormatTagParameter info)
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

        protected override IEnumerable<GeneratedFile> GetGeneratedFiles(CodeStringBuilder builder, string baseFilename)
        {
            yield return new GeneratedFile() { Filename = baseFilename + ".vb", Content = builder.GetString() };
        }

        protected override void GenerateHeaders(CodeStringBuilder builder, bool supportPluralization)
        {
            builder.AppendLine("' File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus");
            if (supportPluralization)
            {
                builder.AppendLine("' The NuGet package ReswPlusLib is necessary to support Pluralization.");
            }
            builder.AppendLine("Imports System");
            builder.AppendLine("Imports Windows.ApplicationModel.Resources");
            builder.AppendLine("Imports Windows.UI.Xaml.Markup");
            builder.AppendLine("Imports Windows.UI.Xaml.Data");
        }

        protected override void OpenNamespace(CodeStringBuilder builder, IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                builder.AppendLine($"Namespace {namespaces.Aggregate((a, b) => a + "." + b)}");
                builder.AddLevel();
            }
        }

        protected override void CloseNamespace(CodeStringBuilder builder, IEnumerable<string> namespaces)
        {
            if (namespaces != null && namespaces.Any())
            {
                builder.RemoveLevel();
                builder.AppendLine($"End Namespace '{namespaces.Aggregate((a, b) => a + "." + b)}");
            }
        }

        protected override void OpenStronglyTypedClass(CodeStringBuilder builder, string resourceFilename, string className)
        {

            builder.AppendLine($"<System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"{Constants.ReswPlusExtensionVersion}\")>");
            builder.AppendLine("<System.Diagnostics.DebuggerNonUserCodeAttribute()>");
            builder.AppendLine("<System.Runtime.CompilerServices.CompilerGeneratedAttribute()>");
            builder.AppendLine($"Public Class {className}");
            builder.AddLevel();
            builder.AppendLine("Private Shared _resourceLoader as ResourceLoader");
            builder.AppendEmptyLine();
            builder.AppendLine($"Shared Sub New()");
            builder.AddLevel();
            builder.AppendLine($"_resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFilename}\")");
            builder.RemoveLevel();
            builder.AppendLine("End Sub");
        }

        protected override void CloseStronglyTypedClass(CodeStringBuilder builder)
        {
            builder.RemoveLevel();
            builder.AppendLine("End Class");
        }

        protected override void OpenRegion(CodeStringBuilder builder, string name)
        {
            builder.AppendLine($"#Region \"{name}\"");
        }

        protected override void CloseRegion(CodeStringBuilder builder, string name)
        {
            builder.AppendLine($"#End Region");
        }

        protected override void CreateFormatMethod(CodeStringBuilder builder, string key, bool isProperty, IEnumerable<IFormatTagParameter> parameters, string summary = null, IEnumerable<FunctionFormatTagParameter> extraParameters = null, FunctionFormatTagParameter parameterForPluralization = null, bool supportNoneState = false, FunctionFormatTagParameter parameterForVariant = null)
        {
            builder.AppendLine("' <summary>");
            builder.AppendLine($"'   {summary}");
            builder.AppendLine("' </summary>");

            if (isProperty)
            {
                builder.AppendLine($"Public Shared ReadOnly Property {key} As String");
                builder.AddLevel();
                builder.AppendLine("Get");
            }
            else
            {
                var functionParameters = parameters != null ? parameters.OfType<FunctionFormatTagParameter>().ToList() :
                                   new List<FunctionFormatTagParameter>();
                if (extraParameters != null && extraParameters.Any())
                {
                    functionParameters.InsertRange(0, extraParameters);
                }

                if (parameters.Any(p => p is FunctionFormatTagParameter functionParam && functionParam.IsVariantId) || extraParameters.Any(p => p.IsVariantId))
                {
                    // one of the parameter is a variantId, we must create a second method with object as the variantId type.
                    var genericParametersStr = functionParameters.Select(p => "ByVal " + p.Name + " As " + (p.IsVariantId ? "Object" : GetParameterTypeString(p.Type))).Aggregate((a, b) => a + ", " + b);
                    builder.AppendLine($"Public Shared Function {key}({genericParametersStr}) As String");
                    builder.AddLevel();
                    builder.AppendLine("Try");
                    builder.AddLevel();
                    builder.AppendLine($"Return {key}({functionParameters.Select(p => p.IsVariantId ? $"Convert.ToInt64({p.Name})" : p.Name).Aggregate((a, b) => a + ", " + b)})");
                    builder.RemoveLevel();
                    builder.AppendLine("Catch");
                    builder.AddLevel();
                    builder.AppendLine("Return \"\"");
                    builder.RemoveLevel();
                    builder.AppendLine("End Try");
                    builder.RemoveLevel();
                    builder.AppendLine("End Function");
                    builder.AppendEmptyLine();
                    builder.AppendLine("' <summary>");
                    builder.AppendLine($"'   {summary}");
                    builder.AppendLine("' </summary>");
                }

                var parametersStr = functionParameters.Any() ? functionParameters.Select(p => "ByVal " + p.Name + " As " + GetParameterTypeString(p.Type)).Aggregate((a, b) => a + ", " + b)
                : "";
                builder.AppendLine($"Public Shared Function {key}({parametersStr}) As String");
            }
            builder.AddLevel();

            string keyToUseStr = $"\"{key}\"";
            if (parameterForVariant != null)
            {
                keyToUseStr = $"\"{key}_Variant\" & {parameterForVariant.Name}";
            }

            string localizationStr;
            if (parameterForPluralization != null)
            {
                var pluralNumber = parameterForPluralization.TypeToCast.HasValue ? $"CType({parameterForPluralization.Name}, {GetParameterTypeString(parameterForPluralization.TypeToCast.Value)})" : parameterForPluralization.Name;

                var supportNoneStateStr = supportNoneState ? "True" : "False";
                localizationStr = $"ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, {keyToUseStr}, {pluralNumber}, {supportNoneStateStr})";
            }
            else
            {
                localizationStr = $"_resourceLoader.GetString({keyToUseStr})";
            }
            if (parameters != null && parameters.Any())
            {
                var formatParameters = parameters.Select(p =>
            {
                switch (p)
                {
                    case FunctionFormatTagParameter functionParam:
                        return functionParam.Name;
                    case MacroFormatTagParameter macroParam:
                        return $"ReswPlusLib.Macros.{macroParam.Id}";
                    case LiteralStringFormatTagParameter constStringParameter:
                        return $"\"{constStringParameter.Value}\"";
                    case LocalizationRefFormatTagParameter localizationStringParameter:
                        return localizationStringParameter.Id;
                    default:
                        //should not happen
                        return "";
                }
            }
         ).Aggregate((a, b) => a + ", " + b);

                builder.AppendLine($"Return String.Format({localizationStr}, {formatParameters})");
            }
            else
            {
                builder.AppendLine($"Return {localizationStr}");
            }

            builder.RemoveLevel();
            if (isProperty)
            {
                builder.RemoveLevel();
                builder.AppendLine("End Get");
                builder.AppendLine("End Property");
            }
            else
            {
                builder.AppendLine("End Function");
            }
        }

        protected override void CreateMarkupExtension(CodeStringBuilder builder, string resourceFileName, string className, IEnumerable<string> keys)
        {
            builder.AppendLine($"<System.CodeDom.Compiler.GeneratedCodeAttribute(\"Huyn.ReswPlus\", \"{Constants.ReswPlusExtensionVersion}\")>");
            builder.AppendLine("<System.Diagnostics.DebuggerNonUserCodeAttribute()>");
            builder.AppendLine("<System.Runtime.CompilerServices.CompilerGeneratedAttribute()>");
            builder.AppendLine("<MarkupExtensionReturnType(ReturnType:=GetType(String))>");
            builder.AppendLine($"Public Class {className}");
            builder.AddLevel();
            builder.AppendLine("Inherits MarkupExtension");
            builder.AppendLine("Public Enum KeyEnum");
            builder.AddLevel();
            builder.AppendLine("__Undefined = 0");
            foreach (var key in keys)
            {
                builder.AppendLine(key);
            }
            builder.RemoveLevel();
            builder.AppendLine("End Enum");
            builder.AppendEmptyLine();
            builder.AppendLine("Private Shared _resourceLoader as ResourceLoader");
            builder.AppendLine("Shared Sub New()");
            builder.AddLevel();
            builder.AppendLine($"_resourceLoader = ResourceLoader.GetForViewIndependentUse(\"{resourceFileName}\")");
            builder.RemoveLevel();
            builder.AppendLine("End Sub");
            builder.AppendEmptyLine();
            builder.AppendLine("Public Property Key As KeyEnum");
            builder.AppendLine("Public Property Converter As IValueConverter");
            builder.AppendLine("Public Property ConverterParameter As Object");
            builder.AppendLine("Protected Overrides Function ProvideValue() As Object");
            builder.AddLevel();
            builder.AppendLine("Dim res As String");
            builder.AppendLine("If Key = KeyEnum.__Undefined Then");
            builder.AddLevel();
            builder.AppendLine("res = \"\"");
            builder.RemoveLevel();
            builder.AppendLine("Else");
            builder.AddLevel();
            builder.AppendLine("res = _resourceLoader.GetString(Key.ToString())");
            builder.RemoveLevel();
            builder.AppendLine("End If");
            builder.AppendLine("Return If(Converter Is Nothing, res, Converter.Convert(res, GetType(String), ConverterParameter, Nothing))");
            builder.RemoveLevel();
            builder.AppendLine("End Function");
            builder.RemoveLevel();
            builder.AppendLine("End Class");
        }

        protected override void AddNewLine(CodeStringBuilder builder)
        {
            builder.AppendEmptyLine();
        }
    }
}
