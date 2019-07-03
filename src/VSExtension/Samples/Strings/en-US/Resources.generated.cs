// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Data;

namespace ReswPlusSample.Strings{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Huyn.ReswPlus", "0.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        private static ResourceLoader _resourceLoader;
        static Resources()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        }

        #region FileShared
        /// <summary>
        ///   Get the pluralized version of the string similar to: {0} shared {1} photo from {2}
        /// </summary>
        public static string FileShared(double pluralNumber)
        {
            return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "FileShared", pluralNumber);
        }

        /// <summary>
        ///   Format the string similar to: {0} shared {1} photo from {2}
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
        public static string MinutesLeft(double pluralNumber)
        {
            return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "MinutesLeft", pluralNumber);
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
        public static string PluralizationTest(double pluralNumber)
        {
            return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "PluralizationTest", pluralNumber);
        }
        #endregion

        #region ReceivedMessages
        /// <summary>
        ///   Get the pluralized version of the string similar to: No new messages from {1}
        /// </summary>
        public static string ReceivedMessages(double pluralNumber)
        {
            if(pluralNumber == 0)
            {
                return _resourceLoader.GetString("ReceivedMessages_None");
            }
            return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "ReceivedMessages", pluralNumber);
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

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Huyn.ReswPlus", "0.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public class ResourcesExtension: MarkupExtension
    {
        public enum KeyEnum
        {
            __Undefined = 0,
            ForecastAnnouncement,
            GotMessages,
            ThisIsATooltip,
            WelcomeTitle,
            YourAgeAndName,
        }

        private static ResourceLoader _resourceLoader;
        static ResourcesExtension()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        }
        public KeyEnum Key { get; set;}
        public IValueConverter Converter { get; set;}
        public object ConverterParameter { get; set;}
        protected override object ProvideValue()
        {
            string res;
            if(Key == KeyEnum.__Undefined)
            {
                res = "";
            }
            else
            {
                res = _resourceLoader.GetString(Key.ToString());
            }
            return Converter == null ? res : Converter.Convert(res, typeof(String), ConverterParameter, null);
        }
    }

} //ReswPlusSample.Strings
