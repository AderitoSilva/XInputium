using System;
using System.Runtime.CompilerServices;

namespace XInputium;

/// <summary>
/// Implements an <see cref="InputEvent"/> that activates/deactivates 
/// when a custom condition is met/unmet, and triggers whenever it 
/// is activated, deactivated or while it is active, depending on the 
/// triggering mode you specify.
/// </summary>
/// <remarks>
/// <see cref="ActivationInputEvent"/> is a very versatile 
/// <see cref="InputEvent"/> that enables you to set up very specific 
/// events without requiring too much code. 
/// <br/><br/>
/// <see cref="ActivationInputEvent"/> can have two states: active 
/// or inactive. Depending on the value of <see cref="TriggerMode"/> 
/// property, it triggers the event only when its state changes from 
/// inactive to active (activation), only when the state changes from 
/// active to inactive (deactivation), whenever the state changes 
/// (activation or deactivation), while it is active, or never.
/// <br/><br/>
/// <see cref="ActivationInputEvent"/> determines its state by calling 
/// a function your provide. That function must return 
/// <see langword="true"/> to indicate the active state, or 
/// <see langword="false"/> to indicate the inactive state. The function 
/// is called on every update from the input system hosting the event.
/// <br/><br/>
/// Although your custom function is what determines the state of the 
/// <see cref="ActivationInputEvent"/>, depending on several properties 
/// of the <see cref="ActivationInputEvent"/> instance, the change of 
/// state may not be immediate. The <see cref="ActivationDelay"/> 
/// property specifies a delay between the moment your custom function
/// starts returning <see langword="true"/> and the moment the event 
/// will activate (change its state to active). If, during this delay, 
/// your function returns <see langword="false"/>, 
/// <see cref="ActivationInputEvent"/> will abort the activation 
/// process. Similarly to <see cref="ActivationDelay"/> property, there 
/// is <see cref="DeactivationDelay"/> property, that works similarly, 
/// but for the deactivation.
/// <br/><br/>
/// There's also <see cref="ActiveTimeout"/> property, that specifies 
/// the maximum duration the <see cref="ActivationInputEvent"/> will 
/// ever be active — once the activation occurs, if this timeout expires, 
/// the event deactivates, even if your custom function keeps returning 
/// <see langword="true"/>. Then, when your function returns 
/// <see langword="false"/> again, activation is allowed to occur again.
/// <br/><br/>
/// To enable you to pass information between the code that registers 
/// the event and the code that handles the event, 
/// <see cref="ActivationInputEvent"/> provides the <see cref="Parameter"/> 
/// property, that can be set with a custom object you specify.
/// <br/><br/>
/// The use cases of <see cref="ActivationInputEvent"/> are many. You 
/// can use it to get notified when a specific condition starts being 
/// met, when it stops being met, both, or while it is met. You can also 
/// fine-tune the <see cref="ActivationDelay"/> and 
/// <see cref="DeactivationDelay"/> properties to ignore short moments of 
/// unmet conditions. A valid example would be to use 
/// <see cref="ActivationInputEvent"/> to notify you after a specific 
/// button or set of buttons is held by, at least, a specific amount of 
/// time. Note that, for single buttons, you can use the specialized 
/// <see cref="DigitalButtonInputEvent{T}"/> class to achieve the same 
/// functionality. See <see cref="DigitalButtonInputEvent{T}"/> for more 
/// information.
/// </remarks>
/// <seealso cref="InputEvent"/>
/// <seealso cref="ActivationInputEventArgs"/>
/// <seealso cref="ActivationInputEventTriggerMode"/>
public class ActivationInputEvent : InputEvent
{


    #region Fields

    private const ActivationInputEventTriggerMode DefaultTriggerMode
        = ActivationInputEventTriggerMode.OnActivationAndDeactivation;  // The default value for TriggerMode property.
    private static readonly TimeSpan s_DefaultActiveTimeout = TimeSpan.MaxValue;  // The default value for ActiveTimeout property.

    private TimeSpan _activationDelay = TimeSpan.Zero;  // Store for the value of ActivationDelay property.
    private TimeSpan _deactivationDelay = TimeSpan.Zero;  // Store for the value of DeactivationDelay property.
    private TimeSpan _activeTimeout = s_DefaultActiveTimeout;  // Store for the value of ActiveTimeout property.
    private ActivationInputEventTriggerMode _triggerMode = DefaultTriggerMode;  // Store for the value of TriggerMode property.

