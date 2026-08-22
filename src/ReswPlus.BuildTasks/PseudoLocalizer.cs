using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ReswPlus.BuildTasks;

internal static class PseudoLocalizer
{
    private static readonly Regex ProtectedToken = new(
        @"(\{\{|\}\}|\{[^{}\r\n]*\}|<[^>\r\n]+>)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string Transform(string value, PseudoLocalizationMode mode, int expansionPercentage)
    {
        if (value.Length == 0)
        {
            return value;
        }

        var transformed = new StringBuilder(value.Length);
        var letterCount = 0;
        var currentIndex = 0;

        foreach (Match match in ProtectedToken.Matches(value))
        {
            AppendAccented(value, currentIndex, match.Index - currentIndex, transformed, ref letterCount);
            transformed.Append(match.Value);
            currentIndex = match.Index + match.Length;
        }

        AppendAccented(value, currentIndex, value.Length - currentIndex, transformed, ref letterCount);

        var paddingLength = (int)Math.Ceiling(letterCount * expansionPercentage / 100d);
        if (paddingLength > 0)
        {
            transformed.Append(' ');
            transformed.Append('~', paddingLength);
        }

        return mode == PseudoLocalizationMode.Mirrored
            ? "\u202e\u27e6" + transformed + "\u27e7\u202c"
            : "\u27e6" + transformed + "\u27e7";
    }

    public static IReadOnlyList<(PseudoLocalizationMode Mode, string Language)> ParseModes(string modes)
    {
        var parsed = new List<(PseudoLocalizationMode, string)>();
        var seen = new HashSet<PseudoLocalizationMode>();

        foreach (var value in modes.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var normalized = value.Trim();
            PseudoLocalizationMode mode;

            if (normalized.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("accented", StringComparison.OrdinalIgnoreCase))
            {
                mode = PseudoLocalizationMode.Accented;
            }
            else if (normalized.Equals("mirrored", StringComparison.OrdinalIgnoreCase))
            {
                mode = PseudoLocalizationMode.Mirrored;
            }
            else
            {
                throw new ArgumentException(
                    $"Unknown pseudo-localization mode '{normalized}'. Use Accented, Mirrored, or both separated by a semicolon.",
                    nameof(modes));
            }

            if (seen.Add(mode))
            {
                parsed.Add((mode, mode == PseudoLocalizationMode.Accented ? "qps-ploc" : "qps-plocm"));
            }
        }

        return parsed;
    }

    private static void AppendAccented(
        string value,
        int start,
        int length,
        StringBuilder output,
        ref int letterCount)
    {
        for (var index = start; index < start + length; index++)
        {
            var character = value[index];
            output.Append(Accent(character));

            if (char.IsLetter(character))
            {
                letterCount++;
            }
        }
    }

    private static char Accent(char character)
    {
        return character switch
        {
            'A' => '\u00c5',
            'B' => '\u0243',
            'C' => '\u00c7',
            'D' => '\u00d0',
            'E' => '\u00cb',
            'F' => '\u0191',
            'G' => '\u011c',
            'H' => '\u0126',
            'I' => '\u00cf',
            'J' => '\u0134',
            'K' => '\u0136',
            'L' => '\u013f',
            'M' => '\u1e40',
            'N' => '\u00d1',
            'O' => '\u00d8',
            'P' => '\u00de',
            'Q' => '\u01ea',
            'R' => '\u0158',
            'S' => '\u0160',
            'T' => '\u0166',
            'U' => '\u00dc',
            'V' => '\u1e7c',
            'W' => '\u0174',
            'X' => '\u1e8a',
            'Y' => '\u0178',
            'Z' => '\u017d',
            'a' => '\u00e5',
            'b' => '\u0180',
            'c' => '\u00e7',
            'd' => '\u00f0',
            'e' => '\u00eb',
            'f' => '\u0192',
            'g' => '\u011d',
            'h' => '\u0127',
            'i' => '\u00ef',
            'j' => '\u0135',
            'k' => '\u0137',
            'l' => '\u0140',
            'm' => '\u1e41',
            'n' => '\u00f1',
            'o' => '\u00f8',
            'p' => '\u00fe',
            'q' => '\u01eb',
            'r' => '\u0159',
            's' => '\u0161',
            't' => '\u0167',
            'u' => '\u00fc',
            'v' => '\u1e7d',
            'w' => '\u0175',
            'x' => '\u1e8b',
            'y' => '\u00ff',
            'z' => '\u017e',
            _ => character,
        };
    }
}
