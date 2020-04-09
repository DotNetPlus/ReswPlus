// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace ReswPlus.SingleFileGenerators
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration(nameof(ReswPlusAdvancedGenerator), "Advanced File Code Generator for Resw files", "1.0")]
    [ComVisible(true)]
    [Guid("96B53337-A048-431A-BE1B-86554C5D2196")]
    [Utils.CodeGeneratorRegistration(
        typeof(ReswPlusAdvancedGenerator),
        nameof(ReswPlusAdvancedGenerator),
        VSConstants.UICONTEXT.CSharpProject_string,
        GeneratesDesignTimeSource = true)]
    [Utils.CodeGeneratorRegistration(
        typeof(ReswPlusAdvancedGenerator),
        nameof(ReswPlusAdvancedGenerator),
        VSConstants.UICONTEXT.VBProject_string,
        GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(ReswPlusAdvancedGenerator))]
    public sealed class ReswPlusAdvancedGenerator : IVsSingleFileGenerator, IObjectWithSite
    {
        private readonly ReswPlusGeneratorBase _base;

        public ReswPlusAdvancedGenerator()
        {
            _base = new ReswPlusGeneratorBase(true);
        }

        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            return _base.DefaultExtension(out pbstrDefaultExtension);
        }

        public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            var res = _base.Generate(_base.GetProjectItem(), bstrInputFileContents, wszDefaultNamespace, out var outputBuffer, pGenerateProgress);
            if (outputBuffer != null)
            {
                var length = outputBuffer.Length;
                rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(outputBuffer, 0, rgbOutputFileContents[0], length);
                pcbOutput = (uint)length;
            }
            else
            {
                pcbOutput = 0;
            }
            return res;
        }

        #region IObjectWithSite Members
        public void GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            _base.GetSite(ref riid, out ppvSite);
        }

        public void SetSite(object pUnkSite)
        {
            _base.SetSite(pUnkSite);
        }
        #endregion

    }
}
