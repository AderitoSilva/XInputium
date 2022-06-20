using System;

namespace XInputium;

/// <summary>
/// Implements an <see cref="EventDispatcherObject"/> that 
/// provides an input event system based on <see cref="InputEvent"/>.
/// </summary>
/// <seealso cref="EventDispatcherObject"/>
/// <seealso cref="InputEvent"/>
public abstract class InputObject : EventDispatcherObject
{


    #region Fields

    private readonly InputEventGroup _inputEvents = new();  // Stores the registered InputEvent instances.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an 
    /// <see cref="InputObject"/> class.
    /// </summary>
    /// <param name="eventDispatchMode"><see cref="EventDispatchMode"/> 
    /// constant that specifies the way events are dispatched.</param>
    /// <exception cref="ArgumentException"><paramref name="eventDispatchMode"/> 
    /// is not a defined constant in an <see cref="EventDispatchMode"/> 
    /// enumeration.</exception>
    /// <seealso cref="EventDispatchMode"/>
    public InputObject(EventDispatchMode eventDispatchMode)
        : base(eventDispatchMode)
    {
        _inputEvents.AddHandler(InputEvents_Event);
    }

    #endregion Constructors


    #region Events

    /// <summary>
    /// It's invoked whenever an <see cref="InputEvent"/> 
    /// registered in the <see cref="InputObject"/> is 
    /// triggered.
    /// </summary>
    /// <seealso cref="OnInputEventTriggered(InputEventArgs)"/>
    /// <seealso cref="InputEvent"/>
    /// <seealso cref="RegisterInputEvent(InputEvent)"/>
    /// <seealso cref="UnregisterInputEvent(InputEvent)"/>
    public event InputEventHandler? InputEventTriggered;

    #endregion Events


    #region Methods

    /// <summary>
    /// Raises the <see cref="InputEventTriggered"/> event.
    /// </summary>
    /// <param name="e"><see cref="InputEventArgs"/> instance 
    /// that contains information about the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="e"/> is <see langword="null"/>.</exception>
    /// <seealso cref="InputEventTriggered"/>
    /// <seealso cref="InputEvent"/>
    protected virtual void OnInputEventTriggered(InputEventArgs e)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        RaiseEvent(() => InputEventTriggered?.Invoke(this, e));
    }


    /// <summary>
    /// Registers the specified <see cref="InputEvent"/> in 
    /// the <see cref="InputObject"/>.
    /// </summary>
    /// <param name="inputEvent"><see cref="InputEvent"/> 
    /// instance to register.</param>
    /// <returns><see langword="true"/> if <paramref name="inputEvent"/> 
    /// was successfully registered; <see langword="false"/> 
    /// if <paramref name="inputEvent"/> is already registered.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputEvent"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method registers the specified <see cref="InputEvent"/> 
    /// instance in the <see cref="InputObject"/>. 
    /// Registered input events will participate in the 
    /// <see cref="InputObject"/>'s input event system, and will fire 
    /// events once their respective conditions are met. Registered 
    /// input events are tied to the event dispatching system and 
    /// will be triggered when 
    /// <see cref="UpdateInputObject(TimeSpan)"/>. method is called.
    /// <br/><br/>
    /// To unregister a registered input event, use 
    /// <see cref="UnregisterInputEvent(InputEvent)"/> method.
    /// </remarks>
    /// <seealso cref="UnregisterInputEvent(InputEvent)"/>
    public bool RegisterInputEvent(InputEvent inputEvent)
    {
        if (inputEvent is null)
            throw new ArgumentNullException(nameof(inputEvent));

        return _inputEvents.Add(inputEvent);
    }


    /// <summary>
    /// Unregisters an <see cref="InputEvent"/> that was 
    /// previously registered with 
    /// <see cref="RegisterInputEvent(InputEvent)"/> 
    /// method.
    /// </summary>
    /// <param name="inputEvent"><see cref="InputEvent"/> 
    /// instance to unregister from the input event system.</param>
    /// <returns><see langword="true"/> if 
    /// <paramref name="inputEvent"/> was found and unregistered; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputEvent"/> is <see langword="null"/>.</exception>
    /// <seealso cref="RegisterInputEvent(InputEvent)"/>
    /// <seealso cref="UnregisterAllInputEvents()"/>
    public bool UnregisterInputEvent(InputEvent inputEvent)
    {
        if (inputEvent is null)
            throw new ArgumentNullException(nameof(inputEvent));

        return _inputEvents.Remove(inputEvent);
    }


    /// <summary>
    /// Unregisters all <see cref="InputEvent"/> instances 
    /// currently registered in the 
    /// <see cref="InputObject"/>'s input event system.
    /// </summary>
    /// <seealso cref="UnregisterInputEvent(InputEvent)"/>
    /// <seealso cref="RegisterInputEvent(InputEvent)"/>
    public void UnregisterAllInputEvents()
    {
        _inputEvents.Clear();
    }


    /// <summary>
    /// Determines if the specified <see cref="InputEvent"/> 
    /// is registered in the current <see cref="InputObject"/>.
    /// </summary>
    /// <param name="inputEvent"><see cref="InputEvent"/> 
    /// to check for.</param>
    /// <returns><see langword="true"/> if <paramref name="inputEvent"/> 
    /// is registered in the <see cref="InputObject"/>;
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputEvent"/> is <see langword="null"/>.</exception>
    public bool IsInputEventRegistered(InputEvent inputEvent)
    {
        if (inputEvent is null)
            throw new ArgumentNullException(nameof(inputEvent));

        return _inputEvents.Contains(inputEvent);
    }


    /// <summary>
    /// Updates the state of <see cref="InputObject"/>.
    /// </summary>
    /// <param name="time">AMount of time elapsed since 
    /// the last update operation.</param>
    /// <remarks>
    /// This method updates the <see cref="InputObject"/>, 
    /// making it update the logic of all registered 
    /// <see cref="InputEvent"/> instances and dispatching 
    /// any enqueued events. Inheritors must call this method 
    /// for every update to the input state.
    /// </remarks>
    protected virtual void UpdateInputObject(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
            time = TimeSpan.Zero;

        _inputEvents.Update(time);

        DispatchEvents();
    }

    #endregion Methods


    #region Event handlers

    private void InputEvents_Event(object? sender, InputEventArgs e)
    {
        OnInputEventTriggered(e);
    }

    #endregion Event handlers


}
