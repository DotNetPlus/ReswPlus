using System;
using System.Collections.Generic;
using System.Linq;

namespace CldrRuleImporter;

/// <summary>
/// The language codes CLDR publishes its rules under a different name from.
/// </summary>
internal static class CldrLanguages
{
    /// <summary>
    /// The codes CLDR renamed, and what it renamed them to.
    /// </summary>
    /// <remarks>
    /// ReswPlus has to answer for the deprecated ISO 639 codes as well as the current ones, because a resource
    /// folder or a Windows display language can still be named with one. CLDR publishes its rules under the
    /// current code only, so the old name is carried alongside the new one.
    /// </remarks>
    private static readonly IReadOnlyDictionary<string, string> Renamed = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["bh"] = "bho",
        ["in"] = "id",
        ["iw"] = "he",
        ["ji"] = "yi",
        ["jw"] = "jv",
        ["mo"] = "ro",
        ["sh"] = "sr",
        ["tl"] = "fil",
    };

    /// <summary>
    /// Gets the deprecated codes CLDR renamed to a language.
    /// </summary>
    /// <param name="language">The current code, as CLDR publishes it.</param>
    /// <returns>The codes that used to name it.</returns>
    public static IEnumerable<string> RenamedTo(string language)
    {
        return Renamed.Where(entry => string.Equals(entry.Value, language, StringComparison.Ordinal))
            .Select(entry => entry.Key);
    }
}
