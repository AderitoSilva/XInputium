using System;

namespace XInputium;

/// <summary>
/// Callback delegate that can be used to update the state of 
/// a <see cref="DigitalButton"/> instance.
/// </summary>
/// <param name="isPressed">A <see cref="bool"/> indicating if 
/// the button is currently being pressed.</param>
/// <param name="time">A <see cref="TimeSpan"/> value that 
/// specifies the amount of time elapsed since the last button 
/// state update.</param>
/// <seealso cref="DigitalButton"/>
public delegate void DigitalButtonUpdateCallback(bool isPressed, TimeSpan time);

