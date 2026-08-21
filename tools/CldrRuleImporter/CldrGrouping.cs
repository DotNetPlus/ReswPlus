using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CldrRuleImporter;

/// <summary>
/// The plural forms ReswPlus ships, worked out from the rules Unicode CLDR publishes.
/// </summary>
/// <remarks>
/// A form is a set of rules and the languages CLDR gives it to. Which languages share a form is not decided
/// here: CLDR publishes the rules of every language, and the languages whose rules are the same thing share a
/// form because their rules compare equal. Two hundred and some languages come out as a few dozen forms, and a
/// language CLDR adds or moves follows its rules without anyone editing a list.
/// </remarks>
internal sealed class CldrGrouping
{
    private CldrGrouping(
        string id,
        IReadOnlyList<string> languages,
        IReadOnlyList<CldrPublishedRules.Rule> rules,
        IReadOnlyList<string> categories,
        IReadOnlyList<string> optionalCategories,
        bool zeroIsOnlyForZeroQuantity)
    {
        Id = id;
        Languages = languages;
        Rules = rules;
        Categories = categories;
        OptionalCategories = optionalCategories;
        ZeroIsOnlyForZeroQuantity = zeroIsOnlyForZeroQuantity;
    }

    /// <summary>
    /// Gets the identifier of the provider implementing this form.
    /// </summary>
    /// <remarks>
    /// Named after the first of its languages in alphabetical order, so that the same rules always produce the
    /// same class whoever builds them, and so that a language added to another form cannot rename this one.
    /// </remarks>
    public string Id { get; }

    /// <summary>
    /// Gets the languages CLDR gives these rules to.
    /// </summary>
    public IReadOnlyList<string> Languages { get; }

    /// <summary>
    /// Gets the rules themselves, in the order CLDR publishes them.
    /// </summary>
    public IReadOnlyList<CldrPublishedRules.Rule> Rules { get; }

    /// <summary>
    /// Gets the plural categories these rules can select, and which a resource declined in one of these
    /// languages therefore has to define.
    /// </summary>
    public IReadOnlyList<string> Categories { get; }

    /// <summary>
    /// Gets the categories of <see cref="Categories"/> a resource does not have to define.
    /// </summary>
    /// <remarks>
    /// A category belongs here when the rules only select it for a quantity an app is very unlikely to display,
    /// so that requiring it would warn about a form almost no resource set has a use for. French is the case
    /// this exists for: its <c>many</c> is selected only by exact non-zero multiples of a million. The lookup
    /// falls back to the <c>_Other</c> form when it isn't declared, which is the wording the resource set
    /// already ships for that quantity.
    /// </remarks>
    public IReadOnlyList<string> OptionalCategories { get; }

    /// <summary>
    /// Gets whether these rules only select <c>zero</c> for a quantity that is itself zero.
    /// </summary>
    /// <remarks>
    /// A resource that declares a <c>_None</c> form short circuits a zero quantity to it, so for such a form
    /// the <c>_Zero</c> resource becomes unreachable and is not required. Latvian is the exception: it also
    /// selects <c>zero</c> for quantities such as 11 or 20.
    /// </remarks>
    public bool ZeroIsOnlyForZeroQuantity { get; }

    /// <summary>
    /// The quantities two sets of rules have to agree on to be the same set of rules.
    /// </summary>
    /// <remarks>
    /// Grouping by the text of a rule would keep <c>i = 0,1</c> and <c>i = 0..1</c> apart, and CLDR writes both:
    /// they are the same rule spelled differently, and generating a class for each would be generating the same
    /// class twice. What the languages share is the answer, so the answer is what they are grouped by.
    /// <para>
    /// The range crosses every boundary the rules are written in terms of -- the tens, hundreds and thousands
    /// they are taken modulo, the millions some of them single out, and one and two decimals.
    /// </para>
    /// </remarks>
    private static readonly double[] Distinguishing = BuildDistinguishing();

