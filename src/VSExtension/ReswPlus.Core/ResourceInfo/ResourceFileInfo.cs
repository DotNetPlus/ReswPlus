using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlus.Core.ResourceInfo
{
    public class ResourceFileInfo
    {
        public string Path { get; }
        public IProject Project { get; }

        public ResourceFileInfo(string path, IProject parentProject)
        {
            Path = path;
            Project = parentProject;
        }
    }
}
