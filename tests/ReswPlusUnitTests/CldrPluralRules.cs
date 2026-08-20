using System.Collections.Generic;

namespace ReswPlusUnitTests;

/// <summary>
/// The cardinal plural rules of Unicode CLDR, as published, for one language of every plural form ReswPlus
/// ships rules for.
/// </summary>
/// <remarks>
/// This is a pinned copy of the "plurals-type-cardinal" section of the CLDR supplemental data, kept verbatim
/// down to the sample lists CLDR publishes with each rule. Those sample lists are what makes the rules
/// testable: CLDR states, for every category of every language, the quantities that select it.
/// <para>
/// The rules of a language are revised between CLDR releases -- categories are added and removed -- and the
/// rules ReswPlus ships are hand written, so nothing makes the two agree on its own. Pinning the published
/// rules here is what turns that from something nobody would notice into a failing test.
/// </para>
/// <para>
/// Regenerate with the "plurals-type-cardinal" section of
/// https://raw.githubusercontent.com/unicode-org/cldr-json/main/cldr-json/cldr-core/supplemental/plurals.json
/// and bump <see cref="Version"/> to the version of the cldr-core package it was taken from.
/// </para>
/// </remarks>
internal static class CldrPluralRules
{
    /// <summary>
    /// The version of CLDR the rules below were taken from.
    /// </summary>
    public const string Version = "48.2.0";

