using System;

namespace XInputium;

/// <summary>
/// Exposes constants that specify when an <see cref="ActivationInputEvent"/>
/// should trigger.
/// </summary>
/// <seealso cref="ActivationInputEvent"/>
public enum ActivationInputEventTriggerMode
{
    /// <summary>
    /// The <see cref="ActivationInputEvent"/> will trigger when it 
    /// activates (when it changes from inactive to active).
    /// </summary>
    OnActivation = 1,
    /// <summary>
    /// The <see cref="ActivationInputEvent"/> will trigger when it 
    /// deactivates (when it changes from active to inactive).
    /// </summary>
    OnDeactivation = 2,
    /// <summary>
    /// The <see cref="ActivationInputEvent"/> will trigger whenever 
    /// its active state changes.
    /// </summary>
    OnActivationAndDeactivation = 3,
}
