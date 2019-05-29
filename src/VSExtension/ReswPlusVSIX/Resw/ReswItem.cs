// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

namespace ReswPlus.Resw
{
    class ReswItem
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
