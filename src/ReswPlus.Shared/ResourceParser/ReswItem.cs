namespace ReswPlus.Core.ResourceParser;

public sealed class ReswItem(string key, string value, string? comment = null)
{
    public string Key { get; } = key;
    public string Value { get; } = value;
    public string? Comment { get; } = comment;
}
