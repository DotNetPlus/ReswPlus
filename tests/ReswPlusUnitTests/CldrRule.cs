using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ReswPlusUnitTests;

/// <summary>
/// Reads the plural rule syntax of UTS #35 and answers which category a quantity selects.
/// </summary>
/// <remarks>
/// This is deliberately a second, independent reading of the rules ReswPlus ships by hand. The providers turn
/// a CLDR condition into C# once, when someone writes them; this turns the same condition into a decision
/// every time the tests run. Where the two disagree, one of them transcribed the rule wrong.
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
        var operands = Operands.Of(quantity);

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
    /// <returns>The condition, as something that can be asked whether it holds.</returns>
    public static ICondition Parse(string condition)
    {
        return Parsed.TryGetValue(condition, out var known) ? known : Parsed[condition] = ParseOr(condition);
    }

    private static readonly Dictionary<string, ICondition> Parsed = [];

    private static ICondition ParseOr(string text)
    {
        var alternatives = Split(text, " or ").Select(ParseAnd).ToList();

        return alternatives.Count == 1 ? alternatives[0] : new AnyOf(alternatives);
    }

    private static ICondition ParseAnd(string text)
    {
        var relations = Split(text, " and ").Select(ParseRelation).ToList();

        return relations.Count == 1 ? relations[0] : new AllOf(relations);
    }

    private static ICondition ParseRelation(string text)
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

        return new Relation(subject[0], modulus, negated, ParseRanges(values));
    }

    private static List<(long From, long To)> ParseRanges(string text)
    {
        var ranges = new List<(long From, long To)>();

        foreach (var item in text.Split(','))
        {
            var value = item.Trim();
            var separator = value.IndexOf("..", StringComparison.Ordinal);

            if (separator < 0)
            {
                var single = long.Parse(value, CultureInfo.InvariantCulture);
                ranges.Add((single, single));
            }
            else
            {
                ranges.Add((
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

    /// <summary>
    /// A condition that can be asked whether it holds for a quantity.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Gets whether the condition holds for the operands of a quantity.
        /// </summary>
        /// <param name="operands">The operands of the quantity.</param>
        /// <returns><see langword="true"/> when the condition holds.</returns>
        bool Holds(Operands operands);
    }

    private sealed record AnyOf(IReadOnlyList<ICondition> Alternatives) : ICondition
    {
        public bool Holds(Operands operands) => Alternatives.Any(alternative => alternative.Holds(operands));
    }

    private sealed record AllOf(IReadOnlyList<ICondition> Parts) : ICondition
    {
        public bool Holds(Operands operands) => Parts.All(part => part.Holds(operands));
    }

    /// <summary>
    /// One operand, optionally taken modulo a value, tested against a list of values and ranges.
    /// </summary>
    private sealed record Relation(char Operand, long Modulus, bool Negated, IReadOnlyList<(long From, long To)> Ranges)
        : ICondition
    {
        public bool Holds(Operands operands)
        {
            var value = operands.Of(Operand);

            if (Modulus != 0)
            {
                value %= Modulus;
            }

            // CLDR's ranges hold integers only, so a quantity with a fractional part matches nothing: 'n = 0..1'
            // is what makes 1.5 take the fallback category in the languages that only decline whole numbers.
            var matches = value == Math.Truncate(value)
                && Ranges.Any(range => value >= range.From && value <= range.To);

            return Negated ? !matches : matches;
        }
    }

    /// <summary>
    /// The operands UTS #35 derives from a quantity.
    /// </summary>
    /// <param name="N">The absolute value of the quantity.</param>
    /// <param name="I">The integer part.</param>
    /// <param name="V">The number of visible decimals.</param>
    /// <param name="W">The number of visible decimals without trailing zeros.</param>
    /// <param name="F">The visible decimals, as an integer.</param>
    /// <param name="T">The visible decimals without trailing zeros, as an integer.</param>
    public readonly record struct Operands(double N, long I, int V, int W, long F, long T)
    {
        /// <summary>
        /// Derives the operands of a quantity.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <returns>Its operands.</returns>
        /// <remarks>
        /// The decimals are read from the shortest representation that round trips, which is the same thing the
        /// providers read them from, and is all a <see cref="double"/> carries: it holds no trailing zeros, so
        /// the operands that count them and the operands that don't come out equal here.
        /// </remarks>
        public static Operands Of(double quantity)
        {
            var value = Math.Abs(quantity);
            var text = value.ToString("R", CultureInfo.InvariantCulture);
            var point = text.IndexOf('.');
            var decimals = point < 0 ? string.Empty : text.Substring(point + 1);

            // An exponent means a quantity far past the range any plural rule distinguishes, and no decimals
            // that can be read off the text.
            if (decimals.IndexOf('E') >= 0)
            {
                decimals = string.Empty;
            }

            var trimmed = decimals.TrimEnd('0');

            return new Operands(
                value,
                (long)Math.Truncate(value),
                decimals.Length,
                trimmed.Length,
                decimals.Length == 0 ? 0 : long.Parse(decimals, CultureInfo.InvariantCulture),
                trimmed.Length == 0 ? 0 : long.Parse(trimmed, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Gets an operand by the letter CLDR writes it as.
        /// </summary>
        /// <param name="operand">The letter.</param>
        /// <returns>The value of the operand.</returns>
        /// <remarks>
        /// <c>c</c> and <c>e</c> are the exponent of a compact notation -- "1c6" for a million -- which a
        /// quantity that reaches a provider as a <see cref="double"/> never carries, so both are zero.
        /// </remarks>
        public double Of(char operand)
        {
            return operand switch
            {
                'n' => N,
                'i' => I,
                'v' => V,
                'w' => W,
                'f' => F,
                't' => T,
                'c' or 'e' => 0,
                _ => throw new FormatException($"'{operand}' is not one of CLDR's operands."),
            };
        }
    }
}
