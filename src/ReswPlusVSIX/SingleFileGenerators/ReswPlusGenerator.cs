// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace ReswPlus.SingleFileGenerators
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
    [Utils.CodeGeneratorRegistration(
        typeof(ReswPlusGenerator),
        nameof(ReswPlusGenerator),
        VSConstants.UICONTEXT.VBProject_string,
        GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(ReswPlusGenerator))]
    public sealed class ReswPlusGenerator : IVsSingleFileGenerator, IObjectWithSite
    {
        private readonly ReswPlusGeneratorBase _base;

        public ReswPlusGenerator()
        {
            _base = new ReswPlusGeneratorBase(false);
        }

        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            return _base.DefaultExtension(out pbstrDefaultExtension);
        }

        public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            return _base.Generate(wszInputFilePath, bstrInputFileContents, wszDefaultNamespace, rgbOutputFileContents, out pcbOutput, pGenerateProgress);
        }

        public void SetSite(object pUnkSite)
        {
            _base.SetSite(pUnkSite);
        }

        public void GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            _base.GetSite(ref riid, out ppvSite);
        }
    }
}
