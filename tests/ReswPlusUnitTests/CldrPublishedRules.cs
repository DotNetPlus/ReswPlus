using System.Collections.Generic;
using System.Linq;

namespace ReswPlusUnitTests;

/// <summary>
/// Every cardinal plural rule Unicode CLDR publishes, for every language it publishes one for.
/// </summary>
/// <remarks>
/// <see cref="CldrPluralRules"/> pins one language per plural form ReswPlus ships, with the sample lists CLDR
/// publishes, which is what makes a form's rules testable. This pins the other half of the problem: which rule
/// set CLDR gives to <em>each</em> language. A form can be faithful to CLDR and still be handed to a language
/// CLDR gives different rules to, and no amount of replaying that form's samples would show it.
/// <para>
/// The conditions are kept without their sample lists, and the languages that share a rule set are grouped
/// under it, because CLDR publishes 40 distinct rule sets for 224 languages.
/// </para>
/// <para>
/// Regenerate with the "plurals-type-cardinal" section of
/// https://raw.githubusercontent.com/unicode-org/cldr-json/main/cldr-json/cldr-core/supplemental/plurals.json
/// and keep <see cref="CldrPluralRules.Version"/> in step with the release it was taken from.
/// </para>
/// </remarks>
internal static class CldrPublishedRules
{
    /// <summary>
    /// A rule CLDR publishes: the category it selects, and the condition that selects it.
    /// </summary>
    /// <param name="Category">The CLDR plural category, upper cased to match the categories ReswPlus declares.</param>
    /// <param name="Condition">The condition, in the syntax of UTS #35, empty for the fallback category.</param>
    public readonly record struct Rule(string Category, string Condition);

    /// <summary>
    /// A set of rules and the languages CLDR gives it to.
    /// </summary>
    /// <param name="Languages">The languages sharing the rules.</param>
    /// <param name="Rules">The rules, in the order CLDR publishes them.</param>
    private sealed record RuleSet(IReadOnlyList<string> Languages, IReadOnlyList<Rule> Rules);

