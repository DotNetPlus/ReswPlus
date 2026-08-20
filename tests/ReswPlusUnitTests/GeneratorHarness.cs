using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReswPlus.SourceGenerator;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Runs <see cref="ReswSourceGenerator"/> the way the compiler runs it, over an in-memory project.
/// </summary>
/// <remarks>
/// The rest of the suite reaches into <see cref="ReswPlus.SourceGenerator.ClassGenerators.ReswClassGenerator"/>
/// directly, which leaves everything the generator itself does untested: reading the MSBuild properties,
/// deciding the kind of app from the references of the compilation, grouping the files of a resource by
/// language, naming the generated types after the folder they sit in, and emitting the shared support sources
/// exactly once for a project holding several <c>.resw</c> files. This harness drives the real
/// <see cref="Microsoft.CodeAnalysis.IIncrementalGenerator"/> through a <see cref="CSharpGeneratorDriver"/> so
/// that those are covered, and it compiles what comes out, which is the only way to catch the failure a
/// generator is most prone to: emitting source that doesn't build in the consumer's project.
/// </remarks>
internal static class ReswGeneratorHarness
{
    /// <summary>
    /// The directory the generated project is rooted at.
    /// </summary>
    public const string ProjectDir = @"C:\Project\";

    /// <summary>
    /// Builds a <c>.resw</c> file sitting in the language folder of a resource.
    /// </summary>
    /// <param name="language">The name of the language folder, such as <c>en-US</c>.</param>
    /// <param name="content">The content of the file.</param>
    /// <param name="baseName">The name of the resource, without extension.</param>
    /// <param name="folder">The folder of the project holding the resource.</param>
    /// <returns>The file to hand to <see cref="Run"/>.</returns>
    public static ReswFile File(string language, string content, string baseName = "Resources", string folder = "Strings")
    {
        return new ReswFile($@"{ProjectDir}{folder}\{language}\{baseName}.resw", content);
    }

