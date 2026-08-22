# Pseudo-localization

Pseudo-localization generates artificial translations from the project's default-language `.resw` files.
It makes localization problems visible before real translations are available:

- accented characters reveal hard-coded UI strings
- expanded values reveal clipped text and fixed-width layouts
- mirrored text reveals right-to-left layout assumptions
- visible boundary markers reveal unintended trimming or concatenation

The generated files are build artifacts under `obj`; ReswPlus never changes or adds files to the source
language folders.

## Enable an accented pseudo-language

Enable pseudo-localization with an MSBuild property:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <ReswPlusPseudoLocalization>Accented</ReswPlusPseudoLocalization>
</PropertyGroup>
```

ReswPlus reads the `.resw` files under the project's `DefaultLanguage` folder and adds generated
`qps-ploc` resources to the PRI before Windows resource indexing. It also adds the pseudo-language to the
generated AppX manifest so Windows accepts it as an application language. For example:

```text
Welcome, {0}!
```

becomes similar to:

```text
⟦Ŵëŀçømë, {0}! ~~~~~~~⟧
```

Composite-format placeholders, escaped braces, and XML-like markup inside values are preserved.
Resource names, comments, and `#Format` declarations are not transformed.

## Test right-to-left layout

Use `Mirrored` to generate the Windows `qps-plocm` pseudo-locale:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <ReswPlusPseudoLocalization>Mirrored</ReswPlusPseudoLocalization>
</PropertyGroup>
```

Generate both pseudo-locales by separating the modes with a semicolon:

```xml
<ReswPlusPseudoLocalization>Accented;Mirrored</ReswPlusPseudoLocalization>
```

| Mode | Windows language | Behavior |
| --- | --- | --- |
| `Accented` | `qps-ploc` | Accents characters, expands text, and adds boundary markers |
| `Mirrored` | `qps-plocm` | Applies the same transformation inside a right-to-left override |

## Select the pseudo-language

The generated resources participate in normal Windows resource resolution. Select one before creating UI
that reads resources, then restart the application:

```csharp
Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "qps-ploc";
```

Use `qps-plocm` for mirrored testing. Clear the override to return to the user's normal language:

```csharp
Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "";
```

An application with an in-app language picker can expose these identifiers only in development builds.
They do not need to appear in production UI.

## Configure text expansion

Accented and mirrored values expand by 30 percent by default. Adjust the percentage from 0 through 200:

```xml
<ReswPlusPseudoLocalizationExpansion>50</ReswPlusPseudoLocalizationExpansion>
```

## Use in CI

Pseudo-localization is disabled unless `ReswPlusPseudoLocalization` is set. A CI job can enable it without
changing the project:

```console
dotnet build -p:ReswPlusPseudoLocalization=Accented
```

This verifies that the generated pseudo-language remains packageable and that all source resources can be
parsed. UI automation can then launch the built application with `qps-ploc` selected and check for clipping,
overlap, untranslated strings, and right-to-left regressions.
