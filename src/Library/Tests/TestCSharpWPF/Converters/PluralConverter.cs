using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Data;
using ReswPlusLib;

namespace TestCSharpWPF.Converters
{
    public sealed class PluralConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = parameter as string;

            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var number = System.Convert.ToDouble(value);
            var pluralFormat = TestCSharpWPF.Resources.Resources.ResourceManager.GetPlural(key, number);

            return string.Format(culture, pluralFormat, number);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
