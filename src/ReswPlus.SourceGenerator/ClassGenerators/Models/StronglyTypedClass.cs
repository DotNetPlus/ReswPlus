using System.Collections.Generic;

namespace ReswPlus.SourceGenerator.ClassGenerators.Models;

internal sealed class StronglyTypedClass
{
    public StronglyTypedClass(bool isAdvanced, string[] namespaces, string resoureFile, string className, AppType appType)
    {
        IsAdvanced = isAdvanced;
        Namespaces = namespaces;
        ResoureFile = resoureFile;
        ClassName = className;
        AppType = appType;
        Items = [];
    }

    public bool IsAdvanced { get; }
    public string[] Namespaces { get; }
    public string ResoureFile { get; }
    public string ClassName { get; }
    public AppType AppType { get; }

    public List<Localization> Items { get; }
}
