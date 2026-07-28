## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RESWP0001 | Compatibility    | Error    | ReswPlus source generator only supports C#.
RESWP0002 | Compatibility    | Error    | ReswPlus cannot determine the namespace.
RESWP0003 | Compatibility    | Error    | Can't retrieve the root path of the project.
RESWP0004 | Compatibility    | Info    | ReswPlus cannot determine the project type, defaulting to application.
RESWP0005 | Compatibility    | Error    | ReswPlus only supports UWP and WinAppSDK applications/libraries.
RESWP0006 | Resources    | Warning    | A translated value drops placeholders its value in the default language uses.
RESWP0007 | Resources    | Warning    | A value uses a placeholder that has no matching parameter in its #Format tag.
RESWP0008 | Resources    | Warning    | A pluralized resource is missing plural forms its language requires.
RESWP0009 | Resources    | Warning    | Two resources of the same file conflict with each other.
RESWP0010 | Resources    | Warning    | A value that is used as a composite format string is malformed.