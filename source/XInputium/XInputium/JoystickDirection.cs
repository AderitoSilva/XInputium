using System;

namespace XInputium;

/// <summary>
/// Exposes constants that represent the push direction of 
/// a joystick's axes.
/// </summary>
/// <seealso cref="Joystick"/>
public enum JoystickDirection
{


    /// <summary>
    /// Represents no direction, for when there is no movement.
    /// </summary>
    None = 0,

    /// <summary>
    /// The joystick is pushed upwards.
    /// </summary>
    Up = 1,

    /// <summary>
    /// The joystick is pushed downwards.
    /// </summary>
    Down = 2,

    /// <summary>
    /// The joystick is pushed towards the left.
    /// </summary>
    Left = 3,

    /// <summary>
    /// The joystick is pushed up towards the right.
    /// </summary>
    Right = 4,


}
