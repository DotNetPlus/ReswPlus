using System.Collections.Generic;

namespace ReswPlus.Core.ResourceParser;

public sealed class ReswInfo
{
    public List<ReswItem> Items { get; }

    public ReswInfo()
    {
        Items = [];
    }
}
