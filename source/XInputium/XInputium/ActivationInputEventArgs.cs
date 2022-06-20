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
    /// The value returned by this property is the current value of the event,
    /// not the value it had in the moment the event was triggered. If the 
    /// event's active state changes after the event is triggered, the value 
    /// of this property will also change.
    /// </remarks>
    /// <seealso cref="ActivationInputEvent.IsActive"/>
    public bool IsActive => _event.IsActive;


    /// <summary>
    /// Gets the user defined object passed to the event when 
    /// the event was created.
    /// </summary>
    public object? Parameter => _event.Parameter;

    #endregion Properties


}
