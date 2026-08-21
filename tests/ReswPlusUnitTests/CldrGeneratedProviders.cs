using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReswPlus.SourceGenerator.ClassGenerators;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Checks whether the plural providers could be written from the rules instead of by hand.
/// </summary>
/// <remarks>
/// The providers ReswPlus ships are hand written, and <see cref="CldrDrift"/> checks that they say what CLDR
/// says. This asks the next question: whether the same C# can be produced from the rules themselves, compiled,
/// and relied on to decide every quantity exactly as the hand written provider does.
/// <para>
/// Nothing here ships. It is the evidence for the question, kept as a test so the answer stays true: the day
/// a rule appears that cannot be written this way, or a generated provider stops agreeing with the one beside
/// it, this is what says so.
/// </para>
/// </remarks>
public class CldrGeneratedProviders
{
    /// <summary>
    /// The plural forms ReswPlus ships, by the identifier of the provider implementing them.
    /// </summary>
    public static TheoryData<string> Forms =>
        [.. PluralFormsRetriever.PluralFormsForTesting.Select(form => form.Id)];

    /// <summary>
    /// The templates a provider is compiled against.
    /// </summary>
    private static readonly string[] SharedTemplates =
    [
        "Plurals.IPluralProvider",
        "Plurals.PluralTypeEnum",
        "Utils.IntExt",
        "Utils.DoubleExt"
    ];

    [Theory]
    [MemberData(nameof(Forms))]
    public void AProviderWrittenFromTheRulesDecidesWhatTheHandWrittenOneDecides(string formId)
    {
        var form = PluralFormsRetriever.PluralFormsForTesting.Single(candidate => candidate.Id == formId);
        var handWritten = PluralProviderHost.GetProvider(formId);
        var checkedAny = false;

        // A form is given languages CLDR may decline differently -- that is what CldrDrift checks -- so the
        // rules of each distinct group are written out and compared on their own.
        foreach (var group in form.Languages.Select(CldrLanguages.RulesOf).Where(rules => rules is not null).Distinct(CldrLanguages.RuleComparer.Instance))
        {
            var generated = Compile(formId, group!);
            var mistakes = new List<string>();

            foreach (var quantity in CldrQuantities.Sweep)
            {
                var fromRules = generated(quantity);
                var byHand = handWritten(quantity);

                if (!string.Equals(fromRules, byHand, StringComparison.Ordinal))
                {
                    mistakes.Add(
                        $"    {quantity.ToString("R", CultureInfo.InvariantCulture)}: written from the rules selects "
                            + $"'{fromRules}', {formId}Provider selects '{byHand}'");
                    break;
                }
            }

            Assert.True(
                mistakes.Count == 0,
                $"A provider written from the rules of '{formId}' disagrees with the one written by hand:"
                    + $"{Environment.NewLine}{string.Join(Environment.NewLine, mistakes)}"
                    + $"{Environment.NewLine}{Environment.NewLine}{CldrEmitter.Emit($"{formId}Provider", group!)}");

            checkedAny = true;
        }

        Assert.True(checkedAny, $"No CLDR rules were found for any language of the '{formId}' form.");
    }

    /// <summary>
    /// Compiles a provider written from a set of rules, and returns it as a function from a quantity to a
    /// plural category.
    /// </summary>
    /// <param name="formId">The identifier to name the class after.</param>
    /// <param name="rules">The rules to write it from.</param>
    /// <returns>A function returning the name of the category selected for a quantity.</returns>
    private static Func<double, string> Compile(string formId, IReadOnlyList<CldrPublishedRules.Rule> rules)
    {
        var className = $"{formId}FromRulesProvider";
        var sources = SharedTemplates.Select(PluralTemplates.Read).Concat([CldrEmitter.Emit(className, rules)]);

        var assembly = PluralTemplates.Compile($"ReswPlusGeneratedPlurals.{className}", sources);
        var type = assembly.GetTypes().Single(candidate => candidate.Name == className);
        var instance = Activator.CreateInstance(type, nonPublic: true);
        var computePlural = type.GetMethod("ComputePlural")!;

        return number => computePlural.Invoke(instance, [number])!.ToString()!;
    }

}
