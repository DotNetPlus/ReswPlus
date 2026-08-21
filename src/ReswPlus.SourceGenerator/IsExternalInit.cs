namespace System.Runtime.CompilerServices;

/// <summary>
/// Lets this project use record types and init-only members.
/// </summary>
/// <remarks>
/// The compiler emits a reference to this type for every init-only property, and .NET Standard 2.0 -- which a
/// Roslyn analyzer has to target so that it loads in every host -- does not carry it. Declaring it here is the
/// usual way of filling that gap; it holds no members and is never referenced by name.
/// </remarks>
internal static class IsExternalInit
{
}
