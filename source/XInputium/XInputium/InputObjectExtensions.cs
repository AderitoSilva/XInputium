using System;

namespace XInputium;

/// <summary>
/// Exposes extension methods for <see cref="InputObject"/> 
/// objects.
/// </summary>
/// <seealso cref="InputObject"/>
public static class InputObjectExtensions
{


    #region Methods

    /// <summary>
    /// Registers a new <see cref="ActivationInputEvent"/> that 
    /// triggers whenever the specified activator function activates 
    /// or deactivates.
    /// </summary>
    /// <param name="inputObject"><see cref="InputObject"/> where 
    /// the event will be registered.</param>
    /// <param name="activator">Function that will be called to 
    /// determine whether a custom condition is met.</param>
    /// <param name="activationDelay">Delay time the event will 
    /// wait after the activator function activates, until the 
    /// event triggers. If you specify <see cref="TimeSpan.Zero"/>, 
    /// the event triggers immediately.</param>
    /// <param name="deactivationDelay">Delay time the event will 
    /// wait after the activator function deactivates, until the 
    /// event triggers. If you specify <see cref="TimeSpan.Zero"/>, 
    /// the event triggers immediately.</param>
    /// <param name="activeTimeout">Maximum duration the event's active 
    /// state is allowed to have. Once the event is continuously active 
    /// for longer than this amount of time, the event deactivates.
    /// If you specify <see cref="TimeSpan.Zero"/>, the event 
    /// deactivates immediately after activating.</param>
    /// <param name="triggerMode">An 
    /// <see cref="ActivationInputEventTriggerMode"/> constant that 
    /// specifies when the event will trigger.</param>
    /// <param name="parameter">An custom object that will be passed 
    /// to the event handler of the event; or <see langword="null"/> 
    /// to use no custom object.</param>
    /// <param name="callback">Callback that will handle the event.</param>
    /// <returns>The newly created and registered 
    /// <see cref="ActivationInputEvent"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerMode"/> 
    /// is not a defined constant in an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    /// <seealso cref="InputObject"/>
    /// <seealso cref="ActivationInputEvent"/>
    public static ActivationInputEvent RegisterActivationInputEvent(
        this InputObject inputObject,
        Func<bool> activator,
        TimeSpan activationDelay, TimeSpan deactivationDelay,
        TimeSpan activeTimeout,
        ActivationInputEventTriggerMode triggerMode,
        object? parameter,
        ActivationInputEventHandler callback)
    {
        if (inputObject is null)
            throw new ArgumentNullException(nameof(inputObject));
        if (activator is null)
            throw new ArgumentNullException(nameof(activator));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        ActivationInputEvent activationEvent = new(activator,
            activationDelay, deactivationDelay, activeTimeout,
            triggerMode, parameter);
        activationEvent.AddHandler((sender, e) => callback(sender, (ActivationInputEventArgs)e));
        inputObject.RegisterInputEvent(activationEvent);
        return activationEvent;
    }


    /// <summary>
    /// Registers a new <see cref="ActivationInputEvent"/> that 
    /// triggers whenever the specified activator function activates 
    /// or deactivates.
    /// </summary>
    /// <param name="inputObject"><see cref="InputObject"/> where 
    /// the event will be registered.</param>
    /// <param name="activator">Function that will be called to 
    /// determine whether a custom condition is met.</param>
    /// <param name="activationDelay">Delay time the event will 
    /// wait after the activator function activates, until the 
    /// event triggers. If you specify <see cref="TimeSpan.Zero"/>, 
    /// the event triggers immediately.</param>
    /// <param name="deactivationDelay">Delay time the event will 
    /// wait after the activator function deactivates, until the 
    /// event triggers. If you specify <see cref="TimeSpan.Zero"/>, 
    /// the event triggers immediately.</param>
    /// <param name="activeTimeout">Maximum duration the event's active 
    /// state is allowed to have. Once the event is continuously active 
    /// for longer than this amount of time, the event deactivates.
    /// If you specify <see cref="TimeSpan.Zero"/>, the event 
    /// deactivates immediately after activating.</param>
    /// <param name="triggerMode">An 
    /// <see cref="ActivationInputEventTriggerMode"/> constant that 
    /// specifies when the event will trigger.</param>
    /// <param name="callback">Callback that will handle the event.</param>
    /// <returns>The newly created and registered 
    /// <see cref="ActivationInputEvent"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerMode"/> 
    /// is not a defined constant in an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    /// <seealso cref="InputObject"/>
    /// <seealso cref="ActivationInputEvent"/>
    public static ActivationInputEvent RegisterActivationInputEvent(
        this InputObject inputObject,
        Func<bool> activator,
        TimeSpan activationDelay, TimeSpan deactivationDelay,
        TimeSpan activeTimeout,
        ActivationInputEventTriggerMode triggerMode,
        ActivationInputEventHandler callback)
    {
        return RegisterActivationInputEvent(inputObject,
            activator, activationDelay, deactivationDelay, activeTimeout,
            triggerMode, (object?)null, callback);
    }


