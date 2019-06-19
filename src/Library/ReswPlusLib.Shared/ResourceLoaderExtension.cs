using ReswPlusLib.Interfaces;
using ReswPlusLib.Utils;
using System.Globalization;
using System;

#if DOTNETCORE
using Microsoft.Extensions.Localization;
#endif

#if WINRT || DOTNETCORE || NETSTANDARD
using System.Resources;
#endif

#if WINDOWS_UWP || WINRT
using Windows.ApplicationModel.Resources;
#endif

namespace ReswPlusLib
{
    public static class ResourceLoaderExtension
    {
        private static IPluralProvider _pluralProvider;
        private static readonly object _objLock = new object();

#if WINRT || DOTNETCORE || NETSTANDARD
        public static string GetPlural(this ResourceManager resource, string key, double number)
        {
            return GetPluralInternal<ResourceManager>((reskey) => resource.GetString(reskey), key, number);
        }
#endif

#if WINRT || WINDOWS_UWP
        public static string GetPlural(this ResourceLoader resource, string key, double number)
        {
            return GetPluralInternal<ResourceLoader>((reskey) => resource.GetString(reskey), key, number);
        }
#endif

#if DOTNETCORE
        public static string GetPlural(this IStringLocalizer resource, string key, double number)
        {
            return GetPluralInternal<IStringLocalizer>((reskey) => resource.GetString(reskey), key, number);
        }
#endif

        private static string GetPluralInternal<T>(Func<string, string> getString, string key, double number)
        {
            if (_pluralProvider == null)
            {
                CreatePluralProvider();
                if (_pluralProvider == null)
                {
                    return "";
                }
            }
            string selectedSentence = null;
            var pluralType = _pluralProvider.ComputePlural(number);
            try
            {
                switch (pluralType)
                {
                    case PluralTypeEnum.ZERO:
                        selectedSentence = getString(key + "_Zero");
                        break;
                    case PluralTypeEnum.ONE:
                        selectedSentence = getString(key + "_One");
                        break;
                    case PluralTypeEnum.OTHER:
                        selectedSentence = getString(key + "_Other");
                        break;
                    case PluralTypeEnum.TWO:
                        selectedSentence = getString(key + "_Two");
                        break;
                    case PluralTypeEnum.FEW:
                        selectedSentence = getString(key + "_Few");
                        break;
                    case PluralTypeEnum.MANY:
                        selectedSentence = getString(key + "_Many");
                        break;
                }
            }
            catch { }
            return selectedSentence ?? "";
        }

        private static void CreatePluralProvider(string forcedCultureName = null)
        {
            lock (_objLock)
            {
                if (_pluralProvider == null)
                {
                    var cultureToUse = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                    if (!string.IsNullOrEmpty(forcedCultureName))
                    {
                        try
                        {
                            var forcedCulture = new CultureInfo(forcedCultureName)?.TwoLetterISOLanguageName;
                            if (!string.IsNullOrEmpty(forcedCulture))
                            {
                                cultureToUse = forcedCulture;
                            }
                        }
                        catch
                        {
                        }
                    }
                    _pluralProvider = PluralHelper.GetPluralChooser(cultureToUse);
                }
            }
        }
    }
}
