using System;

namespace ReswPlus.SourceGenerator.Plurals;

/// <summary>
/// The operands UTS #35 derives a plural decision from.
/// </summary>
/// <remarks>
/// CLDR writes each of these as a single letter, which is what a rule reads and what the generated provider
/// names its locals after. They are named here rather than carried as letters so that a rule says what it
/// reads: <c>CldrOperand.IntegerPart</c> rather than <c>'i'</c>.
/// </remarks>
internal enum CldrOperand
{
    /// <summary>
    /// <c>n</c>: the absolute value of the quantity, decimals included.
    /// </summary>
    AbsoluteValue,

    /// <summary>
    /// <c>i</c>: the integer part of the quantity.
    /// </summary>
    IntegerPart,

    /// <summary>
    /// <c>v</c>: how many decimals the quantity shows, trailing zeros included.
    /// </summary>
    DecimalCount,

    /// <summary>
    /// <c>w</c>: how many decimals the quantity shows, trailing zeros dropped.
    /// </summary>
    DecimalCountWithoutTrailingZeros,

    /// <summary>
    /// <c>f</c>: the decimals themselves, as an integer, trailing zeros included.
    /// </summary>
    Decimals,

    /// <summary>
    /// <c>t</c>: the decimals themselves, as an integer, trailing zeros dropped.
    /// </summary>
    DecimalsWithoutTrailingZeros,

    /// <summary>
    /// <c>c</c>: the exponent of a compact notation, as in "1c6" for a million.
    /// </summary>
    /// <remarks>
    /// A quantity reaches a provider as a <see cref="double"/>, which carries no such notation, so this is
    /// always zero and every relation reading it is decided when the provider is written.
    /// </remarks>
    CompactExponent,

    /// <summary>
    /// <c>e</c>: the same exponent, under the name CLDR published it as first.
    /// </summary>
    Exponent,
}

/// <summary>
/// Reads an operand as CLDR writes it, and writes it back out.
/// </summary>
internal static class CldrOperandLetters
{
    /// <summary>
    /// Gets the letter CLDR writes an operand as.
    /// </summary>
    /// <param name="operand">The operand.</param>
    /// <returns>The letter.</returns>
    public static char Letter(this CldrOperand operand)
    {
        return operand switch
        {
            CldrOperand.AbsoluteValue => 'n',
            CldrOperand.IntegerPart => 'i',
            CldrOperand.DecimalCount => 'v',
            CldrOperand.DecimalCountWithoutTrailingZeros => 'w',
            CldrOperand.Decimals => 'f',
            CldrOperand.DecimalsWithoutTrailingZeros => 't',
            CldrOperand.CompactExponent => 'c',
            CldrOperand.Exponent => 'e',
            _ => throw new InvalidOperationException($"'{operand}' has no letter in CLDR's rule syntax."),
        };
    }

    /// <summary>
    /// Gets whether an operand holds the exponent of a compact notation.
    /// </summary>
    /// <param name="operand">The operand.</param>
    /// <returns><see langword="true"/> when a quantity arriving as a double always reads it as zero.</returns>
    public static bool IsExponent(this CldrOperand operand)
    {
        return operand is CldrOperand.CompactExponent or CldrOperand.Exponent;
    }
}
