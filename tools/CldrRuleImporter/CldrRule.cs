using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReswPlus.SourceGenerator.Plurals;

namespace CldrRuleImporter;

/// <summary>
/// Reads the plural rule syntax of UTS #35 into the objects a rule is made of.
/// </summary>
/// <remarks>
/// This is the only thing that understands CLDR's rule syntax, and it runs offline: what is checked in is the
/// object graph it produces. Turning that graph into C# is the generator's job, so a change to how a rule is
/// written and a change to what the rules say stay separate.
/// <para>
/// The grammar is small: a condition is a list of alternatives separated by <c>or</c>, each a list of relations
/// separated by <c>and</c>, each relation an operand -- optionally taken modulo a value -- tested against a
/// list of values and ranges. <c>and</c> binds tighter than <c>or</c>, which is the part of the syntax a
/// hand written rule is most likely to get wrong.
/// </para>
/// </remarks>
internal static class CldrRule
{
    /// <summary>
    /// The conditions read so far.
    /// </summary>
    private static readonly ConcurrentDictionary<string, ICldrCondition> Parsed = new();

    /// <summary>
    /// Selects the category a quantity takes under a set of rules.
    /// </summary>
    /// <param name="rules">The rules of a language, in the order CLDR publishes them.</param>
    /// <param name="quantity">The quantity to categorise.</param>
    /// <returns>The name of the category selected, upper cased.</returns>
    /// <remarks>
    /// CLDR's fallback category carries no condition and is only reached when no other rule matches, so it is
    /// skipped while looking for a match rather than being allowed to match everything.
    /// </remarks>
    public static string Select(IReadOnlyList<CldrPublishedRules.Rule> rules, double quantity)
    {
        var operands = CldrOperands.Of(quantity);

        foreach (var rule in rules)
        {
            if (rule.Condition.Length != 0 && Parse(rule.Condition).Holds(operands))
            {
                return rule.Category;
            }
        }

        return "OTHER";
    }

    /// <summary>
    /// Reads a condition.
    /// </summary>
    /// <param name="condition">The condition, in the syntax of UTS #35.</param>
    /// <returns>The condition, as the objects it is made of.</returns>
    public static ICldrCondition Parse(string condition)
    {
        return Parsed.GetOrAdd(condition, static text => ParseOr(text));
    }

    private static ICldrCondition ParseOr(string text)
    {
        var alternatives = Split(text, " or ").Select(ParseAnd).ToList();

        return alternatives.Count == 1 ? alternatives[0] : new CldrAnyOf(alternatives);
    }

    private static ICldrCondition ParseAnd(string text)
    {
        var relations = Split(text, " and ").Select(ParseRelation).ToList();

        return relations.Count == 1 ? relations[0] : new CldrAllOf(relations);
    }

    private static ICldrCondition ParseRelation(string text)
    {
        text = text.Trim();

        var negated = false;
        var comparison = text.IndexOf("!=", StringComparison.Ordinal);

        if (comparison >= 0)
        {
            negated = true;
        }
        else
        {
            comparison = text.IndexOf('=');

            if (comparison < 0)
            {
                throw new FormatException($"'{text}' is not a relation: it compares nothing.");
            }
        }

        var subject = text.Substring(0, comparison).Trim();
        var values = text.Substring(comparison + (negated ? 2 : 1)).Trim();

        var modulus = 0L;
        var modulo = subject.IndexOf('%');

        if (modulo >= 0)
        {
            modulus = long.Parse(subject.Substring(modulo + 1).Trim(), CultureInfo.InvariantCulture);
            subject = subject.Substring(0, modulo).Trim();
        }

        if (subject.Length != 1)
        {
            throw new FormatException($"'{subject}' is not an operand: CLDR's operands are single letters.");
        }

        return new CldrRelation(subject[0], modulus, negated, ParseRanges(values));
    }

    private static List<CldrRange> ParseRanges(string text)
    {
        var ranges = new List<CldrRange>();

        foreach (var item in text.Split(','))
        {
            var value = item.Trim();
            var separator = value.IndexOf("..", StringComparison.Ordinal);

            if (separator < 0)
            {
                var single = long.Parse(value, CultureInfo.InvariantCulture);
                ranges.Add(new CldrRange(single, single));
            }
            else
            {
                ranges.Add(new CldrRange(
                    long.Parse(value.Substring(0, separator), CultureInfo.InvariantCulture),
                    long.Parse(value.Substring(separator + 2), CultureInfo.InvariantCulture)));
            }
        }

        return ranges;
    }

    /// <summary>
    /// Splits a condition on a keyword, which CLDR always writes surrounded by spaces.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <param name="keyword">The keyword to split on.</param>
    /// <returns>The parts between the keyword.</returns>
    private static IEnumerable<string> Split(string text, string keyword)
    {
        return text.Split([keyword], StringSplitOptions.None).Select(part => part.Trim());
    }
}
