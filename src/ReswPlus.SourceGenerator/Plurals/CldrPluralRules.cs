// -----------------------------------------------------------------------------------
// DO NOT EDIT BY HAND.
//
// Written by tools/CldrRuleImporter from the plural rules of Unicode CLDR 48.
// To change anything here, change the importer or refresh CLDR, then rerun it:
//
//     dotnet run --project tools/CldrRuleImporter               regenerate
//     dotnet run --project tools/CldrRuleImporter -- --download  refresh CLDR first
//
// See tools/CldrRuleImporter/README.md.
// -----------------------------------------------------------------------------------

using ReswPlus.SourceGenerator.ClassGenerators;

namespace ReswPlus.SourceGenerator.Plurals;

/// <summary>
/// The plural rules of Unicode CLDR, as the classes implementing them.
/// </summary>
/// <remarks>
/// Each entry is one set of rules and the languages CLDR gives it to. Languages sharing a set of rules
/// share an entry, so a project gets one class per set of rules its languages use rather than one per
/// language. The rules are held as the objects they are made of; the code deciding them is written by
/// <see cref="CldrEmitter"/> when a project is generated.
/// </remarks>
internal static class CldrPluralRules
{
    /// <summary>
    /// The CLDR release these rules were published in.
    /// </summary>
    public const string Version = "48";