    /// <summary>
    /// Runs the generator over a project.
    /// </summary>
    /// <param name="files">The <c>.resw</c> files of the project.</param>
    /// <param name="appType">The kind of app to make the references of the compilation describe.</param>
    /// <param name="rootNamespace">The <c>RootNamespace</c> of the project, or <see langword="null"/> to leave it undeclared.</param>
    /// <param name="outputType">The <c>OutputType</c> of the project, or <see langword="null"/> to leave it undeclared.</param>
    /// <param name="projectDir">The <c>ProjectDir</c> of the project, or <see langword="null"/> to leave it undeclared.</param>
    /// <param name="msBuildProjectFullPath">The <c>MSBuildProjectFullPath</c> of the project, which is the fallback for <paramref name="projectDir"/>.</param>
    /// <param name="projectTypeGuids">The <c>ProjectTypeGuids</c> of the project, which is how a legacy project declares that it is a library.</param>
    /// <param name="defaultLanguage">The <c>DefaultLanguage</c> of the project, which picks the language the code is generated from.</param>
    /// <param name="useApplicationLanguages">The <c>ReswPlusUseApplicationLanguages</c> of the project, or <see langword="null"/> to leave it undeclared.</param>
    /// <param name="assemblyName">The name of the assembly being compiled.</param>
    /// <param name="additionalFiles">Files to pass to the compiler on top of <paramref name="files"/>, to cover what the generator does with the ones it doesn't own.</param>
    /// <returns>The result of the run.</returns>
    public static ReswGeneratorRun Run(
        IEnumerable<ReswFile> files,
        AppType appType = AppType.WindowsAppSDK,
        string? rootNamespace = "TestProject",
        string? outputType = "Library",
        string? projectDir = ProjectDir,
        string? msBuildProjectFullPath = null,
        string? projectTypeGuids = null,
        string? defaultLanguage = null,
        bool? useApplicationLanguages = null,
        string assemblyName = "TestProject",
        IEnumerable<ReswFile>? additionalFiles = null)
    {
        var texts = files
            .Concat(additionalFiles ?? [])
            .Select(file => (AdditionalText)new InMemoryAdditionalText(file.Path, file.Content))
            .ToImmutableArray();

        // The compiler keys the options of a project case insensitively, and the generator reads some of them
        // in a different case than MSBuild writes them, so the harness has to key them the same way.
        var options = new Dictionary<string, string>(AnalyzerConfigOptions.KeyComparer);

        Declare("build_property.ProjectDir", projectDir);
        Declare("build_property.MSBuildProjectFullPath", msBuildProjectFullPath);
        Declare("build_property.OutputType", outputType);
        Declare("build_property.ProjectTypeGuids", projectTypeGuids);
        Declare("build_property.DefaultLanguage", defaultLanguage);
        Declare("build_property.RootNamespace", rootNamespace);
        Declare("build_property.ReswPlusUseApplicationLanguages", useApplicationLanguages?.ToString().ToLowerInvariant());

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: null,
            PlatformStubs.ReferencesFor(appType),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(
            [new ReswSourceGenerator().AsSourceGenerator()],
            texts,
            ParseOptions,
            new TestAnalyzerConfigOptionsProvider(options),
            new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        return ReswGeneratorRun.From(driver, compilation);

        void Declare(string key, string? value)
        {
            if (value is not null)
            {
                options[key] = value;
            }
        }
    }

    /// <summary>
    /// The options the generated sources are parsed with.
    /// </summary>
    /// <remarks>
    /// The language version is the one a UWP project gets by default, which is the floor the generated code has
    /// to stay under, and the documentation mode is the one that reports the malformed documentation comments a
    /// resource value can produce.
    /// </remarks>
    private static readonly CSharpParseOptions ParseOptions =
        new(LanguageVersion.CSharp7_3, DocumentationMode.Diagnose);
}

/// <summary>
/// A <c>.resw</c> file of a generated project.
/// </summary>
/// <param name="Path">The full path of the file.</param>
/// <param name="Content">The content of the file.</param>
internal sealed record ReswFile(string Path, string Content);

/// <summary>
/// The outcome of a run of the generator.
/// </summary>
internal sealed class ReswGeneratorRun
{
    private readonly GeneratorDriver _driver;
    private readonly Compilation _inputCompilation;

    private ReswGeneratorRun(GeneratorDriver driver, Compilation inputCompilation, Compilation outputCompilation, ImmutableArray<Diagnostic> diagnostics)
    {
        _driver = driver;
        _inputCompilation = inputCompilation;
        OutputCompilation = outputCompilation;
        Diagnostics = diagnostics;
    }

    /// <summary>
    /// The diagnostics the generator reported.
    /// </summary>
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    /// <summary>
    /// The compilation the generated sources were added to.
    /// </summary>
    public Compilation OutputCompilation { get; }

    /// <summary>
    /// The sources the generator emitted, keyed by hint name.
    /// </summary>
    public IReadOnlyDictionary<string, string> Sources =>
        _driver.GetRunResult().Results.Single().GeneratedSources.ToDictionary(
            source => source.HintName,
            source => source.SourceText.ToString());

    /// <summary>
    /// The identifiers of the diagnostics the generator reported.
    /// </summary>
    public IReadOnlyList<string> DiagnosticIds => [.. Diagnostics.Select(diagnostic => diagnostic.Id)];

    internal static ReswGeneratorRun From(GeneratorDriver driver, Compilation compilation)
    {
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return new ReswGeneratorRun(driver, compilation, outputCompilation, diagnostics);
    }