    /// <summary>
    /// Registers a new <see cref="ActivationInputEvent"/> that 
    /// triggers whenever the specified activator function activates 
    /// or deactivates.
    /// </summary>
    /// <param name="inputObject"><see cref="InputObject"/> where 
    /// the event will be registered.</param>
    /// <param name="activator">Function that will be called to 
    /// determine whether a custom condition is met.</param>
    /// <param name="activationDelay">Delay time the event will 
    /// wait after the activator function activates, until the 
    /// event triggers. If you specify <see cref="TimeSpan.Zero"/>, 
    /// the event triggers immediately.</param>
    /// <param name="deactivationDelay">Delay time the event will 
    /// wait after the activator function deactivates, until the 
    /// event triggers. If you specify <see cref="TimeSpan.Zero"/>, 
    /// the event triggers immediately.</param>
    /// <param name="triggerMode">An 
    /// <see cref="ActivationInputEventTriggerMode"/> constant that 
    /// specifies when the event will trigger.</param>
    /// <param name="callback">Callback that will handle the event.</param>
    /// <returns>The newly created and registered 
    /// <see cref="ActivationInputEvent"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerMode"/> 
    /// is not a defined constant in an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    /// <seealso cref="InputObject"/>
    /// <seealso cref="ActivationInputEvent"/>
    public static ActivationInputEvent RegisterActivationInputEvent(
        this InputObject inputObject,
        Func<bool> activator,
        TimeSpan activationDelay, TimeSpan deactivationDelay,
        ActivationInputEventTriggerMode triggerMode,
        ActivationInputEventHandler callback)
    {
        return RegisterActivationInputEvent(inputObject,
            activator, activationDelay, deactivationDelay, TimeSpan.MaxValue,
            triggerMode, (object?)null, callback);
    }


    /// <summary>
    /// Registers a new <see cref="ActivationInputEvent"/> that 
    /// triggers whenever the specified activator function activates 
    /// or deactivates.
    /// </summary>
    /// <param name="inputObject"><see cref="InputObject"/> where 
    /// the event will be registered.</param>
    /// <param name="activator">Function that will be called to 
    /// determine whether a custom condition is met.</param>
    /// <param name="activationDelay">Delay time the event will 
    /// wait after the activator function activates, until the 
    /// event triggers. If you specify <see cref="TimeSpan.Zero"/>, 
    /// the event triggers immediately.</param>
    /// <param name="triggerMode">An 
    /// <see cref="ActivationInputEventTriggerMode"/> constant that 
    /// specifies when the event will trigger.</param>
    /// <param name="callback">Callback that will handle the event.</param>
    /// <returns>The newly created and registered 
    /// <see cref="ActivationInputEvent"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerMode"/> 
    /// is not a defined constant in an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    /// <seealso cref="InputObject"/>
    /// <seealso cref="ActivationInputEvent"/>
    public static ActivationInputEvent RegisterActivationInputEvent(
        this InputObject inputObject,
        Func<bool> activator,
        TimeSpan activationDelay,
        ActivationInputEventTriggerMode triggerMode,
        ActivationInputEventHandler callback)
    {
        return RegisterActivationInputEvent(inputObject,
            activator, activationDelay, TimeSpan.Zero, TimeSpan.MaxValue,
            triggerMode, (object?)null, callback);
    }


