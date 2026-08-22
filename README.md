<img src="https://user-images.githubusercontent.com/1226538/56482508-6fbd2d00-6479-11e9-8fc0-b20d5f3171ad.png" height="80" />

# ReswPlus - Advanced Code Generator for `.resw` Files
![Type](https://img.shields.io/badge/type-Visual%20Studio%20Extension-blueviolet)
![Compatibility](https://img.shields.io/badge/compatibility-UWP%2C%20.Net%20Core%2C%20.Net%20Standard%2C%20ASP.Net%20Core-blue)
![Language Supported](https://img.shields.io/badge/languages-C%23-brightgreen)
![GitHub](https://img.shields.io/github/license/dotnetplus/reswplus.svg)

_**Now available as a Source Generator!**_

**ReswPlus** is a C# Source Generator for Visual Studio that enhances `.resw` files with a powerful set of features:

- **Strongly typed static properties** for safer and more efficient string access.
- **Automatic generation of string formatting methods**, supporting:
  - Typed and named parameters, literal strings, string references, and macros.
- **Pluralization support** for *196 languages*, including handling empty states when the item count is zero.
- **Variant support** for managing multiple versions of a string.
- **Generation of a markup extension** for accessing strings with **compile-time verification**. *(Deprecated — see [Native AOT](#-native-aot).)*

## ✅ Feature Comparison

| Feature                                       | Resw | Resw + ReswPlus | Resx | Android XML (for reference) |
|-----------------------------------------------|------|-----------------|------|-------------|
| Modify UI properties via resource files (x:uid) | ✅  | ✅             |      |             |
| Generate strongly typed accessors             |      | ✅             | ✅  | ✅           |
| Generate string formatting methods            |      | ✅             |      |             |
| Support pluralization                         |      | ✅             |      | ✅           |
| Support empty states                          |      | ✅             |      |             |
| Auto-generate string formatting methods       |      | ✅             |      |             |
| Support literal strings in formatters         |      | ✅             |      |             |
| Support macros in formatters                  |      | ✅             |      |             |
| Support string references in formatters       |      | ✅             |      |             |
| Strongly typed string formatting              |      | ✅             |      |             |
| Support resources in libraries                |      | ✅             | ✅  |             |
| Support string variants (e.g., gender-based)  |      | ✅             |      |             |

## 📦 Getting Started

⚡ [How to Install ReswPlus](https://github.com/reswplus/ReswPlus/wiki/How-to-install-ReswPlus) – Step-by-step installation guide.

ReswPlus recognizes a UWP project by the `Windows.Foundation.UniversalApiContract` reference it carries, and a Windows App SDK project by its own. A **UWP project built for Native AOT** has no such reference, so it declares what it is with the standard `UseUwp` property instead, which ReswPlus reads and trusts over the references:

```xml
<PropertyGroup>
    <UseUwp>true</UseUwp>
</PropertyGroup>
```

Without it, such a project is reported as `RESWP0005` and no code is generated for it. The `samples/UWP` folder has one sample of each kind: `ReswPlusUWPSample` built with .NET Native, and `ReswPlusNativeAotUwpSample` built on modern .NET with Native AOT.

### Injectable resource interfaces

For `Resources.resw`, ReswPlus generates the static `Resources` class as before, plus an `IResources` interface and a sealed `ResourcesProvider` adapter. The provider delegates to the static API, so existing calls such as `Resources.WelcomeTitle` remain unchanged while view models and services can receive an injectable resource dependency:

```csharp
IResources resources = new ResourcesProvider();
var title = resources.WelcomeTitle;
```

The interface includes `GetString`, regular resource properties, and all generated formatting, plural, and variant overloads.

Generation is enabled by default. Projects that do not use dependency injection can disable the additional types:

```xml
<PropertyGroup>
    <ReswPlusGenerateResourceInterfaces>false</ReswPlusGenerateResourceInterfaces>
</PropertyGroup>
```

### Generator performance diagnostics

To include compiler-measured source-generator timings in detailed build output, enable:

```xml
<PropertyGroup>
    <ReswPlusReportGeneratorPerformance>true</ReswPlusReportGeneratorPerformance>
</PropertyGroup>
```

Then build with detailed verbosity:

```console
dotnet build --verbosity detailed
```

The timing comes from the compiler rather than instrumentation inside generated code. The compiler-wide report also includes every other analyzer and source generator in the project. Roslyn does not expose incremental cache hits to a running generator, so ReswPlus tracks its named pipeline stages in its incrementality test suite instead; those tests verify that unchanged files are cached and that editing one resource regenerates only that resource.

## ⚡ Native AOT

Everything ReswPlus generates works when the app is compiled with Native AOT, **except the markup extension**, which is deprecated for that reason:

```xml
<!-- Works with Native AOT: resolved while the app is compiled -->
<TextBlock Text="{x:Bind strings:Resources.WelcomeTitle}" />

<!-- Deprecated, and does NOT work with Native AOT: created by the XAML parser while the page is read -->
<TextBlock Text="{strings:Resources Key=WelcomeTitle}" />
```

A page that uses the markup extension fails to load, with `Markup extension could not provide value`. It is still generated, so an app that does not use Native AOT keeps building, but it is marked `[Obsolete]`: a page that uses it reports `WMC1500` on the line of the markup itself, so a build with `TreatWarningsAsErrors` will fail until it is rewritten or the warning suppressed.

`x:Bind` replaces it in almost every case. It reads the same generated members, takes a converter and a converter parameter the same way, and is checked while the app is compiled rather than when the page is shown:

| Instead of | Write |
| --- | --- |
| `{strings:Resources Key=Foo}` | `{x:Bind strings:Resources.Foo}` |
| `{strings:Resources Key=Foo, Converter={StaticResource C}}` | `{x:Bind strings:Resources.Foo, Converter={StaticResource C}}` |

`x:Bind` is not available everywhere the markup extension was: a `Setter` in a `Style`, and a standalone `ResourceDictionary` with no code-behind — a shared `Styles.xaml`, say — both lack the compiled-binding host `x:Bind` needs. Set those from code-behind using the same generated members.

This is not something a trimming directive can keep: preserving the generated types with `rd.xml` or a trimmer root does not change it.

## 🔧 Features

### Strongly Typed Class Generator
ReswPlus generates a class that exposes all strings from your `.resw` files as **strongly typed static properties**, ensuring **compile-time safety** in both XAML and C#.

🗨 [How to Generate a Strongly Typed Class](https://github.com/reswplus/ReswPlus/wiki/Features:-Strongly-typed-properties)

### Pluralization Support
Easily add **pluralization** support for *196 languages*, including correct handling of **empty states** when the count is zero.

🗨 [How to Add Pluralization](https://github.com/reswplus/ReswPlus/wiki/Features:-Pluralization-support)  
⚙️ [Handling Empty States](https://github.com/reswplus/ReswPlus/wiki/Features:-Pluralization---Empty-states)  
⚙️ [Supported Languages](https://github.com/reswplus/ReswPlus/wiki/Languages-supported-for-pluralization)

The plural rules follow the cardinal rules of **Unicode CLDR 48**, and are checked against the rules CLDR publishes by the test suite, so that a language whose rules CLDR revises is reported rather than silently declined with the rules of an older release. Ordinal rules (1st, 2nd, 3rd) and plural ranges (1–2) are not supported.

By default the plural rules are picked with the .NET UI culture, which comes from the display languages of the user. Windows resolves the resources themselves with the app runtime language list instead, so the two can disagree and a resource can be shown in one language while its plural form is picked with the rules of another. Set the following property to pick the plural rules with the same language the resources are resolved in:

```xml
<PropertyGroup>
    <ReswPlusUseApplicationLanguages>true</ReswPlusUseApplicationLanguages>
</PropertyGroup>
```

This is opt-in so that existing apps keep the behavior they were built against. Apps that let users pick a language in-app, through `ApplicationLanguages.PrimaryLanguageOverride`, should turn it on. A WinAppSDK project reads the override directly, so it also works when the app runs unpackaged, and needs Windows App SDK 1.6 or later.

### String Formatting
ReswPlus simplifies ViewModels and Views by handling string formatting directly and generating **strongly typed methods**.

🗨 [How to Use String Formatting](https://github.com/reswplus/ReswPlus/wiki/Features:-String-Formatting)  
⚙️ [Named Parameters](https://github.com/reswplus/ReswPlus/wiki/Features:-Named-parameters-for-String-Formatting)  
⚙️ [Using String References](https://github.com/reswplus/ReswPlus/wiki/Features:-String-References-in-String-Formatting)  
⚙️ [Using Literal Strings](https://github.com/reswplus/ReswPlus/wiki/Features:-Literal-Strings-in-String-Formatting)  
⚙️ [Using Macros](https://github.com/reswplus/ReswPlus/wiki/Features:-Macros-in-String-Formatting)

### String Variants
ReswPlus allows multiple variants of a string based on different criteria, such as **gender-based messages** or other conditions.

🗨 [How to Use Variants](https://github.com/reswplus/ReswPlus/wiki/Features:-Variants)

### Resource Diagnostics
ReswPlus checks the content of your `.resw` files while it generates the code, and reports the inconsistencies that would otherwise only show up at runtime, in a language your team may not read.

| Rule | Description |
| --- | --- |
| `RESWP0006` | A translated value drops placeholders its value in the default language uses, silently losing information. |
| `RESWP0007` | A value uses a placeholder that has no matching parameter in its `#Format` tag. |
| `RESWP0008` | A pluralized resource is missing the plural forms its language requires, which silently produces grammatically wrong text. |
| `RESWP0009` | Two resources of the same file conflict with each other, because their names only differ by case or because a plain resource collides with a pluralized one. |
| `RESWP0010` | A value that is used as a composite format string is malformed. |
| `RESWP0012` | A resource carries the name of a member the generated class declares itself, so it is skipped. |
| `RESWP0013` | A `#Format` tag declares the same parameter name twice, so the generated method renames all but the first. |
| `RESWP0014` | A `.resw` file could not be turned into code, and the rest of the project was generated without it. |

These are reported as **warnings**, so that updating the package never breaks a build that already has an inconsistency. Escalate the ones you want to be fatal from your `.editorconfig`:

```ini
dotnet_diagnostic.RESWP0006.severity = error
```

## Tools
In addition to features to enrich resw files, ReswPlus also provides some interesting tools to improve your productivity or make it easier to use/support resw files in your workflow and localization process.

### Convert from/to Android XML files
Unfortunately, not all localization tools and companies support `.resw` files. This becomes even more problematic when dealing with pluralization, as `.resw` does not support it by default.  

To address this, **ReswPlus** includes a converter for **seamless conversion between `.resw` and Android XML**, a format that supports string pluralization and is widely compatible with existing localization tools.

Simply right click on the resw associated to the default language of your app and select `ReswPlus > Export to Android XML format`. To convert the Android files once localized, you can use the command-line tool provided with the nuget package (packages/ReswPlusLib.xxxx/Tools/ReswPlusCmd\ReswPlusCmd.exe with the following arguments `xml-to-resw -i <folder path> <output path>`.

If you don't want to use Visual Studio to convert your resw files to Android XML files, you can use the same command-line tool with the following arguments `resw-to-xml -i <resw file path> <output file path>`

## Other programming languages

The current Source Generator supports only C#. If your project uses VB.NET, C++/CX, or C++/WinRT, you can use our legacy Visual Studio extension, available [here](https://github.com/DotNetPlus/ReswPlus/tree/legacy/visual-studio-extension)

![reswplus](https://user-images.githubusercontent.com/1226538/56525314-a76eb800-64ff-11e9-9e39-1bb4cd2dd012.gif)
