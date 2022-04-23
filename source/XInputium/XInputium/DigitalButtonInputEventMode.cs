using System;

namespace XInputium;

/// <summary>
/// Exposes constants that specify when a 
/// <see cref="DigitalButtonInputEvent{T}"/> will trigger 
/// an event based on an associated <see cref="DigitalButton"/>.
/// </summary>
/// <seealso cref="DigitalButtonInputEvent{T}"/>
public enum DigitalButtonInputEventMode
{


    /// <summary>
    /// The event is triggered when the button state changes 
    /// from released to pressed — that is, when the user 
    /// has just tapped the button.
    /// </summary>
    OnPress,

    /// <summary>
    /// The event is triggered when the button state changes 
    /// from pressed to released — that is, when the user 
    /// has just released the button that was being pressed.
    /// </summary>
    OnRelease,

    /// <summary>
    /// The event is triggered once after the button was 
    /// pressed for a specific minimum amount of time.
    /// </summary>
    OnHold,


}
