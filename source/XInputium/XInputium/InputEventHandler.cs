using System;

namespace XInputium;

/// <summary>
/// Handler for an event associated with an 
/// <see cref="InputEvent"/> instance.
/// </summary>
/// <param name="sender"><see cref="object"/> that invoked the 
/// event.</param>
/// <param name="e"><see cref="InputEventArgs"/> instance 
/// containing information about the event.</param>
/// <seealso cref="InputEventArgs"/>
/// <seealso cref="InputEvent"/>
public delegate void InputEventHandler(object? sender, InputEventArgs e);

