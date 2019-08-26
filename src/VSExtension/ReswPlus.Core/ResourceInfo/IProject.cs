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
}
