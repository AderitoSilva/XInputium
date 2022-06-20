using System;

namespace XInputium;

/// <summary>
/// Implements an <see cref="InputEvent"/> that activates/deactivates 
/// when a custom condition is met/unmet, and triggers whenever it 
/// is activated or deactivated.
/// </summary>
/// <seealso cref="InputEvent"/>
public class ActivationInputEvent : InputEvent
{


    #region Fields

    private const ActivationInputEventTriggerMode DefaultTriggerMode
        = ActivationInputEventTriggerMode.OnActivationAndDeactivation;
    private static TimeSpan s_DefaultActiveTimeout = TimeSpan.MaxValue;

    private TimeSpan _activationDelay = TimeSpan.Zero;
    private TimeSpan _deactivationDelay = TimeSpan.Zero;
    private TimeSpan _activeTimeout = s_DefaultActiveTimeout;
    private ActivationInputEventTriggerMode _triggerMode = DefaultTriggerMode;

    private readonly ActivationInputEventArgs _eventArgs;
    private readonly Func<bool> _activator;
    private TimeSpan _totalTime = TimeSpan.Zero;
    private bool _lastActivatorState = false;
    private bool _hasTimedOut = false;

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
    public object? Parameter { get; set; }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the 
    /// <see cref="ActivationInputEvent"/> is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    #endregion Properties


    #region Methods

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
                    IsActive = true;
                    if (TriggerMode == ActivationInputEventTriggerMode.OnActivationAndDeactivation
                        || TriggerMode == ActivationInputEventTriggerMode.OnActivation)
                    {
                        Raise(this, _eventArgs);
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
                    _hasTimedOut = _totalTime >= ActivationDelay;
                    IsActive = false;
                    if (TriggerMode == ActivationInputEventTriggerMode.OnActivationAndDeactivation
                        || TriggerMode == ActivationInputEventTriggerMode.OnDeactivation)
                    {
                        Raise(this, _eventArgs);
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _lastActivatorState = activatorState;
        }
    }

    #endregion Methods


}
