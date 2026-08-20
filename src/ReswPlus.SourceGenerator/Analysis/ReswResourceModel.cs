using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReswPlus.Core.ResourceParser;
using ReswPlus.SourceGenerator.ClassGenerators;

namespace ReswPlus.SourceGenerator.Analysis;

/// <summary>
/// A member the generator produces for a <c>.resw</c> file, and the entries it is produced from.
/// </summary>
internal sealed class ReswMember
{
    public ReswMember(string name, bool isPlural, IReadOnlyList<ReswEntry> entries, ReswFormatTag formatTag)
    {
        Name = name;
        IsPlural = isPlural;
        Entries = entries;
        FormatParameterCount = formatTag.ParameterCount;
        FormatParameterNames = formatTag.ParameterNames;
    }

    /// <summary>
    /// Gets the name of the generated member.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets whether the member is generated from a set of pluralized resources.
    /// </summary>
    public bool IsPlural { get; }

    /// <summary>
    /// Gets the entries the member is generated from, in document order. A member generated from a plain
    /// resource has a single entry, a pluralized or varianted one has an entry per form.
    /// </summary>
    public IReadOnlyList<ReswEntry> Entries { get; }

    /// <summary>
    /// Gets the names of the parameters declared by the <c>#Format</c> tag of the resource, in declaration
    /// order. Only the parameters the generated member takes an argument for are listed: the literal strings
    /// and the references to other resources of the tag are resolved at generation time and carry no name.
    /// </summary>
    public IReadOnlyList<string> FormatParameterNames { get; }

    /// <summary>
    /// Gets the number of parameters declared by the <c>#Format</c> tag of the resource, or <c>0</c> when the
    /// resource has no usable tag and its value is therefore returned without being formatted.
    /// </summary>
    public int FormatParameterCount { get; }

    /// <summary>
    /// Gets whether the value of the resource is passed to <see cref="string.Format(string, object[])"/>.
    /// </summary>
    public bool IsFormatted => FormatParameterCount > 0;
}

/// <summary>
/// What the <c>#Format</c> tag of a resource declares.
/// </summary>
internal readonly struct ReswFormatTag
{
    public ReswFormatTag(int parameterCount, IReadOnlyList<string> parameterNames)
    {
        ParameterCount = parameterCount;
        ParameterNames = parameterNames;
    }

    /// <summary>
    /// Gets the number of arguments the generated code passes to <see cref="string.Format(string, object[])"/>,
    /// which counts the literal strings and resource references of the tag as well as its parameters.
    /// </summary>
    public int ParameterCount { get; }

    /// <summary>
    /// Gets the names of the parameters the generated member takes, in declaration order.
    /// </summary>
    public IReadOnlyList<string> ParameterNames { get; }

    /// <summary>
    /// The tag of a resource that has none, or whose tag cannot be parsed. In both cases the generated member
    /// returns the value of the resource without formatting it.
    /// </summary>
    public static readonly ReswFormatTag None = new(0, []);
}

/// <summary>
/// The members a <c>.resw</c> file is generated as.
/// </summary>
/// <remarks>
/// This mirrors the classification done by <see cref="ReswClassGenerator"/> when it parses a resource file, so
/// that the diagnostics reason about exactly what the generator emits. It is deliberately a separate, read only
/// pass: the diagnostics are additive and must not influence generation.
/// </remarks>
internal sealed class ReswResourceModel
{
    private readonly Dictionary<string, ReswMember> _membersByName;
    private readonly Dictionary<string, ReswEntry> _entriesByKey;

    private ReswResourceModel(ReswDocument document, IReadOnlyList<ReswMember> members)
    {
        Document = document;
        Members = members;

        // Resource lookup is case insensitive, so a resource is identified the same way here: it is how the
        // runtime matches a plural form or a translation back to the resource the generated member reads.
        _membersByName = new Dictionary<string, ReswMember>(StringComparer.OrdinalIgnoreCase);
        _entriesByKey = new Dictionary<string, ReswEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in members)
        {
            if (!_membersByName.ContainsKey(member.Name))
            {
                _membersByName.Add(member.Name, member);
            }
        }

