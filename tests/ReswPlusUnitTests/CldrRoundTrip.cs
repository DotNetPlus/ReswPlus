using System.Collections.Generic;
using System.Linq;
using CldrRuleImporter;
using ReswPlus.SourceGenerator.Plurals;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Checks that a rule survives being turned into objects and written back out.
/// </summary>
/// <remarks>
/// The rules are checked in as objects, and the condition a generated provider quotes above each branch is
/// written back out of them rather than carried alongside them. That is only honest if the two are the same
/// text, so every rule CLDR publishes is round tripped here: parse it, write it out, and compare.
/// <para>
/// This is what stops the quoted rule from drifting away from the code underneath it. If CLDR ever adopts a
/// spelling this does not reproduce, the rule is named rather than silently misquoted.
/// </para>
/// </remarks>
public class CldrRoundTrip
{
    public static TheoryData<string> Languages
    {
        get
        {
            var languages = new TheoryData<string>();

            foreach (var language in Cldr.Cardinal.Keys.OrderBy(key => key, System.StringComparer.Ordinal))
            {
                languages.Add(language);
            }

            return languages;
        }
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void EveryRuleIsWrittenBackOutAsCldrPublishesIt(string language)
    {
        var mismatches = new List<string>();

        foreach (var rule in Cldr.Cardinal[language])
        {
            if (rule.Condition.Length == 0)
            {
                continue;
            }

            var written = CldrRule.Parse(rule.Condition).ToCldr();

            if (written != rule.Condition)
            {
                mismatches.Add($"{language} {rule.Category.ToLowerInvariant()}: '{rule.Condition}' came back as '{written}'");
            }
        }

        Assert.Empty(mismatches);
    }

    [Fact]
    public void ARuleWrittenBackOutReadsBackAsTheSameRule()
    {
        var mismatches = new List<string>();

        foreach (var (language, rules) in Cldr.Cardinal)
        {
            foreach (var rule in rules.Where(candidate => candidate.Condition.Length != 0))
            {
                var parsed = CldrRule.Parse(rule.Condition);

                // Not the same object: the cache is keyed by text, and the text is what is being checked.
                if (!Equals(CldrRule.Parse(parsed.ToCldr()), parsed))
                {
                    mismatches.Add($"{language} {rule.Category.ToLowerInvariant()}: '{rule.Condition}'");
                }
            }
        }

        Assert.Empty(mismatches);
    }
}
