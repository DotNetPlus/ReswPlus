; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RESWP0012 | Resources    | Warning    | A resource carries the name of a member the generated types declare themselves.
RESWP0013 | Resources    | Warning    | A #Format tag declares two parameters of the same name.
RESWP0014 | Resources    | Warning    | A resource file could not be turned into code.
RESWP0015 | Resources    | Error    | The plural support of the project could not be generated.
RESWP0016 | Resources    | Info     | A resource from the default language is missing from a translation.
RESWP0017 | Resources    | Info     | A translated resource does not exist in the default language.
RESWP0018 | Resources    | Info     | A translated value is identical to its default-language value.
RESWP0019 | Resources    | Warning  | A translation cannot serve the resource shape generated from the default language.
RESWP0020 | Resources    | Info     | A translation defines variants that do not exist in the default language.
