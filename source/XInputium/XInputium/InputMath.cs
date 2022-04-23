using System;
using System.Runtime.CompilerServices;

namespace XInputium;

/// <summary>
/// Exposes static methods that can be used as math utility 
/// methods for common math operations related with input 
/// processing.
/// </summary>
public static class InputMath
{


    #region Methods

    /// <summary>
    /// Truncates the specified <see cref="float"/> value to the 0 to 1 
    /// inclusive range.
    /// </summary>
    /// <param name="value">A <see cref="float"/> value to truncate.</param>
    /// <returns>0 if <paramref name="value"/> is lower than or equal to 0, 
    /// 1 if <paramref name="value"/> is greater than or equal to 1, 
    /// or <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is <see cref="float.NaN"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp01(float value)
    {
        if (float.IsNaN(value))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(value)}' parameter.",
                nameof(value));

        return value < 0f ? 0f : value > 1f ? 1f : value;
    }


    /// <summary>
    /// Truncates the specified <see cref="float"/> value to the -1 to 1 
    /// inclusive range.
    /// </summary>
    /// <param name="value">A <see cref="float"/> value to truncate.</param>
    /// <returns>-1 if <paramref name="value"/> is lower than or equal to -1, 
    /// 1 if <paramref name="value"/> is greater than or equal to 1, 
    /// or <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is <see cref="float.NaN"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp11(float value)
    {
        if (float.IsNaN(value))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(value)}' parameter.",
                nameof(value));

        return value < -1f ? -1f : value > 1f ? 1f : value;
    }


    /// <summary>
    /// Linearly interpolates between the two specified values.
    /// </summary>
    /// <param name="x">Value on interpolation value 0.</param>
    /// <param name="y">Value on interpolation value 1</param>
    /// <param name="interpolation">A number between 0 and 1 
    /// specifying the interpolation amount to apply.</param>
    /// <returns>The result of the interpolation between 
    /// <paramref name="x"/> and <paramref name="y"/>, where an 
    /// <paramref name="interpolation"/> value of 0 would fully 
    /// return <paramref name="x"/> and 1 would return 
    /// <paramref name="y"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="x"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="y"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="interpolation"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="Clamp01(float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Interpolate(float x, float y, float interpolation)
    {
        if (float.IsNaN(x))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(x)}' parameter.",
                nameof(x));
        if (float.IsNaN(y))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(y)}' parameter.",
                nameof(y));
        if (float.IsNaN(interpolation))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(interpolation)}' parameter.",
                nameof(interpolation));
        interpolation = Clamp01(interpolation);

        return (x * (1f - interpolation)) + (y * interpolation);
    }


    /// <summary>
    /// Converts the specified Cartesian coordinates to 
    /// polar coordinates.
    /// </summary>
    /// <param name="x">X Cartesian coordinate.</param>
    /// <param name="y">Y Cartesian coordinate.</param>
    /// <param name="angle">Variable that will be set with the 
    /// converted angle coordinate, in radians.</param>
    /// <param name="radius">Variable that will be set with 
    /// the converted radius coordinate.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="x"/> is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="y"/> is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// <p>
    /// This method converts the specified <paramref name="x"/> 
    /// and <paramref name="y"/> Cartesian coordinates to their 
    /// polar representation, which are composed of the 
    /// <paramref name="angle"/> and <paramref name="radius"/>.
    /// You can convert polar coordinates back to Cartesian 
    /// coordinates using 
    /// <see cref="ConvertToCartesian(float, float, out float, out float)"/>
    /// method, which performs the inverse conversion.
    /// </p>
    /// <p>
    /// The outputted <paramref name="angle"/> is in radians.
    /// You can convert radians to the normalized angle using 
    /// <see cref="ConvertRadiansToNormal(float)"/> method and 
    /// convert that back to radians using 
    /// <see cref="ConvertNormalToRadians(float)"/> method.
    /// </p>
    /// <p>
    /// Note that neither the input nor the output coordinates 
    /// are clamped in any way. If you need to clamp them to a normal 
    /// axis range, you can use <see cref="Clamp01(float)"/> or 
    /// <see cref="Clamp11(float)"/> methods.
    /// </p>
    /// </remarks>
    /// <seealso cref="ConvertToCartesian(float, float, out float, out float)"/>
    /// <seealso cref="ConvertRadiansToNormal(float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertToPolar(float x, float y,
        out float angle, out float radius)
    {
        if (float.IsNaN(x))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(x)}' parameter.",
                nameof(x));
        if (float.IsNaN(y))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(y)}' parameter.",
                nameof(y));

        if (x == 0f && y == 0f)
        {
            angle = 0f;
            radius = 0f;
            return;
        }

        angle = MathF.Atan2(y, x);
        radius = MathF.Sqrt((x * x) + (y * y));
    }


    /// <summary>
    /// Converts the specified polar coordinates to 
    /// 2D Cartesian coordinates.
    /// </summary>
    /// <param name="angle">Input angle coordinate, in radians.</param>
    /// <param name="radius">Input radius coordinate.</param>
    /// <param name="x">Variable that will be set with the
    /// converted X Cartesian coordinate.</param>
    /// <param name="y">Variable that will be set with the
    /// converted Y Cartesian coordinate.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="angle"/> is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="radius"/> is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// <p>
    /// This method converts the specified <paramref name="angle"/> 
    /// and <paramref name="radius"/> polar coordinates to their 
    /// Cartesian representation, which are composed of the 
    /// <paramref name="x"/> and <paramref name="y"/> axes.
    /// You can convert Cartesian coordinates back to polar 
    /// coordinates using 
    /// <see cref="ConvertToPolar(float, float, out float, out float)"/>
    /// method, which performs the inverse conversion.
    /// </p>
    /// <p>
    /// The input <paramref name="angle"/> is in radians.
    /// You can convert a normalized angle to radians using 
    /// <see cref="ConvertNormalToRadians(float)"/> method and 
    /// convert that back to normalized angle using 
    /// <see cref="ConvertRadiansToNormal(float)"/> method.
    /// </p>
    /// <p>
    /// Note that neither the input nor the output coordinates 
    /// are clamped in any way. If you need to clamp them to a normal 
    /// axis range, you can use <see cref="Clamp01(float)"/> or 
    /// <see cref="Clamp11(float)"/> methods.
    /// </p>
    /// </remarks>
    /// <seealso cref="ConvertToPolar(float, float, out float, out float)"/>
    /// <seealso cref="ConvertNormalToRadians(float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertToCartesian(float angle, float radius,
        out float x, out float y)
    {
        if (float.IsNaN(angle))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(angle)}' parameter.",
                nameof(angle));
        if (float.IsNaN(radius))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(radius)}' parameter.",
                nameof(radius));

        if (angle == 0f && radius == 0f)
        {
            x = 0f;
            y = 0f;
            return;
        }

        x = radius * MathF.Cos(angle);
        y = radius * MathF.Sin(angle);
    }


    /// <summary>
    /// Converts the specified angle from radians to a normalized 
    /// 0-1 ranged clockwise angle.
    /// </summary>
    /// <param name="angle">Angle to convert, in radians.</param>
    /// <returns>A number between 0 and 1 representing the normalized 
    /// <paramref name="angle"/>. See remarks for more details.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="angle"/> is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// This method takes in an angle in radians and returns it 
    /// converted to its normalized version. In the context of input 
    /// axes, within <see cref="InputMath"/>, a normalized angle is 
    /// a number between 0 and 1 that is analogous to a quartz clock.
    /// In this analogy, a normalized angle of 0 is analogous to 
    /// 12 o'clock, while the normalized angle increases towards 1 
    /// in clockwise orientation and an angle of 1 is analogous to 
    /// 12 o'clock, completing a full turn. In this case, this method 
    /// assumes <paramref name="angle"/> represents an angle that can 
    /// be represented in Cartesian coordinates with a bottom-up Y axis.
    /// <br/><br/>
    /// The normalized angle can be useful for easy of conversion 
    /// to other angular units.
    /// </remarks>
    /// <see cref="ConvertNormalToRadians(float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertRadiansToNormal(float angle)
    {
        if (float.IsNaN(angle))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(angle)}' parameter.",
                nameof(angle));

        angle = -0.25f + angle / MathF.PI / 2;
        angle = angle < 0.0f ? -angle : 1f - angle;
        return angle -= MathF.Floor(angle);
    }


    /// <summary>
    /// Converts the specified normalized angle, produced by 
    /// <see cref="ConvertRadiansToNormal(float)"/> method, 
    /// back to radians.
    /// </summary>
    /// <param name="normalAngle">A <see cref="float"/> number 
    /// representing the normalized angle.</param>
    /// <returns>The representation of <paramref name="normalAngle"/> 
    /// in radians.</returns>
    /// <exception cref="ArgumentException"><paramref name="normalAngle"/> 
    /// is <see cref="float.NaN"/>, <see cref="float.PositiveInfinity"/> 
    /// or <see cref="float.NegativeInfinity"/>.</exception>
    /// <remarks>
    /// This method assumes <paramref name="normalAngle"/> was 
    /// previously converted from radians using 
    /// <see cref="ConvertRadiansToNormal(float)"/>, and is the 
    /// complementary convert-back method to that method. See the
    /// remarks section of 
    /// <see cref="ConvertRadiansToNormal(float)"/> method for 
    /// more information about normalized angles.
    /// </remarks>
    /// <seealso cref="ConvertRadiansToNormal(float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertNormalToRadians(float normalAngle)
    {
        if (float.IsNaN(normalAngle) || float.IsInfinity(normalAngle))
            throw new ArgumentException(
                $"'{normalAngle}' is not a valid value for " +
                $"'{nameof(normalAngle)}' parameter.",
                nameof(normalAngle));
        normalAngle -= MathF.Floor(MathF.Abs(normalAngle));

        normalAngle = normalAngle < 0.75f ? -normalAngle + 0.25f : 1f - normalAngle + 0.25f;
        normalAngle *= MathF.Tau;
        return normalAngle;
    }


    /// <summary>
    /// Applies a dead-zone to the specified axis inner and outer 
    /// edges.
    /// </summary>
    /// <param name="value">A number between -1 and 1, representing 
    /// the position of an axis.</param>
    /// <param name="innerDeadZone">A value between 0 and 1 
    /// representing the inner portion of the axis that is in 
    /// the dead-zone.</param>
    /// <param name="outerDeadZone">A value between 0 and 1 
    /// representing the outer portion of the axis that is in 
    /// the dead-zone.</param>
    /// <returns>A number between 0 and 1, if <paramref name="value"/> 
    /// is positive, or between -1 and 0 if <paramref name="value"/> is 
    /// negative, which results from applying the inner and outer 
    /// dead-zones to <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="innerDeadZone"/> is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="outerDeadZone"/> is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// Dead-zone, in the context of a controller axis, represents 
    /// the portion of the axis that will be ignored. In this case,
    /// the dead-zone includes two portions of an axis, with the inner 
    /// portion ranging from the beginning of the axis (its 0 position) to 
    /// the value of <paramref name="innerDeadZone"/>, and the outer 
    /// portion ranging from the value of <paramref name="outerDeadZone"/> 
    /// to the end of the axis (its 1 position). After these edge portions 
    /// are excluded from the axis, the remaining space of the axis is 
    /// represented in a range from 0 to 1 or from -1 to 0, depending on 
    /// whether <paramref name="value"/> is positive or negative, 
    /// respectively, and that is the value returned by this method. 
    /// If <paramref name="value"/> is greater than 1 or lower than -1, 
    /// it will be clamped accordingly to stay in the -1 to 1 range 
    /// before the dead-zones are applied.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ApplyDeadZone(float value,
        float innerDeadZone, float outerDeadZone)
    {
        if (float.IsNaN(value))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(value)}' parameter.",
                nameof(value));
        if (float.IsNaN(innerDeadZone))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(innerDeadZone)}' parameter.",
                nameof(innerDeadZone));
        if (float.IsNaN(outerDeadZone))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(outerDeadZone)}' parameter.",
                nameof(outerDeadZone));

        bool isNegativeValue = value < 0f;
        value = Clamp01(MathF.Abs(value));
        innerDeadZone = Clamp01(MathF.Abs(innerDeadZone));
        outerDeadZone = Clamp01(MathF.Abs(outerDeadZone));

        if (value > 0f && (innerDeadZone > 0f || outerDeadZone > 0f))
        {
            if (innerDeadZone + outerDeadZone < 1f) // This check is to prevent potential division by 0.
                value = Clamp01((value - innerDeadZone) / (1f - innerDeadZone - outerDeadZone));
            else
                return 0f;  // We have full dead-zone, so the value will always be 0.
        }
        return isNegativeValue ? -value : value;
    }


    /// <summary>
    /// Applies a dead-zone to the specified axis inner edge.
    /// </summary>
    /// <param name="value">A number between -1 and 1, representing 
    /// the position of an axis.</param>
    /// <param name="innerDeadZone">A value between 0 and 1 
    /// representing the inner portion of the axis that is in 
    /// the dead-zone.</param>
    /// <returns>A number between 0 and 1, if <paramref name="value"/> 
    /// is positive, or between -1 and 0 if <paramref name="value"/> is 
    /// negative, which results from applying the inner dead-zone to 
    /// <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="innerDeadZone"/> is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// Dead-zone, in the context of a controller axis, represents 
    /// the portion of the axis that will be ignored. In this case,
    /// the dead-zone includes one portions of an axis — the inner portion, 
    /// which ranges from the beginning of the axis (its 0 position) to 
    /// the value of <paramref name="innerDeadZone"/>. After this portion 
    /// is excluded from the axis, the remaining space of the axis is 
    /// represented in a range from 0 to 1 or from -1 to 0, depending on 
    /// whether <paramref name="value"/> is positive or negative, 
    /// respectively, and that is the value returned by this method. 
    /// If <paramref name="value"/> is greater than 1 or lower than -1, 
    /// it will be clamped accordingly to stay in the -1 to 1 range 
    /// before the dead-zone is applied.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ApplyDeadZone(float value, float innerDeadZone)
    {
        return ApplyDeadZone(value, innerDeadZone, 0f);
    }

    #endregion Methods


}