    /// <summary>
    /// Registers a new <see cref="ActivationInputEvent"/> that 
    /// triggers whenever the specified activator function activates 
    /// or deactivates.
    /// </summary>
    /// <param name="inputObject"><see cref="InputObject"/> where 
    /// the event will be registered.</param>
    /// <param name="activator">Function that will be called to 
    /// determine whether a custom condition is met.</param>
    /// <param name="triggerMode">An 
    /// <see cref="ActivationInputEventTriggerMode"/> constant that 
    /// specifies when the event will trigger.</param>
    /// <param name="callback">Callback that will handle the event.</param>
    /// <returns>The newly created and registered 
    /// <see cref="ActivationInputEvent"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerMode"/> 
    /// is not a defined constant in an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    /// <seealso cref="InputObject"/>
    /// <seealso cref="ActivationInputEvent"/>
    public static ActivationInputEvent RegisterActivationInputEvent(
        this InputObject inputObject,
        Func<bool> activator,
        ActivationInputEventTriggerMode triggerMode,
        ActivationInputEventHandler callback)
    {
        return RegisterActivationInputEvent(inputObject,
            activator, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.MaxValue,
            triggerMode, (object?)null, callback);
    }


    /// <summary>
    /// Registers a new <see cref="ActivationInputEvent"/> that 
    /// triggers whenever the specified activator function activates 
    /// or deactivates, and uses 
    /// <see cref="ActivationInputEventTriggerMode.OnActivationAndDeactivation"/>
    /// as its trigger mode.
    /// </summary>
    /// <param name="inputObject"><see cref="InputObject"/> where 
    /// the event will be registered.</param>
    /// <param name="activator">Function that will be called to 
    /// determine whether a custom condition is met.</param>
    /// <param name="callback">Callback that will handle the event.</param>
    /// <returns>The newly created and registered 
    /// <see cref="ActivationInputEvent"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <seealso cref="InputObject"/>
    /// <seealso cref="ActivationInputEvent"/>
    public static ActivationInputEvent RegisterActivationInputEvent(
        this InputObject inputObject,
        Func<bool> activator,
        ActivationInputEventHandler callback)
    {
        return RegisterActivationInputEvent(inputObject,
            activator, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.MaxValue,
            ActivationInputEventTriggerMode.OnActivationAndDeactivation,
            (object?)null, callback);
    }


    /// <summary>
    /// Registers a new <see cref="DigitalButtonInputEvent{T}"/> 
    /// that is triggered when the specified button changes its state 
    /// from released to pressed, meaning the user has just tapped 
    /// the button.
    /// </summary>
    /// <typeparam name="T">A type deriving from <see cref="DigitalButton"/> 
    /// that specifies the type of the button that the event will listen to.
    /// </typeparam>
    /// <param name="inputObject">THe <see cref="InputObject"/> instance 
    /// where the event will be registered.</param>
    /// <param name="button">A <typeparamref name="T"/> object deriving
    /// from <see cref="DigitalButton"/>, that represents the button 
    /// the event will listen for.</param>
    /// <param name="callback">Callback that will be called when 
    /// the event is triggered.</param>
    /// <returns>The new <see cref="DigitalButtonInputEvent{T}"/> 
    /// instance that was registered.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="button"/> 
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    public static DigitalButtonInputEvent<T> RegisterButtonPressedEvent<T>(
        this InputObject inputObject, T button,
        DigitalButtonInputEventHandler<T> callback)
        where T : notnull, DigitalButton
    {
        if (inputObject is null)
            throw new ArgumentNullException(nameof(inputObject));
        if (button is null)
            throw new ArgumentNullException(nameof(button));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        DigitalButtonInputEvent<T> inputEvent = new(button,
            DigitalButtonInputEventMode.OnPress, TimeSpan.Zero);
        inputEvent.AddHandler((sender, e) => callback(sender, (DigitalButtonInputEventArgs<T>)e));
        inputObject.RegisterInputEvent(inputEvent);
        return inputEvent;
    }


