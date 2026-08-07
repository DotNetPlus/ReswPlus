using System.Collections.Generic;

namespace ReswPlus.SourceGenerator.ClassGenerators;

/// <summary>
/// Provides functionality to manage and retrieve pluralization rules for various languages.
/// </summary>
internal sealed class PluralFormsRetriever
{
    /// <summary>
    /// A plural form supported by a set of languages.
    /// </summary>
    internal record PluralForm
    {
        public PluralForm(string id, PluralCategory[] categories, string[] languages)
        {
            Id = id;
            Categories = categories;
            Languages = languages;
        }

        /// <summary>
        /// Gets the identifier of the provider implementing this plural form.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the plural categories the provider of this form can return, and which a resource declined in a
        /// language using this form therefore has to define.
        /// </summary>
        public PluralCategory[] Categories { get; set; }

        /// <summary>
        /// Gets the categories of <see cref="Categories"/> that a resource does not have to define.
        /// </summary>
        /// <remarks>
        /// A category belongs here when the provider only returns it for a quantity an app is very unlikely to
        /// display, so that requiring it would warn about a form almost no resource set has a use for. The
        /// lookup falls back to the <c>_Other</c> form when it isn't declared, which is the wording the
        /// resource set already ships for that quantity.
        /// </remarks>
        public PluralCategory[] OptionalCategories { get; set; } = [];

        /// <summary>
        /// Gets whether the provider of this form only returns <see cref="PluralCategory.Zero"/> for a quantity
        /// that is itself zero.
        /// </summary>
        /// <remarks>
        /// A resource that declares a <c>_None</c> form short circuits a zero quantity to it, so for such a form
        /// the <c>_Zero</c> resource becomes unreachable and is not required. Latvian is the exception: its
        /// provider also returns <see cref="PluralCategory.Zero"/> for quantities such as 11 or 20.
        /// </remarks>
        public bool ZeroIsOnlyForZeroQuantity { get; set; } = true;

        /// <summary>
        /// Gets the languages using this plural form.
        /// </summary>
        public string[] Languages { get; set; }
    }

