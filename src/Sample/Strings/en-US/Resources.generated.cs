// File generated automatically by ReswPlus. https://github.com/DotNetPlus/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Data;

namespace ReswPlusSample.Strings{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("DotNetPlus.ReswPlus", "2.1.2")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public static class Resources {
        private static ResourceLoader _resourceLoader;
        static Resources()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        }

        #region AnimalTreat
        /// <summary>
        ///   Get the pluralized version of the string similar to: Reward your pup, give {0} biscuit to {2} the dog!
        /// </summary>
        public static string AnimalTreat(double treatNumber, object petType, string petName)
        {
            try
            {
                return AnimalTreat(treatNumber, Convert.ToInt64(petType), petName);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        ///   Get the pluralized version of the string similar to: Reward your pup, give {0} biscuit to {2} the dog!
        /// </summary>
        public static string AnimalTreat(double treatNumber, long petType, string petName)
        {
            return string.Format(ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "AnimalTreat_Variant" + petType, treatNumber, false), treatNumber, petType, petName);
        }
        #endregion

        #region DriverArrived
        /// <summary>
        ///   Get the variant version of the string similar to: Your driver is here, she is waiting outside. Do you want to message her?
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

        /// <summary>
        ///   Get the variant version of the string similar to: Your driver is here, she is waiting outside. Do you want to message her?
        /// </summary>
        public static string DriverArrived(long variantId)
        {
            return _resourceLoader.GetString("DriverArrived_Variant" + variantId);
        }
        #endregion

        #region FileShared
        /// <summary>
        ///   Get the pluralized version of the string similar to: {0} shared {1} photo from {2}
        /// </summary>
        public static string FileShared(string username, double numberPics, string city)
        {
            return string.Format(ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "FileShared", numberPics, false), username, numberPics, city);
        }
        #endregion

        #region Greeting
        /// <summary>
        ///   Get the variant version of the string similar to: Good morning, enjoy your day!
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

        /// <summary>
        ///   Get the variant version of the string similar to: Good morning, enjoy your day!
        /// </summary>
        public static string Greeting(long variantId)
        {
            return _resourceLoader.GetString("Greeting_Variant" + variantId);
        }
        #endregion