    /// <summary>
    /// The published rule of each category, keyed by language and then by CLDR plural category.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Cardinal =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            // Arabic
            // Arabic
            ["ar"] = new Dictionary<string, string>
            {
                ["ZERO"] = "n = 0 @integer 0 @decimal 0.0, 0.00, 0.000, 0.0000",
                ["ONE"] = "n = 1 @integer 1 @decimal 1.0, 1.00, 1.000, 1.0000",
                ["TWO"] = "n = 2 @integer 2 @decimal 2.0, 2.00, 2.000, 2.0000",
                ["FEW"] = "n % 100 = 3..10 @integer 3~10, 103~110, 1003, \u2026 @decimal 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 103.0, 1003.0, \u2026",
                ["MANY"] = "n % 100 = 11..99 @integer 11~26, 111, 1011, \u2026 @decimal 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 111.0, 1011.0, \u2026",
                ["OTHER"] = "@integer 100~102, 200~202, 300~302, 400~402, 500~502, 600, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.1~0.9, 1.1~1.7, 10.1, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Breizh
            ["br"] = new Dictionary<string, string>
            {
                ["ONE"] = "n % 10 = 1 and n % 100 != 11,71,91 @integer 1, 21, 31, 41, 51, 61, 81, 101, 1001, \u2026 @decimal 1.0, 21.0, 31.0, 41.0, 51.0, 61.0, 81.0, 101.0, 1001.0, \u2026",
                ["TWO"] = "n % 10 = 2 and n % 100 != 12,72,92 @integer 2, 22, 32, 42, 52, 62, 82, 102, 1002, \u2026 @decimal 2.0, 22.0, 32.0, 42.0, 52.0, 62.0, 82.0, 102.0, 1002.0, \u2026",
                ["FEW"] = "n % 10 = 3..4,9 and n % 100 != 10..19,70..79,90..99 @integer 3, 4, 9, 23, 24, 29, 33, 34, 39, 43, 44, 49, 103, 1003, \u2026 @decimal 3.0, 4.0, 9.0, 23.0, 24.0, 29.0, 33.0, 34.0, 103.0, 1003.0, \u2026",
                ["MANY"] = "n != 0 and n % 1000000 = 0 @integer 1000000, \u2026 @decimal 1000000.0, 1000000.00, 1000000.000, 1000000.0000, \u2026",
                ["OTHER"] = "@integer 0, 5~8, 10~20, 100, 1000, 10000, 100000, \u2026 @decimal 0.0~0.9, 1.1~1.6, 10.0, 100.0, 1000.0, 10000.0, 100000.0, \u2026",
            },
            // CentralAtlasTamazight
            ["tzm"] = new Dictionary<string, string>
            {
                ["ONE"] = "n = 0..1 or n = 11..99 @integer 0, 1, 11~24 @decimal 0.0, 1.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0, 20.0, 21.0, 22.0, 23.0, 24.0",
                ["OTHER"] = "@integer 2~10, 100~106, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.1~0.9, 1.1~1.7, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Cornish
            ["kw"] = new Dictionary<string, string>
            {
                ["ZERO"] = "n = 0 @integer 0 @decimal 0.0, 0.00, 0.000, 0.0000",
                ["ONE"] = "n = 1 @integer 1 @decimal 1.0, 1.00, 1.000, 1.0000",
                ["TWO"] = "n % 100 = 2,22,42,62,82 or n % 1000 = 0 and n % 100000 = 1000..20000,40000,60000,80000 or n != 0 and n % 1000000 = 100000 @integer 2, 22, 42, 62, 82, 102, 122, 142, 1000, 10000, 100000, \u2026 @decimal 2.0, 22.0, 42.0, 62.0, 82.0, 102.0, 122.0, 142.0, 1000.0, 10000.0, 100000.0, \u2026",
                ["FEW"] = "n % 100 = 3,23,43,63,83 @integer 3, 23, 43, 63, 83, 103, 123, 143, 1003, \u2026 @decimal 3.0, 23.0, 43.0, 63.0, 83.0, 103.0, 123.0, 143.0, 1003.0, \u2026",
                ["MANY"] = "n != 1 and n % 100 = 1,21,41,61,81 @integer 21, 41, 61, 81, 101, 121, 141, 161, 1001, \u2026 @decimal 21.0, 41.0, 61.0, 81.0, 101.0, 121.0, 141.0, 161.0, 1001.0, \u2026",
                ["OTHER"] = "@integer 4~19, 100, 1004, 1000000, \u2026 @decimal 0.1~0.9, 1.1~1.7, 10.0, 100.0, 1000.1, 1000000.0, \u2026",
            },
            // Croat
            ["bs"] = new Dictionary<string, string>
            {
                ["ONE"] = "v = 0 and i % 10 = 1 and i % 100 != 11 or f % 10 = 1 and f % 100 != 11 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, \u2026 @decimal 0.1, 1.1, 2.1, 3.1, 4.1, 5.1, 6.1, 7.1, 10.1, 100.1, 1000.1, \u2026",
                ["FEW"] = "v = 0 and i % 10 = 2..4 and i % 100 != 12..14 or f % 10 = 2..4 and f % 100 != 12..14 @integer 2~4, 22~24, 32~34, 42~44, 52~54, 62, 102, 1002, \u2026 @decimal 0.2~0.4, 1.2~1.4, 2.2~2.4, 3.2~3.4, 4.2~4.4, 5.2, 10.2, 100.2, 1000.2, \u2026",
                ["OTHER"] = "@integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0, 0.5~1.0, 1.5~2.0, 2.5~2.7, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Czech
            ["cs"] = new Dictionary<string, string>
            {
                ["ONE"] = "i = 1 and v = 0 @integer 1",
                ["FEW"] = "i = 2..4 and v = 0 @integer 2~4",
                ["MANY"] = "v != 0   @decimal 0.0~1.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
                ["OTHER"] = "@integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, \u2026",
            },
            // Danish
            ["da"] = new Dictionary<string, string>
            {
                ["ONE"] = "n = 1 or t != 0 and i = 0,1 @integer 1 @decimal 0.1~1.6",
                ["OTHER"] = "@integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0, 2.0~3.4, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Filipino
            ["fil"] = new Dictionary<string, string>
            {
                ["ONE"] = "v = 0 and i = 1,2,3 or v = 0 and i % 10 != 4,6,9 or v != 0 and f % 10 != 4,6,9 @integer 0~3, 5, 7, 8, 10~13, 15, 17, 18, 20, 21, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0~0.3, 0.5, 0.7, 0.8, 1.0~1.3, 1.5, 1.7, 1.8, 2.0, 2.1, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
                ["OTHER"] = "@integer 4, 6, 9, 14, 16, 19, 24, 26, 104, 1004, \u2026 @decimal 0.4, 0.6, 0.9, 1.4, 1.6, 1.9, 2.4, 2.6, 10.4, 100.4, 1000.4, \u2026",
            },
            // Hebrew
            ["he"] = new Dictionary<string, string>
            {
                ["ONE"] = "i = 1 and v = 0 or i = 0 and v != 0 @integer 1 @decimal 0.0~0.9, 0.00~0.05",
                ["TWO"] = "i = 2 and v = 0 @integer 2",
                ["OTHER"] = "@integer 0, 3~17, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 1.0~2.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Icelandic
            ["is"] = new Dictionary<string, string>
            {
                ["ONE"] = "t = 0 and i % 10 = 1 and i % 100 != 11 or t % 10 = 1 and t % 100 != 11 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, \u2026 @decimal 0.1, 1.0, 1.1, 2.1, 3.1, 4.1, 5.1, 6.1, 7.1, 10.1, 100.1, 1000.1, \u2026",
                ["OTHER"] = "@integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0, 0.2~0.9, 1.2~1.8, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // IntOneOrZero
            ["ak"] = new Dictionary<string, string>
            {
                ["ONE"] = "n = 0..1 @integer 0, 1 @decimal 0.0, 1.0, 0.00, 1.00, 0.000, 1.000, 0.0000, 1.0000",
                ["OTHER"] = "@integer 2~17, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.1~0.9, 1.1~1.7, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Irish
            ["ga"] = new Dictionary<string, string>
            {
                ["ONE"] = "n = 1 @integer 1 @decimal 1.0, 1.00, 1.000, 1.0000",
                ["TWO"] = "n = 2 @integer 2 @decimal 2.0, 2.00, 2.000, 2.0000",
                ["FEW"] = "n = 3..6 @integer 3~6 @decimal 3.0, 4.0, 5.0, 6.0, 3.00, 4.00, 5.00, 6.00, 3.000, 4.000, 5.000, 6.000, 3.0000, 4.0000, 5.0000, 6.0000",
                ["MANY"] = "n = 7..10 @integer 7~10 @decimal 7.0, 8.0, 9.0, 10.0, 7.00, 8.00, 9.00, 10.00, 7.000, 8.000, 9.000, 10.000, 7.0000, 8.0000, 9.0000, 10.0000",
                ["OTHER"] = "@integer 0, 11~25, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0~0.9, 1.1~1.6, 10.1, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Latvian
            ["lv"] = new Dictionary<string, string>
            {
                ["ZERO"] = "n % 10 = 0 or n % 100 = 11..19 or v = 2 and f % 100 = 11..19 @integer 0, 10~20, 30, 40, 50, 60, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
                ["ONE"] = "n % 10 = 1 and n % 100 != 11 or v = 2 and f % 10 = 1 and f % 100 != 11 or v != 2 and f % 10 = 1 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, \u2026 @decimal 0.1, 1.0, 1.1, 2.1, 3.1, 4.1, 5.1, 6.1, 7.1, 10.1, 100.1, 1000.1, \u2026",
                ["OTHER"] = "@integer 2~9, 22~29, 102, 1002, \u2026 @decimal 0.2~0.9, 1.2~1.9, 10.2, 100.2, 1000.2, \u2026",
            },
            // Lithuanian
            ["lt"] = new Dictionary<string, string>
            {
                ["ONE"] = "n % 10 = 1 and n % 100 != 11..19 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, \u2026 @decimal 1.0, 21.0, 31.0, 41.0, 51.0, 61.0, 71.0, 81.0, 101.0, 1001.0, \u2026",
                ["FEW"] = "n % 10 = 2..9 and n % 100 != 11..19 @integer 2~9, 22~29, 102, 1002, \u2026 @decimal 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 22.0, 102.0, 1002.0, \u2026",
                ["MANY"] = "f != 0   @decimal 0.1~0.9, 1.1~1.7, 10.1, 100.1, 1000.1, \u2026",
                ["OTHER"] = "@integer 0, 10~20, 30, 40, 50, 60, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Macedonian
            ["mk"] = new Dictionary<string, string>
            {
                ["ONE"] = "v = 0 and i % 10 = 1 and i % 100 != 11 or f % 10 = 1 and f % 100 != 11 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, \u2026 @decimal 0.1, 1.1, 2.1, 3.1, 4.1, 5.1, 6.1, 7.1, 10.1, 100.1, 1000.1, \u2026",
                ["OTHER"] = "@integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0, 0.2~1.0, 1.2~1.7, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Maltese
            ["mt"] = new Dictionary<string, string>
            {
                ["ONE"] = "n = 1 @integer 1 @decimal 1.0, 1.00, 1.000, 1.0000",
                ["TWO"] = "n = 2 @integer 2 @decimal 2.0, 2.00, 2.000, 2.0000",
                ["FEW"] = "n = 0 or n % 100 = 3..10 @integer 0, 3~10, 103~109, 1003, \u2026 @decimal 0.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 103.0, 1003.0, \u2026",
                ["MANY"] = "n % 100 = 11..19 @integer 11~19, 111~117, 1011, \u2026 @decimal 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 111.0, 1011.0, \u2026",
                ["OTHER"] = "@integer 20~35, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.1~0.9, 1.1~1.7, 10.1, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Manx
            ["gv"] = new Dictionary<string, string>
            {
                ["ONE"] = "v = 0 and i % 10 = 1 @integer 1, 11, 21, 31, 41, 51, 61, 71, 101, 1001, \u2026",
                ["TWO"] = "v = 0 and i % 10 = 2 @integer 2, 12, 22, 32, 42, 52, 62, 72, 102, 1002, \u2026",
                ["FEW"] = "v = 0 and i % 100 = 0,20,40,60,80 @integer 0, 20, 40, 60, 80, 100, 120, 140, 1000, 10000, 100000, 1000000, \u2026",
                ["MANY"] = "v != 0   @decimal 0.0~1.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
                ["OTHER"] = "@integer 3~10, 13~19, 23, 103, 1003, \u2026",
            },
            // OneOrTwo
            ["smn"] = new Dictionary<string, string>
            {
                ["ONE"] = "n = 1 @integer 1 @decimal 1.0, 1.00, 1.000, 1.0000",
                ["TWO"] = "n = 2 @integer 2 @decimal 2.0, 2.00, 2.000, 2.0000",
                ["OTHER"] = "@integer 0, 3~17, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0~0.9, 1.1~1.6, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // OneOrZero
            ["ksh"] = new Dictionary<string, string>
            {
                ["ZERO"] = "n = 0 @integer 0 @decimal 0.0, 0.00, 0.000, 0.0000",
                ["ONE"] = "n = 1 @integer 1 @decimal 1.0, 1.00, 1.000, 1.0000",
                ["OTHER"] = "@integer 2~17, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.1~0.9, 1.1~1.7, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // OneOrZeroToOneExcluded
            ["lag"] = new Dictionary<string, string>
            {
                ["ZERO"] = "n = 0 @integer 0 @decimal 0.0, 0.00, 0.000, 0.0000",
                ["ONE"] = "i = 0,1 and n != 0 @integer 1 @decimal 0.1~1.6",
                ["OTHER"] = "@integer 2~17, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 2.0~3.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // OnlyOne
            ["af"] = new Dictionary<string, string>
            {
                ["ONE"] = "n = 1 @integer 1 @decimal 1.0, 1.00, 1.000, 1.0000",
                ["OTHER"] = "@integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0~0.9, 1.1~1.6, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // OnlyOneOrMillions
            ["ca"] = new Dictionary<string, string>
            {
                ["ONE"] = "i = 1 and v = 0 @integer 1",
                ["MANY"] = "e = 0 and i != 0 and i % 1000000 = 0 and v = 0 or e != 0..5 @integer 1000000, 1c6, 2c6, 3c6, 4c6, 5c6, 6c6, \u2026 @decimal 1.0000001c6, 1.1c6, 2.0000001c6, 2.1c6, 3.0000001c6, 3.1c6, \u2026",
                ["OTHER"] = "@integer 0, 2~16, 100, 1000, 10000, 100000, 1c3, 2c3, 3c3, 4c3, 5c3, 6c3, \u2026 @decimal 0.0~1.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, 1.0001c3, 1.1c3, 2.0001c3, 2.1c3, 3.0001c3, 3.1c3, \u2026",
            },
            // Other
            ["bm"] = new Dictionary<string, string>
            {
                ["OTHER"] = "@integer 0~15, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0~1.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Polish
            ["pl"] = new Dictionary<string, string>
            {
                ["ONE"] = "i = 1 and v = 0 @integer 1",
                ["FEW"] = "v = 0 and i % 10 = 2..4 and i % 100 != 12..14 @integer 2~4, 22~24, 32~34, 42~44, 52~54, 62, 102, 1002, \u2026",
                ["MANY"] = "v = 0 and i != 1 and i % 10 = 0..1 or v = 0 and i % 10 = 5..9 or v = 0 and i % 100 = 12..14 @integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, \u2026",
                ["OTHER"] = "@decimal 0.0~1.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Romanian
            ["ro"] = new Dictionary<string, string>
            {
                ["ONE"] = "i = 1 and v = 0 @integer 1",
                ["FEW"] = "v != 0 or n = 0 or n != 1 and n % 100 = 1..19 @integer 0, 2~16, 101, 1001, \u2026 @decimal 0.0~1.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
                ["OTHER"] = "@integer 20~35, 100, 1000, 10000, 100000, 1000000, \u2026",
            },
            // ScottishGaelic
            ["gd"] = new Dictionary<string, string>
            {
                ["ONE"] = "n = 1,11 @integer 1, 11 @decimal 1.0, 11.0, 1.00, 11.00, 1.000, 11.000, 1.0000",
                ["TWO"] = "n = 2,12 @integer 2, 12 @decimal 2.0, 12.0, 2.00, 12.00, 2.000, 12.000, 2.0000",
                ["FEW"] = "n = 3..10,13..19 @integer 3~10, 13~19 @decimal 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 13.0, 14.0, 15.0, 16.0, 17.0, 18.0, 19.0, 3.00",
                ["OTHER"] = "@integer 0, 20~34, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.0~0.9, 1.1~1.6, 10.1, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Sinhala
            ["si"] = new Dictionary<string, string>
            {
                ["ONE"] = "n = 0,1 or i = 0 and f = 1 @integer 0, 1 @decimal 0.0, 0.1, 1.0, 0.00, 0.01, 1.00, 0.000, 0.001, 1.000, 0.0000, 0.0001, 1.0000",
                ["OTHER"] = "@integer 2~17, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.2~0.9, 1.1~1.8, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Slavic
            ["ru"] = new Dictionary<string, string>
            {
                ["ONE"] = "v = 0 and i % 10 = 1 and i % 100 != 11 @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, \u2026",
                ["FEW"] = "v = 0 and i % 10 = 2..4 and i % 100 != 12..14 @integer 2~4, 22~24, 32~34, 42~44, 52~54, 62, 102, 1002, \u2026",
                ["MANY"] = "v = 0 and i % 10 = 0 or v = 0 and i % 10 = 5..9 or v = 0 and i % 100 = 11..14 @integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, \u2026",
                ["OTHER"] = "@decimal 0.0~1.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Slovenian
            ["sl"] = new Dictionary<string, string>
            {
                ["ONE"] = "v = 0 and i % 100 = 1 @integer 1, 101, 201, 301, 401, 501, 601, 701, 1001, \u2026",
                ["TWO"] = "v = 0 and i % 100 = 2 @integer 2, 102, 202, 302, 402, 502, 602, 702, 1002, \u2026",
                ["FEW"] = "v = 0 and i % 100 = 3..4 or v != 0 @integer 3, 4, 103, 104, 203, 204, 303, 304, 403, 404, 503, 504, 603, 604, 703, 704, 1003, \u2026 @decimal 0.0~1.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
                ["OTHER"] = "@integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, \u2026",
            },
            // Tachelhit
            ["shi"] = new Dictionary<string, string>
            {
                ["ONE"] = "i = 0 or n = 1 @integer 0, 1 @decimal 0.0~1.0, 0.00~0.04",
                ["FEW"] = "n = 2..10 @integer 2~10 @decimal 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 2.00, 3.00, 4.00, 5.00, 6.00, 7.00, 8.00",
                ["OTHER"] = "@integer 11~26, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 1.1~1.9, 2.1~2.7, 10.1, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // Welsh
            ["cy"] = new Dictionary<string, string>
            {
                ["ZERO"] = "n = 0 @integer 0 @decimal 0.0, 0.00, 0.000, 0.0000",
                ["ONE"] = "n = 1 @integer 1 @decimal 1.0, 1.00, 1.000, 1.0000",
                ["TWO"] = "n = 2 @integer 2 @decimal 2.0, 2.00, 2.000, 2.0000",
                ["FEW"] = "n = 3 @integer 3 @decimal 3.0, 3.00, 3.000, 3.0000",
                ["MANY"] = "n = 6 @integer 6 @decimal 6.0, 6.00, 6.000, 6.0000",
                ["OTHER"] = "@integer 4, 5, 7~20, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 0.1~0.9, 1.1~1.7, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // ZeroToOne
            ["am"] = new Dictionary<string, string>
            {
                ["ONE"] = "i = 0 or n = 1 @integer 0, 1 @decimal 0.0~1.0, 0.00~0.04",
                ["OTHER"] = "@integer 2~17, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 1.1~2.6, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // ZeroToTwoExcluded
            ["hy"] = new Dictionary<string, string>
            {
                ["ONE"] = "i = 0,1 @integer 0, 1 @decimal 0.0~1.5",
                ["OTHER"] = "@integer 2~17, 100, 1000, 10000, 100000, 1000000, \u2026 @decimal 2.0~3.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, \u2026",
            },
            // ZeroToTwoExcludedOrMillions
            ["fr"] = new Dictionary<string, string>
            {
                ["ONE"] = "i = 0,1 @integer 0, 1 @decimal 0.0~1.5",
                ["MANY"] = "e = 0 and i != 0 and i % 1000000 = 0 and v = 0 or e != 0..5 @integer 1000000, 1c6, 2c6, 3c6, 4c6, 5c6, 6c6, \u2026 @decimal 1.0000001c6, 1.1c6, 2.0000001c6, 2.1c6, 3.0000001c6, 3.1c6, \u2026",
                ["OTHER"] = "@integer 2~17, 100, 1000, 10000, 100000, 1c3, 2c3, 3c3, 4c3, 5c3, 6c3, \u2026 @decimal 2.0~3.5, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, 1.0001c3, 1.1c3, 2.0001c3, 2.1c3, 3.0001c3, 3.1c3, \u2026",
            },
        };
}
