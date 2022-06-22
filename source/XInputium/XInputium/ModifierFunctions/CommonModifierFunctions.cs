using System;

namespace XInputium.ModifierFunctions;

/// <summary>
/// Provides static members that allow the creation of 
/// customized <see cref="ModifierFunction"/> delegates 
/// for the most common usages.
/// </summary>
/// <seealso cref="ModifierFunction"/>
public static class CommonModifierFunctions
{


    #region Properties

    /// <summary>
    /// Gets a <see cref="ModifierFunction"/> that always returns 0.
    /// </summary>
    public static ModifierFunction Zero { get; }
        = value => 0f;


    /// <summary>
    /// Gets a <see cref="ModifierFunction"/> that reverses a value, 
    /// where a 0 value will return 1 and a 1 value will return 0.
    /// </summary>
    /// <seealso cref="Negate"/>
    public static ModifierFunction Reverse { get; }
        = value => MathF.CopySign(1f - Math.Min(MathF.Abs(value), 1f), value);


    /// <summary>
    /// Gets a <see cref="ModifierFunction"/> that negates a value,
    /// where 1 returns -1 and -1 returns 1.
    /// </summary>
    /// <seealso cref="Reverse"/>
    public static ModifierFunction Negate { get; }
        = value => -value;


    /// <summary>
    /// Gets a <see cref="ModifierFunction"/> that clamps 
    /// a value within the 0 and 1 inclusive range.
    /// </summary>
    /// <seealso cref="Clamp11"/>
    public static ModifierFunction Clamp01 { get; }
        = value => InputMath.Clamp01(value);


    /// <summary>
    /// Gets a <see cref="ModifierFunction"/> that clamps 
    /// a value within the -1 and 1 inclusive range.
    /// </summary>
    /// <seealso cref="Clamp01"/>
    public static ModifierFunction Clamp11 { get; }
        = value => InputMath.Clamp11(value);

    #endregion Properties


    #region Methods

