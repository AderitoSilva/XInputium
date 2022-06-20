using System;
using System.Diagnostics.CodeAnalysis;

namespace XInputium;

/// <summary>
/// Represents the difference between two joystick positions,
/// providing a way to determine a joystick's movement.
/// </summary>
/// <seealso cref="SlimJoystick"/>
/// <seealso cref="Joystick"/>
[Serializable]
public readonly struct JoystickDelta
    : IEquatable<JoystickDelta>
{


    #region Fields

    /// <summary>
    /// A <see cref="JoystickDelta"/> object that represents 
    /// no joystick movement, having a relative position of 0, 0.
    /// </summary>
    /// <seealso cref="HasMoved"/>
    public static readonly JoystickDelta Zero = new(0f, 0f);

    private readonly int _hashCode;  // Cached hash code for the current instance.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Creates a new <see cref="JoystickDelta"/> object that has the 
    /// specified delta coordinates.
    /// </summary>
    /// <param name="x">Target position of the joystick's X axis, 
    /// relative to its source position.</param>
    /// <param name="y">Target position of the joystick's Y axis, 
    /// relative to its source position.</param>
    /// <exception cref="ArgumentException"><paramref name="x"/> is 
    /// <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="y"/> is 
    /// <see cref="float.NaN"/>.</exception>
    /// <seealso cref="FromJoystickPosition(float, float, float, float)"/>
    public JoystickDelta(float x, float y)
    {
        if (float.IsNaN(x))
            throw new ArgumentException(
                $"'{nameof(x)}' cannot be {float.NaN}.",
                nameof(x));
        if (float.IsNaN(y))
            throw new ArgumentException(
                $"'{nameof(y)}' cannot be {float.NaN}.",
                nameof(y)); ;


        x = Math.Clamp(x, -2f, 2f);
        y = Math.Clamp(y, -2f, 2f);
        X = x;
        Y = y;

        if (x == 0f && y == 0f)
        {
            Angle = 0f;
            Distance = 0f;
            Direction = JoystickDirection.None;
        }
        else
        {
            InputMath.ConvertToPolar(X, Y, out float angle, out float distance);
            angle = InputMath.ConvertRadiansToNormal(angle);
            Angle = angle;
            Distance = distance;
            Direction = Joystick.ConvertNormalAngleToJoystickDirection(Angle);
        }

        _hashCode = HashCode.Combine(X, Y);
    }

    #endregion Constructors


    #region Operators

    /// <summary>
    /// Determines if both <see cref="JoystickDelta"/> objects are identical.
    /// </summary>
    /// <param name="left">Left <see cref="JoystickDelta"/> operand.</param>
    /// <param name="right">Right <see cref="JoystickDelta"/> operand.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is identical 
    /// to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(JoystickDelta left, JoystickDelta right)
    {
        return left.Equals(right);
    }


    /// <summary>
    /// Determines if both <see cref="JoystickDelta"/> objects differ.
    /// </summary>
    /// <param name="left">Left <see cref="JoystickDelta"/> operand.</param>
    /// <param name="right">Right <see cref="JoystickDelta"/> operand.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> differs from 
    /// <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(JoystickDelta left, JoystickDelta right)
    {
        return !left.Equals(right);
    }

    #endregion Operators


    #region Properties

    /// <summary>
    /// Gets the target position of the joystick's X axis, relative to 
    /// its source position. This is by how much the joystick's X axis 
    /// has moved.
    /// </summary>
    /// <seealso cref="Y"/>
    public float X { get; }


    /// <summary>
    /// Gets the target position of the joystick's Y axis, relative to 
    /// its source position. This is by how much the joystick's Y axis 
    /// has moved.
    /// </summary>
    /// <seealso cref="X"/>
    public float Y { get; }


    /// <summary>
    /// Gets the angle towards which the joystick has moved, relative 
    /// to its source position.
    /// </summary>
    /// <seealso cref="Distance"/>
    /// <seealso cref="Direction"/>
    public float Angle { get; }


    /// <summary>
    /// Gets the distance the joystick has moved, from its source position 
    /// to its target position.
    /// </summary>
    /// <seealso cref="Angle"/>
    public float Distance { get; }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the joystick 
    /// has moved, considering its delta.
    /// </summary>
    /// <seealso cref="Zero"/>
    /// <seealso cref="Distance"/>
    public bool HasMoved => X != 0f || Y != 0f;


    /// <summary>
    /// Gets a <see cref="JoystickDirection"/> constant that 
    /// indicates the direction to which the joystick was moved.
    /// </summary>
    /// <seealso cref="Angle"/>
    public JoystickDirection Direction { get; }

    #endregion Properties


    #region Methods

    /// <summary>
    /// Gets a <see cref="JoystickDelta"/> object that represents the 
    /// movement delta between the specified source and target 
    /// positions of a joystick.
    /// </summary>
    /// <param name="sourceX">A number between -1 and 1, representing 
    /// the source position of the X axis of the joystick.</param>
    /// <param name="sourceY">A number between -1 and 1, representing 
    /// the source position of the Y axis of the joystick.</param>
    /// <param name="targetX">A number between -1 and 1, representing 
    /// the target position of the X axis of the joystick.</param>
    /// <param name="targetY">A number between -1 and 1, representing 
    /// the target position of the Y axis of the joystick.</param>
    /// <returns>A <see cref="JoystickDelta"/> object that represents 
    /// the difference between the specified source and target 
    /// positions. If there is no difference between the source and 
    /// target positions, <see cref="Zero"/> is returned.</returns>
    /// <exception cref="ArgumentException"><paramref name="sourceX"/>
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sourceY"/>
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="targetX"/>
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="targetY"/>
    /// is <see cref="float.NaN"/>.</exception>
    public static JoystickDelta FromJoystickPosition(
        float sourceX, float sourceY,
        float targetX, float targetY)
    {
        // Validate parameters.
        if (float.IsNaN(sourceX))
            throw new ArgumentException(
                $"'{nameof(sourceX)}' cannot be {float.NaN}",
                nameof(sourceX));
        if (float.IsNaN(sourceY))
            throw new ArgumentException(
                $"'{nameof(sourceY)}' cannot be {float.NaN}",
                nameof(sourceY));
        if (float.IsNaN(targetX))
            throw new ArgumentException(
                $"'{nameof(targetX)}' cannot be {float.NaN}",
                nameof(targetX));
        if (float.IsNaN(targetY))
            throw new ArgumentException(
                $"'{nameof(targetY)}' cannot be {float.NaN}",
                nameof(targetY));

        sourceX = InputMath.Clamp11(sourceX);
        sourceY = InputMath.Clamp11(sourceY);
        targetX = InputMath.Clamp11(targetX);
        targetY = InputMath.Clamp11(targetY);

        // Return a delta object with the relative coordinates.
        if (sourceX == targetX && sourceY == targetY)
        {
            return Zero;
        }
        return new JoystickDelta(targetX - sourceX, targetY - sourceY);
    }


    /// <summary>
    /// Gets a <see cref="JoystickDelta"/> object that represents the 
    /// movement delta between the specified source and target 
    /// positions of a joystick.
    /// </summary>
    /// <param name="source">A <see cref="SlimJoystick"/> object 
    /// representing the source joystick position.</param>
    /// <param name="target">A <see cref="SlimJoystick"/> object 
    /// representing the target joystick position.</param>
    /// <returns>A <see cref="JoystickDelta"/> object that represents 
    /// the difference between the specified source and target 
    /// positions. If there is no difference between the source and 
    /// target positions, <see cref="Zero"/> is returned.</returns>
    public static JoystickDelta FromJoystickPosition(
        SlimJoystick source, SlimJoystick target)
    {
        return FromJoystickPosition(source.X, source.Y,
            target.X, target.Y);
    }


    /// <summary>
    /// Gets the <see cref="string"/> representation of the 
    /// <see cref="JoystickDelta"/> instance.
    /// </summary>
    /// <returns>The <see cref="string"/> representation of 
    /// the <see cref="JoystickDelta"/>.</returns>
    public override string ToString()
    {
        return $"{X}, {Y}";
    }


    /// <summary>
    /// Gets the hash code for the current <see cref="JoystickDelta"/>
    /// object. Overrides <see cref="object.GetHashCode()"/>.
    /// </summary>
    /// <returns>The computed hash code.</returns>
    /// <seealso cref="Equals(object?)"/>
    public override int GetHashCode()
    {
        return _hashCode;
    }


    /// <summary>
    /// Determines if the current <see cref="JoystickDelta"/> is 
    /// identical to the specified <see cref="object"/> instance.
    /// Overrides <see cref="object.Equals(object?)"/>
    /// </summary>
    /// <param name="obj"><see cref="object"/> instance to 
    /// compare with the current <see cref="JoystickDelta"/>.</param>
    /// <returns><see langword="true"/> if the current 
    /// <see cref="JoystickDelta"/> is identical to <paramref name="obj"/>;
    /// otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="GetHashCode()"/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is JoystickDelta joystickDelta)
            return Equals(joystickDelta);
        else
            return false;
    }


    /// <summary>
    /// Determines if the current <see cref="JoystickDelta"/> is 
    /// identical to the specified <see cref="JoystickDelta"/> 
    /// object.
    /// </summary>
    /// <param name="other"><see cref="JoystickDelta"/> object to 
    /// compare with the current <see cref="JoystickDelta"/>.</param>
    /// <returns><see langword="true"/> if the current 
    /// <see cref="JoystickDelta"/> is identical to <paramref name="other"/>;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Equals(JoystickDelta other)
    {
        return X == other.X && Y == other.Y;
    }


    /// <summary>
    /// Gets the source position of the joystick, considering its specified 
    /// target position.
    /// </summary>
    /// <param name="targetX">Target position of the joystick's X axis.</param>
    /// <param name="targetY">Target position of the joystick's Y axis.</param>
    /// <returns>A <see cref="SlimJoystick"/> object that represents the 
    /// source joystick position.</returns>
    /// <exception cref="ArgumentException"><paramref name="targetX"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="targetY"/>
    /// is <see cref="float.NaN"/>.</exception>
    public SlimJoystick GetSourcePosition(float targetX, float targetY)
    {
        if (float.IsNaN(targetX))
            throw new ArgumentException(
                $"'{nameof(targetX)}' cannot be {float.NaN}.",
                nameof(targetX));
        if (float.IsNaN(targetY))
            throw new ArgumentException(
                $"'{nameof(targetY)}' cannot be {float.NaN}.",
                nameof(targetY));

        targetX = InputMath.Clamp11(targetX);
        targetY = InputMath.Clamp11(targetY);
        return new SlimJoystick(targetX - X, targetY - Y);
    }


    /// <summary>
    /// Gets the source position of the joystick, considering its specified 
    /// target position.
    /// </summary>
    /// <param name="joystick"><see cref="SlimJoystick"/> object representing 
    /// the target joystick position.</param>
    public SlimJoystick GetSourcePosition(SlimJoystick joystick)
    {
        return GetSourcePosition(joystick.X, joystick.Y);
    }


    /// <summary>
    /// Gets the source position of the joystick, considering its specified 
    /// target position.
    /// </summary>
    /// <param name="joystick"><see cref="Joystick"/> object representing 
    /// the target joystick position.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="joystick"/> is <see langword="null"/>.</exception>
    public SlimJoystick GetSourcePosition(Joystick joystick)
    {
        if (joystick is null)
            throw new ArgumentNullException(nameof(joystick));

        return GetSourcePosition(joystick.X, joystick.Y);
    }


    /// <summary>
    /// Gets the target position of the joystick, considering its specified 
    /// source position.
    /// </summary>
    /// <param name="sourceX">Source position of the joystick's X axis.</param>
    /// <param name="sourceY">Source position of the joystick's Y axis.</param>
    /// <returns>A <see cref="SlimJoystick"/> object that represents the 
    /// target joystick position.</returns>
    /// <exception cref="ArgumentException"><paramref name="sourceX"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="sourceY"/>
    /// is <see cref="float.NaN"/>.</exception>
    public SlimJoystick GetTargetPosition(float sourceX, float sourceY)
    {
        if (float.IsNaN(sourceX))
            throw new ArgumentException(
                $"'{nameof(sourceX)}' cannot be {float.NaN}.",
                nameof(sourceX));
        if (float.IsNaN(sourceY))
            throw new ArgumentException(
                $"'{nameof(sourceY)}' cannot be {float.NaN}.",
                nameof(sourceY));

        sourceX = InputMath.Clamp11(sourceX);
        sourceY = InputMath.Clamp11(sourceY);
        return new SlimJoystick(sourceX + X, sourceY + Y);
    }


    /// <summary>
    /// Gets the target position of the joystick, considering its specified 
    /// source position.
    /// </summary>
    /// <param name="joystick"><see cref="SlimJoystick"/> object representing 
    /// the source joystick position.</param>
    public SlimJoystick GetTargetPosition(SlimJoystick joystick)
    {
        return GetTargetPosition(joystick.X, joystick.Y);
    }


    /// <summary>
    /// Gets the target position of the joystick, considering its specified 
    /// source position.
    /// </summary>
    /// <param name="joystick"><see cref="Joystick"/> object representing 
    /// the source joystick position.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="joystick"/> is <see langword="null"/>.</exception>
    public SlimJoystick GetTargetPosition(Joystick joystick)
    {
        if (joystick is null)
            throw new ArgumentNullException(nameof(joystick));

        return GetTargetPosition(joystick.X, joystick.Y);
    }

    #endregion Methods


}