    /// <summary>
    /// A static collection of predefined plural forms and their associated languages.
    /// </summary>
    private static readonly PluralForm[] PluralForms =
    [
        new PluralForm(
            "IntOneOrZero",
            [PluralCategory.One, PluralCategory.Other],
            [
                "ak", // Akan
                "bh", // Bihari
                "guw", // Gun
                "ln", // Lingala
                "mg", // Malagasy
                "nso", // Northern Sotho
                "pa", // Punjabi
                "ti", // Tigrinya
                "wa"  // Walloon
            ]
        ),
        new PluralForm(
            "ZeroToOne",
            [PluralCategory.One, PluralCategory.Other],
            [
                "am", // Amharic
                "bn", // Bengali
                "gu", // Gujarati
                "hi", // Hindi
                "kn", // Kannada
                "fa", // Persian
                "zu"  // Zulu
            ]
        ),
        new PluralForm(
            "ZeroToTwoExcluded",
            [PluralCategory.One, PluralCategory.Other],
            [
                "hy", // Armenian
                "ff", // Fulah
                "kab" // Kabyle
            ]
        ),
        new PluralForm(
            "ZeroToTwoExcludedOrMillions",
            [PluralCategory.One, PluralCategory.Many, PluralCategory.Other],
            [
                "fr" // French
            ]
        )
        { OptionalCategories = [PluralCategory.Many] },
        new PluralForm(
            "OnlyOneOrMillions",
            [PluralCategory.One, PluralCategory.Many, PluralCategory.Other],
            [
                "ca", // Catalan
                "it", // Italian
                // Portuguese is left here even though CLDR gives 'pt' the rule of French, because the folder
                // of a resource and the language of the app are both reduced to their primary subtag: moving
                // it would put 'pt-PT', whose rule is this one, on the rule of 'pt-BR'. Telling them apart
                // needs the plural rules to be keyed by the whole tag.
                "pt", // Portuguese
                "es"  // Spanish
            ]
        )
        { OptionalCategories = [PluralCategory.Many] },
        new PluralForm(
            "OnlyOne",
            [PluralCategory.One, PluralCategory.Other],
            [
                "af", // Afrikaans
                "sq", // Albanian
                "ast", // Asturian
                "asa", // Asu
                "az", // Azerbaijani
                "eu", // Basque
                "bem", // Bemba
                "bez", // Bena
                "brx", // Bodo
                "bg", // Bulgarian
                "chr", // Cherokee
                "cgg", // Chiga
                "dv", // Divehi
                "nl", // Dutch
                "en", // English
                "eo", // Esperanto
                "et", // Estonian
                "ee", // Ewe
                "fo", // Faroese
                "fi", // Finnish
                "fur", // Friulian
                "gl", // Galician
                "lg", // Ganda
                "ka", // Georgian
                "de", // German
                "el", // Greek
                "ha", // Hausa
                "haw", // Hawaiian
                "hu", // Hungarian
                "kaj", // Jju
                "kkj", // Kako
                "kl", // Kalaallisut
                "ks", // Kashmiri
                "kk", // Kazakh
                "ku", // Kurdish
                "ky", // Kyrgyz
                "lb", // Luxembourgish
                "jmc", // Machame
                "ml", // Malayalam
                "mas", // Masai
                "mgo", // Meta'
                "mn", // Mongolian
                "mr", // Marathi
                "nah", // Nahuatl
                "ne", // Nepali
                "nnh", // Ngiemboon
                "jgo", // Ngomba
                "nd", // North Ndebele
                "no", // Norwegian
                "nb", // Norwegian Bokmål
                "nn", // Norwegian Nynorsk
                "ny", // Nyanja
                "nyn", // Nyankole
                "or", // Oriya
                "om", // Oromo
                "os", // Ossetic    
                "pap", // Papiamento
                "ps", // Pashto
                "rm", // Romansh
                "rof", // Rombo
                "rwk", // Rwa
                "ssy", // Saho
                "saq", // Samburu
                "seh", // Sena
                "ksb", // Shambala
                "sn", // Shona
                "xog", // Soga
                "so", // Somali
                "ckb", // Sorani Kurdish
                "nr", // South Ndebele
                "st", // Southern Sotho
                "sw", // Swahili
                "ss", // Swati
                "sv", // Swedish
                "gsw", // Swiss German
                "syr", // Syriac
                "ta", // Tamil
                "te", // Telugu
                "teo", // Teso
                "tig", // Tigre
                "ts", // Tsonga
                "tn", // Tswana
                "tr", // Turkish
                "tk", // Turkmen
                "kcg", // Tyap
                "ur", // Urdu
                "ug", // Uyghur
                "uz", // Uzbek
                "ve", // Venda
                "vo", // Volapük
                "vun", // Vunjo
                "wae", // Walser
                "fy", // Western Frisian
                "xh", // Xhosa
                "yi", // Yiddish
                "ji"  // Jiddish
            ]
        ),
        new PluralForm(
            "Sinhala",
            [PluralCategory.One, PluralCategory.Other],
            [
                "si" // Sinhala
            ]
        ),
        new PluralForm(
            "Latvian",
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Other],
            [
                "lv", // Latvian
                "prg" // Prussian
            ]
        )
        { ZeroIsOnlyForZeroQuantity = false },
        new PluralForm(
            "Irish",
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "ga" // Irish
            ]
        ),
        new PluralForm(
            "Romanian",
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Other],
            [
                "ro", // Romanian
                "mo"  // Moldavian
            ]
        ),
        new PluralForm(
            "Lithuanian",
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "lt" // Lithuanian
            ]
        ),
        new PluralForm(
            "Slavic",
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "ru", // Russian
                "uk", // Ukrainian
                "be"  // Belarusian
            ]
        ),
        new PluralForm(
            "Czech",
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "cs", // Czech
                "sk"  // Slovak
            ]
        ),
        new PluralForm(
            "Polish",
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "pl" // Polish
            ]
        ),
        new PluralForm(
            "Slovenian",
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Other],
            [
                "sl" // Slovenian
            ]
        ),
        new PluralForm(
            "Arabic",
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "ar" // Arabic
            ]
        ),
        new PluralForm(
            "Hebrew",
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Many, PluralCategory.Other],
            [
                "he", // Hebrew
                "iw"  // (old code for Hebrew)
            ]
        ),
        new PluralForm(
            "Filipino",
            [PluralCategory.One, PluralCategory.Other],
            [
                "fil", // Filipino
                "tl"   // Tagalog
            ]
        ),
        new PluralForm(
            "Macedonian",
            [PluralCategory.One, PluralCategory.Other],
            [
                "mk" // Macedonian
            ]
        ),
        new PluralForm(
            "Breizh",
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "br" // Breton
            ]
        ),
        new PluralForm(
            "CentralAtlasTamazight",
            [PluralCategory.One, PluralCategory.Other],
            [
                "tzm" // Central Atlas Tamazight
            ]
        ),
        new PluralForm(
            "OneOrZero",
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Other],
            [
                "ksh" // Colognian
            ]
        ),
        new PluralForm(
            "OneOrZeroToOneExcluded",
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Other],
            [
                "lag" // Langi
            ]
        ),
        new PluralForm(
            "OneOrTwo",
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Other],
            [
                "kw",   // Cornish
                "smn",  // Inari Sami
                "iu",   // Inuktitut
                "smj",  // Lule Sami
                "naq",  // Nama
                "se",   // Northern Sami
                "smi",  // Other Sami languages
                "sms",  // Skolt Sami
                "sma"   // Southern Sami
            ]
        ),
        new PluralForm(
            "Croat",
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Other],
            [
                "bs", // Bosnian
                "hr", // Croatian
                "sr", // Serbian
                "sh"  // Serbo-Croatian
            ]
        ),
        new PluralForm(
            "Tachelhit",
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Other],
            [
                "shi" // Tachelhit
            ]
        ),
        new PluralForm(
            "Icelandic",
            [PluralCategory.One, PluralCategory.Other],
            [
                "is" // Icelandic
            ]
        ),
        new PluralForm(
            "Manx",
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "gv" // Manx
            ]
        ),
        new PluralForm(
            "ScottishGaelic",
            [PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Other],
            [
                "gd" // Scottish Gaelic
            ]
        ),
        new PluralForm(
            "Maltese",
            [PluralCategory.One, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "mt" // Maltese
            ]
        ),
        new PluralForm(
            "Welsh",
            [PluralCategory.Zero, PluralCategory.One, PluralCategory.Two, PluralCategory.Few, PluralCategory.Many, PluralCategory.Other],
            [
                "cy" // Welsh
            ]
        ),
        new PluralForm(
            "Danish",
            [PluralCategory.One, PluralCategory.Other],
            [
                "da" // Danish
            ]
        ),
        // Languages with a single plural form. They are mapped explicitly, rather than being left to reach the
        // default branch of the generated selector, so that a language reaching that branch always means
        // ReswPlus has no rules for it rather than that it genuinely has one form. This is the complete set
        // CLDR assigns to the 'other' category alone.
        new PluralForm(
            "Other",
            [PluralCategory.Other],
            [
                "bm", // Bambara
                "bo", // Tibetan
                "dz", // Dzongkha
                "hnj", // Hmong Njua
                "id", // Indonesian
                "ig", // Igbo
                "ii", // Sichuan Yi
                "in", // Indonesian, deprecated code
                "ja", // Japanese
                "jbo", // Lojban
                "jv", // Javanese
                "jw", // Javanese, deprecated code
                "kde", // Makonde
                "kea", // Kabuverdianu
                "km", // Khmer
                "ko", // Korean
                "lkt", // Lakota
                "lo", // Lao
                "ms", // Malay
                "my", // Burmese
                "nqo", // N'Ko
                "osa", // Osage
                "sah", // Yakut
                "ses", // Koyraboro Senni
                "sg", // Sango
                "su", // Sundanese
                "th", // Thai
                "to", // Tongan
                "tpi", // Tok Pisin
                "vi", // Vietnamese
                "wo", // Wolof
                "yo", // Yoruba
                "yue", // Cantonese
                "zh" // Chinese
            ]
        )
    ];

    // Prebuild a dictionary that maps each language code to its plural form.
    private static readonly Dictionary<string, PluralForm> LanguageToPluralForm = BuildLanguageToPluralForm();

    /// <summary>
    /// Gets every plural form known to ReswPlus, so that tests can check them as a set.
    /// </summary>
    internal static IEnumerable<PluralForm> PluralFormsForTesting => PluralForms;

    private static Dictionary<string, PluralForm> BuildLanguageToPluralForm()
    {
        var dict = new Dictionary<string, PluralForm>();
        foreach (var pf in PluralForms)
        {
            foreach (var lang in pf.Languages)
            {
                // Since one language can only have one plural form, add only if not already present.
                if (!dict.ContainsKey(lang))
                {
                    dict[lang] = pf;
                }
            }
        }
        return dict;
    }

    /// <summary>
    /// Retrieves the plural forms that apply to the given list of languages.
    /// Since each language can only have one plural form, this simply looks up each language in the prebuilt dictionary.
    /// </summary>
    /// <param name="languages">A collection of language codes to retrieve plural forms for.</param>
    /// <returns>An enumerable collection of <see cref="PluralForm"/> objects that match the specified languages.</returns>
    public static IEnumerable<PluralForm> RetrievePluralFormsForLanguages(IEnumerable<string> languages)
    {
        var result = new Dictionary<string, PluralForm>();
        foreach (var lang in languages)
        {
            if (LanguageToPluralForm.TryGetValue(lang, out var pf))
            {
                result[pf.Id] = pf;
            }
        }
        return result.Values;
    }

    /// <summary>
    /// Retrieves the plural form of a language.
    /// </summary>
    /// <param name="language">The primary language subtag to retrieve the plural form for.</param>
    /// <returns>
    /// The plural form of <paramref name="language"/>, or <see langword="null"/> if the language has no dedicated
    /// plural provider, in which case no plural form can be assumed to be required.
    /// </returns>
    public static PluralForm? RetrievePluralFormForLanguage(string language)
    {
        return LanguageToPluralForm.TryGetValue(language, out var pluralForm) ? pluralForm : null;
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
        var reported = new HashSet<string>();
        foreach (var lang in languages)
        {
            if (!LanguageToPluralForm.ContainsKey(lang) && reported.Add(lang))
            {
                yield return lang;
            }
        }
    }
}
