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
    [InstalledProductRegistration(nameof(ReswPlusPluralizationGenerator), "Advanced File Code Generator for Resw files", "1.0")]
    [ComVisible(true)]
    [Guid("96B53337-A048-431A-BE1B-86554C5D2196")]
    [Utils.CodeGeneratorRegistration(
        typeof(ReswPlusPluralizationGenerator),
        nameof(ReswPlusPluralizationGenerator),
        VSConstants.UICONTEXT.CSharpProject_string,
        GeneratesDesignTimeSource = true)]
    [Utils.CodeGeneratorRegistration(
        typeof(ReswPlusPluralizationGenerator),
        nameof(ReswPlusPluralizationGenerator),
        VSConstants.UICONTEXT.VBProject_string,
        GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(ReswPlusPluralizationGenerator))]
    public sealed class ReswPlusPluralizationGenerator : IVsSingleFileGenerator, IObjectWithSite
    {
        private readonly ReswPlusGeneratorBase _base;

        public ReswPlusPluralizationGenerator()
        {
            _base = new ReswPlusGeneratorBase(true);
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
