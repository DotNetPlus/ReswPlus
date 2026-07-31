namespace ReswPlus.SourceGenerator.ClassGenerators;

/// <summary>
/// The CLDR plural categories a resource can be declined in.
/// </summary>
/// <remarks>
/// The names match the suffixes ReswPlus appends to the key of a pluralized resource at runtime, so a category
/// maps directly to the resource named <c>&lt;key&gt;_&lt;category&gt;</c>.
/// </remarks>
internal enum PluralCategory
{
    /// <summary>The 'zero' plural category.</summary>
    Zero,

    /// <summary>The 'one' plural category.</summary>
    One,

    /// <summary>The 'two' plural category.</summary>
    Two,

    /// <summary>The 'few' plural category.</summary>
    Few,

    /// <summary>The 'many' plural category.</summary>
    Many,

    /// <summary>The 'other' plural category, used when no other category applies.</summary>
    Other
}