    /// <summary>
    /// Runs the generator again over a new set of files, reusing the state of the previous run.
    /// </summary>
    /// <param name="files">The files of the project for this run.</param>
    /// <returns>The result of the second run, which remembers what the first one computed.</returns>
    /// <remarks>
    /// This is how the compiler behaves while a project is edited, and it is the only way to observe what the
    /// generator recomputes and what it reuses.
    /// </remarks>
    public ReswGeneratorRun RunAgain(IEnumerable<ReswFile> files)
    {
        var texts = files
            .Select(file => (AdditionalText)new InMemoryAdditionalText(file.Path, file.Content))
            .ToImmutableArray();

        return From(_driver.ReplaceAdditionalTexts(texts), _inputCompilation);
    }

    /// <summary>
    /// Returns the source emitted under the hint name holding the given text.
    /// </summary>
    /// <param name="hintNamePart">A part of the hint name of the wanted source.</param>
    /// <returns>The content of the matching source.</returns>
    public string Source(string hintNamePart)
    {
        var sources = Sources;
        var matches = sources.Keys.Where(name => name.Contains(hintNamePart, StringComparison.Ordinal)).ToArray();

        Assert.True(
            matches.Length == 1,
            $"Expected exactly one generated source whose hint name holds '{hintNamePart}', found {matches.Length}. " +
            $"The generator emitted: {string.Join(", ", sources.Keys.OrderBy(name => name, StringComparer.Ordinal))}.");

        return sources[matches[0]];
    }

    /// <summary>
    /// Asserts that the generated sources compile, without an error and without a warning.
    /// </summary>
    /// <remarks>
    /// A generator that emits source that doesn't build breaks the build of its consumer with an error that
    /// points at generated code rather than at the resource that caused it, so what comes out is compiled here
    /// rather than only parsed. Warnings count too: the generated code lands in projects that build with
    /// warnings as errors.
    /// </remarks>
    public void AssertCompiles()
    {
        AssertCompiles(OutputCompilation);
    }

    /// <summary>
    /// Asserts that the generated sources compile beside code of the consumer.
    /// </summary>
    /// <param name="consumerSource">The source of the consumer to compile the generated sources with.</param>
    public void AssertCompilesWith(string consumerSource)
    {
        // The documentation of the consumer is none of the generator's business, so it is not diagnosed.
        var tree = CSharpSyntaxTree.ParseText(
            consumerSource,
            new CSharpParseOptions(LanguageVersion.CSharp7_3, DocumentationMode.None));

        AssertCompiles(OutputCompilation.AddSyntaxTrees(tree));
    }

    private static void AssertCompiles(Compilation compilation)
    {
        using var peStream = new MemoryStream();

        var result = compilation.Emit(peStream);

        var problems = result.Diagnostics
            .Where(diagnostic => diagnostic.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
            .ToArray();

        Assert.True(
            problems.Length == 0,
            $"The generated sources do not compile cleanly:{Environment.NewLine}" +
            string.Join(Environment.NewLine, problems.Select(Describe)));

        string Describe(Diagnostic diagnostic)
        {
            var tree = diagnostic.Location.SourceTree;
            var source = tree is null ? "" : $" [{tree.FilePath}]";

            return $"  {diagnostic.Severity}: {diagnostic}{source}";
        }
    }

    /// <summary>
    /// Returns the reasons a step of the pipeline was run for, keyed by the name the step is tracked under.
    /// </summary>
    /// <remarks>
    /// A step whose inputs didn't change is reported as <see cref="IncrementalStepRunReason.Cached"/> or
    /// <see cref="IncrementalStepRunReason.Unchanged"/>, which is what tells a test that the generator reused
    /// what it had rather than recomputing it.
    /// </remarks>
    public IReadOnlyDictionary<string, IReadOnlyList<IncrementalStepRunReason>> TrackedSteps()
    {
        return _driver.GetRunResult().Results.Single().TrackedSteps.ToDictionary(
            step => step.Key,
            step => (IReadOnlyList<IncrementalStepRunReason>)
                [.. step.Value.SelectMany(run => run.Outputs).Select(output => output.Reason)]);
    }

    /// <summary>
    /// Asserts that a tracked step of the pipeline reused everything it had computed before.
    /// </summary>
    /// <param name="stepName">The name the step is tracked under.</param>
    public void AssertReused(string stepName)
    {
        var steps = TrackedSteps();

        Assert.True(
            steps.ContainsKey(stepName),
            $"No step is tracked under '{stepName}'. The pipeline tracks: " +
            $"{string.Join(", ", steps.Keys.OrderBy(name => name, StringComparer.Ordinal))}.");

        var recomputed = steps[stepName]
            .Where(reason => reason is not (IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged))
            .ToArray();

        Assert.True(
            recomputed.Length == 0,
            $"The step '{stepName}' was expected to be reused, but {recomputed.Length} of its outputs were " +
            $"recomputed: {string.Join(", ", recomputed)}.");
    }
}

/// <summary>
/// An <see cref="AdditionalText"/> whose content is held in memory.
/// </summary>
/// <param name="path">The path to report for the file.</param>
/// <param name="content">The content of the file.</param>
internal sealed class InMemoryAdditionalText(string path, string content) : AdditionalText
{
    private readonly SourceText _text = SourceText.From(content, Encoding.UTF8);

