using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

namespace PPgram_desktop.Core;

[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BoolToColorConverter : IValueConverter
{
    public SolidColorBrush TrueValue { get; set; }
    public SolidColorBrush FalseValue { get; set; }

    public BoolToColorConverter()
    {
        TrueValue = new SolidColorBrush(Colors.LightGreen);
        FalseValue = new SolidColorBrush(Colors.PaleVioletRed);
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!(value is bool))
            return null;
        return (bool)value ? TrueValue : FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Equals(value, TrueValue))
            return true;
        if (Equals(value, FalseValue))
            return false;
        return null;
    }
}
