# ReswPlus

ReswPlus is a C# source generator that turns `.resw` resources into a strongly typed API for UWP and WinUI applications.

## Install

```shell
dotnet add package ReswPlus
```

The package adds the generator and its transitive MSBuild integration. Add or update the default-language `.resw` file in your project and ReswPlus generates resource properties and methods during compilation.

## Features

- Strongly typed resource properties for XAML and C#
- Typed and named string-format parameters
- Literal arguments, resource references, and app/system macros
- CLDR-based pluralization and empty states
- String variants
- Build-time diagnostics for malformed or inconsistent resources

Version 0.4.0 requires a compiler host compatible with Roslyn 5.6, such as the .NET 10 SDK.

See the [documentation and samples](https://github.com/DotNetPlus/ReswPlus) for setup and usage.