# Translation completeness and drift checks

ReswPlus compares each translated `.resw` file with the resource file selected for the project's default language. The checks distinguish normal translation lag from defects that can throw, display empty text, resolve ambiguously, or silently lose information.

## Configure the checks

`ReswPlusTranslationChecks` accepts `Off`, `Default`, or `Strict`. Values are case-insensitive, and an omitted or unrecognized value uses `Default`.

```xml
<PropertyGroup>
    <ReswPlusTranslationChecks>Default</ReswPlusTranslationChecks>
</PropertyGroup>
```

| Value | Behavior |
| --- | --- |
| `Off` | Disables checks that compare translations with the default language. Per-file correctness checks still run. |
| `Default` | Reports harmless incompleteness as Info and output-breaking defects as Warning. This is the default. |
| `Strict` | Promotes incompleteness to Warning and output-breaking defects to Error. |

`Default` is intended for active development. A resource added only to the default language produces an informational diagnostic while translators catch up, so projects using `TreatWarningsAsErrors` keep building.

Use `Strict` when localized resources are expected to be release-ready:

```xml
<PropertyGroup Condition="'$(ContinuousIntegrationBuild)' == 'true'">
    <ReswPlusTranslationChecks>Strict</ReswPlusTranslationChecks>
</PropertyGroup>
```

Use `Off` when another system owns translation validation:

```xml
<PropertyGroup>
    <ReswPlusTranslationChecks>Off</ReswPlusTranslationChecks>
</PropertyGroup>
```

## Severity policy

| Check | Default | Strict |
| --- | --- | --- |
| Resource exists only in the default language | Info | Warning |
| Resource exists only in a translated language | Info | Warning |
| Translation is identical to the default value | Info | Info |
| Translation defines extra variants | Info | Warning |
| Translation drops a placeholder | Warning | Error |
| Translation references an undeclared placeholder | Warning | Error |
| Composite format string is malformed | Warning | Error |
| Required plural form is missing | Warning | Error |
| Required variant is missing or the resource shape is incompatible | Warning | Error |
| Resource names conflict under case-insensitive lookup | Warning | Error |

Plural categories are not compared directly across languages. A Polish translation, for example, legitimately has forms that English does not. ReswPlus validates each language against its own CLDR plural requirements and compares only the generated plain, plural, and variant structure.

## What `Off` keeps checking

`Off` suppresses diagnostics that require a comparison with the default language, including missing or extra resources, unchanged translations, dropped placeholders, and translated plural or variant drift.

It does not suppress correctness checks within one file:

- undeclared placeholder indexes;
- malformed composite format strings;
- conflicting resource names;
- reserved generated member names;
- duplicate `#Format` parameter names;
- required plural forms in the default-language resource.

## Configure individual diagnostics

Normal analyzer configuration still applies. For example, a project can suppress unchanged translations while keeping all other checks:

```ini
dotnet_diagnostic.RESWP0018.severity = none
```

Or it can require missing translations without enabling every strict escalation:

```ini
dotnet_diagnostic.RESWP0016.severity = warning
```

`Off` prevents cross-language diagnostics from being produced, so `.editorconfig` cannot re-enable those rules until `ReswPlusTranslationChecks` is set to `Default` or `Strict`.
