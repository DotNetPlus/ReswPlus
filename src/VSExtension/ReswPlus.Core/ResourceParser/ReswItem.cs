// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/DotNetPlus/ReswPlus

namespace ReswPlus.Core.ResourceParser
{
    public class ReswItem
    {
        public ReswItem(string key, string value, string comment = null)
        {
            Key = key;
            Value = value;
            Comment = comment;
        }
        public string Key { get; }
        public string Value { get; }
        public string Comment { get; }
    }
}
