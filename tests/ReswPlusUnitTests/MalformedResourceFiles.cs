using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using ReswPlus.Core.ResourceParser;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Covers a resource file that is malformed rather than merely wrong.
/// </summary>
/// <remarks>
/// A <c>.resw</c> is written by hand and read on every keystroke, inside the process that serves the editor.
/// Parsing that can be made to take exponential time, or to expand a few hundred bytes into gigabytes, is not
/// a slow build: it is an editor that stops responding, on input a typo can produce.
/// </remarks>
public class MalformedResourceFiles
{
    /// <summary>
    /// How long the parsing of a pathological input is allowed to take.
    /// </summary>
    /// <remarks>
    /// Generous on purpose: the failure being guarded against is measured in minutes, so anything of this
    /// order says the expression no longer backtracks exponentially, without turning a slow machine into a
    /// failing build.
    /// </remarks>
    private static readonly TimeSpan Budget = TimeSpan.FromSeconds(2);

    /// <summary>
    /// An unterminated quoted literal, followed by a run of backslashes.
    /// </summary>
    /// <remarks>
    /// This is the shape that used to take exponential time: every backslash could be read either on its own
    /// or as the start of an escape, so the parser had to try every way of splitting the run before it could
    /// conclude that the literal is never closed.
    /// </remarks>
    private static string UnterminatedLiteral(int backslashes) =>
        "\"Hello" + new string('\\', backslashes);

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    [InlineData(48)]
    [InlineData(64)]
    public void AnUnterminatedLiteralDoesNotTakeExponentialTimeToSplit(int backslashes)
    {
        var source = UnterminatedLiteral(backslashes);

        var elapsed = Time(() => FormatTag.SplitParameters(source).ToArray());

        Assert.True(elapsed < Budget, $"Splitting took {elapsed}, which suggests it backtracks exponentially.");
    }

    [Theory]
    [InlineData(24)]
    [InlineData(32)]
    [InlineData(48)]
    [InlineData(64)]
    public void AnUnterminatedLiteralDoesNotTakeExponentialTimeToParse(int backslashes)
    {
        var resw = ReswTestHelpers.CreateResw(
            ("Greeting", "Hello", "#Format[" + UnterminatedLiteral(backslashes)));

        var elapsed = Time(() => ReswTestHelpers.GenerateCode(resw));

        Assert.True(elapsed < Budget, $"Generating took {elapsed}, which suggests it backtracks exponentially.");
    }

    /// <summary>
    /// A run of quotes inside a format tag that is never closed.
    /// </summary>
    [Theory]
    [InlineData(24)]
    [InlineData(40)]
    public void AnUnclosedFormatTagDoesNotTakeExponentialTimeToParse(int quotes)
    {
        var resw = ReswTestHelpers.CreateResw(
            ("Greeting", "Hello", "#Format[" + new string('"', quotes)));

        var elapsed = Time(() => ReswTestHelpers.GenerateCode(resw));

        Assert.True(elapsed < Budget, $"Generating took {elapsed}, which suggests it backtracks exponentially.");
    }

    /// <summary>
    /// A resource file that declares a document type is refused.
    /// </summary>
    /// <remarks>
    /// Nested entities are what turn a few hundred bytes into gigabytes of expanded text. A resource file has
    /// no use for a document type, so the reader refuses one outright rather than trying to bound it.
    /// </remarks>
    [Fact]
    public void AResourceFileThatDeclaresADocumentTypeIsRefused()
    {
        var bomb = """
            <?xml version="1.0" encoding="utf-8"?>
            <!DOCTYPE root [
              <!ENTITY a "aaaaaaaaaa">
              <!ENTITY b "&a;&a;&a;&a;&a;&a;&a;&a;&a;&a;">
              <!ENTITY c "&b;&b;&b;&b;&b;&b;&b;&b;&b;&b;">
              <!ENTITY d "&c;&c;&c;&c;&c;&c;&c;&c;&c;&c;">
              <!ENTITY e "&d;&d;&d;&d;&d;&d;&d;&d;&d;&d;">
            ]>
            <root>
              <data name="Boom"><value>&e;</value></data>
            </root>
            """;

        var elapsed = Time(() => Assert.Throws<XmlException>(() => ReswParser.Parse(bomb)));

        Assert.True(elapsed < Budget, $"Refusing the document type took {elapsed}.");
    }

    /// <summary>
    /// A resource file the parser cannot read is reported, not thrown out of the generator.
    /// </summary>
    [Fact]
    public void AMalformedResourceFileIsReportedRatherThanFailingTheBuild()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", "<root><data name=\"Truncated\"><value>no closing tags"),
        ]);

        Assert.Contains("RESWP0014", run.DiagnosticIds);
    }

    private static TimeSpan Time(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        var timedOut = false;

        try
        {
            action();
        }
        catch (RegexMatchTimeoutException)
        {
            // The expression gave up on its own. That is the net under it, not the fix, so it counts as a
            // failure here: an expression that only finishes because it was cut off is still exponential.
            timedOut = true;
        }

        var elapsed = stopwatch.Elapsed;

        Assert.False(timedOut, $"The parsing had to be cut off after {elapsed}.");

        return elapsed;
    }
}
