# CLDR rule importer

The plural providers ReswPlus emits are written from the rules Unicode CLDR publishes. Nothing is transcribed
by hand, and nothing about CLDR is read at compile time: this tool turns the published rules into
`src/ReswPlus.SourceGenerator/Plurals/CldrPluralRules.g.cs`, which is checked in, and the generator does no
more than look a language up in it.

That split is deliberate. An analyzer runs inside Visual Studio, so parsing JSON and a rule grammar on every
compile buys nothing that doing it once, offline, does not — and a CLDR release then shows up in review as the
rules that actually moved rather than as a file the build reads differently.

## Running it

```powershell
dotnet run --project tools\CldrRuleImporter              # regenerate from the vendored plurals.json
dotnet run --project tools\CldrRuleImporter -- --download # refresh plurals.json from CLDR first
dotnet test tests\ReswPlusUnitTests
```

Commit `plurals.json` and `CldrPluralRules.g.cs` together.

## The files

| File | What it is |
|---|---|
| `plurals.json` | The rules, vendored byte for byte from [cldr-json](https://raw.githubusercontent.com/unicode-org/cldr-json/main/cldr-json/cldr-core/supplemental/plurals.json). **Never edit this by hand.** |
| `CldrJson.cs` | Reads the shape of JSON that file is written in. |
| `CldrRule.cs` | Reads the plural rule syntax of [UTS #35](https://unicode.org/reports/tr35/tr35-numbers.html#Language_Plural_Rules), and both evaluates a rule and writes it back out as C#. |
| `CldrPublishedRules.cs` | Hands the parsed rules to the rest of the tool. |
| `CldrLanguages.cs` | The legacy ISO codes CLDR publishes under a newer name, and the codes Windows may still hand us. |
| `CldrPluralForm.cs` | Works out which languages share a set of rules, what to call the class, and which categories a translator has to supply. |
| `CldrEmitter.cs` | Assembles a provider class from a set of rules. |

## What is derived, and how

Everything below used to be a hand-maintained table. It is now read out of the rules themselves, so a CLDR
release cannot leave it stale.

- **Which languages share a class.** Two languages share one when their rules *decide alike*, not when they
  are spelled alike: CLDR writes `i = 0,1` for French and `i = 0..1` for Portuguese, which is the same rule.
  Each language's rules are evaluated over a sweep of quantities that crosses every boundary they mention,
  and languages producing the same decisions are one class. Grouping on the text of the rules instead yields
  40 classes; grouping on behaviour yields 35, which is the true minimum.
- **The name of the class.** Taken from whichever of its languages sorts first, which means nothing on its
  own — so the class documents the languages it decides for.
- **`OptionalCategories`** — the categories a translator may leave out, because the provider only returns
  them for a quantity an app is unlikely to show. Derived by asking which categories no everyday quantity
  selects.
- **`ZeroIsOnlyForZeroQuantity`** — whether a `zero` form really means the number zero, rather than a
  category that happens to be called `zero`. Derived by asking whether `zero` is ever selected for a non-zero
  quantity.

The derivation reproduces every judgement the old hand table made, and corrected two it had wrong (Breton's
`many` was not marked optional, and Prussian was not grouped with the languages sharing its zero rule).

## Refreshing CLDR

The tests replay the sample quantities CLDR publishes beside each rule, so a failure is a change map rather
than a puzzle:

| Failing test | What CLDR changed | What to do |
|---|---|---|
| `CldrConformance.TheCategoriesOfALanguageAreTheOnesCldrDeclares` | A language gained or lost a plural category | Nothing here: rerun the importer. **This is a breaking change for that language** — see below. |
| `CldrDrift.EveryLanguageMappedIsOneCldrPublishesRulesFor` | A language code was renamed or withdrawn | Add it to `CldrLanguages`, or drop the mapping |
| `CldrDrift.EveryLanguageOfAFormSelectsWhatCldrSelects` | The languages of a class no longer decline alike | Nothing here: rerun the importer, which splits the class |
| `CldrConformance.TheProviderOfALanguageSelectsTheCategoryCldrSelects` | A rule's bounds moved | Nothing here: rerun the importer |

### When a language gains or loses a category

That is visible to whoever wrote the resources. A language that gains `few` means every pluralized resource in
it now wants a `_Few` string, and one that loses a category leaves a string nobody will read. Bump at least the
minor version, and say which languages moved in the release notes.

## Matching a Windows tag to a CLDR language

CLDR names a language one way. Windows names it several. Bridging that is the one thing
`PluralFormsRetriever` decides:

- Normalising a tag before looking it up — Windows writes `pt-PT` in a resource folder and `pt_PT` in some
  culture names, and neither casing is guaranteed.
- Matching the whole tag first and shortening it one subtag at a time. CLDR keys *both* `pt` and `pt-PT`, so
  the primary subtag alone is not a safe key; but `fr-CA` is not keyed at all and has to fall through to
  `fr`.

Which rules `pt-PT` gets is not decided here — CLDR publishes it as its own language and the importer carries
it through like any other.
