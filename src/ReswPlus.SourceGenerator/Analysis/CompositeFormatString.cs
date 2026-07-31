using System.Collections.Generic;
using System.Globalization;

namespace ReswPlus.SourceGenerator.Analysis;

/// <summary>
/// Parses the composite format strings used by the values of formatted resources.
/// </summary>
/// <remarks>
/// The generated code passes the value of a formatted resource to <see cref="string.Format(string, object[])"/>,
/// so a value that this parser rejects is guaranteed to throw a <see cref="System.FormatException"/> at runtime.
/// The parser is deliberately permissive where the runtime is: it accepts anything the runtime accepts, so that
/// no valid value is ever reported.
/// </remarks>
internal static class CompositeFormatString
{
    /// <summary>
    /// Extracts the set of argument indexes referenced by a composite format string.
    /// </summary>
    /// <param name="value">The composite format string to parse.</param>
    /// <param name="indexes">The distinct argument indexes it references, in ascending order.</param>
    /// <returns>Whether <paramref name="value"/> is a valid composite format string.</returns>
    public static bool TryGetArgumentIndexes(string value, out SortedSet<int> indexes)
    {
        indexes = new SortedSet<int>();

        var position = 0;

        while (position < value.Length)
        {
            var character = value[position];

            if (character == '}')
            {
                // Outside of a format item, a closing brace only stands for itself when it is doubled.
                if (position + 1 < value.Length && value[position + 1] == '}')
                {
                    position += 2;
                    continue;
                }

                return false;
            }

            if (character != '{')
            {
                position++;
                continue;
            }

            // A doubled opening brace stands for a literal brace, it doesn't open a format item.
            if (position + 1 < value.Length && value[position + 1] == '{')
            {
                position += 2;
                continue;
            }

            if (!TryReadFormatItem(value, ref position, out var index))
            {
                return false;
            }

            _ = indexes.Add(index);
        }

        return true;
    }

    /// <summary>
    /// Reads a single <c>{index[,alignment][:format]}</c> item, starting from its opening brace.
    /// </summary>
    private static bool TryReadFormatItem(string value, ref int position, out int index)
    {
        index = 0;

        // Skip the opening brace.
        position++;

        if (!TryReadInteger(value, ref position, out index))
        {
            return false;
        }

        SkipWhiteSpace(value, ref position);

        if (position < value.Length && value[position] == ',')
        {
            position++;

            SkipWhiteSpace(value, ref position);

            if (position < value.Length && (value[position] == '-' || value[position] == '+'))
            {
                position++;
            }

            if (!TryReadInteger(value, ref position, out _))
            {
                return false;
            }

            SkipWhiteSpace(value, ref position);
        }

        if (position < value.Length && value[position] == ':' && !TrySkipFormatSpecifier(value, ref position))
        {
            return false;
        }

        if (position >= value.Length || value[position] != '}')
        {
            return false;
        }

        position++;

        return true;
    }

    /// <summary>
    /// Skips the <c>:format</c> part of a format item, stopping on the closing brace of the item.
    /// </summary>
    private static bool TrySkipFormatSpecifier(string value, ref int position)
    {
        // Skip the colon.
        position++;

        while (position < value.Length)
        {
            var character = value[position];

            if (character is '{' or '}')
            {
                // Braces are escaped by doubling them inside a format specifier as well.
                if (position + 1 < value.Length && value[position + 1] == character)
                {
                    position += 2;
                    continue;
                }

                return character == '}';
            }

            position++;
        }

        return false;
    }

    private static bool TryReadInteger(string value, ref int position, out int result)
    {
        var start = position;

        while (position < value.Length && char.IsDigit(value[position]))
        {
            position++;
        }

        return int.TryParse(value.Substring(start, position - start), NumberStyles.None, CultureInfo.InvariantCulture, out result);
    }

    private static void SkipWhiteSpace(string value, ref int position)
    {
        while (position < value.Length && value[position] == ' ')
        {
            position++;
        }
    }
}
