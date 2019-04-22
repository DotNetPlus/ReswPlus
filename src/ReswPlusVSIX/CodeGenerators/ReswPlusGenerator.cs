// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ReswPlus.ReswGen;

namespace ReswPlus.CodeGenerators
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration(nameof(ReswPlusGenerator), "Advanced File Code Generator for Resw files", "1.0")]
    [ComVisible(true)]
    [Guid("5A25BA4F-0BC7-44FB-B8D2-90C17CBD6CC7")]
    [Utils.CodeGeneratorRegistration(
        typeof(ReswPlusGenerator),
        nameof(ReswPlusGenerator),
        VSConstants.UICONTEXT.CSharpProject_string,
        GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(ReswPlusGenerator))]
    public sealed class ReswPlusGenerator: IVsSingleFileGenerator
    {
        #region IVsSingleFileGenerator Members

        public int DefaultExtension(out string defaultExtension)
        {
            defaultExtension = ".generated.cs";
            return defaultExtension.Length;
        }

        public int Generate(string inputFilePath, string inputFileContents,
          string defaultNamespace, IntPtr[] outputFileContents,
          out uint output, IVsGeneratorProgress generateProgress)
        {
            try
            {
                var content = ReswCodeGenerator.GenerateCode(inputFilePath, inputFileContents, defaultNamespace, false);
                var bytes = Encoding.UTF8.GetBytes(content);
                var length = bytes.Length;
                outputFileContents[0] = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(bytes, 0, outputFileContents[0], length);
                output = (uint)length;
            }
            catch (Exception)
            {
                output = 0;
            }
            return VSConstants.S_OK;
        }

        #endregion
    }
}
