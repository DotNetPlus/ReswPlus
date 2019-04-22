// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ReswPlus.Languages;
using ReswPlus.Resw;

namespace ReswPlus.SingleFileGenerators
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration(nameof(ReswPlusPluralizationGenerator), "Advanced File Code Generator for Resw files", "1.0")]
    [ComVisible(true)]
    [Guid("96B53337-A048-431A-BE1B-86554C5D2196")]
    [Utils.CodeGeneratorRegistration(
        typeof(ReswPlusPluralizationGenerator),
        nameof(ReswPlusPluralizationGenerator),
        VSConstants.UICONTEXT.CSharpProject_string,
        GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(ReswPlusPluralizationGenerator))]
    public sealed class ReswPlusPluralizationGenerator : IVsSingleFileGenerator
    {
        #region IVsSingleFileGenerator Members

        public int DefaultExtension(out string defaultExtension)
        {
            defaultExtension = ".generated.cs";
            return defaultExtension.Length;
        }

        public int Generate(string inputFilePath, string inputFileContents,
          string defaultNamespace, IntPtr[] rgbOutputFileContents,
          out uint pcbOutput, IVsGeneratorProgress generateProgress)
        {
            try
            {
                var content = new ReswCodeGenerator(new CSharpCodeGenerator()).GenerateCode(inputFilePath, inputFileContents, defaultNamespace, true);
                var bytes = Encoding.UTF8.GetBytes(content);
                var length = bytes.Length;
                rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(bytes, 0, rgbOutputFileContents[0], length);
                pcbOutput = (uint)length;
            }
            catch (Exception)
            {
                pcbOutput = 0;
            }
            return VSConstants.S_OK;
        }

        #endregion
    }
}
