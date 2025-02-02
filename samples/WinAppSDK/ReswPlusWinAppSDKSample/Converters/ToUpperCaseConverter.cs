using System;
using Microsoft.UI.Xaml.Data;

namespace ReswPlusWinAppSDKSample.Converters;

internal class ToUpperCaseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is string str ? str.ToUpper() : (object)"";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
