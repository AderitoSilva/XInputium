using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace XInputium;

/// <summary>
/// Represents a joystick that has two axes.
/// </summary>
/// <remarks>
/// <see cref="SlimJoystick"/> is a read-only structure that 
/// is a more lightweight alternative to <see cref="Joystick"/> 
/// class, at the expense of providing less features. 
/// <see cref="SlimJoystick"/> provides only the crucial 
/// features necessary to work with joysticks. Consider using 
/// <see cref="Joystick"/> class if you need more advanced 
/// features.
/// </remarks>
/// <seealso cref="Joystick"/>
/// <seealso cref="SlimTrigger"/>
[DebuggerDisplay($"{nameof(X)} = {{{nameof(X)}}}, " +
    $"{nameof(Y)} = {{{nameof(Y)}}}")]
[Serializable]
public readonly struct SlimJoystick
    : IEquatable<SlimJoystick>
{


    #region Fields

    /// <summary>
    /// A <see cref="SlimJoystick"/> object that represents a 
    /// joystick that has both axes at 0 position.
    /// </summary>
    public static readonly SlimJoystick Zero = new(0f, 0f);

    private readonly int _hashCode = default;  // Cached hash code for the current instance.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Creates a new <see cref="SlimJoystick"/> that has 
    /// the specified axes' position.
    /// </summary>
    /// <param name="x">A value between -1 and 1, that specifies 
    /// the position of the horizontal axis. If you specify a 
    /// value outside this range, it will be clamped 
    /// accordingly.</param>
    /// <param name="y">A value between -1 and 1, that specifies 
    /// the position of the vertical axis. If you specify a 
    /// value outside this range, it will be clamped 
    /// accordingly.</param>
    /// <exception cref="ArgumentException"><paramref name="x"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="y"/> 
    /// is <see cref="float.NaN"/>.</exception>
    public SlimJoystick(float x, float y)
    {
        if (float.IsNaN(x))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(x)}' parameter.",
                nameof(x));
        if (float.IsNaN(y))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(y)}' parameter.",
                nameof(y));

        X = InputMath.Clamp11(x);
        Y = InputMath.Clamp11(y);

        _hashCode = HashCode.Combine(X, Y);
    }

    #endregion Constructors


    #region Operators

    /// <summary>
    /// Determines if both specified <see cref="SlimJoystick"/> 
    /// objects are identical.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> 
    /// is identical to <paramref name="right"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(SlimJoystick left, SlimJoystick right)
    {
        return left.Equals(right);
    }


    /// <summary>
    /// Determines if both specified <see cref="SlimJoystick"/> 
    /// objects are <b>not</b> identical.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> 
    /// is <b>not</b> identical to <paramref name="right"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(SlimJoystick left, SlimJoystick right)
    {
        return !left.Equals(right);
    }

    #endregion Operators


    #region Properties

    /// <summary>
    /// Gets the position of the <see cref="SlimJoystick"/>'s 
    /// horizontal axis.
    /// </summary>
    /// <returns>A value within the -1 and 1 inclusive range.</returns>
    public float X { get; }


    /// <summary>
    /// Gets the position of the <see cref="SlimJoystick"/>'s 
    /// vertical axis.
    /// </summary>
    /// <returns>A value within the -1 and 1 inclusive range.</returns>
    public float Y { get; }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the 
    /// <see cref="SlimJoystick"/> is identical to 
    /// <see cref="Zero"/>, meaning its both axes are 
    /// at position 0.
    /// </summary>
    /// <seealso cref="Zero"/>
    public bool IsZero => Equals(Zero);

    #endregion Properties


    #region Methods

    /// <summary>
    /// Gets the <see cref="string"/> representation of the 
    /// <see cref="SlimJoystick"/>.
    /// </summary>
    /// <returns>The <see cref="string"/> representation of 
    /// the current <see cref="SlimJoystick"/>.</returns>
    public override string ToString()
    {
        return $"({X}; {Y})";
    }


    /// <summary>
    /// Gets the hash code for the <see cref="SlimJoystick"/> 
    /// instance.
    /// </summary>
    /// <returns>The computed hash code.</returns>
    public override int GetHashCode()
    {
        return _hashCode;
    }


    /// <summary>
    /// Determines if the specified <see cref="object"/> is 
    /// identical to the current <see cref="SlimJoystick"/>.
    /// </summary>
    /// <param name="obj"><see cref="object"/> instance to 
    /// compare with the current <see cref="SlimJoystick"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> 
    /// is identical to the current <see cref="SlimJoystick"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="GetHashCode()"/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;
        else if (obj is SlimJoystick joystick)
            return Equals(joystick);
        else
            return false;
    }


    /// <summary>
    /// Determines if the specified <see cref="SlimJoystick"/> is 
    /// identical to the current <see cref="SlimJoystick"/>.
    /// </summary>
    /// <param name="other"><see cref="SlimJoystick"/> object to 
    /// compare.</param>
    /// <returns><see langword="true"/> if <paramref name="other"/> 
    /// is identical to the current <see cref="SlimJoystick"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    public bool Equals(SlimJoystick other)
    {
        return X == other.X && Y == other.Y;
    }


    /// <summary>
    /// Gets the joystick angle, in radians.
    /// </summary>
    /// <returns>The angle of the joystick, in radians.</returns>
    /// <seealso cref="GetRadius()"/>
    /// <seealso cref="InputMath.ConvertToPolar(float, float, out float, out float)"/>
    /// <seealso cref="InputMath.ConvertRadiansToNormal(float)"/>
    public float GetAngle()
    {
        return MathF.Atan2(Y, X);
    }


    /// <summary>
    /// Gets the joystick radius, where the center is with both 
    /// axes at position 0.
    /// </summary>
    /// <returns>The radius of <see cref="X"/> and <see cref="Y"/> 
    /// away from the center 0.</returns>
    /// <seealso cref="GetAngle()"/>
    /// <seealso cref="InputMath.ConvertToPolar(float, float, out float, out float)"/>
    public float GetRadius()
    {
        return MathF.Sqrt((X * X) + (Y * Y));
    }


    /// <summary>
    /// Gets a <see cref="SlimJoystick"/> object that represents the 
    /// position of the current <see cref="SlimJoystick"/> with the 
    /// specified modifier functions applied to its Cartesian X and 
    /// Y coordinates.
    /// </summary>
    /// <param name="xFunction"><see cref="ModifierFunction"/> to 
    /// apply to the X coordinate; or <see langword="null"/> to 
    /// use no function.</param>
    /// <param name="yFunction"><see cref="ModifierFunction"/> to 
    /// apply to the Y coordinate; or <see langword="null"/> to 
    /// use no function.</param>
    /// <returns>The newly created <see cref="SlimJoystick"/> object.</returns>
    /// <seealso cref="ApplyPolarModifierFunctions(ModifierFunction?, ModifierFunction?)"/>
    public SlimJoystick ApplyCartesianModifierFunctions(
        ModifierFunction? xFunction, ModifierFunction? yFunction)
    {
        float x = xFunction is null ? X : InputMath.Clamp11(xFunction(X));
        float y = yFunction is null ? Y : InputMath.Clamp11(yFunction(Y));
        return new SlimJoystick(x, y);
    }


    /// <summary>
    /// Gets a <see cref="SlimJoystick"/> object that represents the 
    /// position of the current <see cref="SlimJoystick"/> with the 
    /// specified modifier functions applied to its polar coordinates.
    /// </summary>
    /// <param name="angleFunction"><see cref="ModifierFunction"/> to 
    /// apply to the normalized angle; or <see langword="null"/> to 
    /// use no function.</param>
    /// <param name="radiusFunction"><see cref="ModifierFunction"/> to 
    /// apply to the radius; or <see langword="null"/> to use no 
    /// function.</param>
    /// <returns>The newly created <see cref="SlimJoystick"/> object.</returns>
    /// <seealso cref="ApplyCartesianModifierFunctions(ModifierFunction?, ModifierFunction?)"/>
    public SlimJoystick ApplyPolarModifierFunctions(
        ModifierFunction? angleFunction, ModifierFunction? radiusFunction)
    {
        InputMath.ConvertToPolar(X, Y, out float angle, out float radius);
        angle = InputMath.ConvertRadiansToNormal(angle);

        angle = angleFunction is null ? angle : InputMath.Clamp01(angleFunction(angle));
        radius = radiusFunction is null ? radius : InputMath.Clamp01(radiusFunction(radius));

        angle = InputMath.ConvertNormalToRadians(angle);
        InputMath.ConvertToCartesian(angle, radius, out float x, out float y);
        x = InputMath.Clamp11(x);
        y = InputMath.Clamp11(y);

        return new SlimJoystick(x, y);
    }


    /// <summary>
    /// Gets a <see cref="SlimJoystick"/> object that represents the position 
    /// of the current <see cref="SlimJoystick"/> with the specified inner 
    /// and outer dead-zones applied to its radius polar coordinate.
    /// </summary>
    /// <param name="innerDeadZone">A value between the 0 and 1 inclusive range, 
    /// that specifies the inner dead-zone.</param>
    /// <param name="outerDeadZone">A value between the 0 and 1 inclusive range, 
    /// that specifies the outer dead-zone</param>
    /// <returns>The newly created <see cref="SlimJoystick"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="innerDeadZone"/>
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outerDeadZone"/>
    /// is <see cref="float.NaN"/>.</exception>
    public SlimJoystick ApplyCircularDeadZone(float innerDeadZone, float outerDeadZone)
    {
        if (float.IsNaN(innerDeadZone))
            throw new ArgumentException($"'{float.NaN}' is not a valid " +
                $"value for '{nameof(innerDeadZone)}' parameter.",
                nameof(innerDeadZone));
        if (float.IsNaN(outerDeadZone))
            throw new ArgumentException($"'{float.NaN}' is not a valid " +
                $"value for '{nameof(outerDeadZone)}' parameter.",
                nameof(outerDeadZone));

        InputMath.ConvertToPolar(X, Y, out float angle, out float radius);
        radius = InputMath.Clamp01(InputMath.ApplyDeadZone(radius, innerDeadZone, outerDeadZone));
        InputMath.ConvertToCartesian(angle, radius, out float x, out float y);
        return new SlimJoystick(x, y);
    }


    /// <summary>
    /// Gets a <see cref="SlimJoystick"/> object that represents the position 
    /// of the current <see cref="SlimJoystick"/> with the specified inner 
    /// dead-zone applied to its radius polar coordinate.
    /// </summary>
    /// <param name="innerDeadZone">A value between the 0 and 1 inclusive range, 
    /// that specifies the inner dead-zone.</param>
    /// <returns>The newly created <see cref="SlimJoystick"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="innerDeadZone"/>
    /// is <see cref="float.NaN"/>.</exception>
    public SlimJoystick ApplyCircularDeadZone(float innerDeadZone)
    {
        return ApplyCircularDeadZone(innerDeadZone);
    }


    /// <summary>
    /// Gets a <see cref="SlimJoystick"/> object that represents the 
    /// position of the current <see cref="SlimJoystick"/> with its 
    /// X axis inverted.
    /// </summary>
    /// <returns>The newly created <see cref="SlimJoystick"/>.</returns>
    /// <seealso cref="InvertY()"/>
    public SlimJoystick InvertX()
    {
        return new SlimJoystick(-X, Y);
    }


    /// <summary>
    /// Gets a <see cref="SlimJoystick"/> object that represents the 
    /// position of the current <see cref="SlimJoystick"/> with its 
    /// Y axis inverted.
    /// </summary>
    /// <returns>The newly created <see cref="SlimJoystick"/>.</returns>
    /// <seealso cref="InvertX()"/>
    public SlimJoystick InvertY()
    {
        return new SlimJoystick(X, -Y);
    }


    /// <summary>
    /// Gets a <see cref="JoystickDelta"/> object that represents the difference 
    /// between the specified <see cref="SlimJoystick"/> object and the 
    /// current <see cref="SlimJoystick"/>.
    /// </summary>
    /// <param name="sourcePosition">A <see cref="SlimJoystick"/> object that 
    /// represents the source joystick position.</param>
    /// <returns>A <see cref="JoystickDelta"/> object representing the 
    /// delta between <paramref name="sourcePosition"/> and the current 
    /// <see cref="SlimJoystick"/>. If there is no position change between 
    /// both <see cref="SlimJoystick"/> objects, <see cref="JoystickDelta.Zero"/>
    /// is returned.</returns>
    /// <seealso cref="JoystickDelta.FromJoystickPosition(SlimJoystick, SlimJoystick)"/>
    /// <seealso cref="JoystickDelta"/>
    public JoystickDelta GetDelta(SlimJoystick sourcePosition)
    {
        return JoystickDelta.FromJoystickPosition(sourcePosition, this);
    }

    #endregion Methods


}
