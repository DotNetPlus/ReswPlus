using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ReswPlus.SourceGenerator.Plurals;

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

    /// <summary>
    /// The text of a condition that always holds.
    /// </summary>
    public const string True = "true";

    /// <summary>
    /// The text of a condition that never holds.
    /// </summary>
    public const string False = "false";

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

        /// <summary>
        /// Writes the condition as the C# that decides it.
        /// </summary>
        /// <returns>An expression over locals named after the operands it reads.</returns>
        string ToCSharp();

        /// <summary>
        /// Adds the operands the condition reads to a set.
        /// </summary>
        /// <param name="operands">The set to add to.</param>
        void CollectOperands(ISet<char> operands);
    }

    private sealed record AnyOf(IReadOnlyList<ICondition> Alternatives) : ICondition
    {
        public bool Holds(Operands operands) => Alternatives.Any(alternative => alternative.Holds(operands));

        public string ToCSharp()
        {
            var kept = new List<string>();

            foreach (var alternative in Alternatives)
            {
                var text = alternative.ToCSharp();

                if (text == True)
                {
                    return True;
                }

                if (text != False)
                {
                    kept.Add(text);
                }
            }

            return kept.Count == 0 ? False : string.Join(" || ", kept);
        }

        public void CollectOperands(ISet<char> operands)
        {
            foreach (var alternative in Alternatives)
            {
                alternative.CollectOperands(operands);
            }
        }
    }

    private sealed record AllOf(IReadOnlyList<ICondition> Parts) : ICondition
    {
        public bool Holds(Operands operands) => Parts.All(part => part.Holds(operands));

        public string ToCSharp()
        {
            var kept = new List<string>();

            foreach (var part in Parts)
            {
                var text = part.ToCSharp();

                if (text == False)
                {
                    return False;
                }

                if (text == True)
                {
                    continue;
                }

                // 'and' binds tighter than 'or', so an alternative nested inside one keeps its brackets.
                kept.Add(part is AnyOf ? $"({text})" : text);
            }

            return kept.Count switch
            {
                0 => True,
                1 => kept[0],
                _ => string.Join(" && ", kept),
            };
        }

        public void CollectOperands(ISet<char> operands)
        {
            foreach (var part in Parts)
            {
                part.CollectOperands(operands);
            }
        }
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

        public string ToCSharp()
        {
            // A quantity that reaches a provider carries no compact notation, so the operands holding its
            // exponent are zero and every relation reading one is decided here rather than at run time.
            if (Operand is 'c' or 'e')
            {
                return Holds(default) ? True : False;
            }

            var subject = Modulus == 0 ? Operand.ToString() : $"({Operand} % {Modulus})";
            var needsIntegerGuard = Operand == 'n' && Ranges.Any(range => range.From != range.To);

            // A single value reads better compared directly than negated, and it is the commonest relation of
            // all: 'i % 100 != 11' rather than '!(i % 100 == 11)'.
            if (Negated && !needsIntegerGuard && Ranges.Count == 1 && Ranges[0].From == Ranges[0].To)
            {
                return $"{subject} != {Ranges[0].From}";
            }

            var tests = Ranges.Select(range => range.From == range.To
                ? $"{subject} == {range.From}"
                : $"{subject}.IsBetween({range.From}, {range.To})");

            var matches = string.Join(" || ", tests);

            if (Ranges.Count > 1)
            {
                matches = $"({matches})";
            }

            // Only 'n' can carry a fractional part, and a range holds whole numbers only, so 'n % 10 = 1' has
            // to stay false for 11.5 rather than match the way 11 does.
            if (needsIntegerGuard)
            {
                matches = $"({subject}.IsInt() && {matches})";
            }

            return Negated ? $"!({matches})" : matches;
        }

        public void CollectOperands(ISet<char> operands)
        {
            // The exponent operands are folded away, so nothing has to be computed for them.
            if (Operand is not ('c' or 'e'))
            {
                operands.Add(Operand);
            }
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
    public readonly record struct Operands(double N, double I, int V, int W, long F, long T)
    {
        /// <summary>
        /// The general formats, by rising number of significant digits, a quantity is searched through.
        /// </summary>
        private static readonly string[] GeneralFormats =
        [
            "G1", "G2", "G3", "G4", "G5", "G6", "G7", "G8",
            "G9", "G10", "G11", "G12", "G13", "G14", "G15"
        ];

        /// <summary>
        /// Derives the operands of a quantity.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <returns>Its operands.</returns>
        /// <remarks>
        /// Which decimals a quantity has is read the same way the shipped helper reads them, deliberately: what
        /// is being checked here is the rules, not the reading of a <see cref="double"/>, and a second answer to
        /// "how many decimals does this have" would only report the two readings disagreeing.
        /// <para>
        /// A <see cref="double"/> carries no trailing zeros, so the operands counting the decimals with them and
        /// the ones counting without come out equal.
        /// </para>
        /// </remarks>
        public static Operands Of(double quantity)
        {
            var value = Math.Abs(quantity);
            var decimals = VisibleDecimals(value);
            var trimmed = decimals.TrimEnd('0');

            return new Operands(
                value,
                Math.Truncate(value),
                decimals.Length,
                trimmed.Length,
                ReadDigits(decimals),
                ReadDigits(trimmed));
        }

        /// <summary>
        /// Reads a run of digits, or zero when there are more of them than a quantity can be declined on.
        /// </summary>
        private static long ReadDigits(string digits)
        {
            return digits.Length != 0
                && long.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : 0;
        }

        /// <summary>
        /// Returns the decimals of a quantity, or an empty string when it has none.
        /// </summary>
        private static string VisibleDecimals(double number)
        {
            var shortest = ShortestRoundTrip(number);
            var mantissaAndExponent = shortest.Split('E');
            var mantissa = mantissaAndExponent[0].Split('.');
            var integerDigits = mantissa[0];
            var decimalDigits = mantissa.Length > 1 ? mantissa[1] : string.Empty;

            if (mantissaAndExponent.Length == 1)
            {
                return decimalDigits;
            }

            // Small and large quantities come back in scientific notation, which has to be expanded before its
            // decimals can be read: those of '1E-06' are five zeros and a one, not none at all.
            var exponent = int.Parse(mantissaAndExponent[1], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            var digits = integerDigits + decimalDigits;
            var pointPosition = integerDigits.Length + exponent;

            if (pointPosition >= digits.Length)
            {
                return string.Empty;
            }

            return pointPosition <= 0
                ? new string('0', -pointPosition) + digits
                : digits.Substring(pointPosition);
        }

        /// <summary>
        /// Returns the shortest representation of a quantity that reads back as the same quantity.
        /// </summary>
        private static string ShortestRoundTrip(double number)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
            {
                return "0";
            }

            foreach (var format in GeneralFormats)
            {
                var candidate = number.ToString(format, CultureInfo.InvariantCulture);

                if (double.TryParse(candidate, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                    && parsed == number)
                {
                    return candidate;
                }
            }

            return number.ToString("R", CultureInfo.InvariantCulture);
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
