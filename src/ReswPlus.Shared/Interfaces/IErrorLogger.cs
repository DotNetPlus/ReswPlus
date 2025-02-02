namespace ReswPlus.Core.Interfaces;

public interface IErrorLogger
{
    void LogError(string message, string document = null);
    void LogWarning(string message, string document = null);
}
