using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace XInputium;

/// <summary>
/// Represents a controller device trigger, that has 
/// a single axis.
/// </summary>
/// <remarks>
/// <see cref="SlimTrigger"/> read-only structure is a more 
/// lightweight alternative to <see cref="Trigger"/> class, 
/// which can be less resource intensive, but at the cost of 
/// providing fewer features. <see cref="SlimTrigger"/> provides 
/// only the most crucial feature necessary for working with 
/// controller devices' triggers. If you need more advanced 
/// features, consider using <see cref="Trigger"/> class.
/// </remarks>
/// <seealso cref="Trigger"/>
/// <seealso cref="SlimJoystick"/>
[DebuggerDisplay($"{nameof(Value)} = {{{nameof(Value)}}}")]
[Serializable]
public readonly struct SlimTrigger
    : IEquatable<SlimTrigger>
{


    #region Fields

    /// <summary>
    /// A <see cref="SlimTrigger"/> object that has its axis 
    /// at position 0.
    /// </summary>
    public static readonly SlimTrigger Zero = new(0f);

    private readonly int _hashCode = default;  // Cached hash code for the current instance.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Creates a new <see cref="SlimTrigger"/> object, 
    /// that has the specified axis value.
    /// </summary>
    /// <param name="value">A value within the 0 and 1 inclusive range. 
    /// If you specify a value outside this range, it will be clamped 
    /// accordingly.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> 
    /// is <see cref="float.NaN"/>.</exception>
    public SlimTrigger(float value)
    {
        if (float.IsNaN(value))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(value)}' parameter.",
                nameof(value));

        Value = InputMath.Clamp01(value);

        _hashCode = Value.GetHashCode();
    }

    #endregion Constructors


    #region Operators

    /// <summary>
    /// Compares both specified <see cref="SlimTrigger"/> objects 
    /// for equality.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> 
    /// is identical to <paramref name="right"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(SlimTrigger left, SlimTrigger right)
    {
        return left.Equals(right);
    }


    /// <summary>
    /// Compares both specified <see cref="SlimTrigger"/> objects 
    /// for inequality.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> 
    /// is <b>not</b> identical to <paramref name="right"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(SlimTrigger left, SlimTrigger right)
    {
        return !left.Equals(right);
    }

    #endregion Operators


    #region Properties

    /// <summary>
    /// Gets the value of the trigger's axis.
    /// </summary>
    /// <returns>A value within the 0 and 1 inclusive range.</returns>
    public float Value { get; }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the 
    /// <see cref="SlimTrigger"/> is identical to 
    /// <see cref="Zero"/>, meaning its axis is at 
    /// position 0.
    /// </summary>
    /// <seealso cref="Zero"/>
    public bool IsZero => Equals(Zero);

    #endregion Properties


    #region Methods

    /// <summary>
    /// Gets the <see cref="string"/> representation of the 
    /// <see cref="SlimTrigger"/>.
    /// </summary>
    /// <returns>The <see cref="string"/> representation of 
    /// the current <see cref="SlimTrigger"/> object.</returns>
    public override string ToString()
    {
        return $"{Value}";
    }


    /// <summary>
    /// Gets the hash code for the current <see cref="SlimTrigger"/>
    /// object.
    /// </summary>
    /// <returns>The computed hash code.</returns>
    /// <seealso cref="Equals(object?)"/>
    public override int GetHashCode()
    {
        return _hashCode;
    }


    /// <summary>
    /// Determines if the specified <see cref="object"/> instance 
    /// is identical to the current <see cref="SlimTrigger"/>.
    /// </summary>
    /// <param name="obj"><see cref="object"/> instance to 
    /// compare.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is 
    /// identical to the current <see cref="SlimTrigger"/> object; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="GetHashCode()"/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;
        else if (obj is SlimTrigger trigger)
            return Equals(trigger);
        else
            return false;
    }


    /// <summary>
    /// Determines if the specified <see cref="SlimTrigger"/> object 
    /// is identical to the current <see cref="SlimTrigger"/>.
    /// </summary>
    /// <param name="other"><see cref="SlimTrigger"/> instance to 
    /// compare.</param>
    /// <returns><see langword="true"/> if <paramref name="other"/> 
    /// is identical to the current <see cref="SlimTrigger"/> object; 
    /// otherwise, <see langword="false"/>.</returns>
    public bool Equals(SlimTrigger other)
    {
        return Value == other.Value;
    }


    /// <summary>
    /// Gets a <see cref="SlimTrigger"/> object that represents the 
    /// value of the current <see cref="SlimTrigger"/> with the 
    /// specified inner and outer dead-zones applied.
    /// </summary>
    /// <param name="innerDeadZone">A value between 0 and 1, that 
    /// specifies the inner dead-zone of the trigger.</param>
    /// <param name="outerDeadZone">A value between 0 and 1, that 
    /// specifies the outer dead-zone of the trigger.</param>
    /// <returns>The created <see cref="SlimTrigger"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="innerDeadZone"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outerDeadZone"/>
    /// is <see cref="float.NaN"/>.</exception>
    public SlimTrigger ApplyDeadZone(float innerDeadZone, float outerDeadZone)
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

        return new SlimTrigger(InputMath.ApplyDeadZone(Value, innerDeadZone, outerDeadZone));
    }


    /// <summary>
    /// Gets a <see cref="SlimTrigger"/> object that represents the 
    /// value of the current <see cref="SlimTrigger"/> with the 
    /// specified inner dead-zone applied.
    /// </summary>
    /// <param name="innerDeadZone">A value between 0 and 1, that 
    /// specifies the inner dead-zone of the trigger.</param>
    /// <returns>The created <see cref="SlimTrigger"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="innerDeadZone"/> 
    /// is <see cref="float.NaN"/>.</exception>
    public SlimTrigger ApplyDeadZone(float innerDeadZone)
    {
        return ApplyDeadZone(innerDeadZone, 0f);
    }


    /// <summary>
    /// Gets a <see cref="SlimTrigger"/> object that represents the 
    /// value of the current <see cref="SlimTrigger"/> with the 
    /// specified <see cref="ModifierFunction"/> applied to its 
    /// value.
    /// </summary>
    /// <param name="modifierFunction"><see cref="ModifierFunction"/> 
    /// to apply.</param>
    /// <returns>The new <see cref="SlimTrigger"/> object.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="modifierFunction"/> is <see langword="null"/>.</exception>
    public SlimTrigger ApplyModifierFunction(ModifierFunction modifierFunction)
    {
        if (modifierFunction is null)
            throw new ArgumentNullException(nameof(modifierFunction));

        return new SlimTrigger(InputMath.Clamp01(modifierFunction(Value)));
    }

    #endregion Methods


}
