using System;

namespace XInputium;

/// <summary>
/// Handler for an event associated with a <see cref="DigitalButton"/>
/// button object or an object that derives from 
/// <see cref="DigitalButton"/>.
/// </summary>
/// <typeparam name="T"><see cref="DigitalButton"/> or a type 
/// that derives from <see cref="DigitalButton"/>. This is the 
/// type of the button associated with the event.</typeparam>
/// <param name="sender">Object that triggered the event.</param>
/// <param name="e"><see cref="DigitalButtonEventArgs{T}"/> object 
/// that contains information about the event.</param>
/// <seealso cref="DigitalButtonEventArgs{T}"/>
/// <seealso cref="DigitalButton"/>
public delegate void DigitalButtonEventHandler<T>(object? sender, DigitalButtonEventArgs<T> e)
    where T : notnull, DigitalButton;

