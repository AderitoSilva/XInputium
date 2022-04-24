using System;
using System.Diagnostics.CodeAnalysis;

namespace XInputium.XInput;

/// <summary>
/// Implements an <see cref="IInputDeviceState"/> interface 
/// that represents the state of an XInput controller device. 
/// This is a read-only structure.
/// </summary>
/// <remarks>
/// You can obtain the current of an XInput controller device 
/// by using the <see cref="XInputDevice"/> class. The static 
/// <see cref="XInputDevice.GetState(XInputUserIndex)"/> method 
/// returns an <see cref="XInputDeviceState"/> object that 
/// represents a device's current state. You can also instantiate 
/// an <see cref="XInputDevice"/> class and use its methods and 
/// properties to get the current device's state.
/// </remarks>
/// <seealso cref="XInputDevice"/>
/// <seealso cref="IInputDeviceState"/>
[Serializable]
public readonly struct XInputDeviceState
    : IInputDeviceState, IEquatable<XInputDeviceState>, ICloneable
{


    #region Fields

    /// <summary>
    /// An <see cref="XInputDeviceState"/> that represents an 
    /// unconnected XInput device state, with no buttons pressed 
    /// and all axes at 0.
    /// </summary>
    /// <seealso cref="SlimJoystick.Zero"/>
    /// <seealso cref="SlimTrigger.Zero"/>
    public static readonly XInputDeviceState Empty = new(
        false, XButtons.None,
        SlimJoystick.Zero, SlimJoystick.Zero,
        SlimTrigger.Zero, SlimTrigger.Zero);

    private readonly int _hashCode;  // Cached hash code for this instance.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Creates a new <see cref="XInputDeviceState"/> that represents 
    /// an XInput device state that has the specified properties.
    /// </summary>
    /// <param name="isConnected"><see langword="true"/> if the device 
    /// is connected to the system, or <see langword="false"/> if the 
    /// device is not connected.</param>
    /// <param name="buttons">A bitwise combination of <see cref="XButtons"/> 
    /// flags that specify what buttons are currently pressed on the 
    /// device.</param>
    /// <param name="leftJoystick">A <see cref="SlimJoystick"/> object 
    /// that represents the state of the device's left joystick.</param>
    /// <param name="rightJoystick">A <see cref="SlimJoystick"/> object 
    /// that represents the state of the device's right joystick.</param>
    /// <param name="leftTrigger">A <see cref="SlimJoystick"/> object 
    /// that represents the state of the device's left trigger.</param>
    /// <param name="rightTrigger">A <see cref="SlimJoystick"/> object 
    /// that represents the state of the device's right trigger.</param>
    public XInputDeviceState(bool isConnected, XButtons buttons,
        SlimJoystick leftJoystick, SlimJoystick rightJoystick,
        SlimTrigger leftTrigger, SlimTrigger rightTrigger)
    {
        IsConnected = isConnected;
        Buttons = buttons;
        LeftJoystick = leftJoystick;
        RightJoystick = rightJoystick;
        LeftTrigger = leftTrigger;
        RightTrigger = rightTrigger;

        _hashCode = HashCode.Combine(isConnected, Buttons,
            LeftJoystick, RightJoystick, LeftTrigger, RightTrigger);
    }

    #endregion Constructors


    #region Operators

    /// <summary>
    /// Compares both specified <see cref="XInputDeviceState"/> 
    /// objects for equality.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> 
    /// is identical to <paramref name="right"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(XInputDeviceState left, XInputDeviceState right)
    {
        return left.Equals(right);
    }


    /// <summary>
    /// Compares both specified <see cref="XInputDeviceState"/> 
    /// objects for inequality.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> 
    /// is <b>not</b> identical to <paramref name="right"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(XInputDeviceState left, XInputDeviceState right)
    {
        return !left.Equals(right);
    }

    #endregion Operators


    #region Properties

    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the device 
    /// is currently connected to the system.
    /// </summary>
    /// <returns><see langword="true"/> if the device is connected; 
    /// otherwise, <see langword="false"/>.</returns>
    public bool IsConnected { get; }


    /// <summary>
    /// Gets a combination of <see cref="XButtons"/> flags that specify the 
    /// XInput buttons that are currently being pressed.
    /// </summary>
    /// <remarks>
    /// <returns>A combination of <see cref="XButtons"/> bitwise flags that 
    /// represent the pressed buttons; or <see cref="XButtons.None"/> if no 
    /// button is currently being pressed.</returns>
    /// For convenience, you can use <see cref="IsButtonPressed(XButtons)"/> 
    /// method to determine if a specific button is being pressed.
    /// </remarks>
    /// <seealso cref="IsButtonPressed(XButtons)"/>
    public XButtons Buttons { get; }


    /// <summary>
    /// Gets a <see cref="SlimJoystick"/> object that represents 
    /// the position of the device's left joystick.
    /// </summary>
    /// <seealso cref="RightJoystick"/>
    public SlimJoystick LeftJoystick { get; }


    /// <summary>
    /// Gets a <see cref="SlimJoystick"/> object that represents 
    /// the position of the device's right joystick.
    /// </summary>
    /// <seealso cref="LeftJoystick"/>
    public SlimJoystick RightJoystick { get; }


    /// <summary>
    /// Gets a <see cref="SlimTrigger"/> object that represents 
    /// the position of the device's left trigger.
    /// </summary>
    /// <seealso cref="RightTrigger"/>
    public SlimTrigger LeftTrigger { get; }


    /// <summary>
    /// Gets a <see cref="SlimTrigger"/> object that represents 
    /// the position of the device's right trigger.
    /// </summary>
    /// <seealso cref="LeftTrigger"/>
    public SlimTrigger RightTrigger { get; }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the current 
    /// <see cref="XInputDeviceState"/> is empty. An empty 
    /// <see cref="XInputDeviceState"/> object is represented 
    /// by <see cref="Empty"/> static field.
    /// </summary>
    /// <seealso cref="Empty"/>
    public bool IsEmpty => Equals(Empty);

    #endregion Properties


    #region Methods

    /// <summary>
    /// Gets the hash code for the current <see cref="XInputDeviceState"/>.
    /// </summary>
    /// <returns>An <see cref="int"/> that represents the 
    /// computed hash code for the current 
    /// <see cref="XInputDeviceState"/>.</returns>
    /// <seealso cref="Equals(object?)"/>
    public override int GetHashCode()
    {
        return _hashCode;
    }


    /// <summary>
    /// Determines if the current <see cref="XInputDeviceState"/> 
    /// object is identical to the specified <see cref="object"/> 
    /// instance.
    /// </summary>
    /// <param name="obj"><see cref="object"/> to compare with the 
    /// current object.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> 
    /// is identical to the current <see cref="XInputDeviceState"/>;
    /// otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="GetHashCode()"/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is null)
            return false;
        else if (obj is XInputDeviceState state)
            return Equals(state);
        else
            return false;
    }


    /// <summary>
    /// Determines if the current <see cref="XInputDeviceState"/> 
    /// object is identical to the specified 
    /// <see cref="XInputDeviceState"/>.
    /// </summary>
    /// <param name="other"><see cref="XInputDeviceState"/> to compare 
    /// with the current <see cref="XInputDeviceState"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="other"/> 
    /// is identical to the current <see cref="XInputDeviceState"/>;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Equals(XInputDeviceState other)
    {
        return !(_hashCode != other._hashCode
            || IsConnected != other.IsConnected
            || Buttons != other.Buttons
            || LeftJoystick != other.LeftJoystick
            || RightJoystick != other.RightJoystick
            || LeftTrigger != other.LeftTrigger
            || RightTrigger != other.RightTrigger);
    }


    bool IInputDeviceState.StateEquals(IInputDeviceState state)
    {
        return state is XInputDeviceState xState && Equals(xState);
    }


    /// <summary>
    /// Determines if the specified XInput button is pressed, 
    /// accordingly to the current <see cref="XInputDeviceState"/>.
    /// </summary>
    /// <param name="button">Button to check. This must be a specific 
    /// button constant, that is not <see cref="XButtons.None"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="button"/> is
    ///  currently pressed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is not a defined constant of an <see cref="XButtons"/> 
    /// enumeration, or it is <see cref="XButtons.None"/>.</exception>
    /// <seealso cref="Buttons"/>
    public bool IsButtonPressed(XButtons button)
    {
        if (button == XButtons.None || !Enum.IsDefined(button))
            throw new ArgumentException(
                $"'{button}' is not a valid value for '{nameof(button)}' " +
                $"parameter. A specific button constant is required.");

        return Buttons.HasFlag(button);
    }


    object ICloneable.Clone()
    {
        return new XInputDeviceState(IsConnected,
            Buttons, LeftJoystick, RightJoystick, LeftTrigger, RightTrigger);
    }

    #endregion Methods


}
