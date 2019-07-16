using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
#if WINDOWS_UWP
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
#endif
#if DOTNETCORE
using System.Reflection;
#endif

namespace ReswPlusLib.Shared
{
    public sealed class MacroProvider
    {
        #region ShortDate
        private static string _shortDate = null;
        public static string ShortDate
        {
            get
            {
                if (_shortDate == null)
                {
                    _shortDate = DateTime.Now.ToString("d");
                }
                return _shortDate;
            }
        }
        #endregion

        #region LongDate
        private static string _longDate = null;
        public static string LongDate
        {
            get
            {
                if (_longDate == null)
                {
                    _longDate = DateTime.Now.ToString("D");
                }
                return _longDate;
            }
        }
        #endregion

        #region ShortTime
        private static string _shortTime = null;
        public static string ShortTime
        {
            get
            {
                if (_shortTime == null)
                {
                    _shortTime = DateTime.Now.ToString("t");
                }
                return _shortTime;
            }
        }
        #endregion

        #region LongTime
        private static string _longTime = null;
        public static string LongTime
        {
            get
            {
                if (_longTime == null)
                {
                    _longTime = DateTime.Now.ToString("T");
                }
                return _longTime;
            }
        }
        #endregion

        #region WeekDay
        private static string _weekDay = null;
        public static string WeekDay
        {
            get
            {
                if (_weekDay == null)
                {
                    _weekDay = DateTime.Now.ToString("dddd");
                }
                return _weekDay;
            }
        }
        #endregion

        #region Year
        private static string _year = null;
        public static string Year
        {
            get
            {
                if (_year == null)
                {
                    _year = DateTime.Now.ToString("yyyy");
                }
                return _year;
            }
        }
        #endregion


        #region YearTwoDigits
        private static string _yearTwoDigits = null;
        public static string YearTwoDigits
        {
            get
            {
                if (_yearTwoDigits == null)
                {
                    _yearTwoDigits = DateTime.Now.ToString("y");
                }
                return _yearTwoDigits;
            }
        }
        #endregion

        #region LocaleName
        private static string _localeName = null;
        public static string LocaleName
        {
            get
            {
                if (_localeName == null)
                {
                    _localeName = CultureInfo.CurrentUICulture.DisplayName;
                }
                return _localeName;
            }
        }
        #endregion

        #region LocaleId
        private static string _localeId = null;
        public static string LocaleId
        {
            get
            {
                if (_localeId == null)
                {
                    _localeId = CultureInfo.CurrentUICulture.Name;
                }
                return _localeId;
            }
        }
        #endregion

