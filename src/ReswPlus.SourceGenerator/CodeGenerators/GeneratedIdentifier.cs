using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace ReswPlus.SourceGenerator.CodeGenerators;

/// <summary>
/// The rules the names of a <c>.resw</c> file have to follow to survive being emitted as C# identifiers.
/// </summary>
/// <remarks>
/// The names in a resource file are written by whoever writes the strings, which is routinely not the person
/// who writes the code, and nothing about the resource format restricts them to what C# accepts. Emitting them
/// as identifiers verbatim makes the generator produce source that doesn't build, and the error the consumer
/// gets points at generated code rather than at the resource that caused it.
/// </remarks>
internal static class GeneratedIdentifier
{
    /// <summary>
    /// The members the generated types declare on their own, whatever the resource file holds.
    /// </summary>
    /// <remarks>
    /// <c>GetString</c> and <c>_resourceStringProvider</c> are declared by the strongly typed class, and
    /// <c>_Undefined</c> is the first member of the key enumeration of the markup extension. <c>KeyEnum</c> is
    /// the name of that enumeration, and a member of an enumeration cannot carry the name of the enumeration
    /// itself.
    /// </remarks>
    private static readonly string[] GeneratedMemberNames =
    [
        "GetString",
        "_resourceStringProvider",
        "_Undefined",
        "KeyEnum",
    ];

    /// <summary>
    /// Returns the name to emit for a resource or a parameter.
    /// </summary>
    /// <param name="name">The name as it is written in the resource file.</param>
    /// <returns>The name, escaped when C# reserves it.</returns>
    /// <remarks>
    /// Only the reserved keywords are escaped. The contextual ones, such as <c>value</c> or <c>var</c>, are
    /// valid identifiers already, and escaping them would change the generated API for no reason.
    /// </remarks>
    public static string Escape(string name)
    {
        return SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None ? name : "@" + name;
    }

    /// <summary>
    /// Returns whether a resource cannot be emitted because the generated types already declare its name.
    /// </summary>
    /// <param name="name">The name of the generated member, which is the name of the resource without the plural or variant suffix.</param>
    /// <param name="className">The name of the generated class, which its static constructor carries as well.</param>
    /// <returns>Whether emitting the resource would produce source that doesn't build.</returns>
    /// <remarks>
    /// The comparison is case sensitive, because C# member lookup is: a resource named <c>getString</c> sits
    /// beside the generated <c>GetString</c> without conflicting with it. Resources whose names only differ by
    /// case conflict for a different reason, and are reported by RESWP0009.
    /// </remarks>
    public static bool ConflictsWithGeneratedMember(string name, string className, bool hasResourceInterface = false)
    {
        return string.Equals(name, className, StringComparison.Ordinal)
            || (hasResourceInterface && string.Equals(name, "I" + className, StringComparison.Ordinal))
            || Array.IndexOf(GeneratedMemberNames, name) >= 0;
    }

    /// <summary>
    /// Renames the parameters that share a name, so that the generated member declares each of them once.
    /// </summary>
    /// <param name="parameters">The parameters of a generated member, in the order they are declared in.</param>
    /// <remarks>
    /// A name can be taken twice either because the <c>#Format</c> tag declares it twice, which RESWP0013
    /// reports, or because the generator adds a parameter of its own -- the quantity of a pluralized resource,
    /// the identifier of a varianted one -- whose name the tag already uses. The first parameter to claim a name
    /// keeps it, so the parameters the tag declares are left alone whenever the generator is the one intruding.
    /// </remarks>
    public static void MakeNamesUnique(IReadOnlyList<Core.ResourceParser.FunctionFormatTagParameter> parameters)
    {
        var taken = new HashSet<string>(StringComparer.Ordinal);

        foreach (var parameter in parameters)
        {
            if (taken.Add(parameter.Name))
            {
                continue;
            }

            var suffix = 2;

            while (!taken.Add(parameter.Name + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            {
                ++suffix;
            }

            parameter.Name += suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
