using System.Collections.Generic;

namespace ReswPlus.SourceGenerator.ClassGenerators;

/// <summary>
/// Provides functionality to manage and retrieve pluralization rules for various languages.
/// </summary>
internal sealed class PluralFormsRetriever
{
    internal record PluralForm
    {
        public PluralForm(string id, string[] languages)
        {
            Id = id;
            Languages = languages;
        }

        public string Id { get; set; }
        public string[] Languages { get; set; }
    }

    /// <summary>
    /// A static collection of predefined plural forms and their associated languages.
    /// </summary>
    private static readonly PluralForm[] PluralForms = new PluralForm[]
    {
        new PluralForm(
            "IntOneOrZero",
            new[]
            {
                "ak", // Akan
                "bh", // Bihari
                "guw", // Gun
                "ln", // Lingala
                "mg", // Malagasy
                "nso", // Northern Sotho
                "pa", // Punjabi
                "ti", // Tigrinya
                "wa"  // Walloon
            }
        ),
        new PluralForm(
            "ZeroToOne",
            new[]
            {
                "am", // Amharic
                "bn", // Bengali
                "ff", // Fulah
                "gu", // Gujarati
                "hi", // Hindi
                "kn", // Kannada
                "mr", // Marathi
                "fa", // Persian
                "zu"  // Zulu
            }
        ),
        new PluralForm(
            "ZeroToTwoExcluded",
            new[]
            {
                "hy", // Armenian
                "fr", // French
                "kab" // Kabyle
            }
        ),
        new PluralForm(
            "OnlyOne",
            new[]
            {
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
                "ca", // Catalan
                "chr", // Cherokee
                "cgg", // Chiga
                "dv", // Divehi
                "nl", // Dutch
                "en", // English
                "eo", // Esperanto
                "et", // Estonian
                "pt", // European Portuguese
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
                "it", // Italian
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
                "sag", // Samburu
                "seh", // Sena
                "ksb", // Shambala
                "sn", // Shona
                "xog", // Soga
                "so", // Somali
                "ckb", // Sorani Kurdish
                "nr", // South Ndebele
                "st", // Southern Sotho
                "es", // Spanish
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
            }
        ),
        new PluralForm(
            "Sinhala",
            new[]
            {
                "si" // Sinhala
            }
        ),
        new PluralForm(
            "Latvian",
            new[]
            {
                "lv", // Latvian
                "prg" // Prussian
            }
        ),
        new PluralForm(
            "Irish",
            new[]
            {
                "ga" // Irish
            }
        ),
        new PluralForm(
            "Romanian",
            new[]
            {
                "ro", // Romanian
                "mo"  // Moldavian
            }
        ),
        new PluralForm(
            "Lithuanian",
            new[]
            {
                "lt" // Lithuanian
            }
        ),
        new PluralForm(
            "Slavic",
            new[]
            {
                "ru", // Russian
                "uk", // Ukrainian
                "be"  // Belarusian
            }
        ),
        new PluralForm(
            "Czech",
            new[]
            {
                "cs", // Czech
                "sk"  // Slovak
            }
        ),
        new PluralForm(
            "Polish",
            new[]
            {
                "pl" // Polish
            }
        ),
        new PluralForm(
            "Slovenian",
            new[]
            {
                "sl" // Slovenian
            }
        ),
        new PluralForm(
            "Arabic",
            new[]
            {
                "ar" // Arabic
            }
        ),
        new PluralForm(
            "Hebrew",
            new[]
            {
                "he", // Hebrew
                "iw"  // (old code for Hebrew)
            }
        ),
        new PluralForm(
            "Filipino",
            new[]
            {
                "fil", // Filipino
                "tl"   // Tagalog
            }
        ),
        new PluralForm(
            "Macedonian",
            new[]
            {
                "mk" // Macedonian
            }
        ),
        new PluralForm(
            "Breizh",
            new[]
            {
                "br" // Breton
            }
        ),
        new PluralForm(
            "CentralAtlasTamazight",
            new[]
            {
                "tzm" // Central Atlas Tamazight
            }
        ),
        new PluralForm(
            "OneOrZero",
            new[]
            {
                "ksh" // Colognian
            }
        ),
        new PluralForm(
            "OneOrZeroToOneExcluded",
            new[]
            {
                "lag" // Langi
            }
        ),
        new PluralForm(
            "OneOrTwo",
            new[]
            {
                "kw",   // Cornish
                "smn",  // Inari Sami
                "iu",   // Inuktitut
                "smj",  // Lule Sami
                "naq",  // Nama
                "se",   // Northern Sami
                "smi",  // Other Sami languages
                "sms",  // Skolt Sami
                "sma"   // Southern Sami
            }
        ),
        new PluralForm(
            "Croat",
            new[]
            {
                "bs", // Bosnian
                "hr", // Croatian
                "sr", // Serbian
                "sh"  // Serbo-Croatian
            }
        ),
        new PluralForm(
            "Tachelhit",
            new[]
            {
                "shi" // Tachelhit
            }
        ),
        new PluralForm(
            "Icelandic",
            new[]
            {
                "is" // Icelandic
            }
        ),
        new PluralForm(
            "Manx",
            new[]
            {
                "gv" // Manx
            }
        ),
        new PluralForm(
            "ScottishGaelic",
            new[]
            {
                "gd" // Scottish Gaelic
            }
        ),
        new PluralForm(
            "Maltese",
            new[]
            {
                "mt" // Maltese
            }
        ),
        new PluralForm(
            "Welsh",
            new[]
            {
                "cy" // Welsh
            }
        ),
        new PluralForm(
            "Danish",
            new[]
            {
                "da" // Danish
            }
        )
    };

    // Prebuild a dictionary that maps each language code to its plural form.
    private static readonly Dictionary<string, PluralForm> LanguageToPluralForm = BuildLanguageToPluralForm();

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
}