    private static double[] BuildDistinguishing()
    {
        var quantities = new List<double>();

        for (var value = 0; value <= 1200; value++)
        {
            quantities.Add(value);
        }

        quantities.AddRange([10000, 100000, 1000000, 1000001, 2000000, 1000000.5]);

        for (var whole = 0; whole <= 20; whole++)
        {
            for (var hundredths = 1; hundredths <= 99; hundredths++)
            {
                quantities.Add(Parse($"{whole}.{hundredths:00}"));
            }

            for (var tenths = 1; tenths <= 9; tenths++)
            {
                quantities.Add(Parse($"{whole}.{tenths}"));
            }
        }

        return [.. quantities];

        static double Parse(string literal) => double.Parse(literal, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// The quantities a category has to be selected by to be worth requiring of a translator.
    /// </summary>
    /// <remarks>
    /// Everything up to a thousand, and the decimals of everything up to twenty. A category no quantity in that
    /// range selects is one a resource set would almost never have a use for.
    /// </remarks>
    private static IEnumerable<double> EverydayQuantities()
    {
        return Distinguishing.Where(quantity => quantity <= 1000);
    }

    /// <summary>
    /// Works out the forms from the rules CLDR publishes.
    /// </summary>
    /// <returns>Every form, in a stable order.</returns>
    public static IReadOnlyList<CldrGrouping> Create(
        IReadOnlyDictionary<string, IReadOnlyList<CldrPublishedRules.Rule>> cardinal)
    {
        var byRules = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var language in cardinal)
        {
            var signature = Signature(language.Value);

            if (!byRules.TryGetValue(signature, out var languages))
            {
                byRules[signature] = languages = [];
            }

            languages.Add(language.Key);

            // The codes CLDR renamed are carried with the language they were renamed to, so that a resource
            // folder or a Windows display language still named with one finds its rules without the generator
            // having to know about them. Some of them CLDR still publishes in their own right, and those are
            // already in hand.
            foreach (var deprecated in CldrLanguages.RenamedTo(language.Key))
            {
                if (!cardinal.ContainsKey(deprecated))
                {
                    languages.Add(deprecated);
                }
            }
        }

        var forms = new List<CldrGrouping>();

        foreach (var group in byRules)
        {
            var languages = group.Value.OrderBy(language => language, StringComparer.Ordinal).ToArray();
            var rules = cardinal[languages[0]];

            forms.Add(new CldrGrouping(
                Identifier(languages[0]),
                languages,
                rules,
                [.. rules.Select(rule => rule.Category)],
                Optional(rules),
                ZeroOnlyForZero(rules)));
        }

        // The identifier is derived from a language code, so two sets of rules whose first languages differ
        // only where the derivation strips could end up sharing a class name. Nothing in CLDR 48 does, and if
        // one ever did the generated code would fail to compile with a duplicate type rather than say why.
        var collisions = forms
            .GroupBy(form => form.Id, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} ({string.Join(", ", group.Select(form => form.Languages[0]))})")
            .ToArray();

        if (collisions.Length != 0)
        {
            throw new InvalidOperationException(
                $"Two sets of rules would share a class name: {string.Join("; ", collisions)}.");
        }

        return [.. forms.OrderBy(form => form.Id, StringComparer.Ordinal)];
    }

    /// <summary>
    /// Returns the categories no everyday quantity selects.
    /// </summary>
    private static IReadOnlyList<string> Optional(IReadOnlyList<CldrPublishedRules.Rule> rules)
    {
        var selected = new HashSet<string>(StringComparer.Ordinal);

        foreach (var quantity in EverydayQuantities())
        {
            selected.Add(CldrRule.Select(rules, quantity));
        }

        return [.. rules.Select(rule => rule.Category).Where(category => !selected.Contains(category))];
    }

    /// <summary>
    /// Returns whether <c>zero</c> is selected by nothing but zero itself.
    /// </summary>
    private static bool ZeroOnlyForZero(IReadOnlyList<CldrPublishedRules.Rule> rules)
    {
        return EverydayQuantities()
            .Where(quantity => quantity != 0)
            .All(quantity => !string.Equals(CldrRule.Select(rules, quantity), "ZERO", StringComparison.Ordinal));
    }

    /// <summary>
    /// Turns a language tag into the name of the class implementing its rules.
    /// </summary>
    private static string Identifier(string languageTag)
    {
        var identifier = new StringBuilder();
        var startOfWord = true;

        foreach (var character in languageTag)
        {
            if (!char.IsLetterOrDigit(character))
            {
                startOfWord = true;
                continue;
            }

            identifier.Append(startOfWord ? char.ToUpperInvariant(character) : character);
            startOfWord = false;
        }

        return identifier.ToString();
    }

    /// <summary>
    /// Returns what makes one set of rules the same as another: the answer it gives, not the way it is written.
    /// </summary>
    private static string Signature(IReadOnlyList<CldrPublishedRules.Rule> rules)
    {
        var answers = new StringBuilder();

        foreach (var quantity in Distinguishing)
        {
            answers.Append(CldrRule.Select(rules, quantity)).Append('|');
        }

        return answers.ToString();
    }
}
