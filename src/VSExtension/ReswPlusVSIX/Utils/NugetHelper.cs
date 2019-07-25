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
using VSLangProj;

namespace ReswPlus.Utils
{
    public static class NugetHelper
    {
        public static bool InstallNuGetPackage(this Project project, string package, bool dontOverrideLocalDll)
        {
            try
            {
                var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                var installerServices = componentModel.GetService<IVsPackageInstallerServices>();

                bool containsDLL = false;
                if (dontOverrideLocalDll)
                {
                    var vsProject = (VSProject)project.Object;
                    if (vsProject != null)
                    {
                        foreach (Reference reference in vsProject.References)
                        {
                            if (reference.Name == package || reference.Name.StartsWith(package + "."))
                            {
                                containsDLL = true;
                                break;
                            }
                        }
                    }
                }

                var currentlyInstalledPackage = installerServices.GetInstalledPackages(project).FirstOrDefault(p => p.Id == package);
                if (containsDLL && currentlyInstalledPackage == null)
                {
                    // the project already uses a local reference to ReswPlusLib
                    // We should not install the nuget version to allow debugging.
                    return true;
                }

                if (currentlyInstalledPackage == null)
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
