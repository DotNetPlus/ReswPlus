using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReswPlus.SourceGenerator.Plurals;

/// <summary>
/// Writes the C# of a plural provider from the rules CLDR publishes.
/// </summary>
/// <remarks>
/// The providers are not kept as files. Each one is written here from the rules of the language it implements,
/// so that the code a project compiles says what CLDR says by construction rather than by someone having
/// transcribed it correctly, and so that a new CLDR release is a change to the rules rather than an edit to
/// thirty-four files.
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
    /// the operands that count them without come out equal. <c>c</c> and <c>e</c> are the exponent of a compact
    /// notation, which a quantity never arrives here carrying.
    /// </remarks>
    private static readonly (char Operand, string Expression)[] Operands =
    [
        ('i', "System.Math.Truncate(n)"),
        ('v', "n.GetNumberOfDigitsAfterDecimal()"),
        ('w', "n.GetNumberOfDigitsAfterDecimal()"),
        ('f', "n.DigitsAfterDecimal()"),
        ('t', "n.DigitsAfterDecimal()"),
        ('c', "0"),
        ('e', "0"),
    ];

    /// <summary>
    /// Writes the provider of a set of rules.
    /// </summary>
    /// <param name="className">The name to give the class.</param>
    /// <param name="rules">The rules, in the order CLDR publishes them.</param>
    /// <returns>The source of a provider implementing them.</returns>
    public static string Emit(string className, IReadOnlyList<CldrPublishedRules.Rule> rules)
    {
        var used = new HashSet<char>();
        var conditions = new List<(string Category, string Condition, string Source)>();

        foreach (var rule in rules)
        {
            if (rule.Condition.Length == 0)
            {
                continue;
            }

            var parsed = CldrRule.Parse(rule.Condition);
            var condition = parsed.ToCSharp();

            // A rule that can never hold is left out rather than written as an unreachable branch: the ones
            // reading a compact notation's exponent are all of this kind, since a quantity never carries one.
            if (condition == CldrRule.False)
            {
                continue;
            }

            parsed.CollectOperands(used);
            conditions.Add((rule.Category, condition, rule.Condition));

            // Nothing after a rule that always holds can be reached.
            if (condition == CldrRule.True)
            {
                break;
            }
        }

        var body = new StringBuilder();

        // Only the operands the rules read are declared, so that nothing is computed twice and nothing is
        // computed for nothing.
        foreach (var (operand, expression) in Operands.Where(candidate => used.Contains(candidate.Operand)))
        {
            body.Append("            double ").Append(operand).Append(" = ").Append(expression).AppendLine(";");
        }

        if (body.Length != 0)
        {
            body.AppendLine();
        }

        var alwaysReturns = false;

        foreach (var (category, condition, published) in conditions)
        {
            body.Append("            // ").Append(category.ToLowerInvariant()).Append(": ").AppendLine(published);

            // A rule that always holds is written as the answer itself: wrapping it in a condition would leave
            // everything after it unreachable, which the compiler warns about.
            if (condition == CldrRule.True)
            {
                body.Append("            return PluralTypeEnum.").Append(category).AppendLine(";");
                alwaysReturns = true;
                break;
            }

            body.Append("            if (").Append(Unwrap(condition)).AppendLine(")");
            body.AppendLine("            {");
            body.Append("                return PluralTypeEnum.").Append(category).AppendLine(";");
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
