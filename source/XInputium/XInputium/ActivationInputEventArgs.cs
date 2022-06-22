using System;

namespace XInputium;

/// <summary>
/// Encapsulates event arguments for an <see cref="ActivationInputEvent"/> 
/// event.
/// </summary>
/// <seealso cref="ActivationInputEvent"/>
/// <seealso cref="ActivationInputEventHandler"/>
public class ActivationInputEventArgs : InputEventArgs
{


    #region Fields

    private readonly ActivationInputEvent _event;  // Activation event associated with the current instance.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Creates a new instance of an <see cref="ActivationInputEventArgs"/> 
    /// class, that is associated with the specified 
    /// <see cref="ActivationInputEventArgs"/> instance.
    /// </summary>
    /// <param name="inputEvent"></param>
    public ActivationInputEventArgs(ActivationInputEvent inputEvent)
        : base(inputEvent)
    {
        _event = inputEvent;
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the activation event 
    /// is currently active.
    /// </summary>
    /// <returns><see langword="true"/> if the event is active;
    /// otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// The value this property returns is the current real-time value
    /// the triggering <see cref="ActivationInputEvent"/> has, not the 
    /// value it had in the moment the event was triggered. If you're 
    /// consuming the current <see cref="ActivationInputEventArgs"/> 
    /// instance in the moment the event was triggered, the value 
    /// returned by this property is accurate. This is because each  
    /// <see cref="ActivationInputEventArgs"/> instance is a singleton 
    /// that is associated with an <see cref="ActivationInputEvent"/> 
    /// instance.
    /// </remarks>
    /// <seealso cref="PreviousStateDuration"/>
    /// <seealso cref="CurrentStateDuration"/>
    /// <seealso cref="ActivationInputEvent.IsActive"/>
    public bool IsActive => _event.IsActive;


    /// <summary>
    /// Gets the amount of input time spent on the previous 
    /// active/inactive state of the <see cref="ActivationInputEvent"/>.
    /// </summary>
    /// <returns>The duration of the last active/inactive state, before 
    /// the current state. If that state was 'active', the returned value 
    /// is never greater than 
    /// <see cref="ActivationInputEvent.ActiveTimeout"/>.</returns>
    /// <remarks>
    /// This property returns the amount of time elapsed between 
    /// the second to last and the last change to the value of 
    /// <see cref="IsActive"/> property. For instance, if the current 
    /// state is 'inactive', this property returns the duration of the 
    /// most recent active state. The time information is in input time,
    /// as measured by the input code that is hosting the 
    /// <see cref="ActivationInputEvent"/>.
    /// <br/><br/>
    /// The value this property returns is the current real-time value
    /// the triggering <see cref="ActivationInputEvent"/> has, not the 
    /// value it had in the moment the event was triggered. If you're 
    /// consuming the current <see cref="ActivationInputEventArgs"/> 
    /// instance in the moment the event was triggered, the value 
    /// returned by this property is accurate. This is because each  
    /// <see cref="ActivationInputEventArgs"/> instance is a singleton 
    /// that is associated with an <see cref="ActivationInputEvent"/> 
    /// instance.
    /// </remarks>
    /// <seealso cref="CurrentStateDuration"/>
    /// <seealso cref="IsActive"/>
    /// <seealso cref="ActivationInputEvent.PreviousStateDuration"/>
    public TimeSpan PreviousStateDuration => _event.PreviousStateDuration;


    /// <summary>
    /// Gets the amount of input time elapsed since the last 
    /// change to the active/inactive state.
    /// </summary>
    /// <remarks>
    /// This property returns the amount of time elapsed since 
    /// the last change to the value of <see cref="IsActive"/>
    /// property. For instance, if the current state is 'inactive',
    /// this property returns the amount of time elapsed since the 
    /// deactivation occurred. The time information is in input 
    /// time, as measured by the input code that is hosting the 
    /// <see cref="ActivationInputEvent"/>.
    /// <br/><br/>
    /// To obtain the duration of the state that preceded the 
    /// current state, use <see cref="PreviousStateDuration"/> property.
    /// See <see cref="PreviousStateDuration"/> for more information.
    /// <br/><br/>
    /// The value this property returns is the current real-time value
    /// the triggering <see cref="ActivationInputEvent"/> has, not the 
    /// value it had in the moment the event was triggered. If you're 
    /// consuming the current <see cref="ActivationInputEventArgs"/> 
    /// instance in the moment the event was triggered, the value 
    /// returned by this property is accurate. This is because each  
    /// <see cref="ActivationInputEventArgs"/> instance is a singleton 
    /// that is associated with an <see cref="ActivationInputEvent"/> 
    /// instance.
    /// </remarks>
    /// <seealso cref="PreviousStateDuration"/>
    /// <seealso cref="IsActive"/>
    /// <seealso cref="ActivationInputEvent.CurrentStateDuration"/>
    public TimeSpan CurrentStateDuration => _event.CurrentStateDuration;


    /// <summary>
    /// Gets the user defined object passed to the 
    /// <see cref="ActivationInputEvent"/>.
    /// </summary>
    /// <remarks>
    /// The value this property returns is the current real-time value
    /// the triggering <see cref="ActivationInputEvent"/> has, not the 
    /// value it had in the moment the event was triggered. If you're 
    /// consuming the current <see cref="ActivationInputEventArgs"/> 
    /// instance in the moment the event was triggered, the value 
    /// returned by this property is accurate. This is because each  
    /// <see cref="ActivationInputEventArgs"/> instance is a singleton 
    /// that is associated with an <see cref="ActivationInputEvent"/> 
    /// instance.
    /// </remarks>
    /// <seealso cref="ActivationInputEvent.Parameter"/>
    public object? Parameter => _event.Parameter;


    /// <summary>
    /// Gets the <see cref="ActivationInputEventTriggerMode"/> constant 
    /// that specifies the trigger mode the associated 
    /// <see cref="ActivationInputEvent"/> is using.
    /// </summary>
    /// <seealso cref="ActivationInputEvent.TriggerMode"/>
    /// <seealso cref="ActivationInputEventTriggerMode"/>
    public ActivationInputEventTriggerMode TriggerMode => _event.TriggerMode;

    #endregion Properties


}
