using System;
using System.Globalization;

namespace NAPLPSApp.Converters;

public class EqualityConverter : EqualBaseConverter
{
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ConvertInternal(value, targetType, parameter, culture);
    }
}