    private readonly ActivationInputEventArgs _eventArgs;  // Singleton event arguments instance, that we will use.
    private readonly Func<bool> _activator;  // User defined function that is used to determine the active/inactive state of the event.
    private TimeSpan _totalTime = TimeSpan.Zero;  // Counter that accumulates the time elapsed between hot-points in the state checking.
    private bool _lastActivatorState = false;  // Stores the most recent state reported by the activation function that is different from the last one.
    private bool _hasTimedOut = false;  // Indicates if the active state has timed out.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an <see cref="ActivationInputEvent"/> 
    /// class, that uses the specified activator function, activation and 
    /// deactivation delays, an active state timeout, a specific trigger
    /// mode and a custom parameter object.
    /// </summary>
    /// <param name="activator">A function that is called on every update
    /// that determines the active/inactive state the event must have. 
    /// This is usually a function that evaluates a specific condition.</param>
    /// <param name="activationDelay">The amount of delay since the 
    /// <paramref name="activator"/> function starts returning 
    /// <see langword="true"/>, until the event activates. If you specify 
    /// <see cref="TimeSpan.Zero"/>, the event triggers immediately upon 
    /// activation.</param>
    /// <param name="deactivationDelay">The amount of delay since the 
    /// <paramref name="activator"/> function starts returning 
    /// <see langword="false"/>, until the event deactivates. If you specify 
    /// <see cref="TimeSpan.Zero"/>, the event triggers immediately upon 
    /// deactivation.</param>
    /// <param name="activeTimeout">The maximum amount of time the event 
    /// will be allowed to be active. After this expires, the event will
    /// deactivate, regardless of other conditions. If you specify 
    /// <see cref="TimeSpan.Zero"/>, the event deactivates immediately 
    /// after activation.</param>
    /// <param name="triggerMode">An 
    /// <see cref="ActivationInputEventTriggerMode"/> constant that 
    /// specifies when the event can trigger.</param>
    /// <param name="parameter">A custom object that will be passed with 
    /// the event arguments to event handlers. You can specify 
    /// <see langword="null"/> to use no parameter object.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerMode"/>
    /// is not a defined constant in an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    public ActivationInputEvent(Func<bool> activator,
        TimeSpan activationDelay, TimeSpan deactivationDelay,
        TimeSpan activeTimeout,
        ActivationInputEventTriggerMode triggerMode,
        object? parameter)
        : base()
    {
        if (activator is null)
            throw new ArgumentNullException(nameof(activator));
        if (!Enum.IsDefined(triggerMode))
            throw new ArgumentException($"'{triggerMode}' is not a defined " +
                $"constant of an '{nameof(ActivationInputEventTriggerMode)}' enumeration.",
                nameof(triggerMode));

        ActivationDelay = activationDelay;
        DeactivationDelay = deactivationDelay;
        ActiveTimeout = activeTimeout;
        TriggerMode = triggerMode;
        Parameter = parameter;
        _activator = activator;
        _eventArgs = new(this);
    }


    /// <summary>
    /// Initializes a new instance of an <see cref="ActivationInputEvent"/> 
    /// class, that uses the specified activator function, activation and 
    /// deactivation delays, an active state timeout, and a specific trigger 
    /// mode.
    /// </summary>
    /// <param name="activator">A function that is called on every update
    /// that determines the active/inactive state the event must have. 
    /// This is usually a function that evaluates a specific condition.</param>
    /// <param name="activationDelay">The amount of delay since the 
    /// <paramref name="activator"/> function starts returning 
    /// <see langword="true"/>, until the event activates. If you specify 
    /// <see cref="TimeSpan.Zero"/>, the event triggers immediately upon 
    /// activation.</param>
    /// <param name="deactivationDelay">The amount of delay since the 
    /// <paramref name="activator"/> function starts returning 
    /// <see langword="false"/>, until the event deactivates. If you specify 
    /// <see cref="TimeSpan.Zero"/>, the event triggers immediately upon 
    /// deactivation.</param>
    /// <param name="activeTimeout">The maximum amount of time the event 
    /// will be allowed to be active. After this expires, the event will
    /// deactivate, regardless of other conditions. If you specify 
    /// <see cref="TimeSpan.Zero"/>, the event deactivates immediately 
    /// after activation.</param>
    /// <param name="triggerMode">An 
    /// <see cref="ActivationInputEventTriggerMode"/> constant that 
    /// specifies when the event can trigger.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerMode"/>
    /// is not a defined constant in an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    public ActivationInputEvent(Func<bool> activator,
        TimeSpan activationDelay, TimeSpan deactivationDelay,
        TimeSpan activeTimeout,
        ActivationInputEventTriggerMode triggerMode)
        : this(activator, activationDelay, deactivationDelay,
              activeTimeout, triggerMode, null)
    {

    }


    /// <summary>
    /// Initializes a new instance of an <see cref="ActivationInputEvent"/> 
    /// class, that uses the specified activator function, activation and 
    /// deactivation delays, and a specific trigger mode.
    /// </summary>
    /// <param name="activator">A function that is called on every update
    /// that determines the active/inactive state the event must have. 
    /// This is usually a function that evaluates a specific condition.</param>
    /// <param name="activationDelay">The amount of delay since the 
    /// <paramref name="activator"/> function starts returning 
    /// <see langword="true"/>, until the event activates. If you specify 
    /// <see cref="TimeSpan.Zero"/>, the event triggers immediately upon 
    /// activation.</param>
    /// <param name="deactivationDelay">The amount of delay since the 
    /// <paramref name="activator"/> function starts returning 
    /// <see langword="false"/>, until the event deactivates. If you specify 
    /// <see cref="TimeSpan.Zero"/>, the event triggers immediately upon 
    /// deactivation.</param>
    /// <param name="triggerMode">An 
    /// <see cref="ActivationInputEventTriggerMode"/> constant that 
    /// specifies when the event can trigger.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerMode"/>
    /// is not a defined constant in an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    public ActivationInputEvent(Func<bool> activator,
        TimeSpan activationDelay, TimeSpan deactivationDelay,
        ActivationInputEventTriggerMode triggerMode)
        : this(activator, activationDelay, deactivationDelay,
              s_DefaultActiveTimeout, triggerMode, null)
    {

    }


    /// <summary>
    /// Initializes a new instance of an <see cref="ActivationInputEvent"/> 
    /// class, that uses the specified activator function, and a specific 
    /// trigger mode.
    /// </summary>
    /// <param name="activator">A function that is called on every update
    /// that determines the active/inactive state the event must have. 
    /// This is usually a function that evaluates a specific condition.</param>
    /// <param name="triggerMode">An 
    /// <see cref="ActivationInputEventTriggerMode"/> constant that 
    /// specifies when the event can trigger.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerMode"/>
    /// is not a defined constant in an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    public ActivationInputEvent(Func<bool> activator,
        ActivationInputEventTriggerMode triggerMode)
        : this(activator, TimeSpan.Zero, TimeSpan.Zero,
              s_DefaultActiveTimeout, triggerMode, null)
    {

    }


    /// <summary>
    /// Initializes a new instance of an <see cref="ActivationInputEvent"/> 
    /// class, that uses the specified activator function, and uses 
    /// <see cref="ActivationInputEventTriggerMode.OnActivationAndDeactivation"/> 
    /// as its trigger mode.
    /// </summary>
    /// <param name="activator">A function that is called on every update
    /// that determines the active/inactive state the event must have. 
    /// This is usually a function that evaluates a specific condition.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activator"/> is <see langword="null"/>.</exception>
    public ActivationInputEvent(Func<bool> activator)
        : this(activator, DefaultTriggerMode)
    {

    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets or sets a <see cref="TimeSpan"/> that represents the minimum 
    /// amount of time by which the activator function must return 
    /// <see langword="true"/> for the event to change from inactive to 
    /// active state, triggering the event on activation.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/> object.
    /// The default value is <see cref="TimeSpan.Zero"/>.</value>
    /// <remarks>
    /// When the activation function starts returning <see langword="true"/>, 
    /// the event will only be triggered if the function keeps returning 
    /// <see langword="true"/> continuously during, at least, the amount of 
    /// time specified by this property. This is useful for conditions that 
    /// need to hold for an event to be triggered (for instance, a button hold 
    /// event).
    /// </remarks>
    /// <seealso cref="DeactivationDelay"/>
    /// <seealso cref="ActiveTimeout"/>
    /// <seealso cref="TriggerMode"/>
    public TimeSpan ActivationDelay
    {
        get => _activationDelay;
        set => _activationDelay = value <= TimeSpan.Zero ? TimeSpan.Zero : value;
    }


    /// <summary>
    /// Gets or sets a <see cref="TimeSpan"/> that represents the minimum 
    /// amount of time by which the activator function must return 
    /// <see langword="false"/> for the event to change from active to 
    /// inactive state, triggering the event on deactivation.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/> object.
    /// The default value is <see cref="TimeSpan.Zero"/>.</value>
    /// <remarks>
    /// When the activation function starts returning <see langword="false"/>, 
    /// the event will only be triggered if the function keeps returning 
    /// <see langword="false"/> continuously during, at least, the amount of 
    /// time specified by this property. This is useful for conditions that 
    /// must ignore short periods of unmet criteria (for instance, an event 
    /// that triggers when a joystick stops moving, but needs to ignore very
    /// short no-movement periods).
    /// </remarks>
    /// <seealso cref="ActivationDelay"/>
    /// <seealso cref="TriggerMode"/>
    public TimeSpan DeactivationDelay
    {
        get => _deactivationDelay;
        set => _deactivationDelay = value <= TimeSpan.Zero ? TimeSpan.Zero : value;
    }


    /// <summary>
    /// Gets or sets a <see cref="TimeSpan"/> that represents the maximum 
    /// amount of time the event will be active, even when the activator 
    /// function keeps returning <see langword="true"/>. In other words, 
    /// this specifies the maximum allowed active state duration for the 
    /// event.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/> object. If you specify 
    /// <see cref="TimeSpan.Zero"/>, the event will deactivate immediately 
    /// after it activates.
    /// The default value is <see cref="TimeSpan.MaxValue"/>.</value>
    /// <remarks>
    /// After the event is activated, if it doesn't get deactivated during 
    /// the period of time specified by this property, it is forced to 
    /// deactivate when this period expires, even if other properties specify 
    /// otherwise. This can be useful in situations where you need to limit 
    /// the duration a specific criteria is valid.
    /// </remarks>
    /// <seealso cref="ActivationDelay"/>
    /// <seealso cref="DeactivationDelay"/>
    public TimeSpan ActiveTimeout
    {
        get => _activeTimeout;
        set => _activeTimeout = value <= TimeSpan.Zero ? TimeSpan.Zero : value;
    }


    /// <summary>
    /// Gets or sets an <see cref="ActivationInputEventTriggerMode"/> constant 
    /// that specifies when the <see cref="ActivationInputEvent"/> must 
    /// trigger.
    /// </summary>
    /// <value>An <see cref="ActivationInputEventTriggerMode"/> constant.
    /// The default value is 
    /// <see cref="ActivationInputEventTriggerMode.OnActivationAndDeactivation"/>.</value>
    /// <exception cref="ArgumentException">The value being set to the 
    /// property is not a defined constant of an 
    /// <see cref="ActivationInputEventTriggerMode"/> enumeration.</exception>
    /// <seealso cref="ActivationInputEventTriggerMode"/>
    public ActivationInputEventTriggerMode TriggerMode
    {
        get => _triggerMode;
        set
        {
            if (!Enum.IsDefined(value))
                throw new ArgumentException($"'{value}' is not a defined " +
                    $"constant of an '{nameof(ActivationInputEventTriggerMode)}' enumeration.",
                    nameof(value));
            _triggerMode = value;
        }
    }


    /// <summary>
    /// Gets or sets the user defined object associated with the 
    /// current <see cref="ActivationInputEvent"/> instance.
    /// </summary>
    /// <remarks>
    /// You can use this property to set a custom object, that is passed 
    /// to the <see cref="ActivationInputEventArgs"/> instance when the 
    /// event is triggered. See 
    /// <see cref="ActivationInputEventArgs.Parameter"/> for more 
    /// information.
    /// <br/><br/>
    /// This property can be useful when you need to pass information
    /// from the code that is registering the 
    /// <see cref="ActivationInputEvent"/> to the code that is handling
    /// it when it is triggered.
    /// </remarks>
    /// <seealso cref="ActivationInputEventArgs.Parameter"/>
    public object? Parameter { get; set; }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the 
    /// <see cref="ActivationInputEvent"/> is currently active.
    /// </summary>
    /// <seealso cref="CurrentStateDuration"/>
    /// <seealso cref="PreviousStateDuration"/>
    /// <seealso cref="ActivationInputEventArgs.IsActive"/>
    public bool IsActive { get; private set; }


    /// <summary>
    /// Gets the amount of input time spent on the previous 
    /// active/inactive state of the <see cref="ActivationInputEvent"/>.
    /// </summary>
    /// <returns>The duration of the last active/inactive state, before 
    /// the current state. If that state was 'active', the returned value 
    /// is never greater than <see cref="ActiveTimeout"/>.</returns>
    /// <remarks>
    /// This property returns the amount of time elapsed between 
    /// the second to last and the last change to the value of 
    /// <see cref="IsActive"/> property. For instance, if the current 
    /// state is 'inactive', this property returns the duration of the 
    /// most recent active state. The time information is in input time,
    /// as measured by the input code that is hosting the 
    /// <see cref="ActivationInputEvent"/>.
    /// </remarks>
    /// <seealso cref="CurrentStateDuration"/>
    /// <seealso cref="IsActive"/>
    /// <seealso cref="ActivationInputEventArgs.PreviousStateDuration"/>
    public TimeSpan PreviousStateDuration { get; private set; } = TimeSpan.Zero;


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
    /// </remarks>
    /// <seealso cref="PreviousStateDuration"/>
    /// <seealso cref="IsActive"/>
    /// <seealso cref="ActivationInputEventArgs.CurrentStateDuration"/>
    public TimeSpan CurrentStateDuration { get; private set; } = TimeSpan.Zero;

    #endregion Properties


    #region Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RaiseEvent()
    {
        Raise(this, _eventArgs);
    }


    /// <summary>
    /// Updates the event logic and reevaluates triggering conditions, 
    /// and, if these conditions are met, triggers the event.
    /// Overrides <see cref="InputEvent.OnUpdate(TimeSpan)"/>.
    /// </summary>
    /// <param name="time">Amount of time elapsed since the 
    /// last call to <see cref="InputEvent.Update(TimeSpan)"/> method.</param>
    protected override void OnUpdate(TimeSpan time)
    {
        if (time <= TimeSpan.Zero)
            time = TimeSpan.Zero;

        // Determine the activator state.
        bool activatorState = _activator();
        if (activatorState != _lastActivatorState)
        {
            _totalTime = time;
            _hasTimedOut = false;
        }
        else
        {
            _totalTime += time;
        }

        try
        {
            // The event is inactive.
            if (!IsActive)
            {
                if (activatorState && _totalTime >= ActivationDelay
                    && !_hasTimedOut)
                {
                    _totalTime = TimeSpan.Zero;
                    _hasTimedOut = false;
                    PreviousStateDuration = CurrentStateDuration;
                    CurrentStateDuration = _totalTime;
                    IsActive = true;
                    if (TriggerMode == ActivationInputEventTriggerMode.OnActivationAndDeactivation
                        || TriggerMode == ActivationInputEventTriggerMode.OnActivation)
                    {
                        RaiseEvent();
                    }
                }
            }
            // The event is active.
            else
            {
                if ((!activatorState && _totalTime >= DeactivationDelay)
                    || (_totalTime >= ActiveTimeout && !_hasTimedOut))
                {
                    _totalTime = TimeSpan.Zero;
                    _hasTimedOut = true;
                    PreviousStateDuration = CurrentStateDuration;
                    if (PreviousStateDuration > ActiveTimeout)
                    {
                        // Our active state time must not be greater than the timeout.
                        PreviousStateDuration = ActiveTimeout;
                    }
                    CurrentStateDuration = _totalTime;
                    IsActive = false;
                    if (TriggerMode == ActivationInputEventTriggerMode.OnActivationAndDeactivation
                        || TriggerMode == ActivationInputEventTriggerMode.OnDeactivation)
                    {
                        RaiseEvent();
                    }
                }
            }
            // Trigger the event based on the determine state, if
            // we are using a continuous trigger mode.
            if (TriggerMode == ActivationInputEventTriggerMode.WhileActive
                && IsActive)
            {
                RaiseEvent();
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            CurrentStateDuration += time;
            _lastActivatorState = activatorState;
        }
    }

    #endregion Methods


}
