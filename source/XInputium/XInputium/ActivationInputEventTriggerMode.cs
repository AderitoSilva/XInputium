using System;

namespace XInputium;

/// <summary>
/// Exposes constants that specify when an <see cref="ActivationInputEvent"/>
/// should trigger.
/// </summary>
/// <seealso cref="ActivationInputEvent"/>
/// <seealso cref="ActivationInputEvent.TriggerMode"/>
public enum ActivationInputEventTriggerMode
{
    /// <summary>
    /// The <see cref="ActivationInputEvent"/> will never trigger.
    /// This can be used to temporarily disable the event.
    /// </summary>
    Never = 0,
    /// <summary>
    /// The <see cref="ActivationInputEvent"/> will trigger when it 
    /// activates (when it changes from inactive to active).
    /// </summary>
    /// <seealso cref="OnDeactivation"/>
    OnActivation = 1,
    /// <summary>
    /// The <see cref="ActivationInputEvent"/> will trigger when it 
    /// deactivates (when it changes from active to inactive).
    /// </summary>
    /// <seealso cref="OnActivation"/>
    OnDeactivation = 2,
    /// <summary>
    /// The <see cref="ActivationInputEvent"/> will trigger whenever 
    /// its active state changes.
    /// </summary>
    /// <seealso cref="OnActivation"/>
    /// <seealso cref="OnDeactivation"/>
    OnActivationAndDeactivation = 3,
    /// <summary>
    /// The <see cref="ActivationInputEvent"/> will trigger on every 
    /// update, while its state is 'active'. This can be used to 
    /// continuously trigger the event while a specific condition is 
    /// met.
    /// </summary>
    WhileActive = 4,
}