    /// <summary>
    /// Every set of rules CLDR publishes.
    /// </summary>
    public static readonly CldrPluralForm[] Forms =
    [
        new(
            "Af",
            ["af", "an", "asa", "ast", "az", "bal", "bem", "bez", "bg", "brx", "ce", "cgg", "chr", "ckb", "de", "dv", "ee", "el", "en", "eo", "et", "eu", "fi", "fo", "fur", "fy", "gl", "gsw", "ha", "haw", "hu", "ia", "ie", "io", "jgo", "ji", "jmc", "ka", "kaj", "kcg", "kk", "kkj", "kl", "ks", "ksb", "ku", "ky", "lb", "lg", "lij", "mas", "mgo", "ml", "mn", "mr", "nah", "nb", "nd", "ne", "nl", "nn", "nnh", "no", "nr", "ny", "nyn", "om", "or", "os", "pap", "ps", "rm", "rof", "rwk", "saq", "sc", "sd", "sdh", "seh", "sn", "so", "sq", "ss", "ssy", "st", "sv", "sw", "syr", "ta", "te", "teo", "tig", "tk", "tn", "tr", "ts", "ug", "ur", "uz", "ve", "vo", "vun", "wae", "xh", "xog", "yi"],
            [PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Ak",
            ["ak", "bh", "bho", "csw", "guw", "ln", "mg", "nso", "pa", "ti", "wa"],
            [PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 1)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Am",
            ["am", "as", "bn", "doi", "fa", "gu", "hi", "kn", "kok", "kok-Latn", "pcm", "zu"],
            [PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Ar",
            ["ar", "ars"],
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.Zero, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 0)])),
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])),
                new(PluralCategory.Two, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(2, 2)])),
                new(PluralCategory.Few, new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(3, 10)])),
                new(PluralCategory.Many, new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(11, 99)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Be",
            ["be", "ru", "uk"],
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(11, 11)])])),
                new(PluralCategory.Few, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(2, 4)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(12, 14)])])),
                new(PluralCategory.Many, new CldrAnyOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(0, 0)]), new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(5, 9)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(11, 14)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Blo",
            ["blo", "cv", "ksh"],
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.Zero, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 0)])),
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Bm",
            ["bm", "bo", "dz", "hnj", "id", "ig", "ii", "in", "ja", "jbo", "jv", "jw", "kde", "kea", "km", "ko", "lkt", "lo", "ms", "my", "nqo", "osa", "sah", "ses", "sg", "su", "th", "to", "tpi", "und", "vi", "wo", "yo", "yue", "zh"],
            [PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.Other, null),
            ]),
        new(
            "Br",
            ["br"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [PluralCategory.Many],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(11, 11), new(71, 71), new(91, 91)])])),
                new(PluralCategory.Two, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(2, 2)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(12, 12), new(72, 72), new(92, 92)])])),
                new(PluralCategory.Few, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(3, 4), new(9, 9)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(10, 19), new(70, 79), new(90, 99)])])),
                new(PluralCategory.Many, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 0, true, [new(0, 0)]), new CldrRelation(CldrOperand.AbsoluteValue, 1000000, false, [new(0, 0)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Bs",
            ["bs", "hr", "sh", "sr"],
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.IntegerPart, 100, true, [new(11, 11)])]), new CldrAllOf([new CldrRelation(CldrOperand.Decimals, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.Decimals, 100, true, [new(11, 11)])])])),
                new(PluralCategory.Few, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 10, false, [new(2, 4)]), new CldrRelation(CldrOperand.IntegerPart, 100, true, [new(12, 14)])]), new CldrAllOf([new CldrRelation(CldrOperand.Decimals, 10, false, [new(2, 4)]), new CldrRelation(CldrOperand.Decimals, 100, true, [new(12, 14)])])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Ca",
            ["ca", "es", "it", "lld", "pt-PT", "scn", "vec"],
            [PluralCategory.One, PluralCategory.Many, PluralCategory.Other],
            [PluralCategory.Many],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(1, 1)]), new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)])])),
                new(PluralCategory.Many, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.Exponent, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 0, true, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 1000000, false, [new(0, 0)]), new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)])]), new CldrRelation(CldrOperand.Exponent, 0, true, [new(0, 5)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Ceb",
            ["ceb", "fil", "tl"],
            [PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(1, 1), new(2, 2), new(3, 3)])]), new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 10, true, [new(4, 4), new(6, 6), new(9, 9)])]), new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, true, [new(0, 0)]), new CldrRelation(CldrOperand.Decimals, 10, true, [new(4, 4), new(6, 6), new(9, 9)])])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Cs",
            ["cs", "sk"],
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(1, 1)]), new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)])])),
                new(PluralCategory.Few, new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(2, 4)]), new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)])])),
                new(PluralCategory.Many, new CldrRelation(CldrOperand.DecimalCount, 0, true, [new(0, 0)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Cy",
            ["cy"],
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.Zero, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 0)])),
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])),
                new(PluralCategory.Two, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(2, 2)])),
                new(PluralCategory.Few, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(3, 3)])),
                new(PluralCategory.Many, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(6, 6)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Da",
            ["da"],
            [PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)]), new CldrAllOf([new CldrRelation(CldrOperand.DecimalsWithoutTrailingZeros, 0, true, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(0, 0), new(1, 1)])])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Dsb",
            ["dsb", "hsb"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 100, false, [new(1, 1)])]), new CldrRelation(CldrOperand.Decimals, 100, false, [new(1, 1)])])),
                new(PluralCategory.Two, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 100, false, [new(2, 2)])]), new CldrRelation(CldrOperand.Decimals, 100, false, [new(2, 2)])])),
                new(PluralCategory.Few, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 100, false, [new(3, 4)])]), new CldrRelation(CldrOperand.Decimals, 100, false, [new(3, 4)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Ff",
            ["ff", "hy", "kab"],
            [PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(0, 0), new(1, 1)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Fr",
            ["fr", "pt"],
            [PluralCategory.One, PluralCategory.Many, PluralCategory.Other],
            [PluralCategory.Many],
            true,
            [
                new(PluralCategory.One, new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(0, 0), new(1, 1)])),
                new(PluralCategory.Many, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.Exponent, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 0, true, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 1000000, false, [new(0, 0)]), new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)])]), new CldrRelation(CldrOperand.Exponent, 0, true, [new(0, 5)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Ga",
            ["ga"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])),
                new(PluralCategory.Two, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(2, 2)])),
                new(PluralCategory.Few, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(3, 6)])),
                new(PluralCategory.Many, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(7, 10)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Gd",
            ["gd"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1), new(11, 11)])),
                new(PluralCategory.Two, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(2, 2), new(12, 12)])),
                new(PluralCategory.Few, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(3, 10), new(13, 19)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Gv",
            ["gv"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 10, false, [new(1, 1)])])),
                new(PluralCategory.Two, new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 10, false, [new(2, 2)])])),
                new(PluralCategory.Few, new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 100, false, [new(0, 0), new(20, 20), new(40, 40), new(60, 60), new(80, 80)])])),
                new(PluralCategory.Many, new CldrRelation(CldrOperand.DecimalCount, 0, true, [new(0, 0)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "He",
            ["he", "iw"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(1, 1)]), new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)])]), new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.DecimalCount, 0, true, [new(0, 0)])])])),
                new(PluralCategory.Two, new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(2, 2)]), new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Is",
            ["is", "mk"],
            [PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.DecimalsWithoutTrailingZeros, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.IntegerPart, 100, true, [new(11, 11)])]), new CldrAllOf([new CldrRelation(CldrOperand.DecimalsWithoutTrailingZeros, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.DecimalsWithoutTrailingZeros, 100, true, [new(11, 11)])])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Iu",
            ["iu", "naq", "sat", "se", "sma", "smi", "smj", "smn", "sms"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])),
                new(PluralCategory.Two, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(2, 2)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Kw",
            ["kw"],
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.Zero, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 0)])),
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])),
                new(PluralCategory.Two, new CldrAnyOf([new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(2, 2), new(22, 22), new(42, 42), new(62, 62), new(82, 82)]), new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 1000, false, [new(0, 0)]), new CldrRelation(CldrOperand.AbsoluteValue, 100000, false, [new(1000, 20000), new(40000, 40000), new(60000, 60000), new(80000, 80000)])]), new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 0, true, [new(0, 0)]), new CldrRelation(CldrOperand.AbsoluteValue, 1000000, false, [new(100000, 100000)])])])),
                new(PluralCategory.Few, new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(3, 3), new(23, 23), new(43, 43), new(63, 63), new(83, 83)])),
                new(PluralCategory.Many, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 0, true, [new(1, 1)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(1, 1), new(21, 21), new(41, 41), new(61, 61), new(81, 81)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Lag",
            ["lag"],
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.Zero, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 0)])),
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(0, 0), new(1, 1)]), new CldrRelation(CldrOperand.AbsoluteValue, 0, true, [new(0, 0)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Lt",
            ["lt"],
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(11, 19)])])),
                new(PluralCategory.Few, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(2, 9)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(11, 19)])])),
                new(PluralCategory.Many, new CldrRelation(CldrOperand.Decimals, 0, true, [new(0, 0)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Lv",
            ["lv", "prg"],
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Other],
            [],
            false,
            [
                new(PluralCategory.Zero, new CldrAnyOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(0, 0)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(11, 19)]), new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(2, 2)]), new CldrRelation(CldrOperand.Decimals, 100, false, [new(11, 19)])])])),
                new(PluralCategory.One, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(11, 11)])]), new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(2, 2)]), new CldrRelation(CldrOperand.Decimals, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.Decimals, 100, true, [new(11, 11)])]), new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, true, [new(2, 2)]), new CldrRelation(CldrOperand.Decimals, 10, false, [new(1, 1)])])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Mo",
            ["mo", "ro"],
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(1, 1)]), new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)])])),
                new(PluralCategory.Few, new CldrAnyOf([new CldrRelation(CldrOperand.DecimalCount, 0, true, [new(0, 0)]), new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 0)]), new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 0, true, [new(1, 1)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(1, 19)])])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Mt",
            ["mt"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])),
                new(PluralCategory.Two, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(2, 2)])),
                new(PluralCategory.Few, new CldrAnyOf([new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(3, 10)])])),
                new(PluralCategory.Many, new CldrRelation(CldrOperand.AbsoluteValue, 100, false, [new(11, 19)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Pl",
            ["pl"],
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(1, 1)]), new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)])])),
                new(PluralCategory.Few, new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 10, false, [new(2, 4)]), new CldrRelation(CldrOperand.IntegerPart, 100, true, [new(12, 14)])])),
                new(PluralCategory.Many, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 0, true, [new(1, 1)]), new CldrRelation(CldrOperand.IntegerPart, 10, false, [new(0, 1)])]), new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 10, false, [new(5, 9)])]), new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 100, false, [new(12, 14)])])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Sgs",
            ["sgs"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(1, 1)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(11, 11)])])),
                new(PluralCategory.Two, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(2, 2)])),
                new(PluralCategory.Few, new CldrAllOf([new CldrRelation(CldrOperand.AbsoluteValue, 0, true, [new(2, 2)]), new CldrRelation(CldrOperand.AbsoluteValue, 10, false, [new(2, 9)]), new CldrRelation(CldrOperand.AbsoluteValue, 100, true, [new(11, 19)])])),
                new(PluralCategory.Many, new CldrRelation(CldrOperand.Decimals, 0, true, [new(0, 0)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Shi",
            ["shi"],
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(1, 1)])])),
                new(PluralCategory.Few, new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(2, 10)])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Si",
            ["si"],
            [PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 0), new(1, 1)]), new CldrAllOf([new CldrRelation(CldrOperand.IntegerPart, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.Decimals, 0, false, [new(1, 1)])])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Sl",
            ["sl"],
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 100, false, [new(1, 1)])])),
                new(PluralCategory.Two, new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 100, false, [new(2, 2)])])),
                new(PluralCategory.Few, new CldrAnyOf([new CldrAllOf([new CldrRelation(CldrOperand.DecimalCount, 0, false, [new(0, 0)]), new CldrRelation(CldrOperand.IntegerPart, 100, false, [new(3, 4)])]), new CldrRelation(CldrOperand.DecimalCount, 0, true, [new(0, 0)])])),
                new(PluralCategory.Other, null),
            ]),
        new(
            "Tzm",
            ["tzm"],
            [PluralCategory.One, PluralCategory.Other],
            [],
            true,
            [
                new(PluralCategory.One, new CldrAnyOf([new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(0, 1)]), new CldrRelation(CldrOperand.AbsoluteValue, 0, false, [new(11, 99)])])),
                new(PluralCategory.Other, null),
            ]),
    ];
}
