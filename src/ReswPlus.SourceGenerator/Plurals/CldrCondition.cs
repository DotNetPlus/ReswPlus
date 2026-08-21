using System;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.SourceGenerator.Plurals;

/// <summary>
/// A plural rule condition, as the objects a rule is made of.
/// </summary>
/// <remarks>
/// The rules Unicode CLDR publishes are written in the syntax of UTS #35, which is read once, offline, by
/// <c>tools/CldrRuleImporter</c>. What is checked in is the result: the conditions as an object graph, held in
/// <see cref="CldrPluralRules"/>. Turning them into C# happens here, when a project is compiled, so the code a
/// provider is made of is decided by the generator rather than baked into a table.
/// <para>
/// The shape is small: a condition is a list of alternatives (<see cref="CldrAnyOf"/>), each a list of parts
/// (<see cref="CldrAllOf"/>), each part an operand -- optionally taken modulo a value -- tested against a list
/// of values and ranges (<see cref="CldrRelation"/>).
/// </para>
/// </remarks>
internal interface ICldrCondition
{
    /// <summary>
    /// Gets whether the condition holds for the operands of a quantity.
    /// </summary>
    /// <param name="operands">The operands of the quantity.</param>
    /// <returns><see langword="true"/> when the condition holds.</returns>
    bool Holds(CldrOperands operands);

    /// <summary>
    /// Writes the condition as the C# that decides it.
    /// </summary>
    /// <returns>An expression over locals named after the operands it reads.</returns>
    string ToCSharp();

    /// <summary>
    /// Writes the condition back out in the syntax CLDR publishes it in.
    /// </summary>
    /// <returns>The condition, as UTS #35 writes it.</returns>
    /// <remarks>
    /// The generated provider quotes the rule above the code written from it. Rendering that quote from the
    /// objects rather than carrying the published text alongside them keeps the rules a single
    /// representation: there is nothing for a refresh to leave saying one thing while the objects say another.
    /// </remarks>
    string ToCldr();

    /// <summary>
    /// Adds the operands the condition reads to a set.
    /// </summary>
    /// <param name="operands">The set to add to.</param>
    void CollectOperands(ISet<char> operands);
}

/// <summary>
/// The expressions a condition folds away to when it is decided without reading a quantity.
/// </summary>
internal static class CldrConditions
{
    /// <summary>
    /// A condition that always holds.
    /// </summary>
    public const string True = "true";

    /// <summary>
    /// A condition that never holds.
    /// </summary>
    public const string False = "false";
}

/// <summary>
/// Alternatives, of which one holding is enough. CLDR writes them separated by <c>or</c>.
/// </summary>
/// <param name="Alternatives">The alternatives.</param>
internal sealed record CldrAnyOf(IReadOnlyList<ICldrCondition> Alternatives) : ICldrCondition
{
    public bool Holds(CldrOperands operands) => Alternatives.Any(alternative => alternative.Holds(operands));

    public string ToCSharp()
    {
        var kept = new List<string>();

        foreach (var alternative in Alternatives)
        {
            var text = alternative.ToCSharp();

            if (text == CldrConditions.True)
            {
                return CldrConditions.True;
            }

            if (text != CldrConditions.False)
            {
                kept.Add(text);
            }
        }

        return kept.Count == 0 ? CldrConditions.False : string.Join(" || ", kept);
    }

    public string ToCldr() => string.Join(" or ", Alternatives.Select(alternative => alternative.ToCldr()));

    public void CollectOperands(ISet<char> operands)
    {
        foreach (var alternative in Alternatives)
        {
            alternative.CollectOperands(operands);
        }
    }
}

/// <summary>
/// Parts, all of which have to hold. CLDR writes them separated by <c>and</c>.
/// </summary>
/// <param name="Parts">The parts.</param>
internal sealed record CldrAllOf(IReadOnlyList<ICldrCondition> Parts) : ICldrCondition
{
    public bool Holds(CldrOperands operands) => Parts.All(part => part.Holds(operands));

    public string ToCSharp()
    {
        var kept = new List<string>();

        foreach (var part in Parts)
        {
            var text = part.ToCSharp();

            if (text == CldrConditions.False)
            {
                return CldrConditions.False;
            }

            if (text == CldrConditions.True)
            {
                continue;
            }

            // 'and' binds tighter than 'or', so an alternative nested inside one keeps its brackets.
            kept.Add(part is CldrAnyOf ? $"({text})" : text);
        }

        return kept.Count switch
        {
            0 => CldrConditions.True,
            1 => kept[0],
            _ => string.Join(" && ", kept),
        };
    }

    // 'and' binds tighter than 'or' and CLDR's syntax has no brackets, so an alternative can only ever be
    // nested inside an 'and' the other way round. Parsing produces alternatives of parts, never the reverse.
    public string ToCldr() => string.Join(" and ", Parts.Select(part => part.ToCldr()));

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
/// <param name="Operand">The letter CLDR writes the operand as.</param>
/// <param name="Modulus">The value the operand is taken modulo of, or zero when it is read whole.</param>
/// <param name="Negated">Whether the relation holds when the operand is <em>not</em> one of the values.</param>
/// <param name="Ranges">The values and ranges the operand is tested against, each inclusive of both ends.</param>
internal sealed record CldrRelation(char Operand, long Modulus, bool Negated, IReadOnlyList<CldrRange> Ranges)
    : ICldrCondition
{
    public bool Holds(CldrOperands operands)
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
            return Holds(default) ? CldrConditions.True : CldrConditions.False;
        }

        var subject = Modulus == 0 ? Operand.ToString() : $"({Operand} % {Modulus})";
        var needsIntegerGuard = Operand == 'n' && Ranges.Any(range => range.From != range.To);

        // A single value reads better compared directly than negated, and it is the commonest relation of
        // all: 'i % 100 != 11' rather than '!(i % 100 == 11)'.
        if (Negated && Ranges.Count == 1 && Ranges[0].From == Ranges[0].To)
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

    public string ToCldr()
    {
        var subject = Modulus == 0 ? Operand.ToString() : $"{Operand} % {Modulus}";
        var values = string.Join(",", Ranges.Select(range =>
            range.From == range.To ? range.From.ToString() : $"{range.From}..{range.To}"));

        return $"{subject} {(Negated ? "!=" : "=")} {values}";
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
/// A run of values an operand is tested against, inclusive of both ends. A single value has equal ends.
/// </summary>
/// <param name="From">The first value.</param>
/// <param name="To">The last value.</param>
internal readonly record struct CldrRange(long From, long To);
