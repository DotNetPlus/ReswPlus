// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ReswPlus.Languages;
using ReswPlus.Resw;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace ReswPlus.SingleFileGenerators
{

    internal class ReswPlusGeneratorBase
    {
        public ReswPlusGeneratorBase(bool usePluralization)
        {
            _usePluralization = usePluralization;
        }

        #region IVsSingleFileGenerator Members

        public int DefaultExtension(out string defaultExtension)
        {
            defaultExtension = ".generated." + GetCodeProvider().FileExtension;
            return defaultExtension.Length;
        }

        public int Generate(string inputFilePath, string inputFileContents,
          string defaultNamespace, IntPtr[] outputFileContents,
          out uint output, IVsGeneratorProgress generateProgress)
        {
            output = 0;
            try
            {
                var codeProvider = GetCodeProvider();
                if (codeProvider == null)
                {
                    return VSConstants.E_UNEXPECTED;
                }

                Languages.ICodeGenerator codeGenerator = null;
                switch (codeProvider.FileExtension.ToLower())
                {
                    case "cs":
                        codeGenerator = new CSharpCodeGenerator();
                        break;
                    case "vb":
                        codeGenerator = new VBCodeGenerator();
                        break;
                }
                if(codeGenerator == null)
                {
                    return VSConstants.E_UNEXPECTED;
                }
                var content = new ReswCodeGenerator(GetProjectItem(), codeGenerator).GenerateCode(inputFilePath, inputFileContents, defaultNamespace, _usePluralization);
                var bytes = Encoding.UTF8.GetBytes(content);
                var length = bytes.Length;
                outputFileContents[0] = Marshal.AllocCoTaskMem(length);
                Marshal.Copy(bytes, 0, outputFileContents[0], length);
                output = (uint)length;
            }
            catch (Exception)
            {
                return VSConstants.E_FAIL;
            }
            return VSConstants.S_OK;
        }

        #endregion

        #region IObjectWithSite Members
        public void GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            var pUnk = Marshal.GetIUnknownForObject(Site);
            var intPointer = IntPtr.Zero;
            Marshal.QueryInterface(pUnk, ref riid, out intPointer);
            ppvSite = intPointer;
        }

        public void SetSite(object pUnkSite)
        {
            Site = pUnkSite;
        }
        public object Site { get; private set; }
        #endregion

        #region Helper
        /// <summary>
        /// Get the ServiceProvider related to the Site
        /// </summary>
        private ServiceProvider GetServiceProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_serviceProvider == null)
            {
                _serviceProvider = new ServiceProvider(Site as IServiceProvider);
            }
            return _serviceProvider;
        }

        /// <summary>
        /// Returns a CodeDomProvider object for the language of the project containing
        /// the project item the generator was called on
        /// </summary>
        /// <returns>A CodeDomProvider object</returns>
        private CodeDomProvider GetCodeProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_codeDomProvider == null)
            {
                var serviceProvider = GetServiceProvider();
                if (serviceProvider != null && serviceProvider.GetService(typeof(SVSMDCodeDomProvider)) is IVSMDCodeDomProvider provider)
                {
                    _codeDomProvider = provider.CodeDomProvider as CodeDomProvider;
                }
                else
                {
                    //Fallback
                    _codeDomProvider = CodeDomProvider.CreateProvider("C#");
                }
            }
            return _codeDomProvider;
        }

        /// <summary>
        /// Returns the EnvDTE.ProjectItem object that corresponds to the project item the code 
        /// generator was called on
        /// </summary>
        /// <returns>The EnvDTE.ProjectItem of the project item the code generator was called on</returns>
        private ProjectItem GetProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var p = GetServiceProvider()?.GetService(typeof(ProjectItem));
            Debug.Assert(p != null, "Unable to get Project Item.");
            return (ProjectItem)p;
        }

        private CodeDomProvider _codeDomProvider;
        private ServiceProvider _serviceProvider;
        private bool _usePluralization;
        #endregion
    }
}