        #region MinutesLeft
        /// <summary>
        ///   Get the pluralized version of the string similar to: {0} minute left
        /// </summary>
        public static string MinutesLeft(double pluralCount)
        {
            return string.Format(ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "MinutesLeft", pluralCount, false), pluralCount);
        }
        #endregion

        #region PluralizationTest
        /// <summary>
        ///   Get the pluralized version of the string similar to: This is the singular form
        /// </summary>
        public static string PluralizationTest(double pluralizationReferenceNumber)
        {
            return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "PluralizationTest", pluralizationReferenceNumber, false);
        }
        #endregion

        #region ReceivedMessages
        /// <summary>
        ///   Get the pluralized version of the string similar to: No new messages from {1}
        /// </summary>
        public static string ReceivedMessages(double pluralCount, string paramString2)
        {
            return string.Format(ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "ReceivedMessages", pluralCount, true), pluralCount, paramString2);
        }
        #endregion

        #region AndroidApplicationName
        /// <summary>
        ///   Looks up a localized string similar to: Contoso for Android
        /// </summary>
        public static string AndroidApplicationName
        {
            get
            {
                return _resourceLoader.GetString("AndroidApplicationName");
            }
        }
        #endregion

        #region AppVersion
        /// <summary>
        ///   Looks up a localized string similar to: version: {0} v{1}
        /// </summary>
        public static string AppVersion
        {
            get
            {
                return string.Format(_resourceLoader.GetString("AppVersion"), ReswPlusLib.Macros.ApplicationName, ReswPlusLib.Macros.AppVersionFull);
            }
        }
        #endregion

        #region CopyrightNotice
        /// <summary>
        ///   Looks up a localized string similar to: © {0} - {1}. All rights reserved.
        /// </summary>
        public static string CopyrightNotice
        {
            get
            {
                return string.Format(_resourceLoader.GetString("CopyrightNotice"), ReswPlusLib.Macros.Year, ReswPlusLib.Macros.PublisherName);
            }
        }
        #endregion

        #region DonateToAssociation
        /// <summary>
        ///   Looks up a localized string similar to: Hey {1}, donate {2:C2} to {0}!
        /// </summary>
        public static string DonateToAssociation(string username, int amount)
        {
            return string.Format(_resourceLoader.GetString("DonateToAssociation"), "WWF", username, amount);
        }
        #endregion

        #region DownloadAndroidApp
        /// <summary>
        ///   Looks up a localized string similar to: Click here to download {0}!
        /// </summary>
        public static string DownloadAndroidApp
        {
            get
            {
                return string.Format(_resourceLoader.GetString("DownloadAndroidApp"), AndroidApplicationName);
            }
        }
        #endregion

        #region DownloadOurApp
        /// <summary>
        ///   Looks up a localized string similar to: Download our apps in {0}
        /// </summary>
        public static string DownloadOurApp
        {
            get
            {
                return string.Format(_resourceLoader.GetString("DownloadOurApp"), "Microsoft Store");
            }
        }
        #endregion

        #region ForecastAnnouncement
        /// <summary>
        ///   Looks up a localized string similar to: The current temperature in {2} is {0}°F ({1}°C)
        /// </summary>
        public static string ForecastAnnouncement(int tempFahrenheit, int tempCelsius, string city)
        {
            return string.Format(_resourceLoader.GetString("ForecastAnnouncement"), tempFahrenheit, tempCelsius, city);
        }
        #endregion

        #region GotMessages
        /// <summary>
        ///   Looks up a localized string similar to: Welcome {0}, you got {1} emails!
        /// </summary>
        public static string GotMessages(string paramString1, uint paramUint2)
        {
            return string.Format(_resourceLoader.GetString("GotMessages"), paramString1, paramUint2);
        }
        #endregion

        #region HelloUsername
        /// <summary>
        ///   Looks up a localized string similar to: Hello <b>{0}</b>, have a good {1}?
        /// </summary>
        public static string HelloUsername(string username)
        {
            return string.Format(_resourceLoader.GetString("HelloUsername"), username, ReswPlusLib.Macros.WeekDay);
        }
        #endregion

        #region LearnMoreAboutAndroidApp
        /// <summary>
        ///   Looks up a localized string similar to: To learn more about '{0}', visit our website.
        /// </summary>
        public static string LearnMoreAboutAndroidApp
        {
            get
            {
                return string.Format(_resourceLoader.GetString("LearnMoreAboutAndroidApp"), AndroidApplicationName);
            }
        }
        #endregion

        #region PleaseInstallNetFramework
        /// <summary>
        ///   Looks up a localized string similar to: Please <b>Install <i>.Net Framework 4.6</i></b> first.
        /// </summary>
        public static string PleaseInstallNetFramework
        {
            get
            {
                return _resourceLoader.GetString("PleaseInstallNetFramework");
            }
        }
        #endregion

        #region SendDocumentToUser
        /// <summary>
        ///   Looks up a localized string similar to: Send <i>{0}</i> to <b>{1}</b>.
        /// </summary>
        public static string SendDocumentToUser(string filename, string username)
        {
            return string.Format(_resourceLoader.GetString("SendDocumentToUser"), filename, username);
        }
        #endregion

        #region ThisIsATooltip
        /// <summary>
        ///   Looks up a localized string similar to: this is a tooltip text
        /// </summary>
        public static string ThisIsATooltip
        {
            get
            {
                return _resourceLoader.GetString("ThisIsATooltip");
            }
        }
        #endregion

        #region UseDesktopBridge
        /// <summary>
        ///   Looks up a localized string similar to: Use <strike>Centennial</strike> Desktop Bridge to convert your app
        /// </summary>
        public static string UseDesktopBridge
        {
            get
            {
                return _resourceLoader.GetString("UseDesktopBridge");
            }
        }
        #endregion

        #region WelcomeDownloadApp
        /// <summary>
        ///   Looks up a localized string similar to: Hi {0}! Do you want to download {1}?
        /// </summary>
        public static string WelcomeDownloadApp(string username)
        {
            return string.Format(_resourceLoader.GetString("WelcomeDownloadApp"), username, AndroidApplicationName);
        }
        #endregion

        #region WelcomeMessageDay
        /// <summary>
        ///   Looks up a localized string similar to: Welcome {0}! Have a good {1}!
        /// </summary>
        public static string WelcomeMessageDay(string username)
        {
            return string.Format(_resourceLoader.GetString("WelcomeMessageDay"), username, ReswPlusLib.Macros.WeekDay);
        }
        #endregion

        #region WelcomeTitle
        /// <summary>
        ///   Looks up a localized string similar to: Hello World!
        /// </summary>
        public static string WelcomeTitle
        {
            get
            {
                return _resourceLoader.GetString("WelcomeTitle");
            }
        }
        #endregion

        #region YourAgeAndName
        /// <summary>
        ///   Looks up a localized string similar to: Your are {0}yo and named {1}!
        /// </summary>
        public static string YourAgeAndName(double paramDouble1, string paramString2)
        {
            return string.Format(_resourceLoader.GetString("YourAgeAndName"), paramDouble1, paramString2);
        }
        #endregion
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("DotNetPlus.ReswPlus", "2.1.2")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public partial class ResourcesExtension: MarkupExtension
    {
        public enum KeyEnum
        {
            __Undefined = 0,
            AnimalTreat,
            DriverArrived,
            FileShared,
            Greeting,
            MinutesLeft,
            PluralizationTest,
            ReceivedMessages,
            AndroidApplicationName,
            AppVersion,
            CopyrightNotice,
            DonateToAssociation,
            DownloadAndroidApp,
            DownloadOurApp,
            ForecastAnnouncement,
            GotMessages,
            HelloUsername,
            LearnMoreAboutAndroidApp,
            PleaseInstallNetFramework,
            SendDocumentToUser,
            ThisIsATooltip,
            UseDesktopBridge,
            WelcomeDownloadApp,
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
