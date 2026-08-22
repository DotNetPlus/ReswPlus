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

### System-level availability

Nothing needs to be installed or enabled at the system level on Windows 10 version 1803 and newer,
including Windows 11. Windows includes the `qps-*` pseudo-locales in its National Language Support
APIs, but intentionally hides them from language enumeration. They are not display-language packs,
so they cannot be selected as the Windows display language through Settings.

On these Windows versions, registry edits do not make pseudo-locales appear in the system language
list. Select `qps-ploc` or `qps-plocm` inside the application as shown above instead. This also keeps
pseudo-localization isolated to the application under test.

Windows 10 version 1709 and older allowed pseudo-locales to be exposed for enumeration by adding
their LCIDs under `HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Nls\Locale`. ReswPlus supports
modern UWP and Windows App SDK targets, so this legacy system configuration is not required. See
[Using pseudo-locales for localizability testing](https://learn.microsoft.com/windows/win32/intl/using-pseudo-locales-for-localization-testing)
for the legacy registry values and NLS details.

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