    private static readonly RuleSet[] Published =
    [
        new RuleSet(
            [
                "af", "an", "asa", "az", "bal", "bem", "bez", "bg", "brx", "ce", "cgg", "chr", "ckb", "dv", "ee",
                "el", "eo", "eu", "fo", "fur", "gsw", "ha", "haw", "hu", "jgo", "jmc", "ka", "kaj", "kcg", "kk",
                "kkj", "kl", "ks", "ksb", "ku", "ky", "lb", "lg", "mas", "mgo", "ml", "mn", "mr", "nah", "nb",
                "nd", "ne", "nn", "nnh", "no", "nr", "ny", "nyn", "om", "or", "os", "pap", "ps", "rm", "rof",
                "rwk", "saq", "sd", "sdh", "seh", "sn", "so", "sq", "ss", "ssy", "st", "syr", "ta", "te", "teo",
                "tig", "tk", "tn", "tr", "ts", "ug", "uz", "ve", "vo", "vun", "wae", "xh", "xog"
            ],
            [
                new("ONE", "n = 1"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "ak", "bho", "csw", "guw", "ln", "mg", "nso", "pa", "ti", "wa"
            ],
            [
                new("ONE", "n = 0..1"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "am", "as", "bn", "doi", "fa", "gu", "hi", "kn", "kok", "kok-Latn", "pcm", "zu"
            ],
            [
                new("ONE", "i = 0 or n = 1"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "ar", "ars"
            ],
            [
                new("ZERO", "n = 0"),
                new("ONE", "n = 1"),
                new("TWO", "n = 2"),
                new("FEW", "n % 100 = 3..10"),
                new("MANY", "n % 100 = 11..99"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "ast", "de", "en", "et", "fi", "fy", "gl", "ia", "ie", "io", "lij", "nl", "sc", "sv", "sw", "ur",
                "yi"
            ],
            [
                new("ONE", "i = 1 and v = 0"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "be"
            ],
            [
                new("ONE", "n % 10 = 1 and n % 100 != 11"),
                new("FEW", "n % 10 = 2..4 and n % 100 != 12..14"),
                new("MANY", "n % 10 = 0 or n % 10 = 5..9 or n % 100 = 11..14"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "blo", "cv", "ksh"
            ],
            [
                new("ZERO", "n = 0"),
                new("ONE", "n = 1"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "bm", "bo", "dz", "hnj", "id", "ig", "ii", "ja", "jbo", "jv", "jw", "kde", "kea", "km", "ko",
                "lkt", "lo", "ms", "my", "nqo", "osa", "sah", "ses", "sg", "su", "th", "to", "tpi", "und", "vi",
                "wo", "yo", "yue", "zh"
            ],
            [
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "br"
            ],
            [
                new("ONE", "n % 10 = 1 and n % 100 != 11,71,91"),
                new("TWO", "n % 10 = 2 and n % 100 != 12,72,92"),
                new("FEW", "n % 10 = 3..4,9 and n % 100 != 10..19,70..79,90..99"),
                new("MANY", "n != 0 and n % 1000000 = 0"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "bs", "hr", "sh", "sr"
            ],
            [
                new("ONE", "v = 0 and i % 10 = 1 and i % 100 != 11 or f % 10 = 1 and f % 100 != 11"),
                new("FEW", "v = 0 and i % 10 = 2..4 and i % 100 != 12..14 or f % 10 = 2..4 and f % 100 != 12..14"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "ca", "it", "lld", "pt-PT", "scn", "vec"
            ],
            [
                new("ONE", "i = 1 and v = 0"),
                new("MANY", "e = 0 and i != 0 and i % 1000000 = 0 and v = 0 or e != 0..5"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "ceb", "fil", "tl"
            ],
            [
                new("ONE", "v = 0 and i = 1,2,3 or v = 0 and i % 10 != 4,6,9 or v != 0 and f % 10 != 4,6,9"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "cs", "sk"
            ],
            [
                new("ONE", "i = 1 and v = 0"),
                new("FEW", "i = 2..4 and v = 0"),
                new("MANY", "v != 0"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "cy"
            ],
            [
                new("ZERO", "n = 0"),
                new("ONE", "n = 1"),
                new("TWO", "n = 2"),
                new("FEW", "n = 3"),
                new("MANY", "n = 6"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "da"
            ],
            [
                new("ONE", "n = 1 or t != 0 and i = 0,1"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "dsb", "hsb"
            ],
            [
                new("ONE", "v = 0 and i % 100 = 1 or f % 100 = 1"),
                new("TWO", "v = 0 and i % 100 = 2 or f % 100 = 2"),
                new("FEW", "v = 0 and i % 100 = 3..4 or f % 100 = 3..4"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "es"
            ],
            [
                new("ONE", "n = 1"),
                new("MANY", "e = 0 and i != 0 and i % 1000000 = 0 and v = 0 or e != 0..5"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "ff", "hy", "kab"
            ],
            [
                new("ONE", "i = 0,1"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "fr"
            ],
            [
                new("ONE", "i = 0,1"),
                new("MANY", "e = 0 and i != 0 and i % 1000000 = 0 and v = 0 or e != 0..5"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "ga"
            ],
            [
                new("ONE", "n = 1"),
                new("TWO", "n = 2"),
                new("FEW", "n = 3..6"),
                new("MANY", "n = 7..10"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "gd"
            ],
            [
                new("ONE", "n = 1,11"),
                new("TWO", "n = 2,12"),
                new("FEW", "n = 3..10,13..19"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "gv"
            ],
            [
                new("ONE", "v = 0 and i % 10 = 1"),
                new("TWO", "v = 0 and i % 10 = 2"),
                new("FEW", "v = 0 and i % 100 = 0,20,40,60,80"),
                new("MANY", "v != 0"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "he"
            ],
            [
                new("ONE", "i = 1 and v = 0 or i = 0 and v != 0"),
                new("TWO", "i = 2 and v = 0"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "is"
            ],
            [
                new("ONE", "t = 0 and i % 10 = 1 and i % 100 != 11 or t % 10 = 1 and t % 100 != 11"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "iu", "naq", "sat", "se", "sma", "smi", "smj", "smn", "sms"
            ],
            [
                new("ONE", "n = 1"),
                new("TWO", "n = 2"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "kw"
            ],
            [
                new("ZERO", "n = 0"),
                new("ONE", "n = 1"),
                new("TWO", "n % 100 = 2,22,42,62,82 or n % 1000 = 0 and n % 100000 = 1000..20000,40000,60000,80000 or n != 0 and n % 1000000 = 100000"),
                new("FEW", "n % 100 = 3,23,43,63,83"),
                new("MANY", "n != 1 and n % 100 = 1,21,41,61,81"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "lag"
            ],
            [
                new("ZERO", "n = 0"),
                new("ONE", "i = 0,1 and n != 0"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "lt"
            ],
            [
                new("ONE", "n % 10 = 1 and n % 100 != 11..19"),
                new("FEW", "n % 10 = 2..9 and n % 100 != 11..19"),
                new("MANY", "f != 0"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "lv", "prg"
            ],
            [
                new("ZERO", "n % 10 = 0 or n % 100 = 11..19 or v = 2 and f % 100 = 11..19"),
                new("ONE", "n % 10 = 1 and n % 100 != 11 or v = 2 and f % 10 = 1 and f % 100 != 11 or v != 2 and f % 10 = 1"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "mk"
            ],
            [
                new("ONE", "v = 0 and i % 10 = 1 and i % 100 != 11 or f % 10 = 1 and f % 100 != 11"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "mo", "ro"
            ],
            [
                new("ONE", "i = 1 and v = 0"),
                new("FEW", "v != 0 or n = 0 or n != 1 and n % 100 = 1..19"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "mt"
            ],
            [
                new("ONE", "n = 1"),
                new("TWO", "n = 2"),
                new("FEW", "n = 0 or n % 100 = 3..10"),
                new("MANY", "n % 100 = 11..19"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "pl"
            ],
            [
                new("ONE", "i = 1 and v = 0"),
                new("FEW", "v = 0 and i % 10 = 2..4 and i % 100 != 12..14"),
                new("MANY", "v = 0 and i != 1 and i % 10 = 0..1 or v = 0 and i % 10 = 5..9 or v = 0 and i % 100 = 12..14"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "pt"
            ],
            [
                new("ONE", "i = 0..1"),
                new("MANY", "e = 0 and i != 0 and i % 1000000 = 0 and v = 0 or e != 0..5"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "ru", "uk"
            ],
            [
                new("ONE", "v = 0 and i % 10 = 1 and i % 100 != 11"),
                new("FEW", "v = 0 and i % 10 = 2..4 and i % 100 != 12..14"),
                new("MANY", "v = 0 and i % 10 = 0 or v = 0 and i % 10 = 5..9 or v = 0 and i % 100 = 11..14"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "sgs"
            ],
            [
                new("ONE", "n % 10 = 1 and n % 100 != 11"),
                new("TWO", "n = 2"),
                new("FEW", "n != 2 and n % 10 = 2..9 and n % 100 != 11..19"),
                new("MANY", "f != 0"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "shi"
            ],
            [
                new("ONE", "i = 0 or n = 1"),
                new("FEW", "n = 2..10"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "si"
            ],
            [
                new("ONE", "n = 0,1 or i = 0 and f = 1"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "sl"
            ],
            [
                new("ONE", "v = 0 and i % 100 = 1"),
                new("TWO", "v = 0 and i % 100 = 2"),
                new("FEW", "v = 0 and i % 100 = 3..4 or v != 0"),
                new("OTHER", ""),
            ]),
        new RuleSet(
            [
                "tzm"
            ],
            [
                new("ONE", "n = 0..1 or n = 11..99"),
                new("OTHER", ""),
            ]),
    ];

    /// <summary>
    /// The rules of every language CLDR publishes rules for, keyed by language.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<Rule>> Cardinal =
        Published
            .SelectMany(set => set.Languages.Select(language => (language, set.Rules)))
            .ToDictionary(entry => entry.language, entry => entry.Rules);

    /// <summary>
    /// The number of distinct rule sets CLDR publishes, which is how many the rules ReswPlus ships stand in for.
    /// </summary>
    public static int DistinctRuleSets => Published.Length;
}
