using System;

namespace XInputium;

/// <summary>
/// Event handler for an event that was triggered by an 
/// <see cref="InputEvent"/> and is associated with a 
/// <see cref="DigitalButton"/> object.
/// </summary>
/// <typeparam name="T"><see cref="DigitalButton"/> or a 
/// type deriving from <see cref="DigitalButton"/>, which 
/// is the type of the button associated with the event.</typeparam>
/// <param name="sender">Object that raised the event.</param>
/// <param name="e"><see cref="DigitalButtonInputEventArgs{T}"/> 
/// instance containing information about the event.</param>
/// <seealso cref="DigitalButtonInputEventArgs{T}"/>
/// <seealso cref="DigitalButtonInputEvent{T}"/>
/// <seealso cref="DigitalButton"/>
public delegate void DigitalButtonInputEventHandler<T>(object? sender,
    DigitalButtonInputEventArgs<T> e)
    where T : notnull, DigitalButton;
