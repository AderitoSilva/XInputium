using System;
using System.Globalization;
using System.Windows.Data;

namespace XInputium.Preview.Data.Converters;

/// <summary>
/// Implements an <see cref="IValueConverter"/> that converts 
/// between <see cref="TimeSpan"/> and <see cref="IConvertible"/> 
/// types, using the <see cref="TimeSpan"/>'s total milliseconds 
/// as time unit.
/// </summary>
/// <seealso cref="IValueConverter"/>
/// <seealso cref="TimeSpan"/>
/// <seealso cref="IConvertible"/>
public class TimeSpanMillisecondsConverter : IValueConverter
{


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));
        if (!targetType.IsAssignableTo(typeof(IConvertible)))
            throw new NotSupportedException(
                $"'{nameof(targetType)}' must represent a type that " +
                $"implements '{nameof(IConvertible)}' interface.");
        if (value is not TimeSpan time)
            throw new NotSupportedException($"'{nameof(value)}' must be of type" +
                $" '{typeof(TimeSpan).FullName}'.");

        return System.Convert.ChangeType(time.TotalMilliseconds, targetType, culture);
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IConvertible convertible)
            throw new NotSupportedException(
                $"'{nameof(value)}' must implement '{nameof(IConvertible)}' interface.");
        if (!targetType.IsAssignableTo(typeof(TimeSpan)))
            throw new NotSupportedException(
                $"'{nameof(targetType)}' must represent a '{typeof(TimeSpan).FullName}' type.");

        double milliseconds = System.Convert.ToDouble(convertible, culture);
        if (double.IsNaN(milliseconds))
            throw new ArgumentException(
                $"'{nameof(value)}' cannot be '{double.NaN}'.");

        return TimeSpan.FromMilliseconds(milliseconds);
    }


}
