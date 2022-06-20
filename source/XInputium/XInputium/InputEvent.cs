using System;
using System.Collections.Generic;

namespace XInputium;

/// <summary>
/// Provides the base for classes that implement a dynamic 
/// input event. This is an abstract class.
/// </summary>
/// <remarks>
/// <see cref="InputEvent"/> represents a dynamic event 
/// that can be triggered by very specific conditions. 
/// This allows for the creation and invocation of events 
/// that would not be possible using the regular CLR events. 
/// An example of a dynamic event that is specific to the 
/// needs of a consumer is an event that triggers only after 
/// a digital button is held by the user for a specified 
/// amount of time. <see cref="DigitalButtonInputEvent{T}"/> 
/// class, which derives from <see cref="InputEvent"/>, 
/// provides this exact functionality.
/// <br/><br/>
/// Inside <see cref="XInputium"/> namespace, 
/// <see cref="LogicalInputDevice{TDevice, TState}"/> is 
/// the class that is intended to make the most use of 
/// <see cref="InputEvent"/> derived classes. Although it 
/// provides regular CLR events, it also provides an input 
/// event system that uses <see cref="InputEvent"/> for its 
/// dynamic events. You can create a class that derives 
/// from <see cref="InputEvent"/>, that works with your 
/// own event triggering logic, and register an instance 
/// of your class in a class that derived from 
/// <see cref="LogicalInputDevice{TDevice, TState}"/>.
/// </remarks>
/// <seealso cref="InputEventGroup"/>
/// <seealso cref="LogicalInputDevice{TDevice, TState}"/>
public abstract class InputEvent
{


    #region Fields

    private readonly InputEventArgs _defaultEventArgs;
    private readonly Lazy<HashSet<InputEventHandler>> _handlers  // Lazy initializer for the value of Handlers property.
        = new(() => new(2), true);

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an <see cref="InputEvent"/> 
    /// class.
    /// </summary>
    public InputEvent()
    {
        _defaultEventArgs = new(this);
    }

    #endregion Constructors


    #region Properties

    private ISet<InputEventHandler> Handlers => _handlers.Value;


    /// <summary>
    /// Gets the number of handlers currently registered in the 
    /// <see cref="InputEvent"/>.
    /// </summary>
    /// <seealso cref="AddHandler(InputEventHandler)"/>
    /// <seealso cref="RemoveHandler(InputEventHandler)"/>
    /// <seealso cref="HasHandler(InputEventHandler)"/>
    public int HandlerCount => _handlers.Value.Count;

    #endregion Properties


    #region Methods

    /// <summary>
    /// Triggers the event, invoking all the event handlers 
    /// registered in the current <see cref="InputEvent"/> instance.
    /// </summary>
    /// <param name="source">The source object that triggered the 
    /// input event.</param>
    /// <param name="e">Arguments to pass to the callback. 
    /// You can use <see langword="null"/> to specify that 
    /// the default <see cref="InputEventArgs"/> for this 
    /// <see cref="InputEvent"/> instance is sent to handlers.</param>
    /// <seealso cref="OnUpdate(TimeSpan)"/>
    protected void Raise(object? source, InputEventArgs? e)
    {
        e ??= _defaultEventArgs;
        foreach (var hanler in Handlers)
        {
            hanler.Invoke(source, e);
        }
    }


    /// <summary>
    /// When overridden in a derived class, updates the event 
    /// logic and reevaluates triggering conditions, and, if 
    /// these conditions are met, triggers the event.
    /// </summary>
    /// <param name="time">Amount of time elapsed since the 
    /// last call to <see cref="Update(TimeSpan)"/> method.</param>
    /// <remarks>
    /// This method is called by <see cref="Update(TimeSpan)"/> 
    /// method, to allow inheritors to reevaluate the event's 
    /// triggering conditions and to trigger the event when 
    /// these conditions are met.
    /// </remarks>
    /// <seealso cref="Update(TimeSpan)"/>
    protected abstract void OnUpdate(TimeSpan time);


    /// <summary>
    /// Updates the event logic to reevaluate its event triggering 
    /// conditions, and, if these conditions are met, triggers 
    /// the <see cref="InputEvent"/>.
    /// </summary>
    /// <param name="time">Amount of time elapsed since the last 
    /// call to <see cref="Update(TimeSpan)"/>.</param>
    /// <seealso cref="OnUpdate(TimeSpan)"/>
    public void Update(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
            time = TimeSpan.Zero;

        OnUpdate(time);
    }


    /// <summary>
    /// Adds the specified <see cref="InputEventHandler"/> to 
    /// the <see cref="InputEvent"/>, allowing it to 
    /// handle events triggered by the <see cref="InputEvent"/>.
    /// </summary>
    /// <param name="handler"><see cref="InputEventHandler"/> to 
    /// add.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <seealso cref="RemoveHandler(InputEventHandler)"/>
    /// <seealso cref="HasHandler(InputEventHandler)"/>
    public void AddHandler(InputEventHandler handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        Handlers.Add(handler);
    }


    /// <summary>
    /// Removes the specified <see cref="InputEventHandler"/> from 
    /// the <see cref="InputEvent"/>.
    /// </summary>
    /// <param name="handler"><see cref="InputEventHandler"/> to 
    /// remove.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <seealso cref="AddHandler(InputEventHandler)"/>
    /// <seealso cref="ClearHandlers()"/>
    /// <seealso cref="HasHandler(InputEventHandler)"/>
    public void RemoveHandler(InputEventHandler handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        Handlers.Remove(handler);
    }


    /// <summary>
    /// Removes all event handlers currently registered 
    /// in the <see cref="InputEvent"/>.
    /// </summary>
    /// <seealso cref="RemoveHandler(InputEventHandler)"/>
    /// <seealso cref="AddHandler(InputEventHandler)"/>
    /// <seealso cref="HandlerCount"/>
    public void ClearHandlers()
    {
        Handlers.Clear();
    }


    /// <summary>
    /// Determines if the specified <see cref="InputEventHandler"/> 
    /// is registered in the <see cref="InputEvent"/>.
    /// </summary>
    /// <param name="handler"><see cref="InputEventHandler"/> to 
    /// check for.</param>
    /// <returns><see langword="true"/> if <paramref name="handler"/> 
    /// is registered in the <see cref="InputEvent"/>;
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <seealso cref="AddHandler(InputEventHandler)"/>
    /// <seealso cref="RemoveHandler(InputEventHandler)"/>
    public bool HasHandler(InputEventHandler handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        return Handlers.Contains(handler);
    }

    #endregion Methods


}
