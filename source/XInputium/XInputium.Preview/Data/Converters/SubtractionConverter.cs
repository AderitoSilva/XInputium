using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace XInputium.Preview.Data.Converters;

public class SubtractionConverter : IMultiValueConverter
{


    public object Convert(object[] values, Type targetType,
        object parameter, CultureInfo culture)
    {
        if (!targetType.IsAssignableTo(typeof(IConvertible)))
            throw new NotSupportedException(
                $"The conversion target type must implement '{nameof(IConvertible)}'.");

        if (values is not null && values.Length > 0)
        {
            double? result = null;
            foreach (var value in values)
            {
                if (DependencyProperty.UnsetValue.Equals(value))
                {
                    continue;
                }
                if (value is IConvertible convertible)
                {
                    double doubleValue = System.Convert.ToDouble(value, culture);
                    if (result is null)
                        result = doubleValue;
                    else
                        result -= doubleValue;
                }
                else
                {
                    throw new NotSupportedException(
                        $"All values must be of a type that implements '{nameof(IConvertible)}'.");
                }
            }
            if (result is not null)
            {
                return result.Value;
            }
        }
        return 0;
    }


    public object[] ConvertBack(object value, Type[] targetTypes,
        object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }


}
