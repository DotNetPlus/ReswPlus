// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ReswPlusCore.CodeGenerator;
using ReswPlusCore.Utils;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace ReswPlus.SingleFileGenerators
{

    internal class ReswPlusGeneratorBase : IDisposable
    {
        public ReswPlusGeneratorBase(bool usePluralizationAndVariant)
        {
            _isAdvanced = usePluralizationAndVariant;
        }

        #region IVsSingleFileGenerator Members

        public int DefaultExtension(out string defaultExtension)
        {
            defaultExtension = ".generated." + GetCodeProvider().FileExtension;
            return defaultExtension.Length;
        }

        public int Generate(ProjectItem projectItem, string inputFileContents,
          string defaultNamespace, out byte[] output, IVsGeneratorProgress generateProgress)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            VSUIIntegration.Instance?.ClearErrors();

            output = null;
            try
            {
                var language = projectItem.GetLanguage();
                var reswCodeGenerator = ReswCodeGenerator.CreateGenerator(projectItem, language, VSUIIntegration.Instance);
                if (reswCodeGenerator == null)
                {
                    return VSConstants.E_FAIL;
                }
                var inputFilepath = projectItem.Properties.Item("FullPath").Value as string;
                VSUIIntegration.Instance?.SetStatusBar($"Generating class for {Path.GetFileName(inputFilepath)}...");

                var baseFilename = "resources.generated." + GetCodeProvider().FileExtension; //won't be used.
                var files = reswCodeGenerator.GenerateCode(inputFilepath, baseFilename, inputFileContents, defaultNamespace, _isAdvanced);
                if (files.Count() != 1)
                {
                    return VSConstants.E_FAIL;
                }


                // IVsSingleFileGenerator supports only 1 file.
                output = Encoding.UTF8.GetBytes(files.First().Content);
            }
            catch (Exception)
            {
                return VSConstants.E_FAIL;
            }
            finally
            {
                VSUIIntegration.Instance?.CleanStatusBar();
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

        #region IDisposable

        public void Dispose()
        {
            _codeDomProvider?.Dispose();
            _serviceProvider?.Dispose();
        }

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
        public ProjectItem GetProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var p = GetServiceProvider()?.GetService(typeof(ProjectItem));
            Debug.Assert(p != null, "Unable to get Project Item.");
            return (ProjectItem)p;
        }

        private CodeDomProvider _codeDomProvider;
        private ServiceProvider _serviceProvider;
        private readonly bool _isAdvanced;
        #endregion
    }
}
