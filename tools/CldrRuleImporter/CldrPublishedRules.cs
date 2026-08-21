using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace CldrRuleImporter;

/// <summary>
/// Every cardinal plural rule Unicode CLDR publishes, for every language it publishes one for.
/// </summary>
/// <remarks>
/// <para>
/// <b>Source.</b> The rules are read from <c>plurals.json</c>, which sits beside this file and is the data
/// published by the Unicode Common Locale Data Repository, vendored byte for byte from
/// <see href="https://raw.githubusercontent.com/unicode-org/cldr-json/main/cldr-json/cldr-core/supplemental/plurals.json"/>.
/// Their syntax is the plural rule syntax of UTS #35,
/// <see href="https://unicode.org/reports/tr35/tr35-numbers.html#Language_Plural_Rules"/>. CLDR data is
/// published by Unicode, Inc. under the Unicode licence.
/// </para>
/// <para>
/// <b>What depends on this.</b> The plural providers ReswPlus emits into a consumer's compilation are written
/// from these conditions, so this is the only source of the plural logic that ships.
/// </para>
/// <para>
/// <b>Refreshing it.</b> Replace <c>plurals.json</c> with the same file from a newer release. Nothing else is
/// transcribed, so there is nothing else to keep in step; the tests replay the sample quantities the file
/// carries and will say what changed.
/// </para>
/// </remarks>
internal static class CldrPublishedRules
{


    /// <remarks>
    /// Read once and kept, because a generator's statics outlive the compilation that filled them: the file is
    /// the same for every project the host builds.
    /// <para>
    /// The failure is deliberately not remembered with the value. A <see cref="Lazy{T}"/> left to itself keeps
    /// the exception it first threw and hands it to every later caller, which would turn one unreadable file
    /// into a failure for every project built for the rest of the session.
    /// </para>
    /// </remarks>


    /// <summary>
    /// The CLDR release the rules were published in.
    /// </summary>


    /// <summary>
    /// The rules of every language CLDR publishes rules for, keyed by language.
    /// </summary>


    /// <summary>
    /// A rule CLDR publishes.
    /// </summary>
    /// <param name="Category">The CLDR plural category, upper cased to match the categories ReswPlus declares.</param>
    /// <param name="Condition">The condition, in the syntax of UTS #35, empty for the fallback category.</param>
    /// <param name="Published">
    /// The rule as CLDR publishes it, condition and sample quantities together. The samples are what let a rule
    /// be tested without anyone having to decide what it ought to mean.
    /// </param>
    public readonly record struct Rule(string Category, string Condition, string Published);

    public sealed record Data(string Version, IReadOnlyDictionary<string, IReadOnlyList<Rule>> Cardinal);

    /// <summary>
    /// Reads the vendored data.
    /// </summary>
    /// <returns>The rules it holds.</returns>
    /// <remarks>
    /// Read once and kept, because a generator's statics outlive the compilation that filled them: the file is
    /// the same for every project the host builds.
    /// </remarks>
    public static Data Read(string json)
    {
        var supplemental = CldrJson.Parse(json).Object("supplemental");
        var cardinal = new Dictionary<string, IReadOnlyList<Rule>>(StringComparer.Ordinal);

        foreach (var language in supplemental.Object("plurals-type-cardinal").Objects)
        {
            var rules = new List<Rule>();

            foreach (var rule in language.Value.Strings)
            {
                // CLDR names a rule after the category it selects, and writes the sample quantities that select
                // it after an '@'.
                var category = rule.Key.Substring(rule.Key.LastIndexOf('-') + 1).ToUpperInvariant();
                var separator = rule.Value.IndexOf('@');
                var condition = (separator < 0 ? rule.Value : rule.Value.Substring(0, separator)).Trim();

                rules.Add(new Rule(category, condition, rule.Value));
            }

            cardinal[language.Key] = rules;
        }

        return new Data(supplemental.Object("version").String("_cldrVersion"), cardinal);
    }


}
