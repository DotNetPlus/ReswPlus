using System;

namespace ReswPlus.SourceGenerator.Analysis;

/// <summary>
/// Controls diagnostics that compare translated resources with the default language.
/// </summary>
internal enum ReswTranslationChecks
{
    Off,
    Default,
    Strict,
}

internal static class ReswTranslationChecksParser
{
    public static ReswTranslationChecks Parse(string? value)
    {
        return Enum.TryParse(value, ignoreCase: true, out ReswTranslationChecks parsed)
            ? parsed
            : ReswTranslationChecks.Default;
    }
}