    /// <summary>
    /// Gets a <see cref="ModifierFunction"/> that scales its 
    /// input value to the specified scale.
    /// </summary>
    /// <param name="scale">A number that will be multiplied 
    /// by the function's input value.</param>
    /// <returns>A new <see cref="ModifierFunction"/> that 
    /// multiplies its input value by <paramref name="scale"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="scale"/> is <see cref="float.NaN"/>.</exception>
    public static ModifierFunction Scale(float scale)
    {
        if (float.IsNaN(scale))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(scale)}' parameter.",
                nameof(scale));

        return value => value * scale;
    }


    /// <summary>
    /// Gets a new <see cref="ModifierFunction"/> that combines 
    /// the two specified functions in sequence.
    /// </summary>
    /// <param name="function1">First function. This function's 
    /// return value will be the input value of 
    /// <paramref name="function2"/>.</param>
    /// <param name="function2">Second function, which will receive 
    /// the return value of <paramref name="function2"/>.</param>
    /// <returns>A new <see cref="ModifierFunction"/> instance that 
    /// combines <paramref name="function1"/> and 
    /// <paramref name="function2"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function1"/> is null.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function2"/> is null.</exception>
    public static ModifierFunction Combine(
        ModifierFunction function1, ModifierFunction function2)
    {
        if (function1 is null)
            throw new ArgumentNullException(nameof(function1));
        if (function2 is null)
            throw new ArgumentNullException(nameof(function2));

        return value => function2(function1(value));
    }


    /// <summary>
    /// Gets a new <see cref="ModifierFunction"/> that returns 
    /// 0 if input value is less than the specified absolute middle 
    /// or 1 if the value is equal to or greater than the specified 
    /// absolute middle or -1 if the value is less than the specified 
    /// negated absolute middle.
    /// </summary>
    /// <param name="middle">Value that determines where any input 
    /// value should start returning true.</param>
    /// <returns>A new <see cref="ModifierFunction"/> that returns 
    /// 0 if its input value is less than the absolute 
    /// <paramref name="middle"/>, 1 if the input value is greater 
    /// than the absolute <paramref name="middle"/> or -1 if the 
    /// input value is less that the negated absolute 
    /// <paramref name="middle"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="middle"/> is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="Quantize(float)"/>
    public static ModifierFunction Boolean(float middle = 0.5f)
    {
        if (float.IsNaN(middle))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for " +
                $"'{nameof(middle)}' parameter.",
                nameof(middle));
        middle = MathF.Abs(middle);

        return value => value >= middle ? 1f : value < -middle ? -1f : 0f;
    }


    /// <summary>
    /// Gets a <see cref="ModifierFunction"/> that applies the 
    /// specified inner and outer dead-zone to a value.
    /// </summary>
    /// <param name="innerDeadZone">Region from 0 where the 
    /// dead-zone will be applied.</param>
    /// <param name="outerDeadZone">Region from 1 or -1 where 
    /// the dead-zone will be applied.</param>
    /// <returns>The newly created <see cref="ModifierFunction"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="innerDeadZone"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outerDeadZone"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="InputMath.ApplyDeadZone(float, float, float)"/>
    public static ModifierFunction ApplyDeadZone(
        float innerDeadZone, float outerDeadZone)
    {
        if (float.IsNaN(innerDeadZone))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for " +
                $"'{nameof(innerDeadZone)}' parameter.",
                nameof(innerDeadZone));
        if (float.IsNaN(outerDeadZone))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for " +
                $"'{nameof(outerDeadZone)}' parameter.",
                nameof(outerDeadZone));

        return value => MathF.CopySign(MathF.Abs(
            InputMath.ApplyDeadZone(value, innerDeadZone, outerDeadZone)),
            value);
    }


    /// <summary>
    /// Gets a <see cref="ModifierFunction"/> that applies the 
    /// specified inner dead-zone to a value.
    /// </summary>
    /// <param name="innerDeadZone">Region from 0 where the 
    /// dead-zone will be applied.</param>
    /// <returns>The newly created <see cref="ModifierFunction"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="innerDeadZone"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="InputMath.ApplyDeadZone(float, float)"/>
    public static ModifierFunction ApplyDeadZone(float innerDeadZone)
    {
        return ApplyDeadZone(innerDeadZone, 0f);
    }


    /// <summary>
    /// Gets a <see cref="ModifierFunction"/> that quantizes its input 
    /// value to the specified step size.
    /// </summary>
    /// <param name="stepSize">Number by how much the quantization 
    /// is performed. If 0 is specified, no quantization is applied, 
    /// making the <see cref="ModifierFunction"/> return its input 
    /// value.</param>
    /// <returns>A <see cref="ModifierFunction"/> that quantizes its 
    /// input value to the specified <paramref name="stepSize"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="stepSize"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// The <see cref="ModifierFunction"/> returned by this method 
    /// quantizes its input to a specified step size. In other words, 
    /// its output is equal to <paramref name="stepSize"/>
    /// multiplied by the closest integer that is lower or equal to 
    /// its input. For instance, if <paramref name="stepSize"/> is 
    /// 0.25, the function's output will be a multiplier of 0.25 
    /// (ex. 0, 0.25, 0.5, 0.75, or 1).
    /// <br/><br/>
    /// The returned <see cref="ModifierFunction"/> can be useful in 
    /// scenarios where you intent to divide an axis in several even
    /// chunks. One valid example would be to use the 
    /// <see cref="ModifierFunction"/> returned by this method with 
    /// a <paramref name="stepSize"/> of 0.125 (or 1/8th) in the angle
    /// axis of a joystick. This way, the joystick's angle would always
    /// get snapped to a horizontal, vertical or diagonal direction.
    /// </remarks>
    /// <seealso cref="Boolean(float)"/>
    public static ModifierFunction Quantize(float stepSize)
    {
        if (float.IsNaN(stepSize))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for " +
                $"'{nameof(stepSize)}' parameter.",
                nameof(stepSize));
        stepSize = MathF.Abs(stepSize);

        if (stepSize == 0f)
            return value => value;
        else
            return value => MathF.Floor(value / stepSize) * stepSize;
    }

    #endregion Methods


}
