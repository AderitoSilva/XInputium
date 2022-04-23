using System;

namespace XInputium;

/// <summary>
/// Callback delegate that is used by <see cref="Joystick"/> 
/// to enable external code to update its state.
/// </summary>
/// <param name="x">A value between -1 and 1, that specifies 
/// the raw position of the joystick's horizontal axis.</param>
/// <param name="y">A value between -1 and 1, that specifies 
/// the raw position of the joystick's vertical axis.</param>
/// <param name="time">Amount of time elapsed since the last 
/// call to this callback.</param>
/// <seealso cref="Joystick"/>
public delegate void JoystickUpdateCallback(float x, float y, TimeSpan time);
