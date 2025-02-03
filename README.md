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
- **Generation of a markup extension** for accessing strings with **compile-time verification**.

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

## 🔧 Features

### Strongly Typed Class Generator
ReswPlus generates a class that exposes all strings from your `.resw` files as **strongly typed static properties**, ensuring **compile-time safety** in both XAML and C#.

🗨 [How to Generate a Strongly Typed Class](https://github.com/reswplus/ReswPlus/wiki/Features:-Strongly-typed-properties)

### Pluralization Support
Easily add **pluralization** support for *196 languages*, including correct handling of **empty states** when the count is zero.

🗨 [How to Add Pluralization](https://github.com/reswplus/ReswPlus/wiki/Features:-Pluralization-support)  
⚙️ [Handling Empty States](https://github.com/reswplus/ReswPlus/wiki/Features:-Pluralization---Empty-states)  
⚙️ [Supported Languages](https://github.com/reswplus/ReswPlus/wiki/Languages-supported-for-pluralization)

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
