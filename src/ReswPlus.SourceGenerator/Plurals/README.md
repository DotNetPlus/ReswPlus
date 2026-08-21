# Plural rules

The plural providers ReswPlus emits are written from the rules Unicode CLDR publishes, at the moment a project
is compiled. Nothing here is transcribed by hand.

## The files

| File | What it is |
|---|---|
| `plurals.json` | The rules, vendored byte for byte from [cldr-json](https://raw.githubusercontent.com/unicode-org/cldr-json/main/cldr-json/cldr-core/supplemental/plurals.json). **Never edit this by hand.** |
| `CldrJson.cs` | Reads the shape of JSON that file is written in. Not a general JSON reader: an analyzer's dependencies are loaded into the host, so a file whose shape is fixed is read here instead of taking one. |
| `CldrRule.cs` | Reads the plural rule syntax of [UTS #35](https://unicode.org/reports/tr35/tr35-numbers.html#Language_Plural_Rules), and both evaluates a rule and writes it back out as C#. |
| `CldrEmitter.cs` | Assembles a provider class from a set of rules. |
| `CldrPublishedRules.cs` | Hands the rules of a language to the rest of the generator. |
| `CldrLanguages.cs` | The legacy ISO codes CLDR publishes under a newer name, and which rules a plural form stands for. |

## Refreshing CLDR

```powershell
pwsh eng\Update-CldrPlurals.ps1
dotnet test tests\ReswPlusUnitTests
```

The tests replay the sample quantities CLDR publishes beside each rule, so a failure is a change map rather
than a puzzle:

| Failing test | What CLDR changed | What to do |
|---|---|---|
| `CldrConformance.TheCategoriesOfALanguageAreTheOnesCldrDeclares` | A language gained or lost a plural category | Move it to a form whose categories match, or add a form in `PluralFormsRetriever`. **This is a breaking change for that language** — see below. |
| `CldrDrift.EveryLanguageMappedIsOneCldrPublishesRulesFor` | A language code was renamed or withdrawn | Add it to `DeprecatedCodes`, or drop the mapping |
| `CldrDrift.EveryLanguageOfAFormSelectsWhatCldrSelects` | The languages of a form no longer decline alike | Split the form |
| `CldrConformance.TheProviderOfALanguageSelectsTheCategoryCldrSelects` | A rule's bounds moved | Nothing: the providers are written from the rules, so this passing again is the fix |

### When a language gains or loses a category

That is visible to whoever wrote the resources. A language that gains `few` means every pluralized resource in
it now wants a `_Few` string, and one that loses a category leaves a string nobody will read. Bump at least the
minor version, and say which languages moved in the release notes.

## What stays hand written

CLDR does not express these, so they are decided here and are the exception to everything above:

- The language to plural form table in `PluralFormsRetriever`, including tags CLDR treats separately such as
  `pt-PT`.
- `OptionalCategories` — the categories a resource may leave out, because the provider only returns them for a
  quantity an app is unlikely to show. No test can check this against CLDR.
- `ZeroIsOnlyForZeroQuantity` — whether a `zero` form really means the number zero.
- `DeprecatedCodes` — the legacy codes Windows may still hand us.