    /// <summary>
    /// Registers a new <see cref="DigitalButtonInputEvent{T}"/> 
    /// that is triggered when the specified button changes its state 
    /// from pressed to released, meaning the user has just released 
    /// the button that was being pressed.
    /// </summary>
    /// <typeparam name="T">A type deriving from <see cref="DigitalButton"/> 
    /// that specifies the type of the button that the event will listen to.
    /// </typeparam>
    /// <param name="inputObject">THe <see cref="InputObject"/> instance 
    /// where the event will be registered.</param>
    /// <param name="button">A <typeparamref name="T"/> object deriving
    /// from <see cref="DigitalButton"/>, that represents the button 
    /// the event will listen for.</param>
    /// <param name="callback">Callback that will be called when the event 
    /// is triggered.</param>
    /// <returns>The new <see cref="DigitalButtonInputEvent{T}"/> 
    /// instance that was registered.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="button"/> 
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    public static DigitalButtonInputEvent<T> RegisterButtonReleasedEvent<T>(
        this InputObject inputObject, T button,
        DigitalButtonInputEventHandler<T> callback)
        where T : notnull, DigitalButton
    {
        if (inputObject is null)
            throw new ArgumentNullException(nameof(inputObject));
        if (button is null)
            throw new ArgumentNullException(nameof(button));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        DigitalButtonInputEvent<T> inputEvent = new(button,
            DigitalButtonInputEventMode.OnRelease, TimeSpan.Zero);
        inputEvent.AddHandler((sender, e) => callback(sender, (DigitalButtonInputEventArgs<T>)e));
        inputObject.RegisterInputEvent(inputEvent);
        return inputEvent;
    }


    /// <summary>
    /// Registers a new <see cref="DigitalButtonInputEvent{T}"/> 
    /// that is triggered when the specified button is held by the 
    /// specified duration.
    /// </summary>
    /// <typeparam name="T">A type deriving from <see cref="DigitalButton"/> 
    /// that specifies the type of the button that the event will listen to.
    /// </typeparam>
    /// <param name="inputObject">THe <see cref="InputObject"/> instance 
    /// where the event will be registered.</param>
    /// <param name="button">A <typeparamref name="T"/> object deriving
    /// from <see cref="DigitalButton"/>, that represents the button 
    /// the event will listen for.</param>
    /// <param name="holdDuration">The amount of time the user must 
    /// hold down the button for the event to trigger. If you specify 
    /// <see cref="TimeSpan.Zero"/>, this event will behave like a pressed 
    /// event.</param>
    /// <param name="callback">Callback that will be called when 
    /// the event is triggered.</param>
    /// <returns>The new <see cref="DigitalButtonInputEvent{T}"/> 
    /// instance that was registered.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="button"/> 
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The returned registered <see cref="DigitalButtonInputEvent{T}"/> 
    /// will be triggered once, when the user presses and holds the button 
    /// for the specified amount of time. Once the event is triggered, 
    /// it will only be triggered again after the user releases the 
    /// button and repeats the same action (pressing and holding the 
    /// button for <paramref name="holdDuration"/>).
    /// </remarks>
    public static DigitalButtonInputEvent<T> RegisterButtonHoldEvent<T>(
        this InputObject inputObject,
        T button, TimeSpan holdDuration,
        DigitalButtonInputEventHandler<T> callback)
        where T : notnull, DigitalButton
    {
        if (inputObject is null)
            throw new ArgumentNullException(nameof(inputObject));
        if (button is null)
            throw new ArgumentNullException(nameof(button));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        DigitalButtonInputEvent<T> inputEvent = new(button,
            DigitalButtonInputEventMode.OnHold, holdDuration);
        inputEvent.AddHandler((sender, e) => callback.Invoke(sender, (DigitalButtonInputEventArgs<T>)e));
        inputObject.RegisterInputEvent(inputEvent);
        return inputEvent;
    }


