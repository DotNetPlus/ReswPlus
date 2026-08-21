using System.Collections.Generic;
using ReswPlus.SourceGenerator.ClassGenerators;

namespace ReswPlus.SourceGenerator.Plurals;

/// <summary>
/// One set of plural rules, and the languages Unicode CLDR gives it to.
/// </summary>
/// <param name="Id">The identifier of the class, generated from the languages that share the rules.</param>
/// <param name="Languages">The languages these rules apply to, including the codes CLDR has renamed.</param>
/// <param name="Categories">The categories the rules can select, which a resource has to define.</param>
/// <param name="OptionalCategories">The categories of <paramref name="Categories"/> a resource may leave out.</param>
/// <param name="ZeroIsOnlyForZeroQuantity">Whether <c>zero</c> is selected by nothing but zero itself.</param>
/// <param name="Rules">The rules themselves, in the order CLDR publishes them.</param>
/// <remarks>
/// Which languages share a set of rules, what the class is called, and which categories are optional are all
/// worked out by <c>tools/CldrRuleImporter</c> and written into <c>CldrPluralRules.cs</c>, so nothing about
/// CLDR's file or its rule syntax is read while a project compiles. The rules arrive as objects and the code
/// deciding them is written by <see cref="CldrEmitter"/> when a project is generated.
/// </remarks>
internal sealed record CldrPluralForm(
    string Id,
    string[] Languages,
    PluralCategory[] Categories,
    PluralCategory[] OptionalCategories,
    bool ZeroIsOnlyForZeroQuantity,
    IReadOnlyList<CldrPluralRule> Rules);

/// <summary>
/// One rule: the category it selects, and the condition under which it does.
/// </summary>
/// <param name="Category">The category selected.</param>
/// <param name="Condition">
/// The condition, or <see langword="null"/> for CLDR's fallback rule, which carries none and is reached only
/// when no other rule matches.
/// </param>
/// <remarks>
/// The condition as CLDR publishes it is not kept beside the objects: it is written back out of them by
/// <see cref="ICldrCondition.ToCldr"/> when a provider quotes it. One representation, so there is nothing to
/// disagree with itself.
/// </remarks>
internal sealed record CldrPluralRule(PluralCategory Category, ICldrCondition? Condition);
