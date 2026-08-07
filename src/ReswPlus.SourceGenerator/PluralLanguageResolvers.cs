namespace ReswPlus.SourceGenerator;

/// <summary>
/// The bodies substituted into the <c>GetPluralLanguage</c> method of the generated resource loader extension.
/// </summary>
/// <remarks>
/// Which one is used is decided by the <c>ReswPlusUseApplicationLanguages</c> MSBuild property and by the kind
/// of app being built. They are all indented to sit inside a method body, which is where the template
/// substitutes them.
/// </remarks>
internal static class PluralLanguageResolvers
{
    /// <summary>
    /// Reads the plural language from the .NET UI culture of the thread.
    /// </summary>
    /// <remarks>
    /// This is the historical behaviour, and it is what a project gets unless it opts in to one of the others.
    /// It can disagree with the language the resources themselves are resolved in, because the .NET UI culture
    /// comes from the display languages of the user while the resources come from the app runtime language
    /// list.
    /// </remarks>
    private const string CurrentUICulture = """
                        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            """;

    /// <summary>
    /// Reads the plural language from the app runtime language list, which is what resolves the resources.
    /// </summary>
    /// <remarks>
    /// The first entry of the list is the language the resource loader resolves against, so taking it keeps
    /// the plural rules and the resources on the same language.
    /// </remarks>
    private const string ApplicationLanguages = """
                        try
                        {
                            var applicationLanguages = global::Windows.Globalization.ApplicationLanguages.Languages;
                            if (applicationLanguages != null && applicationLanguages.Count != 0)
                            {
                                var applicationLanguage = ReadLanguage(applicationLanguages[0]);
                                if (applicationLanguage != null)
                                {
                                    return applicationLanguage;
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
    /// Reads the plural language the same way, but through the override an app can set outside of a package.
    /// </summary>
    /// <remarks>
    /// The Windows App SDK keeps the override of an unpackaged app to itself and applies it straight to the
    /// resource context, so it never reaches the app runtime language list. Reading it first is what keeps an
    /// unpackaged app that picks its own language on the plural rules of that language.
    /// </remarks>
    private const string WindowsAppSDKApplicationLanguages = """
                        try
                        {
                            var languageOverride = ReadLanguage(
                                global::Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride);
                            if (languageOverride != null)
                            {
                                return languageOverride;
                            }

                            var applicationLanguages = global::Windows.Globalization.ApplicationLanguages.Languages;
                            if (applicationLanguages != null && applicationLanguages.Count != 0)
                            {
                                var applicationLanguage = ReadLanguage(applicationLanguages[0]);
                                if (applicationLanguage != null)
                                {
                                    return applicationLanguage;
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
    /// <param name="appType">The kind of app being built.</param>
    /// <returns>The body of the generated <c>GetPluralLanguage</c> method.</returns>
    public static string GetResolver(bool useApplicationLanguages, AppType appType)
    {
        if (!useApplicationLanguages)
        {
            return CurrentUICulture;
        }

        return appType == AppType.WindowsAppSDK ? WindowsAppSDKApplicationLanguages : ApplicationLanguages;
    }
}
