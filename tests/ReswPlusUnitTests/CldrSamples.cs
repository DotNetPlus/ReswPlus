using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ReswPlusUnitTests;

/// <summary>
/// Reads the quantities CLDR publishes alongside its rules.
/// </summary>
/// <remarks>
/// CLDR states, for every category of every language, the quantities that select it. Those lists are what make
/// a rule testable without anyone having to decide what a rule ought to mean.
/// </remarks>
internal static class CldrSamples
{
    /// <summary>
    /// Reads the quantities CLDR publishes as selecting a rule.
    /// </summary>
    /// <param name="rule">The rule as CLDR publishes it, sample lists included.</param>
    /// <returns>Each sample quantity, with the literal CLDR writes it as.</returns>
    /// <remarks>
    /// The decimal samples that a <see cref="double"/> cannot carry faithfully are left out. CLDR distinguishes
    /// <c>1.0</c> from <c>1</c>, and some rules read the number of decimals of the quantity, but the generated
    /// members take a <see cref="double"/>, which does not carry the trailing zeros the two differ by. Those
    /// samples are not something the providers can be asked to get right.
    /// </remarks>
    public static IEnumerable<(double Quantity, string Literal)> Read(string rule)
    {
        foreach (var section in rule.Split('@').Skip(1))
        {
            var space = section.IndexOf(' ');

            if (space < 0)
            {
                continue;
            }

            var kind = section.Substring(0, space);

            if (kind is not ("integer" or "decimal"))
            {
                continue;
            }

            foreach (var item in section.Substring(space + 1).Split(','))
            {
                var sample = item.Trim();

                // CLDR closes an open ended sample list with an ellipsis, and writes the quantities of the
                // compact notations -- "1c6" for one million -- that a plain double carries no trace of.
                if (sample.Length == 0 || sample == "\u2026" || sample.IndexOf('c') >= 0)
                {
                    continue;
                }

                foreach (var literal in Expand(sample))
                {
                    var quantity = double.Parse(literal, CultureInfo.InvariantCulture);

                    // The shortest representation that round trips is what the providers read the number of
                    // decimals of a quantity from, so a literal it doesn't reproduce is one a double cannot
                    // carry.
                    if (quantity.ToString("R", CultureInfo.InvariantCulture) == literal)
                    {
                        yield return (quantity, literal);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Expands a CLDR sample, which is either a single quantity or a range of them.
    /// </summary>
    /// <param name="sample">The sample to expand.</param>
    /// <returns>The literals the sample stands for.</returns>
    private static IEnumerable<string> Expand(string sample)
    {
        var separator = sample.IndexOf('~');

        if (separator < 0)
        {
            return [sample];
        }

        var from = sample.Substring(0, separator);
        var to = sample.Substring(separator + 1);

        // A range steps by one unit of the last decimal it is written with, so '0.0~0.9' stands for ten
        // quantities and '0~9' for ten others.
        var dot = from.IndexOf('.');
        var decimals = dot < 0 ? 0 : from.Length - dot - 1;
        var step = Math.Pow(10, -decimals);
        var format = decimals == 0 ? "0" : "0." + new string('0', decimals);

        var start = double.Parse(from, CultureInfo.InvariantCulture);
        var end = double.Parse(to, CultureInfo.InvariantCulture);
        var literals = new List<string>();

        for (var value = start; value <= end + (step / 2); value += step)
        {
            literals.Add(value.ToString(format, CultureInfo.InvariantCulture));
        }

        return literals;
    }
}