        #region LocaleTwoLetters
        private static string _localeTwoLetters = null;
        public static string LocaleTwoLetters
        {
            get
            {
                if (_localeTwoLetters == null)
                {
                    _localeTwoLetters = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                }
                return _localeTwoLetters;
            }
        }
        #endregion


#if WINDOWS_UWP || DOTNETCORE
        #region App Version: Full
        private static string _appVersionFull = null;
        public static string AppVersionFull
        {
            get
            {
                if (_appVersionFull == null)
                {
#if WINDOWS_UWP
                    var version = Package.Current.Id.Version;
#endif
#if DOTNETCORE
                    var version = Assembly.GetEntryAssembly().GetName().Version;
#endif
                    _appVersionFull = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
                return _appVersionFull;
            }
        }
        #endregion
#endif

#if WINDOWS_UWP || DOTNETCORE
        #region App Version: Major.Minor.Build
        private static string _appVersionMajorMinorBuild = null;
        public static string AppVersionMajorMinorBuild
        {
            get
            {
                if (_appVersionMajorMinorBuild == null)
                {
#if WINDOWS_UWP
                    var version = Package.Current.Id.Version;
#endif
#if DOTNETCORE
                    var version = Assembly.GetEntryAssembly().GetName().Version;
#endif
                    _appVersionMajorMinorBuild = $"{version.Major}.{version.Minor}.{version.Build}";
                }
                return _appVersionMajorMinorBuild;
            }
        }
        #endregion
#endif

#if WINDOWS_UWP || DOTNETCORE
        #region App Version: Major.Minor
        private static string _appVersionMajorMinor = null;
        public static string AppVersionMajorMinor
        {
            get
            {
                if (_appVersionMajorMinor == null)
                {
#if WINDOWS_UWP
                    var version = Package.Current.Id.Version;
#endif
#if DOTNETCORE
                    var version = Assembly.GetEntryAssembly().GetName().Version;
#endif
                    _appVersionMajorMinor = $"{version.Major}.{version.Minor}";
                }
                return _appVersionMajorMinor;
            }
        }
        #endregion
#endif

#if WINDOWS_UWP || DOTNETCORE
        #region App Version: Major
        private static string _appVersionMajor = null;
        public static string AppVersionMajor
        {
            get
            {
                if (_appVersionMajor == null)
                {
#if WINDOWS_UWP
                    var version = Package.Current.Id.Version;
#endif
#if DOTNETCORE
                    var version = Assembly.GetEntryAssembly().GetName().Version;
#endif
                    _appVersionMajor = version.Major.ToString();
                }
                return _appVersionMajor;
            }
        }
        #endregion
#endif

#if WINDOWS_UWP
        #region Architecture
        private static string _architecture = null;
        public static string Architecture
        {
            get
            {
                if (_architecture == null)
                {
                    switch (Package.Current.Id.Architecture)
                    {
                        case Windows.System.ProcessorArchitecture.X86:
                        case Windows.System.ProcessorArchitecture.X86OnArm64:
                            _architecture = "x86";
                            break;
                        case Windows.System.ProcessorArchitecture.X64:
                            _architecture = "x64";
                            break;
                        case Windows.System.ProcessorArchitecture.Arm:
                            _architecture = "ARM";
                            break;
                        case Windows.System.ProcessorArchitecture.Arm64:
                            _architecture = "ARM64";
                            break;
                        case Windows.System.ProcessorArchitecture.Neutral:
                            _architecture = "Any";
                            break;
                        case Windows.System.ProcessorArchitecture.Unknown:
                            _architecture = "Unknown";
                            break;
                    }
                }
                return _architecture;
            }
        }
        #endregion
#endif

#if WINDOWS_UWP || DOTNETCORE
        #region ApplicationName
        private static string _applicationName = null;
        public static string ApplicationName
        {
            get
            {
                if (_applicationName == null)
                {
#if WINDOWS_UWP
                    _applicationName = Package.Current.Id.Name;
#endif
#if DOTNETCORE
                    _applicationName = Assembly.GetEntryAssembly().GetName().Name;
#endif
                }
                return _applicationName;
            }
        }
        #endregion
#endif

#if WINDOWS_UWP
        #region DeviceFamily
        private static string _deviceFamily = null;
        public static string DeviceFamily
        {
            get
            {
                if (_deviceFamily == null)
                {
                    _deviceFamily = AnalyticsInfo.VersionInfo.DeviceFamily;
                }
                return _deviceFamily;
            }
        }
        #endregion
#endif

#if WINDOWS_UWP
        #region DeviceManufacturer
        private static string _deviceManufacturer = null;
        public static string DeviceManufacturer
        {
            get
            {
                if (_deviceManufacturer == null)
                {
                    _deviceManufacturer = new EasClientDeviceInformation().SystemManufacturer;
                }
                return _deviceManufacturer;
            }
        }
        #endregion
#endif

#if WINDOWS_UWP
        #region DeviceModel
        private static string _deviceModel = null;
        public static string DeviceModel
        {
            get
            {
                if (_deviceModel == null)
                {
                    _deviceModel = new EasClientDeviceInformation().SystemProductName;
                }
                return _deviceModel;
            }
        }
        #endregion
#endif

#if WINDOWS_UWP
        #region OperatingSystem
        private static string _operatingSystem = null;
        public static string OperatingSystem
        {
            get
            {
                if (_operatingSystem == null)
                {
                    _operatingSystem = new EasClientDeviceInformation().OperatingSystem;
                }
                return _operatingSystem;
            }
        }
        #endregion
#endif
    }
}
