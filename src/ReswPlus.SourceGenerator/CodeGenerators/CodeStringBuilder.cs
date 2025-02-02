using System.Text;

namespace ReswPlus.SourceGenerator.CodeGenerators;

/// <summary>
/// A helper class to build code strings with indentation support.
/// </summary>
/// <param name="indentString">The string used for indentation.</param>
internal sealed class CodeStringBuilder(string indentString)
{
    private readonly StringBuilder _stringBuilder = new();
    private readonly string _indentString = indentString;
    private uint _level = 0;

    /// <summary>
    /// Appends a line to the string builder.
    /// </summary>
    /// <param name="value">The line to append.</param>
    /// <param name="addSpaces">Whether to add indentation spaces.</param>
    /// <returns>The current instance of <see cref="CodeStringBuilder"/>.</returns>
    public CodeStringBuilder AppendLine(string value, bool addSpaces = true)
    {
        if (addSpaces)
        {
            AddSpaces(_level);
        }
        _ = _stringBuilder.AppendLine(value);
        return this;
    }

    /// <summary>
    /// Adds indentation spaces based on the current level.
    /// </summary>
    /// <param name="level">The current indentation level.</param>
    private void AddSpaces(uint level)
    {
        for (var i = 0; i < level; ++i)
        {
            _ = _stringBuilder.Append(_indentString);
        }
    }

    /// <summary>
    /// Increases the indentation level.
    /// </summary>
    /// <returns>The current instance of <see cref="CodeStringBuilder"/>.</returns>
    public CodeStringBuilder AddLevel()
    {
        ++_level;
        return this;
    }

    /// <summary>
    /// Decreases the indentation level.
    /// </summary>
    /// <returns>The current instance of <see cref="CodeStringBuilder"/>.</returns>
    public CodeStringBuilder RemoveLevel()
    {
        if (_level > 0)
        {
            --_level;
        }
        return this;
    }

    /// <summary>
    /// Appends an empty line to the string builder.
    /// </summary>
    /// <returns>The current instance of <see cref="CodeStringBuilder"/>.</returns>
    public CodeStringBuilder AppendEmptyLine()
    {
        _ = _stringBuilder.AppendLine("");
        return this;
    }

    /// <summary>
    /// Gets the built string.
    /// </summary>
    /// <returns>The built string.</returns>
    public string GetString()
    {
        return _stringBuilder.ToString();
    }
}