    /// <summary>
    /// Registers a new a <see cref="RepeatDigitalButtonInputEvent{T}"/> 
    /// that is triggered repeatedly while the specified button is held, and 
    /// uses the specified acceleration parameters for acceleration or 
    /// deceleration of repeat delay times.
    /// </summary>
    /// <typeparam name="T">A type deriving from <see cref="DigitalButton"/> 
    /// that specifies the type of the button that the event will listen to.
    /// </typeparam>
    /// <param name="inputObject"><see cref="InputObject"/> instance where the 
    /// event will be registered.</param>
    /// <param name="button">A <typeparamref name="T"/> object deriving from 
    /// <see cref="DigitalButton"/> to which the event will listen.</param>
    /// <param name="initialDelay">Amount of time the button must be held for 
    /// the repeating to start.</param>
    /// <param name="repeatDelay">Base amount of time to wait between each 
    /// repeat.</param>
    /// <param name="accelerationRatio">A number greater than 0, that specifies 
    /// the acceleration ratio of the <paramref name="repeatDelay"/> time that 
    /// will be applied on each triggering repeat. A value less than 1 causes 
    /// the repeats to be slower, more than 1 causes the repeats to be faster, 
    /// and 1 uses no acceleration or deceleration.</param>
    /// <param name="minRepeatDelay">When <paramref name="accelerationRatio"/> 
    /// is greater than 1, causing the repeat delay time to be shorter on each 
    /// triggering repeat, this specifies the minimum delay time allowed between 
    /// each repeat.</param>
    /// <param name="maxRepeatDelay">When <paramref name="accelerationRatio"/> 
    /// is lower than 1, causing the repeat delay time to be longer on each 
    /// triggering repeat, this specifies the maximum delay time allowed between 
    /// each repeat.</param>
    /// <param name="callback">Callback that will be called when the event is 
    /// triggered.</param>
    /// <returns>The new <see cref="RepeatDigitalButtonInputEvent{T}"/> that 
    /// was registered.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="button"/>
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="accelerationRatio"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="accelerationRatio"/> is equal to or lower than 0.</exception>
    /// <seealso cref="RepeatDigitalButtonInputEvent{T}"/>
    public static RepeatDigitalButtonInputEvent<T> RegisterButtonRepeatEvent<T>(
        this InputObject inputObject, T button,
        TimeSpan initialDelay, TimeSpan repeatDelay,
        float accelerationRatio, TimeSpan minRepeatDelay, TimeSpan maxRepeatDelay,
        DigitalButtonInputEventHandler<T> callback)
        where T : notnull, DigitalButton
    {
        if (inputObject is null)
            throw new ArgumentNullException(nameof(inputObject));
        if (button is null)
            throw new ArgumentNullException(nameof(button));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        RepeatDigitalButtonInputEvent<T> inputEvent = new(button,
            initialDelay, repeatDelay,
            accelerationRatio, minRepeatDelay, maxRepeatDelay);
        inputEvent.AddHandler((sender, e) => callback(sender, (DigitalButtonInputEventArgs<T>)e));
        inputObject.RegisterInputEvent(inputEvent);
        return inputEvent;
    }


    /// <summary>
    /// Registers a new a <see cref="RepeatDigitalButtonInputEvent{T}"/> 
    /// that is triggered repeatedly while the specified button is held.
    /// </summary>
    /// <typeparam name="T">A type deriving from <see cref="DigitalButton"/> 
    /// that specifies the type of the button that the event will listen to.
    /// </typeparam>
    /// <param name="inputObject"><see cref="InputObject"/> instance where the 
    /// event will be registered.</param>
    /// <param name="button">A <typeparamref name="T"/> object deriving from 
    /// <see cref="DigitalButton"/> to which the event will listen.</param>
    /// <param name="initialDelay">Amount of time the button must be held for 
    /// the repeating to start.</param>
    /// <param name="repeatDelay">Amount of time to wait between each 
    /// repeat.</param>
    /// <param name="callback">Callback that will be called when the event is 
    /// triggered.</param>
    /// <returns>The new <see cref="RepeatDigitalButtonInputEvent{T}"/> that 
    /// was registered.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputObject"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="button"/>
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <seealso cref="RepeatDigitalButtonInputEvent{T}"/>
    public static RepeatDigitalButtonInputEvent<T> RegisterButtonRepeatEvent<T>(
        this InputObject inputObject, T button,
        TimeSpan initialDelay, TimeSpan repeatDelay,
        DigitalButtonInputEventHandler<T> callback)
        where T : notnull, DigitalButton
    {
        return RegisterButtonRepeatEvent(inputObject, button, initialDelay, repeatDelay,
            1f, TimeSpan.Zero, TimeSpan.MaxValue,
            callback);
    }

    #endregion Methods


}
