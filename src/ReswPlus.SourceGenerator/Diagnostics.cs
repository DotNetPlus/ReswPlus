using Microsoft.CodeAnalysis;

namespace ReswPlus.SourceGenerator;

/// <summary>
/// The descriptors of all the diagnostics reported by ReswPlus.
/// </summary>
/// <remarks>
/// Every rule declared here must also be listed in <c>AnalyzerReleases.Unshipped.md</c> until it ships,
/// otherwise the build fails with RS2008.
/// </remarks>
internal static class Diagnostics
{
    /// <summary>
    /// The category of the diagnostics reporting an unsupported or misconfigured project.
    /// </summary>
    private const string CompatibilityCategory = "Compatibility";

    /// <summary>
    /// The category of the diagnostics reporting a problem in the content of the <c>.resw</c> files.
    /// </summary>
    private const string ResourcesCategory = "Resources";

    /// <summary>
    /// RESWP0001: the compilation is not a C# compilation.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsupportedLanguage = new(
        "RESWP0001",
        "Language not supported",
        "ReswPlus source generator only supports C#",
        CompatibilityCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// RESWP0002: the root namespace of the project could not be determined.
    /// </summary>
    public static readonly DiagnosticDescriptor UnknownNamespace = new(
        "RESWP0002",
        "Unknown namespace",
        "ReswPlus cannot determine the namespace",
        CompatibilityCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// RESWP0003: the root path of the project could not be determined.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingRootPath = new(
        "RESWP0003",
        "Root path missing",
        "Can't retrieve the root path of the project",
        CompatibilityCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// RESWP0004: the project type could not be determined.
    /// </summary>
    public static readonly DiagnosticDescriptor UnknownProjectType = new(
        "RESWP0004",
        "Unknown Project Type",
        "ReswPlus cannot determine the project type, defaulting to application",
        CompatibilityCategory,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// RESWP0005: the project is neither a UWP nor a WinAppSDK project.
    /// </summary>
    public static readonly DiagnosticDescriptor UnrecognizedAppType = new(
        "RESWP0005",
        "Project type not recognized",
        "ReswPlus only supports UWP and WinAppSDK applications/libraries",
        CompatibilityCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// RESWP0006: a translated value doesn't use the same placeholders as the default language.
    /// </summary>
    public static readonly DiagnosticDescriptor PlaceholderMismatch = new(
        "RESWP0006",
        "Placeholder mismatch between languages",
        "The value of the resource '{0}' uses {1}, while its value in the default language uses {2}",
        ResourcesCategory,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// RESWP0007: a value uses a placeholder that has no matching parameter in its <c>#Format</c> tag.
    /// </summary>
    public static readonly DiagnosticDescriptor UndeclaredFormatParameter = new(
        "RESWP0007",
        "Undeclared format parameter",
        "The value of the resource '{0}' uses the placeholder '{{{1}}}', but its '#Format' tag only declares {2} parameter(s)",
        ResourcesCategory,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// RESWP0008: a pluralized resource is missing plural forms its language requires.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingPluralForms = new(
        "RESWP0008",
        "Missing plural forms",
        "The pluralized resource '{0}' is missing the {1} form(s) required by the '{2}' language",
        ResourcesCategory,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// RESWP0009: two resources of the same file conflict with each other.
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateResource = new(
        "RESWP0009",
        "Conflicting resources",
        "The resource '{0}' conflicts with the resource '{1}' declared earlier in the same file",
        ResourcesCategory,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// RESWP0010: a value that is used as a composite format string is malformed.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidFormatString = new(
        "RESWP0010",
        "Invalid composite format string",
        "The value of the resource '{0}' is used as a composite format string, but it is not a valid one",
        ResourcesCategory,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
