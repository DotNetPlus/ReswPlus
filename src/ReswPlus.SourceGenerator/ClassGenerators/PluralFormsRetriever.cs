using System.Collections.Generic;

namespace ReswPlus.SourceGenerator.ClassGenerators;

/// <summary>
/// Provides functionality to manage and retrieve pluralization rules for various languages.
/// </summary>
internal sealed class PluralFormsRetriever
{
    internal record PluralForm
    {
        public string[] Languages { get; set; }
        public string Id { get; set; }
    }

    /// <summary>
    /// A static collection of predefined plural forms and their associated languages.
    /// </summary>
    private static readonly PluralForm[] PluralForms =
    [
        new PluralForm {
            Languages =
            [
                "ak", // Akan
                "bh", // Bihari
                "guw", // Gun
                "ln", // Lingala
                "mg", // Malagasy
                "nso", // Northern Sotho
                "pa", // Punjabi
                "ti", // Tigrinya
                "wa" // Walloon
            ],
            Id = "IntOneOrZero"
        },
        new PluralForm {
            Languages =
            [
                "am", // Amharic
                "bn", // Bengali
                "ff", // Fulah
                "gu", // Gujarati
                "hi", // Hindi
                "kn", // Kannada
                "mr", // Marathi
                "fa", // Persian
                "zu"  // Zulu
            ],
            Id = "ZeroToOne"
        },
        new PluralForm {
            Languages =
            [
                "hy", // Armenian
                "fr", // French
                "kab" // Kabyle
            ],
            Id = "ZeroToTwoExcluded"
        },
        new PluralForm {
            Languages =
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
            ],
            Id = "OnlyOne"
        },
        new PluralForm {
            Languages =
            [
                "si" // Sinhala
            ],
            Id = "Sinhala"
        },
        new PluralForm {
            Languages =
            [
                "lv", // Latvian
                "prg" // Prussian
            ],
            Id = "Latvian"
        },
        new PluralForm {
            Languages =
            [
                "ga" // Irish
            ],
            Id = "Irish"
        },
        new PluralForm {
            Languages =
            [
                "ro", // Romanian
                "mo"  // Moldavian
            ],
            Id = "Romanian"
        },
        new PluralForm {
            Languages =
            [
                "lt" // Lithuanian
            ],
            Id = "Lithuanian"
        },
        new PluralForm {
            Languages =
            [
                "ru", // Russian
                "uk", // Ukrainian
                "be"  // Belarusian
            ],
            Id = "Slavic"
        },
        new PluralForm {
            Languages =
            [
                "cs", // Czech
                "sk"  // Slovak
            ],
            Id = "Czech"
        },
        new PluralForm {
            Languages =
            [
                "pl" // Polish
            ],
            Id = "Polish"
        },
        new PluralForm {
            Languages =
            [
                "sl" // Slovenian
            ],
            Id = "Slovenian"
        },
        new PluralForm {
            Languages =
            [
                "ar" // Arabic
            ],
            Id = "Arabic"
        },
        new PluralForm {
            Languages =
            [
                "he", // Hebrew
                "iw"  // (old code for Hebrew)
            ],
            Id = "Hebrew"
        },
        new PluralForm {
            Languages =
            [
                "fil", // Filipino
                "tl"   // Tagalog
            ],
            Id = "Filipino"
        },
        new PluralForm {
            Languages =
            [
                "mk" // Macedonian
            ],
            Id = "Macedonian"
        },
        new PluralForm {
            Languages =
            [
                "br" // Breton
            ],
            Id = "Breizh"
        },
        new PluralForm {
            Languages =
            [
                "tzm" // Central Atlas Tamazight
            ],
            Id = "CentralAtlasTamazight"
        },
        new PluralForm {
            Languages =
            [
                "ksh" // Colognian
            ],
            Id = "OneOrZero"
        },
        new PluralForm {
            Languages =
            [
                "lag" // Langi
            ],
            Id = "OneOrZeroToOneExcluded"
        },
        new PluralForm {
            Languages =
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
            ],
            Id = "OneOrTwo"
        },
        new PluralForm {
            Languages =
            [
                "bs", // Bosnian
                "hr", // Croatian
                "sr", // Serbian
                "sh"  // Serbo-Croatian
            ],
            Id = "Croat"
        },
        new PluralForm {
            Languages =
            [
                "shi" // Tachelhit
            ],
            Id = "Tachelhit"
        },
        new PluralForm {
            Languages =
            [
                "is" // Icelandic
            ],
            Id = "Icelandic"
        },
        new PluralForm {
            Languages =
            [
                "gv" // Manx
            ],
            Id = "Manx"
        },
        new PluralForm {
            Languages =
            [
                "gd" // Scottish Gaelic
            ],
            Id = "ScottishGaelic"
        },
        new PluralForm {
            Languages =
            [
                "mt" // Maltese
            ],
            Id = "Maltese"
        },
        new PluralForm {
            Languages =
            [
                "cy" // Welsh
            ],
            Id = "Welsh"
        },
        new PluralForm {
            Languages =
            [
                "da" // Danish
            ],
            Id = "Danish"
        }
    ];

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
