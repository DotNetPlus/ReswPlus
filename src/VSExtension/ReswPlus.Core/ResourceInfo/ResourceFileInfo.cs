// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

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
