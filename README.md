<img src="https://user-images.githubusercontent.com/1226538/56482508-6fbd2d00-6479-11e9-8fc0-b20d5f3171ad.png" height="80" />

# ReswPlus - Advanced File Code Generator for Resw files.
![Type](https://img.shields.io/badge/type-Visual%20Studio%20Extension-blueviolet)
![Compatibility](https://img.shields.io/badge/compatibility-UWP%2C%20.Net%20Core%2C%20.Net%20Standard%2C%20ASP.Net%20Core-blue)
![LanguageSupported](https://img.shields.io/badge/languages-C%23%2C%20VB.Net%2C%20C%2B%2B%2FCX%2C%20C%2B%2B%2FWinRT-brightgreen)
![GitHub](https://img.shields.io/github/license/dotnetplus/reswplus.svg)

ReswPlus is a Visual Studio extension enriching your existing .resw files with many highly valuable features:
- Access to strings via strongly typed static properties.
- Automatically generate methods to format your strings
    - Support typed and named parameters, literal strings, string references and Macros
- Pluralization support (for 196 languages!).
    - Support empty states when the number of items is zero.
- Variants support 
- Generate a Markup extension to access to your strings with compile-time verification.

Supported: 
- Visual Studio 2017 and 2019 (all versions).
- C#, VB.Net, C++/CX and C++/WinRT.

![reswplus](https://user-images.githubusercontent.com/1226538/56525314-a76eb800-64ff-11e9-9e39-1bb4cd2dd012.gif)



|                                                 | Resw | Resw with ReswPlus | Resx | Android XML (for reference) |
|-------------------------------------------------|------|-----------------|------|-------------|
| Modify UI properties via resource files (x:uid) | ✅    | ✅               |      |             |
| Generate strongly typed accessors               |      | ✅               | ✅    | ✅           |
| Generate String Formatting methods              |      | ✅               |     |            |
| Support Plural forms                            |      | ✅               |      | ✅           |
| Support 'None' state                            |      | ✅               |      |             |
| Auto-generate methods for string formatting                |      | ✅               |      |             |
| Support literal strings in string formatter                |      | ✅               |      |             |
| Support Macros in string formatter                |      | ✅               |      |             |
| Support String references in string formatter                |      | ✅               |      |             |
| Strongly typed string formatting                |      | ✅               |      |             |
| Support Resources in libraries                  |      | ✅               | ✅    |             |
| Support String variants (including genders)                        |      | ✅               |     |             |

## 📦 Guide
⚡ [How to install ReswPlus](https://github.com/reswplus/ReswPlus/wiki/How-to-install-ReswPlus) - Learn how to install ReswPlus for Visual Studio 2017 and 2019<br>
⚡ [Use ReswPlus in your project](https://github.com/reswplus/ReswPlus/wiki/Use-ReswPlus-in-my-project) - Learn how to use ReswPlus in your projects

## 🔧 Features
### Strongly Typed class generator
_ReswPlus can generate a class exposing all strings from your .resw files as strongly typed static properties, providing a compile-time-safe way to access those strings XAML-side or code-side._

🗨 [How to generate a strongly typed class](https://github.com/reswplus/ReswPlus/wiki/Features:-Strongly-typed-properties)
### Pluralization
_ReswPlus can add support of pluralization and plural forms to your localization strings. Plural forms of 196 languages are currently supported by ReswPlus._

🗨 [How to add pluralization](https://github.com/reswplus/ReswPlus/wiki/Features:-Pluralization-support)<br>
⚙️ [Support Empty States](https://github.com/reswplus/ReswPlus/wiki/Features:-Pluralization---Empty-states)<br>
⚙️ [Languages supported](https://github.com/reswplus/ReswPlus/wiki/Languages-supported-for-pluralization)
### String Formatting
_To simplify your ViewModels and Views, ReswPlus can directly manage the formatting of your localization and generate strongly typed methods to format your strings._

🗨 [How to use String Formatting](https://github.com/reswplus/ReswPlus/wiki/Features:-String-Formatting)<br>
⚙️ [Named parameters](https://github.com/reswplus/ReswPlus/wiki/Features:-Named-parameters-for-String-Formatting)<br>
⚙️ [Use String References](https://github.com/reswplus/ReswPlus/wiki/Features:-String-References-in-String-Formatting)<br>
⚙️ [Use Literal Strings](https://github.com/reswplus/ReswPlus/wiki/Features:-Literal-Strings-in-String-Formatting)<br>
⚙️ [Use Macros](https://github.com/reswplus/ReswPlus/wiki/Features:-Macros-in-String-Formatting)

### Variants
_ReswPlus can support many variants/versions of the same string and allow you to display the one you want based on criteria (variants to support genders, different messages depend on some criteria...)_

🗨 [How to use variants](https://github.com/reswplus/ReswPlus/wiki/Features:-Variants)

### .Net String formatting for C++ projects
_String formatting in C++ is quite different and more complicated than in C#/VB.Net. ReswPlus provides a way to use the same string templates as you use in .Net (via `String.Format`) but in your C++ project, making your resource files shareable with .Net libraries and simplifying your code._

🗨 [Use .Net String Formatting](https://github.com/reswplus/ReswPlus/wiki/Features:-.Net-String-Formatting-for-Cpp)
