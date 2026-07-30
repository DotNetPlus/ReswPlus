using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Boundary tests for the CLDR plural rules of the providers emitted by the generator.
/// </summary>
/// <remarks>
/// The expectations come from the CLDR 48 cardinal plural rules, whose operands are named below as CLDR names
/// them: <c>n</c> is the quantity, <c>i</c> its integer part, <c>v</c> the number of visible decimals and
/// <c>f</c> those decimals read as an integer.
/// </remarks>
public class PluralProviders
{
    [Theory]
    // v = 0 and i % 100 = 1 is 'one'.
    [InlineData(1, "ONE")]
    [InlineData(101, "ONE")]
    [InlineData(201, "ONE")]
    // v = 0 and i % 100 = 2 is 'two'.
    [InlineData(2, "TWO")]
    [InlineData(102, "TWO")]
    // v = 0 and i % 100 in 3..4 is 'few'.
    [InlineData(3, "FEW")]
    [InlineData(4, "FEW")]
    [InlineData(103, "FEW")]
    [InlineData(104, "FEW")]
    // Everything else with no decimals is 'other'.
    [InlineData(0, "OTHER")]
    [InlineData(5, "OTHER")]
    [InlineData(100, "OTHER")]
    [InlineData(105, "OTHER")]
    // v != 0 is 'few', whatever the integer part is.
    [InlineData(0.5, "FEW")]
    [InlineData(1.5, "FEW")]
    [InlineData(5.5, "FEW")]
    // The integer part is read without a 32-bit cast, which would overflow and pick the wrong form.
    [InlineData(2147483701d, "ONE")]
    [InlineData(2147483702d, "TWO")]
    public void Slovenian(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("Slovenian")(number));
    }

    [Theory]
    // v = 0 and i in 1..3 is 'one'.
    [InlineData(1, "ONE")]
    [InlineData(2, "ONE")]
    [InlineData(3, "ONE")]
    // v = 0 and i % 10 not in 4, 6, 9 is 'one'.
    [InlineData(0, "ONE")]
    [InlineData(5, "ONE")]
    [InlineData(7, "ONE")]
    [InlineData(8, "ONE")]
    [InlineData(10, "ONE")]
    [InlineData(23, "ONE")]
    // v = 0 and i % 10 in 4, 6, 9 is 'other'.
    [InlineData(4, "OTHER")]
    [InlineData(6, "OTHER")]
    [InlineData(9, "OTHER")]
    [InlineData(14, "OTHER")]
    [InlineData(16, "OTHER")]
    [InlineData(19, "OTHER")]
    [InlineData(24, "OTHER")]
    // v != 0 and f % 10 not in 4, 6, 9 is 'one'.
    [InlineData(0.5, "ONE")]
    [InlineData(1.5, "ONE")]
    [InlineData(1.3, "ONE")]
    [InlineData(1.25, "ONE")]
    // v != 0 and f % 10 in 4, 6, 9 is 'other'.
    [InlineData(1.4, "OTHER")]
    [InlineData(1.6, "OTHER")]
    [InlineData(1.9, "OTHER")]
    [InlineData(2.16, "OTHER")]
    // A quantity small enough that .NET writes it in scientific notation still has its decimals read.
    [InlineData(0.000004, "OTHER")]
    [InlineData(0.000005, "ONE")]
    [InlineData(4e-22, "OTHER")]
    [InlineData(5e-22, "ONE")]
    // The decimals are read from the shortest representation that round-trips, not from a rounded one.
    [InlineData(0.12345678901234568, "ONE")]
    // The same quantity has to select the same form whichever runtime reads it, so the representation is
    // searched for rather than being asked of a format whose result differs between them.
    [InlineData(664496.3397013473, "ONE")]
    [InlineData(5e-324, "ONE")]
    [InlineData(1e-300, "ONE")]
    public void Filipino(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("Filipino")(number));
    }

    [Theory]
    // n % 10 = 0 is 'zero', which covers every multiple of ten and not just zero itself.
    [InlineData(0, "ZERO")]
    [InlineData(10, "ZERO")]
    [InlineData(20, "ZERO")]
    [InlineData(30, "ZERO")]
    [InlineData(100, "ZERO")]
    // n % 100 in 11..19 is 'zero' too.
    [InlineData(11, "ZERO")]
    [InlineData(15, "ZERO")]
    [InlineData(19, "ZERO")]
    [InlineData(111, "ZERO")]
    // n % 10 = 1 and n % 100 != 11 is 'one'.
    [InlineData(1, "ONE")]
    [InlineData(21, "ONE")]
    [InlineData(101, "ONE")]
    // The CLDR ranges only hold integers, so a fractional remainder is not part of 11..19.
    [InlineData(11.5, "OTHER")]
    [InlineData(15.5, "OTHER")]
    // v != 2 and f % 10 = 1 is 'one', and the 11..19 range of the 'zero' rule needs v = 2 to apply to f.
    [InlineData(0.011, "ONE")]
    [InlineData(0.1, "ONE")]
    [InlineData(2.1, "ONE")]
    // v = 2 and f % 100 in 11..19 is 'zero'.
    [InlineData(0.11, "ZERO")]
    [InlineData(1.19, "ZERO")]
    // v = 2 and f % 10 = 1 and f % 100 != 11 is 'one'.
    [InlineData(0.21, "ONE")]
    // Everything else is 'other'.
    [InlineData(2, "OTHER")]
    [InlineData(5, "OTHER")]
    [InlineData(22, "OTHER")]
    [InlineData(0.2, "OTHER")]
    public void Latvian(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("Latvian")(number));
    }

    [Theory]
    // n = 1 is 'one'.
    [InlineData(1, "ONE")]
    // t != 0 and i in 0..1 is 'one', which includes quantities greater than 1.
    [InlineData(0.5, "ONE")]
    [InlineData(1.5, "ONE")]
    [InlineData(0.25, "ONE")]
    [InlineData(1.75, "ONE")]
    // Everything else is 'other'.
    [InlineData(0, "OTHER")]
    [InlineData(2, "OTHER")]
    [InlineData(2.5, "OTHER")]
    [InlineData(10, "OTHER")]
    public void Danish(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("Danish")(number));
    }

    [Theory]
    // i = 1 and v = 0 is 'one'.
    [InlineData(1, "ONE")]
    // i = 2..4 and v = 0 is 'few'.
    [InlineData(2, "FEW")]
    [InlineData(3, "FEW")]
    [InlineData(4, "FEW")]
    // Every other quantity with no decimals is 'other'.
    [InlineData(0, "OTHER")]
    [InlineData(5, "OTHER")]
    [InlineData(10, "OTHER")]
    [InlineData(100, "OTHER")]
    // v != 0 is 'many'.
    [InlineData(0.5, "MANY")]
    [InlineData(1.5, "MANY")]
    [InlineData(2.5, "MANY")]
    // Quantities .NET writes in scientific notation still report their decimals correctly.
    [InlineData(0.000001, "MANY")]
    [InlineData(1e-22, "MANY")]
    [InlineData(1.2e20, "OTHER")]
    // A value just below a whole number keeps its decimals rather than being rounded up to it.
    [InlineData(0.9999999999999999, "MANY")]
    public void Czech(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("Czech")(number));
    }

    [Theory]
    // n % 10 = 1 and n % 100 not in 11..19 is 'one'.
    [InlineData(1, "ONE")]
    [InlineData(21, "ONE")]
    [InlineData(101, "ONE")]
    // n % 10 = 2..9 and n % 100 not in 11..19 is 'few'.
    [InlineData(2, "FEW")]
    [InlineData(9, "FEW")]
    [InlineData(22, "FEW")]
    // The teens are excluded from both, and fall through to 'other'.
    [InlineData(11, "OTHER")]
    [InlineData(12, "OTHER")]
    [InlineData(19, "OTHER")]
    // Multiples of ten match neither range either.
    [InlineData(0, "OTHER")]
    [InlineData(10, "OTHER")]
    [InlineData(20, "OTHER")]
    // f != 0 is 'many'. The ranges above are integer ranges, so 2.5 is not 'few'.
    [InlineData(0.5, "MANY")]
    [InlineData(1.5, "MANY")]
    [InlineData(2.5, "MANY")]
    [InlineData(0.000001, "MANY")]
    // The ranges are matched past the 32-bit range too.
    [InlineData(2147483651d, "ONE")]
    [InlineData(2147483652d, "FEW")]
    public void Lithuanian(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("Lithuanian")(number));
    }

    [Theory]
    // Catalan, Italian, Portuguese and Spanish select 'one' for exactly 1.
    [InlineData(1, "ONE")]
    // 'many' is the exact non-zero multiples of a million.
    [InlineData(1000000, "MANY")]
    [InlineData(2000000, "MANY")]
    [InlineData(1000000000, "MANY")]
    // Everything else is 'other', including zero and the quantities around a million.
    [InlineData(0, "OTHER")]
    [InlineData(2, "OTHER")]
    [InlineData(100, "OTHER")]
    [InlineData(999999, "OTHER")]
    [InlineData(1000001, "OTHER")]
    [InlineData(1000000.5, "OTHER")]
    [InlineData(0.5, "OTHER")]
    public void OnlyOneOrMillions(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("OnlyOneOrMillions")(number));
    }

    [Theory]
    // French selects 'one' for an integer part of 0 or 1.
    [InlineData(0, "ONE")]
    [InlineData(1, "ONE")]
    [InlineData(0.5, "ONE")]
    [InlineData(1.5, "ONE")]
    // 'many' is the exact non-zero multiples of a million.
    [InlineData(1000000, "MANY")]
    [InlineData(2000000, "MANY")]
    // Everything else is 'other'.
    [InlineData(2, "OTHER")]
    [InlineData(100, "OTHER")]
    [InlineData(999999, "OTHER")]
    [InlineData(1000001, "OTHER")]
    [InlineData(1000000.5, "OTHER")]
    public void ZeroToTwoExcludedOrMillions(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("ZeroToTwoExcludedOrMillions")(number));
    }

    [Theory]
    // i = 1 and v = 0 is 'one'.
    [InlineData(1, "ONE")]
    // v != 0, or n = 0, or n % 100 in 1..19 is 'few'. 101 is therefore 'few' under current CLDR, which the QA
    // campaign initially recorded as a defect against an older release.
    [InlineData(0, "FEW")]
    [InlineData(2, "FEW")]
    [InlineData(19, "FEW")]
    [InlineData(101, "FEW")]
    [InlineData(119, "FEW")]
    [InlineData(1.5, "FEW")]
    [InlineData(0.000001, "FEW")]
    // Everything else is 'other'.
    [InlineData(20, "OTHER")]
    [InlineData(100, "OTHER")]
    [InlineData(120, "OTHER")]
    public void Romanian(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("Romanian")(number));
    }

    [Theory]
    // Every provider reads its integer part by truncating, so a quantity past the 32-bit range selects the
    // same form as the equivalent small one instead of overflowing into another.
    [InlineData("Icelandic", 21, "ONE")]
    [InlineData("Icelandic", 2147483651d, "ONE")]
    [InlineData("Croat", 21, "ONE")]
    [InlineData("Croat", 2147483651d, "ONE")]
    [InlineData("Croat", 2147483652d, "FEW")]
    [InlineData("Hebrew", 10, "MANY")]
    [InlineData("Hebrew", 2147483650d, "MANY")]
    [InlineData("Manx", 2147483651d, "ONE")]
    [InlineData("Maltese", 2147483602d, "FEW")]
    [InlineData("ScottishGaelic", 2147483651d, "OTHER")]
    [InlineData("Welsh", 2147483651d, "OTHER")]
    public void TheIntegerPartIsReadPastTheThirtyTwoBitRange(string providerId, double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider(providerId)(number));
    }
}