    /// <inheritdoc/>
    public override string Path { get; } = path;

    /// <inheritdoc/>
    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return _text;
    }
}

/// <summary>
/// The MSBuild properties of a generated project, as the compiler exposes them.
/// </summary>
/// <param name="globalOptions">The properties of the project.</param>
internal sealed class TestAnalyzerConfigOptionsProvider(IReadOnlyDictionary<string, string> globalOptions)
    : AnalyzerConfigOptionsProvider
{
    /// <inheritdoc/>
    public override AnalyzerConfigOptions GlobalOptions { get; } = new TestAnalyzerConfigOptions(globalOptions);

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => TestAnalyzerConfigOptions.Empty;

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => TestAnalyzerConfigOptions.Empty;

    private sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> options) : AnalyzerConfigOptions
    {
        public static readonly TestAnalyzerConfigOptions Empty = new(new Dictionary<string, string>());

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            return options.TryGetValue(key, out value);
        }
    }
}

/// <summary>
/// The references that make a compilation look like the kind of app the generator supports.
/// </summary>
/// <remarks>
/// The generator decides between UWP and the Windows App SDK by looking for a reference whose name holds
/// <c>Windows.Foundation.UniversalApiContract</c> or <c>Microsoft.WindowsAppSdk</c>, and the code it emits
/// derives from the XAML markup extension of the matching framework and talks to its resource loader. Both are
/// stubbed here rather than pulled in as real packages: the real ones are a Windows-only, version-pinned
/// dependency, while their shape is small and is exactly what the generated code needs to compile against.
/// </remarks>
internal static class PlatformStubs
{
    /// <summary>
    /// The types the code generated for the Windows App SDK binds to.
    /// </summary>
    private const string WindowsAppSdkStub = """
        namespace Microsoft.UI.Xaml.Markup
        {
            public abstract class MarkupExtension
            {
                protected MarkupExtension() { }
                protected virtual object ProvideValue() { return null; }
            }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
            public sealed class MarkupExtensionReturnTypeAttribute : System.Attribute
            {
                public System.Type ReturnType { get; set; }
            }
        }

        namespace Microsoft.UI.Xaml.Data
        {
            public interface IValueConverter
            {
                object Convert(object value, System.Type targetType, object parameter, string language);
                object ConvertBack(object value, System.Type targetType, object parameter, string language);
            }
        }

        namespace Microsoft.Windows.ApplicationModel.Resources
        {
            public sealed class ResourceLoader
            {
                public ResourceLoader(string fileName, string resourceMap) { }
                public static string GetDefaultResourceFilePath() { return ""; }
                public string GetString(string resourceId) { return ""; }
            }
        }

        namespace Microsoft.Windows.Globalization
        {
            public static class ApplicationLanguages
            {
                public static string PrimaryLanguageOverride { get; set; }
            }
        }

        namespace Windows.Globalization
        {
            public static class ApplicationLanguages
            {
                public static System.Collections.Generic.IReadOnlyList<string> Languages { get; set; }
            }
        }
        """;

