using System;
using System.Collections.Generic;
using System.Linq;
using ReswPlus.SourceGenerator.Plurals;

namespace ReswPlus.SourceGenerator.ClassGenerators;

/// <summary>
/// A set of plural rules, and the languages Unicode CLDR gives it to.
/// </summary>
/// <summary>
/// Finds the plural rules of a language.
/// </summary>
/// <remarks>
/// Which languages share a set of rules is not written down anywhere here. CLDR publishes the rules of every
/// language it knows, and the languages whose rules are the same thing share a form because their rules compare
/// equal, so a language CLDR adds, moves or revises follows its rules without anyone editing a list.
/// </remarks>
internal static class PluralFormsRetriever
{
    private static readonly IReadOnlyList<CldrPluralForm> PluralForms = CldrPluralRules.Forms;

    private static readonly Dictionary<string, CldrPluralForm> LanguageToPluralForm = BuildLanguageToPluralForm();

    /// <summary>
    /// Gets every plural form ReswPlus ships, so that tests can check them as a set.
    /// </summary>
    internal static IEnumerable<CldrPluralForm> PluralFormsForTesting => PluralForms;

    private static Dictionary<string, CldrPluralForm> BuildLanguageToPluralForm()
    {
        var byLanguage = new Dictionary<string, CldrPluralForm>(StringComparer.Ordinal);

        foreach (var form in PluralForms)
        {
            foreach (var language in form.Languages)
            {
                byLanguage[NormalizeTag(language)] = form;
            }
        }

        return byLanguage;
    }

    /// <summary>
    /// Puts a language tag in the form the plural forms are keyed by.
    /// </summary>
    /// <param name="languageTag">The tag, as a resource folder or a culture names it.</param>
    /// <returns>The tag, lower cased and written with the separator BCP 47 uses.</returns>
    /// <remarks>
    /// Windows writes a tag either way round -- <c>pt-PT</c> in a resource folder, <c>pt_PT</c> in some culture
    /// names -- and neither casing is guaranteed.
    /// </remarks>
    public static string NormalizeTag(string languageTag)
    {
        return languageTag.Replace('_', '-').ToLowerInvariant();
    }

    /// <summary>
    /// Retrieves the plural forms that apply to the given list of languages.
    /// </summary>
    /// <param name="languages">A collection of language codes to retrieve plural forms for.</param>
    /// <returns>The distinct plural forms those languages use.</returns>
    public static IEnumerable<CldrPluralForm> RetrievePluralFormsForLanguages(IEnumerable<string> languages)
    {
        var result = new Dictionary<string, CldrPluralForm>(StringComparer.Ordinal);

        foreach (var language in languages)
        {
            if (RetrievePluralFormForLanguage(language) is { } form)
            {
                result[form.Id] = form;
            }
        }

        return result.Values;
    }

    /// <summary>
    /// Retrieves the plural form of a language.
    /// </summary>
    /// <param name="language">The language tag to retrieve the plural form for.</param>
    /// <returns>
    /// The plural form of <paramref name="language"/>, or <see langword="null"/> if CLDR publishes no rules for
    /// it, in which case no plural form can be assumed to be required.
    /// </returns>
    /// <remarks>
    /// The whole tag is looked up before the language on its own, because a region can decline differently from
    /// the language it belongs to: <c>pt-PT</c> does not follow the rules CLDR gives <c>pt</c>. A tag no rules
    /// are held for falls back on the rules of the language, which is what makes <c>fr-CA</c> decline like
    /// <c>fr</c> without either having to be listed.
    /// </remarks>
    public static CldrPluralForm? RetrievePluralFormForLanguage(string language)
    {
        var tag = NormalizeTag(language);

        while (tag.Length != 0)
        {
            if (LanguageToPluralForm.TryGetValue(tag, out var pluralForm))
            {
                return pluralForm;
            }

            var separator = tag.LastIndexOf('-');
            tag = separator <= 0 ? string.Empty : tag.Substring(0, separator);
        }

        return null;
    }

    /// <summary>
    /// Retrieves the languages ReswPlus has no plural rules for.
    /// </summary>
    /// <param name="languages">A collection of language codes to check.</param>
    /// <returns>The distinct language codes that aren't mapped to a plural form.</returns>
    /// <remarks>
    /// Those languages fall back to the single-form provider, which is silently correct for a language that
    /// really has one form and silently wrong for one that doesn't, so the caller reports them.
    /// </remarks>
    public static IEnumerable<string> RetrieveLanguagesWithoutPluralForm(IEnumerable<string> languages)
    {
        var reported = new HashSet<string>(StringComparer.Ordinal);

        foreach (var language in languages)
        {
            if (RetrievePluralFormForLanguage(language) is null && reported.Add(language))
            {
                yield return language;
            }
        }
    }
}