        foreach (var entry in document.Entries)
        {
            if (!_entriesByKey.ContainsKey(entry.Key))
            {
                _entriesByKey.Add(entry.Key, entry);
            }
        }
    }

    /// <summary>
    /// Gets the file the members are generated from.
    /// </summary>
    public ReswDocument Document { get; }

    /// <summary>
    /// Gets the generated members, ordered by the position of their first entry in the file.
    /// </summary>
    public IReadOnlyList<ReswMember> Members { get; }

    /// <summary>
    /// Looks up a generated member by name.
    /// </summary>
    /// <param name="name">The name of the member to look up.</param>
    /// <param name="member">The member, if it was found.</param>
    /// <returns>Whether a member with that name is generated.</returns>
    public bool TryGetMember(string name, out ReswMember member)
    {
        return _membersByName.TryGetValue(name, out member);
    }

    /// <summary>
    /// Looks up an entry of the file by resource name.
    /// </summary>
    /// <param name="key">The name of the resource to look up.</param>
    /// <param name="entry">The entry, if it was found.</param>
    /// <returns>Whether the file declares that resource.</returns>
    public bool TryGetEntry(string key, out ReswEntry entry)
    {
        return _entriesByKey.TryGetValue(key, out entry);
    }

    /// <summary>
    /// Builds the model of a parsed <c>.resw</c> file.
    /// </summary>
    /// <param name="document">The file to build the model of.</param>
    /// <returns>The members the file is generated as.</returns>
    public static ReswResourceModel Create(ReswDocument document)
    {
        var entriesByItem = new Dictionary<ReswItem, ReswEntry>();
        var positions = new Dictionary<ReswItem, int>();

        for (var i = 0; i < document.Entries.Count; i++)
        {
            var entry = document.Entries[i];

            entriesByItem[entry.Item] = entry;
            positions[entry.Item] = i;
        }

        var items = document.Entries.Select(entry => entry.Item).ToArray();

        // The classification below follows ReswClassGenerator.Parse: pluralized and varianted resources are
        // grouped first, out of all the items, and whatever is left and has a usable name becomes a plain member.
        var stringItems = items
            .Where(item => ReswClassGenerator.IsValidPropertyName(item.Key) && !(item.Comment?.Contains(ReswClassGenerator.TagIgnore) ?? false))
            .ToArray();

        var groups = items.GetItemsWithVariantOrPlural().ToArray();
        var basicItems = stringItems.Except(groups.SelectMany(group => group.Items)).ToArray();
        var resourceFileName = Path.GetFileName(document.Path);

        var members = new List<ReswMember>();

        foreach (var group in groups)
        {
            // Only one of the resources of a group carries the #Format tag, and it is not necessarily the first.
            var comment = group.Items.FirstOrDefault(item => HasFormatTag(item.Comment))?.Comment;

            members.Add(new ReswMember(
                group.Key,
                group.SupportPlural,
                group.Items.Where(entriesByItem.ContainsKey).Select(item => entriesByItem[item]).ToArray(),
                ReadFormatTag(group.Key, comment, basicItems, resourceFileName)));
        }

        foreach (var item in basicItems)
        {
            // The generator resolves the references of a plain resource against the same set: ReswClassGenerator
            // narrows its own list of items down to the ones left after grouping before it parses their tags.
            members.Add(new ReswMember(
                item.Key,
                isPlural: false,
                [entriesByItem[item]],
                ReadFormatTag(item.Key, item.Comment, basicItems, resourceFileName)));
        }

        return new ReswResourceModel(
            document,
            members.Where(member => member.Entries.Count > 0)
                   .OrderBy(member => positions[member.Entries[0].Item])
                   .ToArray());
    }

    private static bool HasFormatTag(string? comment)
    {
        return ReswClassGenerator.ParseTag(comment).format is not null;
    }

    /// <summary>
    /// Reads what the <c>#Format</c> tag of a resource declares.
    /// </summary>
    /// <param name="key">The name of the resource, used for diagnostics of the tag parser.</param>
    /// <param name="comment">The comment carrying the <c>#Format</c> tag, if any.</param>
    /// <param name="knownItems">The resources a <c>Reference()</c> parameter of the tag can point at.</param>
    /// <param name="resourceFileName">The name of the resource file, used for diagnostics of the tag parser.</param>
    /// <returns>
    /// What the tag declares, or <see cref="ReswFormatTag.None"/> when there is no tag or the tag cannot be
    /// parsed, since in both cases the generated member returns the value of the resource without formatting it.
    /// </returns>
    private static ReswFormatTag ReadFormatTag(string key, string? comment, IEnumerable<ReswItem> knownItems, string resourceFileName)
    {
        var (format, _) = ReswClassGenerator.ParseTag(comment);

        if (format is null)
        {
            return ReswFormatTag.None;
        }

        var parameters = FormatTag.ParseParameters(key, FormatTag.SplitParameters(format), knownItems, resourceFileName, logger: null);

        if (parameters is null)
        {
            return ReswFormatTag.None;
        }

        return new ReswFormatTag(
            parameters.Parameters.Count,
            parameters.Parameters.OfType<FunctionFormatTagParameter>().Select(parameter => parameter.Name).ToArray());
    }
}
