using System;
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
    public void AConditionNestedTheWayCldrCannotWriteIsRefused()
    {
        // 'and' binds tighter than 'or' and CLDR's syntax has no brackets, so 'a or b' inside an 'and' cannot
        // be written back out. Parsing never produces that shape; building it by hand has to say so rather
        // than emit text that reads back as a different rule.
        var nested = new CldrAllOf(
        [
            new CldrRelation(CldrOperand.IntegerPart, 0, false, [new CldrRange(1, 1)]),
            new CldrAnyOf(
            [
                new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new CldrRange(2, 2)]),
                new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new CldrRange(3, 3)]),
            ]),
        ]);

        Assert.Throws<InvalidOperationException>(() => nested.ToCldr());

        // The same shape is written as C#, where brackets exist, and keeps its meaning.
        Assert.Equal("i == 1 && (n == 2 || n == 3)", nested.ToCSharp());
    }
}
