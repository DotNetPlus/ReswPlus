using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlus.Core.ResourceInfo
{
    public interface IProject
    {
        bool IsLibrary { get; }
        string Name { get; }
        ResourceInfo.Language Language { get; }
        string GetPrecompiledHeader();
        string GetIndentString();
    }

    public interface IResourceFileInfo
    {
        string Path { get; }
        IProject ContainingProject { get; }
    }
}