    /// <summary>
    /// The types the code generated for UWP binds to.
    /// </summary>
    private const string UwpStub = """
        namespace Windows.UI.Xaml.Markup
        {
            public abstract class MarkupExtension
            {
                protected MarkupExtension() { }
                protected virtual object ProvideValue() { return null; }
            }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
            public sealed class MarkupExtensionReturnTypeAttribute : System.Attribute
            {
                public System.Type ReturnType { get; set; }
            }
        }

        namespace Windows.UI.Xaml.Data
        {
            public interface IValueConverter
            {
                object Convert(object value, System.Type targetType, object parameter, string language);
                object ConvertBack(object value, System.Type targetType, object parameter, string language);
            }
        }

        namespace Windows.ApplicationModel.Resources
        {
            public sealed class ResourceLoader
            {
                public static ResourceLoader GetForViewIndependentUse(string name) { return null; }
                public string GetString(string resourceId) { return ""; }
            }
        }

        namespace Windows.Globalization
        {
            public static class ApplicationLanguages
            {
                public static System.Collections.Generic.IReadOnlyList<string> Languages { get; set; }
            }
        }
        """;

    /// <summary>
    /// The assemblies of the running runtime, which is what the generated code takes its BCL types from.
    /// </summary>
    /// <remarks>
    /// The Windows projection is left out on purpose: it declares some of the types stubbed here, and having
    /// both would make the generated code compile against whichever one wins rather than against a known shape.
    /// </remarks>
    private static readonly Lazy<ImmutableArray<MetadataReference>> RuntimeReferences = new(() =>
        [.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Where(path => path.Length != 0)
            .Where(path => !IsWindowsProjection(path))
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))]);

    private static readonly Lazy<MetadataReference> WindowsAppSdk = new(() =>
        Compile("ReswPlusTests.WindowsAppSdkStub", WindowsAppSdkStub, @"C:\Packages\Microsoft.WindowsAppSdk\Microsoft.WindowsAppSdk.dll"));

    private static readonly Lazy<MetadataReference> Uwp = new(() =>
        Compile("ReswPlusTests.UwpStub", UwpStub, @"C:\Packages\UAP\Windows.Foundation.UniversalApiContract.winmd"));

    /// <summary>
    /// Returns the references a compilation of the given kind of app is built with.
    /// </summary>
    /// <param name="appType">The kind of app to describe.</param>
    /// <returns>The references of the compilation.</returns>
    public static ImmutableArray<MetadataReference> ReferencesFor(AppType appType)
    {
        return appType switch
        {
            AppType.WindowsAppSDK => RuntimeReferences.Value.Add(WindowsAppSdk.Value),
            AppType.UWP => RuntimeReferences.Value.Add(Uwp.Value),
            _ => RuntimeReferences.Value,
        };
    }

    private static bool IsWindowsProjection(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);

        return name.Equals("Microsoft.Windows.SDK.NET", StringComparison.OrdinalIgnoreCase)
            || name.Equals("WinRT.Runtime", StringComparison.OrdinalIgnoreCase);
    }

    private static MetadataReference Compile(string assemblyName, string source, string filePath)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            RuntimeReferences.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var peStream = new MemoryStream();

        var result = compilation.Emit(peStream);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"The '{assemblyName}' stub does not compile:{Environment.NewLine}" +
                string.Join(Environment.NewLine, result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        }

        // The generator reads the kind of app off the name of the referenced files, so the stub is given the
        // path the real framework would have.
        return AssemblyMetadata.CreateFromImage(peStream.ToArray())
            .GetReference(filePath: filePath, display: filePath);
    }
}
