using System.Collections.Generic;

namespace ReswPlus.SourceGenerator.ClassGenerators.Models;

internal sealed class StronglyTypedClass
{
    public bool IsAdvanced { get; set; }
    public string[] Namespaces { get; set; }
    public string ResoureFile { get; set; }
    public string ClassName { get; set; }
    public AppType AppType { get; set; }

    public List<Localization> Items { get; set; } = [];
}
