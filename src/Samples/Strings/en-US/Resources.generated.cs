// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package PluralNet is necessary to support Pluralization.
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Data;

namespace ReswPlusSample.Strings {
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Huyn.ReswPlus", "0.1.1.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        private static ResourceLoader  _resourceLoader;
        static Resources()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
        }

        #region FileShared
        /// <summary>
        ///   Get the pluralized version of the string similar to: {0} shared {1} photos from {2}
        /// </summary>
        public static string FileShared(double number)
        {
            return Huyn.PluralNet.ResourceLoaderExtension.GetPlural(_resourceLoader, "FileShared", (decimal)number);
        }
        /// <summary>
        ///   Format the string similar to: {0} shared {1} photos from {2}
        /// </summary>
        public static string FileShared_Format(string username, double pluralCount, string city)
        {
            return string.Format(FileShared(pluralCount), username, pluralCount, city);
        }
        #endregion

        #region MinutesLeft
        /// <summary>
        ///   Get the pluralized version of the string similar to: {0} minute left
        /// </summary>
        public static string MinutesLeft(double number)
        {
            return Huyn.PluralNet.ResourceLoaderExtension.GetPlural(_resourceLoader, "MinutesLeft", (decimal)number);
        }
        /// <summary>
        ///   Format the string similar to: {0} minute left
        /// </summary>
        public static string MinutesLeft_Format(double pluralCount)
        {
            return string.Format(MinutesLeft(pluralCount), pluralCount);
        }
        #endregion

        #region PluralizationTest
        /// <summary>
        ///   Get the pluralized version of the string similar to: This is the singular form
        /// </summary>
        public static string PluralizationTest(double number)
        {
            return Huyn.PluralNet.ResourceLoaderExtension.GetPlural(_resourceLoader, "PluralizationTest", (decimal)number);
        }
        #endregion

        #region ReceivedMessages
        /// <summary>
        ///   Get the pluralized version of the string similar to: No new messages from {1}
        /// </summary>
        public static string ReceivedMessages(double number)
        {
            if(number == 0)
            {
                return _resourceLoader.GetString("ReceivedMessages_None");
            }
            return Huyn.PluralNet.ResourceLoaderExtension.GetPlural(_resourceLoader, "ReceivedMessages", (decimal)number);
        }
        /// <summary>
        ///   Format the string similar to: No new messages from {1}
        /// </summary>
        public static string ReceivedMessages_Format(double pluralCount, string paramString2)
        {
            return string.Format(ReceivedMessages(pluralCount), pluralCount, paramString2);
        }
        #endregion

        #region ForecastAnnouncement
        /// <summary>
        ///   Looks up a localized string similar to: The current temperature in {2} is {0}°F ({1}°C)
        /// </summary>
        public static string ForecastAnnouncement => _resourceLoader.GetString("ForecastAnnouncement");
        /// <summary>
        ///   Format the string similar to: The current temperature in {2} is {0}°F ({1}°C)
        /// </summary>
        public static string ForecastAnnouncement_Format(int tempFahrenheit, int tempCelsius, string city)
        {
            return string.Format(ForecastAnnouncement, tempFahrenheit, tempCelsius, city);
        }
        #endregion

        #region GotMessages
        /// <summary>
        ///   Looks up a localized string similar to: Welcome {0}, you got {1} emails!
        /// </summary>
        public static string GotMessages => _resourceLoader.GetString("GotMessages");
        /// <summary>
        ///   Format the string similar to: Welcome {0}, you got {1} emails!
        /// </summary>
        public static string GotMessages_Format(string paramString1, uint paramUint2)
        {
            return string.Format(GotMessages, paramString1, paramUint2);
        }
        #endregion

        /// <summary>
        ///   Looks up a localized string similar to: this is a tooltip text
        /// </summary>
        public static string ThisIsATooltip => _resourceLoader.GetString("ThisIsATooltip");

        /// <summary>
        ///   Looks up a localized string similar to: Hello World!
        /// </summary>
        public static string WelcomeTitle => _resourceLoader.GetString("WelcomeTitle");

        #region YourAgeAndName
        /// <summary>
        ///   Looks up a localized string similar to: Your are {0}yo and named {1}!
        /// </summary>
        public static string YourAgeAndName => _resourceLoader.GetString("YourAgeAndName");
        /// <summary>
        ///   Format the string similar to: Your are {0}yo and named {1}!
        /// </summary>
        public static string YourAgeAndName_Format(double paramDouble1, string paramString2)
        {
            return string.Format(YourAgeAndName, paramDouble1, paramString2);
        }
        #endregion

    }

    public enum ReswPlusKeyExtension
    {
        ForecastAnnouncement,
        GotMessages,
        ThisIsATooltip,
        WelcomeTitle,
        YourAgeAndName,
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Huyn.ReswPlus", "0.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public class ResourcesExtension: MarkupExtension
    {
        private static ResourceLoader  _resourceLoader;
        static ResourcesExtension()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
        }
        public ReswPlusKeyExtension? Key { get; set;}
        public IValueConverter Converter { get; set;}
        public object ConverterParameter { get; set;}
        protected override object ProvideValue()
        {
            string res;
            if(!Key.HasValue)
            {
                res = "";
            }
            else
            {
                res = _resourceLoader.GetString(Key.Value.ToString());
            }
            return Converter == null ? res : Converter.Convert(res, typeof(String), ConverterParameter, null);
        }
    }

}

