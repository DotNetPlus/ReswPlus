using EnvDTE;
using ReswPlus.Core.ResourceInfo;
using ReswPlus.Utils;
using System;

namespace ReswPlus.ResourceInfo
{
    public static class ResourceFileInfoBuilder
    {
        public static ResourceFileInfo Create(ProjectItem reswProjectItem)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var project = reswProjectItem?.ContainingProject;
            if (project == null)
            {
                return null;
            }

            var path = reswProjectItem.Properties.Item("FullPath").Value as string;
            var parentProject = new ProjectInfo(reswProjectItem);
            return new ResourceFileInfo(path, parentProject);
        }
    }
}
