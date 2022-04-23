using System;

namespace XInputium;

/// <summary>
/// Callback delegate used to update a <see cref="Trigger"/> 
/// instance from external code.
/// </summary>
/// <param name="value">A value within the 0 and 1 inclusive range, 
/// that represents the position of the trigger's axis.</param>
/// <param name="time">The amount of time elapsed since the last 
/// time this callback was last invoked.</param>
/// <seealso cref="Trigger"/>
public delegate void TriggerUpdateCallback(float value, TimeSpan time);
