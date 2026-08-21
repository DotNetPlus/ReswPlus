using System.Collections.Generic;
using System.Globalization;

namespace ReswPlusUnitTests;

/// <summary>
/// The quantities the plural rules are checked over.
/// </summary>
/// <remarks>
/// The rules are written in terms of the last digits of a quantity and the digits after its decimal point, so
/// the sweep is built to cross every one of those boundaries rather than to be large: every integer to 1200
/// exhausts the hundreds and thousands the rules are written modulo, the values around a million cover the
/// rules that single it out, and the decimal grid crosses one and two decimals against every value the rules
/// read the decimals of a quantity as.
/// </remarks>
internal static class CldrQuantities
{
    /// <summary>
    /// The quantities themselves.
    /// </summary>
    public static readonly double[] Sweep = Build();

    private static double[] Build()
    {
        var quantities = new List<double>();

        for (var value = 0; value <= 1200; value++)
        {
            quantities.Add(value);
        }

        quantities.AddRange([9999, 10000, 100000, 1000000, 1000001, 1100000, 2000000, 123456]);

        // Multiples of a thousand on both sides of the bounds Cornish is written in terms of, which reads the
        // quantity modulo a hundred thousand and modulo a million and would otherwise be checked only well
        // inside its first range.
        quantities.AddRange([20000, 21000, 30000, 40000, 50000, 60000, 70000, 80000, 90000, 120000, 1000000 + 100000]);

        // Quantities carrying decimals with an integer part large enough to reach the rules that also test the
        // integer part: 'i % 1000000 = 0 and v = 0' is only really tested by a million and a half.
        quantities.AddRange([100.5, 101.5, 111.5, 1000000.5, 1000000.1, 1000001.5, 2000000.5]);

        // Quantities whose shortest representation is written in scientific notation, at both ends. Their
        // decimals are still decimals -- those of '1E-06' are five zeros and a one -- and the rules that read
        // how many a quantity has are decided by them.
        quantities.AddRange([1e15, 1e20, 1e21, 1.5e21, 1e-4, 1e-5, 1e-6, 1.5e-5, 2.5e-7]);

        // Three decimals and more, which the grid below does not reach, for the rules that turn on there being
        // any decimals at all rather than on their value.
        quantities.AddRange([0.001, 0.002, 0.011, 0.101, 1.001, 1.002, 1.011, 2.001, 0.123456, 1.000001]);

        for (var whole = 0; whole <= 30; whole++)
        {
            // Built from the text of the quantity rather than by arithmetic: 4 + 94/100d is not the double
            // nearest 4.94, and a rule reads the decimals of a quantity off the shortest text that round trips
            // to it, so arithmetic would sweep quantities carrying sixteen decimals instead of two.
            for (var hundredths = 1; hundredths <= 99; hundredths++)
            {
                quantities.Add(Parse($"{whole}.{hundredths:00}"));
            }

            for (var tenths = 1; tenths <= 9; tenths++)
            {
                quantities.Add(Parse($"{whole}.{tenths}"));
            }
        }

        return [.. quantities];

        static double Parse(string literal) => double.Parse(literal, CultureInfo.InvariantCulture);
    }
}
