using System;
using System.Globalization;

namespace ReswPlus.SourceGenerator.Plurals;

/// <summary>
/// The operands UTS #35 derives from a quantity.
/// </summary>
/// <param name="N">The absolute value of the quantity.</param>
/// <param name="I">The integer part.</param>
/// <param name="V">The number of visible decimals.</param>
/// <param name="W">The number of visible decimals without trailing zeros.</param>
/// <param name="F">The visible decimals, as an integer.</param>
/// <param name="T">The visible decimals without trailing zeros, as an integer.</param>
/// <remarks>
/// A rule is turned into C# without ever deriving these -- the generated provider computes the operands it
/// reads itself. They exist so that a rule can also be <em>decided</em> here: the exponent operands are folded
/// away at generation time, and the tests replay CLDR's own sample quantities through the rules to check the
/// code written from them agrees.
/// </remarks>
internal readonly record struct CldrOperands(double N, double I, int V, int W, long F, long T)
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
    /// is being checked is the rules, not the reading of a <see cref="double"/>, and a second answer to
    /// "how many decimals does this have" would only report the two readings disagreeing.
    /// <para>
    /// A <see cref="double"/> carries no trailing zeros, so the operands counting the decimals with them and
    /// the ones counting without come out equal.
    /// </para>
    /// </remarks>
    public static CldrOperands Of(double quantity)
    {
        var value = Math.Abs(quantity);
        var decimals = VisibleDecimals(value);
        var trimmed = decimals.TrimEnd('0');

        return new CldrOperands(
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
    /// <param name="operand">The operand.</param>
    /// <returns>The value of the operand.</returns>
    /// <remarks>
    /// The exponent operands hold the exponent of a compact notation -- "1c6" for a million -- which a
    /// quantity that reaches a provider as a <see cref="double"/> never carries, so both are zero.
    /// </remarks>
    public double Of(CldrOperand operand)
    {
        return operand switch
        {
            CldrOperand.AbsoluteValue => N,
            CldrOperand.IntegerPart => I,
            CldrOperand.DecimalCount => V,
            CldrOperand.DecimalCountWithoutTrailingZeros => W,
            CldrOperand.Decimals => F,
            CldrOperand.DecimalsWithoutTrailingZeros => T,
            _ => 0,
        };
    }
}
