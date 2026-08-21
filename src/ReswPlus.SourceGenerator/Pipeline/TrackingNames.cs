namespace ReswPlus.SourceGenerator.Pipeline;

/// <summary>
/// The names the stages of the pipeline are tracked under.
/// </summary>
/// <remarks>
/// Tracking a stage is what lets a test observe whether the generator reused what it had computed before or
/// recomputed it. Without it, the incremental behaviour of the generator can only be described, not asserted,
/// and a change that quietly makes the whole project regenerate on every keystroke looks exactly like one that
/// doesn't.
/// </remarks>
internal static class TrackingNames
{
    public const string Options = "ReswPlus.Options";

    public const string CompilationInfo = "ReswPlus.CompilationInfo";

    public const string Project = "ReswPlus.Project";

    public const string Paths = "ReswPlus.Paths";

    public const string Layout = "ReswPlus.Layout";

    /// <summary>

    /// The step reading, out of the layout, what generating one resource file depends on.

    /// </summary>

    public const string FilesToGenerate = "ReswPlus.FilesToGenerate";

    

    public const string Generation = "ReswPlus.Generation";

    public const string Support = "ReswPlus.Support";
}
