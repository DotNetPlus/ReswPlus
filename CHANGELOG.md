# Changelog

## 0.4.0 - 2026-08-17

### Added

- Resource diagnostics `RESWP0006` through `RESWP0011` for placeholder mismatches, undeclared format parameters, missing plural forms, conflicting resources, malformed format strings, and unknown plural languages.
- Opt-in `ReswPlusUseApplicationLanguages` support so plural rules can follow the language selected by UWP and WinUI applications.

### Changed

- Rebuilt generated C# with Roslyn syntax APIs and hygienic `.g.cs` output, nullable annotations, XML documentation, and generated-code markers.
- Updated the repository toolchain to .NET 10, C# `latest`, and Roslyn 5.6.
- Improved the UWP and WinUI samples with guided explanations and clearer input, generated-code, usage, and result sections.

### Fixed

- Corrected CLDR plural selection for fractional values, large values, and multiple language-specific rules.
- Added current CLDR `many` handling for supported Romance languages and reliable fallback to the `Other` form.
- Fixed generated code under strict nullable, documentation, and analyzer configurations.

### Compatibility

ReswPlus still targets `netstandard2.0`, but version 0.4.0 requires a compiler host compatible with Roslyn 5.6, such as the .NET 10 SDK.
