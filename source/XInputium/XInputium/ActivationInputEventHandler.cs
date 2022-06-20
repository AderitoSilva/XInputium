using System;

namespace XInputium;

/// <summary>
/// Event handler for an <see cref="ActivationInputEvent"/> event.
/// </summary>
/// <param name="sender">Object that triggered the event.</param>
/// <param name="e"><see cref="ActivationInputEventArgs"/> object 
/// containing data about the event.</param>
/// <seealso cref="ActivationInputEvent"/>
/// <seealso cref="ActivationInputEventArgs"/>
public delegate void ActivationInputEventHandler(object? sender, ActivationInputEventArgs e);
