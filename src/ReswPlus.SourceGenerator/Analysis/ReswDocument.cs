using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ReswPlus.Core.ResourceParser;
using ReswPlus.SourceGenerator.ClassGenerators;

namespace ReswPlus.SourceGenerator.Analysis;

/// <summary>
/// A single <c>&lt;data&gt;</c> entry of a <c>.resw</c> file, together with its position in the file.
/// </summary>
internal sealed class ReswEntry
{
    public ReswEntry(ReswItem item, Location location)
    {
        Item = item;
        Location = location;
    }

    /// <summary>
    /// Gets the parsed resource, in the shape consumed by the generation pipeline.
    /// </summary>
    public ReswItem Item { get; }

    /// <summary>
    /// Gets the location of the name of the resource, so diagnostics can point at the offending line.
    /// </summary>
    public Location Location { get; }

    /// <summary>
    /// Gets the name of the resource.
    /// </summary>
    public string Key => Item.Key;

    /// <summary>
    /// Gets the value of the resource.
    /// </summary>
    public string Value => Item.Value;
}

/// <summary>
/// A parsed <c>.resw</c> file.
/// </summary>
/// <remarks>
/// This mirrors <see cref="ReswParser"/>, but it is based on <see cref="XmlReader"/> so that the position of
/// every entry can be recorded. Diagnostics that can be double clicked are much more useful than diagnostics
/// reported on the project.
/// </remarks>
internal sealed class ReswDocument
{
    private ReswDocument(string path, string? language, IReadOnlyList<ReswEntry> entries)
    {
        Path = path;
        Language = language;
        Entries = entries;
    }

    /// <summary>
    /// Gets the path of the <c>.resw</c> file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the language of the file, taken from the name of the folder containing it, or <see langword="null"/>
    /// if the file is not inside a folder.
    /// </summary>
    /// <remarks>
    /// This is the whole tag the folder is named with, normalised, because a region can decline differently
    /// from the language it belongs to: CLDR publishes separate rules for <c>pt-PT</c> and for bare <c>pt</c>,
    /// which is what <c>pt-BR</c> follows.
    /// </remarks>
    public string? Language { get; }

    /// <summary>
    /// Gets the entries of the file, in document order.
    /// </summary>
    public IReadOnlyList<ReswEntry> Entries { get; }

    /// <summary>
    /// Parses a <c>.resw</c> file.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="text">The content of the file.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The parsed file, or <see langword="null"/> if the content is not valid XML.</returns>
    public static ReswDocument? Parse(string path, SourceText text, CancellationToken cancellationToken)
    {
        var entries = new List<ReswEntry>();

        try
        {
            using var reader = XmlReader.Create(new StringReader(text.ToString()), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
            var lineInfo = reader as IXmlLineInfo;

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "data" || reader.NamespaceURI.Length != 0)
                {
                    continue;
                }

                if (TryReadEntry(reader, lineInfo, path, text, out var entry))
                {
                    entries.Add(entry!);
                }
            }
        }
        catch (XmlException)
        {
            // The .resw file is not valid XML, which the generation pipeline reports on its own. There is
            // nothing useful this analysis can add, and every rule would fire on incomplete data.
            return null;
        }

        return new ReswDocument(path, GetLanguage(path), entries);
    }

    /// <summary>
    /// Reads the <c>&lt;data&gt;</c> element the reader is positioned on, leaving the reader on its end tag.
    /// </summary>
    private static bool TryReadEntry(XmlReader reader, IXmlLineInfo? lineInfo, string path, SourceText text, out ReswEntry? entry)
    {
        entry = null;

        var dataDepth = reader.Depth;
        var isEmpty = reader.IsEmptyElement;

        if (!reader.MoveToAttribute("name"))
        {
            return false;
        }

        var key = reader.Value;
        var location = GetAttributeValueLocation(reader, lineInfo, path, text, key.Length);

        _ = reader.MoveToElement();

        if (isEmpty)
        {
            return false;
        }

        string? value = null;
        string? comment = null;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == dataDepth)
            {
                break;
            }

            if (reader.NodeType != XmlNodeType.Element || reader.Depth != dataDepth + 1 || reader.NamespaceURI.Length != 0)
            {
                continue;
            }

            if (value is null && reader.LocalName == "value")
            {
                value = ReadTextContent(reader);
            }
            else if (comment is null && reader.LocalName == "comment")
            {
                comment = ReadTextContent(reader);
            }
        }

        // A <data> element with no <value> child is skipped by the parser used for generation as well.
        if (value is null)
        {
            return false;
        }

        entry = new ReswEntry(new ReswItem(key, value, comment), location);

        return true;
    }

    /// <summary>
    /// Reads the concatenated text of the element the reader is positioned on, leaving the reader on its end tag.
    /// </summary>
    private static string ReadTextContent(XmlReader reader)
    {
        if (reader.IsEmptyElement)
        {
            return string.Empty;
        }

        var elementDepth = reader.Depth;
        var builder = new StringBuilder();

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == elementDepth)
            {
                break;
            }

            switch (reader.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    _ = builder.Append(reader.Value);
                    break;
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the location of the value of the attribute the reader is currently positioned on.
    /// </summary>
    private static Location GetAttributeValueLocation(XmlReader reader, IXmlLineInfo? lineInfo, string path, SourceText text, int length)
    {
        // Moving to the value of the attribute makes the line info point inside the quotes, which is where the
        // name of the resource actually starts.
        if (lineInfo is null || !lineInfo.HasLineInfo() || !reader.ReadAttributeValue())
        {
            return CreateFileLocation(path);
        }

        var line = lineInfo.LineNumber - 1;
        var character = lineInfo.LinePosition - 1;

        if (line < 0 || character < 0 || line >= text.Lines.Count)
        {
            return CreateFileLocation(path);
        }

        var startOffset = Math.Min(text.Lines[line].Start + character, text.Length);
        var endOffset = Math.Min(startOffset + length, text.Length);

        return Location.Create(
            path,
            TextSpan.FromBounds(startOffset, endOffset),
            new LinePositionSpan(new LinePosition(line, character), new LinePosition(line, character + length)));
    }

    /// <summary>
    /// Builds a location pointing at the beginning of a file, used when no precise position is available.
    /// </summary>
    private static Location CreateFileLocation(string path)
    {
        return Location.Create(path, default, default);
    }

    /// <summary>
    /// Returns the language a <c>.resw</c> file is written in, based on the folder containing it.
    /// </summary>
    /// <remarks>
    /// The whole tag is kept rather than just the language: a region can decline differently from the language
    /// it belongs to, so a <c>pt-PT</c> folder has to stay distinguishable from a <c>pt-BR</c> one.
    /// </remarks>
    private static string? GetLanguage(string path)
    {
        var folder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));

        if (string.IsNullOrEmpty(folder))
        {
            return null;
        }

        return PluralFormsRetriever.NormalizeTag(folder);
    }
}
