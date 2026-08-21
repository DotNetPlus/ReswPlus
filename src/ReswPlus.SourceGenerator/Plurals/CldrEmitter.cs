using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReswPlus.SourceGenerator.ClassGenerators;

namespace ReswPlus.SourceGenerator.Plurals;

/// <summary>
/// Writes the C# of a plural provider from the rules CLDR publishes.
/// </summary>
/// <remarks>
/// The providers are not kept as files, and their code is not kept in a table either. Each one is written here,
/// while a project is generated, from the rules of the languages it implements, so that the code a project
/// compiles says what CLDR says by construction rather than by someone having transcribed it correctly, and so
/// that a new CLDR release is a change to the rules rather than an edit to thirty-four files.
/// <para>
/// What comes out is ordinary C#, the same shape the providers had when they were written by hand: no
/// reflection, nothing built at run time, and the rule it came from above each branch.
/// </para>
/// </remarks>
internal static class CldrEmitter
{
    /// <summary>
    /// The operands, in the order they are declared, and the expression each is read with.
    /// </summary>
    /// <remarks>
    /// A <see cref="double"/> carries no trailing zeros, so the operands that count the decimals with them and
    /// the operands that count them without come out equal. The exponent operands are absent because a
    /// relation reading one is decided when the provider is written, so nothing is left to compute.
    /// </remarks>
    private static readonly (CldrOperand Operand, string Expression)[] Operands =
    [
        (CldrOperand.IntegerPart, "System.Math.Truncate(n)"),
        (CldrOperand.DecimalCount, "n.GetNumberOfDigitsAfterDecimal()"),
        (CldrOperand.DecimalCountWithoutTrailingZeros, "n.GetNumberOfDigitsAfterDecimal()"),
        (CldrOperand.Decimals, "n.DigitsAfterDecimal()"),
        (CldrOperand.DecimalsWithoutTrailingZeros, "n.DigitsAfterDecimal()"),
    ];

    /// <summary>
    /// Writes the provider of a set of rules.
    /// </summary>
    /// <param name="className">The name to give the class.</param>
    /// <param name="rules">The rules, in the order CLDR publishes them.</param>
    /// <param name="languages">The languages CLDR publishes those rules for, named in the generated code.</param>
    /// <returns>The source of a provider implementing them.</returns>
    public static string Emit(string className, IReadOnlyList<CldrPluralRule> rules, IReadOnlyList<string>? languages = null, string version = "")
    {
        var conditions = new List<(PluralCategory Category, string Condition, string Source)>();

        foreach (var rule in rules)
        {
            if (rule.Condition is not { } parsed)
            {
                continue;
            }

            var condition = parsed.ToCSharp();

            // A rule that can never hold is left out rather than written as an unreachable branch: the ones
            // reading a compact notation's exponent are all of this kind, since a quantity never carries one.
            if (condition == CldrConditions.False)
            {
                continue;
            }

            conditions.Add((rule.Category, condition, parsed.ToCldr()));

            // Nothing after a rule that always holds can be reached.
            if (condition == CldrConditions.True)
            {
                break;
            }
        }

        var body = new StringBuilder();

        // Read from the code that survived rather than from the rules it was written from: an alternative
        // folded away for reading an exponent takes its other operands with it, and a local declared for one
        // of those would be computed for nothing.
        var emitted = string.Join(" ", conditions.Select(entry => entry.Condition));

        // Only the operands the rules read are declared, so that nothing is computed twice and nothing is
        // computed for nothing.
        foreach (var (operand, expression) in Operands.Where(candidate => Reads(emitted, candidate.Operand)))
        {
            body.Append("            double ").Append(operand.Letter()).Append(" = ").Append(expression).AppendLine(";");
        }

        if (body.Length != 0)
        {
            body.AppendLine();
        }

        var alwaysReturns = false;

        foreach (var (category, condition, published) in conditions)
        {
            body.Append("            // ").Append(category.ToString().ToLowerInvariant()).Append(": ").AppendLine(published);

            // A rule that always holds is written as the answer itself: wrapping it in a condition would leave
            // everything after it unreachable, which the compiler warns about.
            if (condition == CldrConditions.True)
            {
                body.Append("            return PluralTypeEnum.").Append(Name(category)).AppendLine(";");
                alwaysReturns = true;
                break;
            }

            body.Append("            if (").Append(Unwrap(condition)).AppendLine(")");
            body.AppendLine("            {");
            body.Append("                return PluralTypeEnum.").Append(Name(category)).AppendLine(";");
            body.AppendLine("            }");
            body.AppendLine();
        }

        if (!alwaysReturns)
        {
            body.AppendLine("            return PluralTypeEnum.OTHER;");
        }

        var source = new StringBuilder();

        // The helpers live in another namespace, and a provider whose rules need none of them must not import
        // it: the generated code is compiled with warnings treated as errors by some projects.
        if (NeedsHelpers(body.ToString()))
        {
            source.AppendLine("using _ReswPlus_AutoGenerated.Utils;");
            source.AppendLine();
        }

        source.AppendLine("namespace _ReswPlus_AutoGenerated.Plurals");
        source.AppendLine("{");

        // A reader of the generated file should not have to guess where the rules came from, so the class says
        // it: the release, the languages, and the syntax the conditions quoted below are written in. The name of
        // the class is taken from the first of its languages and means nothing on its own, so the languages it
        // actually decides for are spelled out rather than left to be inferred from it.
        source.Append("    /// <summary>The plural rules Unicode CLDR ").Append(version)
            .Append(" publishes");

        if (languages is { Count: > 0 })
        {
            source.Append(" for '").Append(languages[0]).Append('\'');

            if (languages.Count > 1)
            {
                source.Append(" and ").Append(languages.Count - 1)
                    .Append(languages.Count == 2 ? " other language" : " other languages");
            }
        }

        source.AppendLine(".</summary>");
        source.AppendLine("    /// <remarks>");

        if (languages is { Count: > 1 })
        {
            source.Append("    /// Shared by ").Append(string.Join(", ", languages)).AppendLine(".");
            source.AppendLine("    /// <para>");
        }

        source.AppendLine("    /// The condition quoted above each branch is the rule as CLDR publishes it, in the plural rule");
        source.AppendLine("    /// syntax of UTS #35: https://unicode.org/reports/tr35/tr35-numbers.html#Language_Plural_Rules");

        if (languages is { Count: > 1 })
        {
            source.AppendLine("    /// </para>");
        }

        source.AppendLine("    /// </remarks>");
        source.Append("    internal sealed class ").Append(className).AppendLine(" : IPluralProvider");
        source.AppendLine("    {");
        source.AppendLine("        public PluralTypeEnum ComputePlural(double n)");
        source.AppendLine("        {");
        source.Append(body);
        source.AppendLine("        }");
        source.AppendLine("    }");
        source.AppendLine("}");

        return source.ToString();
    }

