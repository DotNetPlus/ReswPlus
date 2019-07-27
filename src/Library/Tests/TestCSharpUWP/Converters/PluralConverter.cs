using ReswPlusLib;
using System;
using System.Globalization;
using System.Resources;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace TestCSharpUWP.Converters
{
    public sealed class PluralConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var culture = !string.IsNullOrWhiteSpace(language) ? new CultureInfo(language) : null;
            var key = parameter as string;
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }
            var number = System.Convert.ToDouble(value);

            var resource = ResourceLoader.GetForCurrentView();
            var pluralFormat = resource.GetPlural(key, number);

            return string.Format(culture, pluralFormat, number);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
