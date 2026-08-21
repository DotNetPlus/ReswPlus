using ReswPlus.SourceGenerator.ClassGenerators;

namespace ReswPlus.SourceGenerator.Plurals;

/// <summary>
/// One set of plural rules, the languages Unicode CLDR gives it to, and the class implementing it.
/// </summary>
/// <param name="Id">The identifier of the class, generated from the languages that share the rules.</param>
/// <param name="Languages">The languages these rules apply to, including the codes CLDR has renamed.</param>
/// <param name="Categories">The categories the rules can select, which a resource has to define.</param>
/// <param name="OptionalCategories">The categories of <paramref name="Categories"/> a resource may leave out.</param>
/// <param name="ZeroIsOnlyForZeroQuantity">Whether <c>zero</c> is selected by nothing but zero itself.</param>
/// <param name="Source">The source of the class, ready to be emitted into a compilation.</param>
/// <remarks>
/// Everything here is worked out by <c>tools/CldrRuleImporter</c> and written into
/// <c>CldrPluralRules.g.cs</c>. Nothing about CLDR's file or its rule syntax is read while a project compiles.
/// </remarks>
internal sealed record CldrPluralForm(
    string Id,
    string[] Languages,
    PluralCategory[] Categories,
    PluralCategory[] OptionalCategories,
    bool ZeroIsOnlyForZeroQuantity,
    string Source);