    /// <summary>
    /// Gets whether emitted code reads an operand.
    /// </summary>
    /// <param name="code">The conditions that were emitted.</param>
    /// <param name="operand">The operand.</param>
    /// <returns><see langword="true"/> when a local has to be declared for it.</returns>
    /// <remarks>
    /// The letter is matched as a whole word, so that the <c>n</c> of <c>n.IsInt()</c> counts and the <c>n</c>
    /// of <c>Between</c> does not.
    /// </remarks>
    private static bool Reads(string code, CldrOperand operand)
    {
        var letter = operand.Letter();

        for (var index = 0; index < code.Length; index++)
        {
            if (code[index] != letter
                || (index != 0 && (char.IsLetterOrDigit(code[index - 1]) || code[index - 1] == '_'))
                || (index + 1 < code.Length && (char.IsLetterOrDigit(code[index + 1]) || code[index + 1] == '_')))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Names a category as the generated enum spells it.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <returns>The name of the matching <c>PluralTypeEnum</c> member.</returns>
    private static string Name(PluralCategory category)
    {
        return category.ToString().ToUpperInvariant();
    }

    /// <summary>
    /// Gets whether a body calls any of the helpers the generated code shares.
    /// </summary>
    /// <param name="body">The body of the method.</param>
    /// <returns><see langword="true"/> when the helpers have to be imported.</returns>
    private static bool NeedsHelpers(string body)
    {
        return body.IndexOf("IsBetween", StringComparison.Ordinal) >= 0
            || body.IndexOf("IsInt", StringComparison.Ordinal) >= 0
            || body.IndexOf("AfterDecimal", StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Removes the brackets around a condition that is already bracketed by the <c>if</c> holding it.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <returns>The condition without a redundant outer pair of brackets.</returns>
    private static string Unwrap(string condition)
    {
        if (condition.Length < 2 || condition[0] != '(' || condition[condition.Length - 1] != ')')
        {
            return condition;
        }

        var depth = 0;

        for (var index = 0; index < condition.Length; index++)
        {
            depth += condition[index] switch { '(' => 1, ')' => -1, _ => 0 };

            // The opening bracket closed before the end, so it does not wrap the whole condition.
            if (depth == 0 && index != condition.Length - 1)
            {
                return condition;
            }
        }

        return condition.Substring(1, condition.Length - 2);
    }
}
