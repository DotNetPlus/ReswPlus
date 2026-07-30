namespace ReswPlus.SourceGenerator;

/// <summary>
/// The bodies substituted into the <c>GetPluralLanguage</c> method of the generated resource loader extension.
/// </summary>
/// <remarks>
/// Which one is used is decided by the <c>ReswPlusUseApplicationLanguages</c> MSBuild property. Both are
/// indented to sit inside a method body, which is where the template substitutes them.
/// </remarks>
internal static class PluralLanguageResolvers
{
    /// <summary>
    /// Reads the plural language from the .NET UI culture of the thread.
    /// </summary>
    /// <remarks>
    /// This is the historical behaviour, and it is what a project gets unless it opts in to the other one. It
    /// can disagree with the language the resources themselves are resolved in, because the .NET UI culture
    /// comes from the display languages of the user while the resources come from the app runtime language
    /// list.
    /// </remarks>
    public const string CurrentUICulture = """
                        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            """;

    /// <summary>
    /// Reads the plural language from the app runtime language list, which is what resolves the resources.
    /// </summary>
    /// <remarks>
    /// The first entry of the list is the language the resource loader resolves against, so taking it keeps
    /// the plural rules and the resources on the same language. The .NET UI culture is still used when the
    /// list cannot be read, which is the case outside of an app package.
    /// </remarks>
    public const string ApplicationLanguages = """
                        try
                        {
                            var applicationLanguages = global::Windows.Globalization.ApplicationLanguages.Languages;
                            if (applicationLanguages != null && applicationLanguages.Count != 0)
                            {
                                var applicationLanguage = applicationLanguages[0];
                                if (!string.IsNullOrEmpty(applicationLanguage))
                                {
                                    return new CultureInfo(applicationLanguage).TwoLetterISOLanguageName;
                                }
                            }
                        }
                        catch
                        {
                            // The app runtime language list is not readable outside of an app package.
                        }
                        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            """;

    /// <summary>
    /// Returns the body to substitute for a project.
    /// </summary>
    /// <param name="useApplicationLanguages">Whether the project opted into the app runtime language list.</param>
    /// <returns>The body of the generated <c>GetPluralLanguage</c> method.</returns>
    public static string GetResolver(bool useApplicationLanguages)
    {
        return useApplicationLanguages ? ApplicationLanguages : CurrentUICulture;
    }
}
