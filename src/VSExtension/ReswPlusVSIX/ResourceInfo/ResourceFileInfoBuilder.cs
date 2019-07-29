using EnvDTE;
using ReswPlus.Utils;
using System;

namespace ReswPlus.ResourceInfo
{
    public static class ResourceFileInfoBuilder
    {
        public static ResourceFileInfo Create(ProjectItem reswProjectItem)
        {
            var project = reswProjectItem?.ContainingProject;
            if (project == null)
            {
                return null;
            }

            return new ResourceFileInfo(reswProjectItem);
        }
    }
}
