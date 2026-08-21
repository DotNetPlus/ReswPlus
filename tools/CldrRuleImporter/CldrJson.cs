using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CldrRuleImporter;

/// <summary>
/// Reads the shape of JSON the CLDR supplemental data is published in.
/// </summary>
/// <remarks>
/// This is deliberately not a general JSON reader. The file it reads is vendored into this assembly, so its
/// shape is known and fixed -- objects, nested objects and strings, nothing else -- and reading only that
/// shape is what lets the rules be kept in their published form without taking a dependency on a JSON library.
/// A generator's dependencies are loaded into the host, where they collide with whatever the host already has,
/// so an analyzer is the one place where hand reading a known file is the smaller risk.
/// <para>
/// Anything outside that shape is refused rather than guessed at, and the position it was refused at is
/// reported, so a file that is not the one this was written for fails immediately and says where.
/// </para>
/// </remarks>
internal sealed class CldrJson
{
    private readonly List<KeyValuePair<string, object>> _members;

    private CldrJson(List<KeyValuePair<string, object>> members)
    {
        _members = members;
    }

    /// <summary>
    /// The members of the object that are themselves objects, in the order the file declares them.
    /// </summary>
    public IEnumerable<KeyValuePair<string, CldrJson>> Objects
    {
        get
        {
            foreach (var member in _members)
            {
                if (member.Value is CldrJson child)
                {
                    yield return new KeyValuePair<string, CldrJson>(member.Key, child);
                }
            }
        }
    }

    /// <summary>
    /// The members of the object that are strings, in the order the file declares them.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> Strings
    {
        get
        {
            foreach (var member in _members)
            {
                if (member.Value is string text)
                {
                    yield return new KeyValuePair<string, string>(member.Key, text);
                }
            }
        }
    }

    /// <summary>
    /// Gets a member that is an object.
    /// </summary>
    /// <param name="name">The name of the member.</param>
    /// <returns>The object.</returns>
    /// <exception cref="FormatException">The member is missing, or is not an object.</exception>
    public CldrJson Object(string name)
    {
        return Member(name) as CldrJson
            ?? throw new FormatException($"'{name}' is missing from the CLDR data, or is not an object.");
    }

    /// <summary>
    /// Gets a member that is a string.
    /// </summary>
    /// <param name="name">The name of the member.</param>
    /// <returns>The string.</returns>
    /// <exception cref="FormatException">The member is missing, or is not a string.</exception>
    public string String(string name)
    {
        return Member(name) as string
            ?? throw new FormatException($"'{name}' is missing from the CLDR data, or is not a string.");
    }

    private object? Member(string name)
    {
        foreach (var member in _members)
        {
            if (string.Equals(member.Key, name, StringComparison.Ordinal))
            {
                return member.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Reads an object out of the text of a file.
    /// </summary>
    /// <param name="text">The text of the file.</param>
    /// <returns>The object it holds.</returns>
    /// <exception cref="FormatException">The text is not the shape this reads.</exception>
    public static CldrJson Parse(string text)
    {
        var position = 0;
        var value = ReadObject(text, ref position);

        SkipWhitespace(text, ref position);

        if (position != text.Length)
        {
            throw Failure(text, position, "the file carries something after the object it holds");
        }

        return value;
    }

    private static CldrJson ReadObject(string text, ref int position)
    {
        Expect(text, ref position, '{');
        SkipWhitespace(text, ref position);

        var members = new List<KeyValuePair<string, object>>();

        if (Peek(text, position) == '}')
        {
            position++;
            return new CldrJson(members);
        }

        while (true)
        {
            SkipWhitespace(text, ref position);
            var name = ReadString(text, ref position);

            SkipWhitespace(text, ref position);
            Expect(text, ref position, ':');
            SkipWhitespace(text, ref position);

            object value = Peek(text, position) == '{'
                ? ReadObject(text, ref position)
                : ReadString(text, ref position);

            members.Add(new KeyValuePair<string, object>(name, value));

            SkipWhitespace(text, ref position);
            var next = Peek(text, position);

            if (next == ',')
            {
                position++;
                continue;
            }

            if (next == '}')
            {
                position++;
                return new CldrJson(members);
            }

            throw Failure(text, position, "a ',' or a '}' was expected");
        }
    }

    private static string ReadString(string text, ref int position)
    {
        Expect(text, ref position, '"');

        var value = new StringBuilder();

        while (true)
        {
            if (position >= text.Length)
            {
                throw Failure(text, position, "the file ends inside a string");
            }

            var character = text[position++];

            if (character == '"')
            {
                return value.ToString();
            }

            if (character != '\\')
            {
                value.Append(character);
                continue;
            }

            if (position >= text.Length)
            {
                throw Failure(text, position, "the file ends inside an escape");
            }

            var escaped = text[position++];

            switch (escaped)
            {
                case '"': value.Append('"'); break;
                case '\\': value.Append('\\'); break;
                case '/': value.Append('/'); break;
                case 'b': value.Append('\b'); break;
                case 'f': value.Append('\f'); break;
                case 'n': value.Append('\n'); break;
                case 'r': value.Append('\r'); break;
                case 't': value.Append('\t'); break;
                case 'u':
                    if (position + 4 > text.Length)
                    {
                        throw Failure(text, position, "the file ends inside an escape");
                    }

                    value.Append((char)ushort.Parse(
                        text.Substring(position, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    position += 4;
                    break;
                default:
                    throw Failure(text, position - 1, $"'\\{escaped}' is not an escape");
            }
        }
    }

    private static void Expect(string text, ref int position, char expected)
    {
        SkipWhitespace(text, ref position);

        if (Peek(text, position) != expected)
        {
            throw Failure(text, position, $"'{expected}' was expected");
        }

        position++;
    }

    private static char Peek(string text, int position)
    {
        return position < text.Length ? text[position] : '\0';
    }

    private static void SkipWhitespace(string text, ref int position)
    {
        while (position < text.Length && char.IsWhiteSpace(text[position]))
        {
            position++;
        }
    }

    private static FormatException Failure(string text, int position, string reason)
    {
        var line = 1;

        for (var index = 0; index < position && index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                line++;
            }
        }

        return new FormatException($"The CLDR data could not be read at line {line}: {reason}.");
    }
}
