// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio;
using ReswPlus.Languages;
using ReswPlus.Resw;
using ReswPlus.Utils;
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

        public int Generate(ProjectItem projectItem, string inputFileContents,
          string defaultNamespace, out byte[] output, IVsGeneratorProgress generateProgress)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            output = null;
            try
            {
                var language = projectItem.GetLanguage();
                Languages.ICodeGenerator codeGenerator = null;
                switch (language)
                {
                    case Utils.Language.CSHARP:
                        codeGenerator = new CSharpCodeGenerator();
                        break;
                    case Utils.Language.VB:
                        codeGenerator = new VBCodeGenerator();
                        break;
                    case Utils.Language.CPP:
                        codeGenerator = new CppCodeGenerator();
                        break;
                }
                if (codeGenerator == null)
                {
                    return VSConstants.E_UNEXPECTED;
                }

                var inputFilepath = projectItem.Properties.Item("FullPath").Value as string;
                var content = new ReswCodeGenerator(projectItem, codeGenerator).GenerateCode(inputFilepath, inputFileContents, defaultNamespace, _usePluralization);
                output = Encoding.UTF8.GetBytes(content);

                //Install nuget package
                if (_usePluralization)
                {
                    InstallNuGetPackage(projectItem.ContainingProject, "ReswPlusLib");
                }
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
        public ProjectItem GetProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var p = GetServiceProvider()?.GetService(typeof(ProjectItem));
            Debug.Assert(p != null, "Unable to get Project Item.");
            return (ProjectItem)p;
        }

        private CodeDomProvider _codeDomProvider;
        private ServiceProvider _serviceProvider;
        private readonly bool _usePluralization;
        #endregion

        public bool InstallNuGetPackage(Project project, string package)
        {
            try
            {
                var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                var installerServices = componentModel.GetService<IVsPackageInstallerServices>();
                if (!installerServices.IsPackageInstalled(project, package))
                {
                    var installer = componentModel.GetService<IVsPackageInstaller>();
                    installer.InstallPackage(null, project, package, (System.Version)null, false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Can't install {package} nuget package: {ex.Message}");
            }
            return false;
        }
    }
}
