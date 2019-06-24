using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlus.Utils
{
    public static class NugetHelper
    {
        public static bool InstallNuGetPackage(this Project project, string package)
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
