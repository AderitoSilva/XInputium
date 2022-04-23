using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using XInputium.Preview.Ui.Controls;

namespace XInputium.Preview.Data.Converters;

/// <summary>
/// Converts the value of an outer dead-zone to and from 
/// a range ending value.
/// </summary>
/// <remarks>
/// This converter is intended to allow the value of a 
/// joystick or trigger outer dead-zone to be bound to 
/// a <see cref="RangeSlider.To"/> property of a 
/// <see cref="RangeSlider"/> control.
/// </remarks>
public class OuterDeadZoneConverter : IValueConverter
{


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));
        if (!targetType.IsAssignableTo(typeof(IConvertible)))
            throw new NotSupportedException(
                $"'{nameof(targetType)}' must be an '{nameof(IConvertible)}' derived type.");

        if (value is IConvertible convertible)
        {
            double dValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            dValue = 1d - dValue;  // To convert back, we will use the same operation. Ex.: 1-0.1=0.9; 1-0.9=0.1;
            return System.Convert.ChangeType(dValue, targetType, culture);
        }
        else if (DependencyProperty.UnsetValue.Equals(value))
        {
            return System.Convert.ChangeType(0d, targetType, culture);
        }

        throw new NotSupportedException($"Conversion from '{nameof(value)}' is not supported.");
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Thanks to the very specific arithmetic characteristics of the 
        // problem, we can convert back exactly the same way we converted.
        return Convert(value, targetType, parameter, culture);
    }


}
