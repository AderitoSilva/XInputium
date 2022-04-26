using System;

namespace XInputium;

/// <summary>
/// Implements an <see cref="InputEvent"/> that monitors the state
/// of a <see cref="DigitalButton"/> derived class, and repeatedly 
/// triggers while the associated button is held.
/// </summary>
/// <typeparam name="T"><see cref="DigitalButton"/> or a type derived 
/// from <see cref="DigitalButton"/>, that is the type of the button 
/// monitored by the input event.</typeparam>
/// <remarks>
/// <see cref="RepeatDigitalButtonInputEvent{T}"/> provides a way 
/// for adding button repeating functionality to your game or 
/// application. Once the user starts pressing and holding the button, 
/// the following happens while the button is held:
/// <br/>1. The event is immediately triggered.
/// <br/>2. The event waits for an optional <i>initial delay</i> (specified at 
/// <see cref="InitialDelay"/> property). If no <i>initial delay</i> is 
/// used, skips to step 4.
/// <br/>3. The event triggers.
/// <br/>4. Waits for the <i>repeat delay time</i> (specified at 
/// <see cref="RepeatDelay"/> property).
/// <br/>5. Repeats from step 3.
/// <br/><br/>
/// At step 4, the <i>repeat delay time</i> is affected by the optional 
/// <i>acceleration ratio</i> (specified at <see cref="AccelerationRatio"/> 
/// property), which either decreases (accelerates) or increases (decelerates) 
/// the <i>repeat delay time</i>. On each repeat, this time is divided by the 
/// value of <see cref="AccelerationRatio"/> to perform the acceleration or 
/// deceleration. When accelerating, the effective <i>repeat delay time</i> 
/// will be clamped in order to never be less than 
/// <see cref="MinimumRepeatDelay"/>. Similarly, when decelerating, it will 
/// be clamped in order to never be greater than 
/// <see cref="MaximumRepeatDelay"/>.
/// <br/><br/>
/// You can get the current <i>repeat delay time</i> using 
/// <see cref="CurrentRepeatDelay"/> property. To determine if the 
/// <see cref="RepeatDigitalButtonInputEvent{T}"/> is currently repeating, 
/// use <see cref="IsRepeating"/> property. To get the number of triggered 
/// repeats since the user pressed the button, use <see cref="RepeatCount"/> 
/// property.
/// <br/><br/>
/// <see cref="RepeatDigitalButtonInputEvent{T}"/> is useful in scenarios 
/// where you need a button to repeatedly trigger as the user holds the 
/// associated physical button. This is common in the UI of many game titles, 
/// usually in menus, lists and controls that support items cycling. Although 
/// such functionality is usually implemented in the UI layer,
/// <see cref="RepeatDigitalButtonInputEvent{T}"/> enables you to get this 
/// functionality directly from the input layer.
/// </remarks>
/// <seealso cref="InputEvent"/>
/// <seealso cref="DigitalButton"/>
/// <seealso cref="DigitalButtonInputEvent{T}"/>
public class RepeatDigitalButtonInputEvent<T> : InputEvent
    where T : notnull, DigitalButton
{


    #region Fields

    private readonly DigitalButtonInputEventArgs<T> _eventArgs;  // Cached event arguments for the event.

    private TimeSpan _initialDelay = TimeSpan.Zero;  // Store for the value of InitialDelay property.
    private TimeSpan _repeatDelay = TimeSpan.Zero;  // Store for the value of RepeatDelay property.
    private float _accelerationRatio = 1f;  // Store for the value of AccelerationRatio property.
    private TimeSpan _minimumRepeatDelay;  // Store for the value of MinimumRepeatDelay property.
    private TimeSpan _maximumRepeatDelay;  // Store for the value of MaximumRepeatDelay property.

    private bool _wasPressed = false;  // Was the button pressed, on the last update?
    private TimeSpan _cumulativeTime = TimeSpan.Zero;  // Amount of time accumulated since the button tap or the last repeat.
    private bool _isRepeating = false;  // Is the event currently in repeating mode?
    private int _repeatCount = 0;  // Number of repeats since the button was tapped.
    private float _capturedAccelerationRatio = 1f;  // AccelerationRatio property value, captured when the repeating started.
    private TimeSpan _currentRepeatDelay = TimeSpan.Zero;  // The current, potentially accelerated, repeat delay.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of a <see cref="RepeatDigitalButtonInputEvent{T}"/>
    /// class, that is associated with the specified button, uses the specified delay 
    /// times and accelerates its repeat delay using the specified acceleration 
    /// parameters.
    /// </summary>
    /// <param name="button"><typeparamref name="T"/> instance that 
    /// represents the button of which state will be monitored by 
    /// the <see cref="RepeatDigitalButtonInputEvent{T}"/> instance.</param>
    /// <param name="initialDelay">Amount of time the user must hold 
    /// the button before the event triggering starts repeating.</param>
    /// <param name="repeatDelay">Time to wait between each event triggering 
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="button"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="accelerationRatio"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="accelerationRatio"/> is equal to or lower than 0.</exception>
    public RepeatDigitalButtonInputEvent(T button,
        TimeSpan initialDelay, TimeSpan repeatDelay,
        float accelerationRatio, TimeSpan minRepeatDelay, TimeSpan maxRepeatDelay)
        : base()
    {
        if (button is null)
            throw new ArgumentNullException(nameof(button));
        if (float.IsNaN(accelerationRatio))
            throw new ArgumentException($"'{float.NaN}' is not a valid " +
                $"value for '{nameof(accelerationRatio)}' parameter.",
                nameof(accelerationRatio));
        if (accelerationRatio <= 0f)
            throw new ArgumentOutOfRangeException(nameof(accelerationRatio),
                $"'{accelerationRatio}' must be greater than 0.");

        Button = button;
        InitialDelay = initialDelay;
        RepeatDelay = repeatDelay;
        AccelerationRatio = accelerationRatio;
        MinimumRepeatDelay = minRepeatDelay;
        MaximumRepeatDelay = maxRepeatDelay;

        _eventArgs = new(this, button);
    }


    /// <summary>
    /// Initializes a new instance of a <see cref="RepeatDigitalButtonInputEvent{T}"/>
    /// class, that is associated with the specified button 
    /// and uses the specified delay times.
    /// </summary>
    /// <param name="button"><typeparamref name="T"/> instance that 
    /// represents the button of which state will be monitored by 
    /// the <see cref="RepeatDigitalButtonInputEvent{T}"/> instance.</param>
    /// <param name="initialDelay">Amount of time the user must hold 
    /// the button before the event triggering starts repeating.</param>
    /// <param name="repeatDelay">Time to wait between each event triggering 
    /// repeat.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="button"/> is <see langword="null"/>.</exception>
    public RepeatDigitalButtonInputEvent(T button,
        TimeSpan initialDelay, TimeSpan repeatDelay)
        : this(button, initialDelay, repeatDelay,
              1f, TimeSpan.Zero, TimeSpan.MaxValue)
    {

    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the <typeparamref name="T"/> instance that represents the 
    /// button associated with the current input event.
    /// </summary>
    public T Button { get; }


    /// <summary>
    /// Gets or sets the amount of time the user must press the button 
    /// before the button starts repeating.
    /// </summary>
    public TimeSpan InitialDelay
    {
        get => _initialDelay;
        set => _initialDelay = value > TimeSpan.Zero ? value : TimeSpan.Zero;
    }


    /// <summary>
    /// Gets or sets the amount of time to wait between each repeat.
    /// </summary>
    /// <seealso cref="InitialDelay"/>
    public TimeSpan RepeatDelay
    {
        get => _repeatDelay;
        set => _repeatDelay = value > TimeSpan.Zero ? value : TimeSpan.Zero;
    }


    /// <summary>
    /// Gets or sets the value by which <see cref="RepeatDelay"/> 
    /// is divided, during the acceleration period.
    /// </summary>
    /// <value>A number greater than 0, where less than 1 increases 
    /// the amount of time between each repeat, more than 1 
    /// decreases this time, and 1 has no effect.
    /// The default value is 1.</value>
    /// <exception cref="ArgumentException">The value being set to the 
    /// property is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The value being 
    /// set to the property is equal to or less than 0.</exception>
    /// <seealso cref="RepeatDelay"/>
    public float AccelerationRatio
    {
        get => _accelerationRatio;
        set
        {
            if (float.IsNaN(value))
                throw new ArgumentException($"'{float.NaN}' is not a " +
                    $"valid value for '{nameof(AccelerationRatio)}' property.",
                    nameof(value));
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(nameof(value));

            _accelerationRatio = value;
        }
    }


    /// <summary>
    /// Gets or sets the minimum repeat delay time allowed for the 
    /// decelerated initial repeat delay.
    /// </summary>
    /// <value>A positive <see cref="TimeSpan"/> value. 
    /// The default value is <see cref="TimeSpan.Zero"/>.</value>
    /// <seealso cref="MaximumRepeatDelay"/>
    /// <seealso cref="RepeatDelay"/>
    public TimeSpan MinimumRepeatDelay
    {
        get => _minimumRepeatDelay;
        set => _minimumRepeatDelay = value > TimeSpan.Zero ? value : TimeSpan.Zero;
    }


    /// <summary>
    /// Gets or sets the maximum repeat delay time allowed for the 
    /// accelerated initial repeat delay.
    /// </summary>
    /// <value>A positive <see cref="TimeSpan"/> value. 
    /// The default value is <see cref="TimeSpan.MaxValue"/>.</value>
    /// <seealso cref="MinimumRepeatDelay"/>
    /// <seealso cref="RepeatDelay"/>
    public TimeSpan MaximumRepeatDelay
    {
        get => _maximumRepeatDelay;
        set => _maximumRepeatDelay = value > TimeSpan.Zero ? value : TimeSpan.Zero;
    }


    /// <summary>
    /// Gets the calculated repeat delay currently in effect, 
    /// which can potentially be accelerated or decelerated.
    /// </summary>
    /// <returns>If <see cref="IsRepeating"/> is <see langword="true"/>, 
    /// returns the current effective repeat delay; otherwise, returns 
    /// the value of <see cref="RepeatDelay"/>.</returns>
    /// <seealso cref="RepeatDelay"/>
    /// <seealso cref="AccelerationRatio"/>
    public TimeSpan CurrentRepeatDelay
        => IsRepeating ? _currentRepeatDelay : RepeatDelay;


    /// <summary>
    /// Gets a <see cref="bool"/> indicating if the input event
    /// is currently repeating the button press.
    /// </summary>
    /// <seealso cref="RepeatCount"/>
    public bool IsRepeating => _isRepeating;


    /// <summary>
    /// Gets the number of triggering repeats since the button 
    /// was tapped.
    /// </summary>
    /// <seealso cref="IsRepeating"/>
    public int RepeatCount => _repeatCount;

    #endregion Properties


    #region Methods

    /// <summary>
    /// Evaluates event logic and, if conditions are met, triggers 
    /// the event. Overrides 
    /// <see cref="InputEvent.OnUpdate(TimeSpan)"/> method.
    /// </summary>
    /// <param name="time">Amount of time elapsed since the last 
    /// update operation.</param>
    protected override void OnUpdate(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
            time = TimeSpan.Zero;

        try
        {
            if (Button.IsPressed)
            {
                _cumulativeTime += time;
                // The button was just tapped — that is, changed its state from released to pressed.
                if (!_wasPressed)
                {
                    _cumulativeTime = TimeSpan.Zero;
                    Raise(this, _eventArgs);
                }
                // The button is repeating.
                else if (_isRepeating)
                {
                    if (_cumulativeTime >= _currentRepeatDelay)
                    {
                        _cumulativeTime -= _currentRepeatDelay;
                        if (_repeatCount < int.MaxValue)
                        {
                            _repeatCount++;
                        }
                        // Calculate the next repeat delay, if necessary.
                        if (_capturedAccelerationRatio != 1f && _capturedAccelerationRatio > 0f)
                        {
                            _currentRepeatDelay /= _capturedAccelerationRatio;
                            if (_capturedAccelerationRatio > 1f
                                && _currentRepeatDelay < MinimumRepeatDelay)
                            {
                                _currentRepeatDelay = MinimumRepeatDelay;
                            }
                            else if (_capturedAccelerationRatio < 1f
                                && _currentRepeatDelay > MaximumRepeatDelay)
                            {
                                _currentRepeatDelay = MaximumRepeatDelay;
                            }
                        }
                        // Raise event.
                        Raise(this, _eventArgs);
                    }
                }
                // The button has just completed the initial delay and can now start repeating.
                else if (_cumulativeTime >= InitialDelay)
                {
                    _cumulativeTime -= InitialDelay;
                    _isRepeating = true;
                    _capturedAccelerationRatio = AccelerationRatio;  // Capture the acceleration ration, so it doesn't change if the property value is changed.
                    _currentRepeatDelay = RepeatDelay;
                    // This is our first repeat. Raise the event only if we have some initial delay.
                    if (InitialDelay > TimeSpan.Zero)
                    {
                        _repeatCount++;
                        Raise(this, _eventArgs);
                    }
                }

            }
            // The button has just been released.
            else if (_wasPressed)
            {
                _cumulativeTime = TimeSpan.Zero;
                _isRepeating = false;
                _repeatCount = 0;
                _currentRepeatDelay = TimeSpan.Zero;
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            _wasPressed = Button.IsPressed;
        }
    }

    #endregion Methods


}
