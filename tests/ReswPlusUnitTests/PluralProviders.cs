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
    public void Lithuanian(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("Lithuanian")(number));
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
    // Everything else is 'other'.
    [InlineData(20, "OTHER")]
    [InlineData(100, "OTHER")]
    [InlineData(120, "OTHER")]
    public void Romanian(double number, string expected)
    {
        Assert.Equal(expected, PluralProviderHost.GetProvider("Romanian")(number));
    }
}
