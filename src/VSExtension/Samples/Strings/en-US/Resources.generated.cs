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
        #region AnimalTreat
        /// <summary>
        ///   Get the pluralized version of the string similar to: Reward your pup, give {0} biscuit to {2} the dog!
        /// </summary>
        public static string AnimalTreat(long variantId, double pluralNumber)
        {
            return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "AnimalTreat_Variant" + variantId, pluralNumber);
        }

        /// <summary>
        ///   Get the pluralized version of the string similar to: Reward your pup, give {0} biscuit to {2} the dog!
        /// </summary>
        public static string AnimalTreat(object variantId, double pluralNumber)
        {
            try
            {
                return AnimalTreat(Convert.ToInt64(variantId), pluralNumber);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        ///   Format the string similar to: Reward your pup, give {0} biscuit to {2} the dog!
        /// </summary>
        public static string AnimalTreat_Format(double treatNumber, long petType, string petName)
        {
            return string.Format(AnimalTreat(petType, treatNumber), treatNumber, petType, petName);
        }

        /// <summary>
        ///   Format the string similar to: Reward your pup, give {0} biscuit to {2} the dog!
        /// </summary>
        public static string AnimalTreat_Format(double treatNumber, object petType, string petName)
        {
            try
            {
                return AnimalTreat_Format(treatNumber, Convert.ToInt64(petType), petName);
            }
            catch
            {
                return "";
            }
        }
        #endregion

        #region DriverArrived
        /// <summary>
        ///   Get the variant version of the string similar to: DriverArrived_Variant1
        /// </summary>
        public static string DriverArrived(long variantId)
        {
            return _resourceLoader.GetString("DriverArrived_Variant" + variantId);
        }

        /// <summary>
        ///   Get the variant version of the string similar to: DriverArrived_Variant1
        /// </summary>
        public static string DriverArrived(object variantId)
        {
            try
            {
                return DriverArrived(Convert.ToInt64(variantId));
            }
            catch
            {
                return "";
            }
        }
        #endregion

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

        #region Greeting
        /// <summary>
        ///   Get the variant version of the string similar to: Greeting_Variant1
        /// </summary>
        public static string Greeting(long variantId)
        {
            return _resourceLoader.GetString("Greeting_Variant" + variantId);
        }

        /// <summary>
        ///   Get the variant version of the string similar to: Greeting_Variant1
        /// </summary>
        public static string Greeting(object variantId)
        {
            try
            {
                return Greeting(Convert.ToInt64(variantId));
            }
            catch
            {
                return "";
            }
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

        #region AppVersion
        /// <summary>
        ///   Looks up a localized string similar to: version: {0} v{1}
        /// </summary>
        public static string AppVersion => _resourceLoader.GetString("AppVersion");

        /// <summary>
        ///   Format the string similar to: version: {0} v{1}
        /// </summary>
        public static string AppVersion_Format()
        {
            return string.Format(AppVersion, ReswPlusLib.Macros.ApplicationName, ReswPlusLib.Macros.AppVersionFull);
        }
        #endregion

        #region CopyrightNotice
        /// <summary>
        ///   Looks up a localized string similar to: © {0} - {1}. All rights reserved.
        /// </summary>
        public static string CopyrightNotice => _resourceLoader.GetString("CopyrightNotice");

        /// <summary>
        ///   Format the string similar to: © {0} - {1}. All rights reserved.
        /// </summary>
        public static string CopyrightNotice_Format()
        {
            return string.Format(CopyrightNotice, ReswPlusLib.Macros.Year, ReswPlusLib.Macros.PublisherName);
        }
        #endregion

        #region DonateToAssociation
        /// <summary>
        ///   Looks up a localized string similar to: Hey {1}, donate {2:C2} to {0}!
        /// </summary>
        public static string DonateToAssociation => _resourceLoader.GetString("DonateToAssociation");

        /// <summary>
        ///   Format the string similar to: Hey {1}, donate {2:C2} to {0}!
        /// </summary>
        public static string DonateToAssociation_Format(string username, int amount)
        {
            return string.Format(DonateToAssociation, "WWF", username, amount);
        }
        #endregion

        #region DownloadOurApp
        /// <summary>
        ///   Looks up a localized string similar to: Download our apps in {0}
        /// </summary>
        public static string DownloadOurApp => _resourceLoader.GetString("DownloadOurApp");

        /// <summary>
        ///   Format the string similar to: Download our apps in {0}
        /// </summary>
        public static string DownloadOurApp_Format()
        {
            return string.Format(DownloadOurApp, "Microsoft Store");
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

        #region ThisIsATooltip
        /// <summary>
        ///   Looks up a localized string similar to: this is a tooltip text
        /// </summary>
        public static string ThisIsATooltip => _resourceLoader.GetString("ThisIsATooltip");
        #endregion

        #region WelcomeMessageDay
        /// <summary>
        ///   Looks up a localized string similar to: Welcome {0}! Have a good {1}!
        /// </summary>
        public static string WelcomeMessageDay => _resourceLoader.GetString("WelcomeMessageDay");

        /// <summary>
        ///   Format the string similar to: Welcome {0}! Have a good {1}!
        /// </summary>
        public static string WelcomeMessageDay_Format(string username)
        {
            return string.Format(WelcomeMessageDay, username, ReswPlusLib.Macros.WeekDay);
        }
        #endregion

        #region WelcomeTitle
        /// <summary>
        ///   Looks up a localized string similar to: Hello World!
        /// </summary>
        public static string WelcomeTitle => _resourceLoader.GetString("WelcomeTitle");
        #endregion

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
            AppVersion,
            CopyrightNotice,
            DonateToAssociation,
            DownloadOurApp,
            ForecastAnnouncement,
            GotMessages,
            ThisIsATooltip,
            WelcomeMessageDay,
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
