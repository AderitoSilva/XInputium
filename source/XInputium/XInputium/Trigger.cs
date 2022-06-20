using System;
using System.ComponentModel;
using System.Diagnostics;

namespace XInputium;

/// <summary>
/// Represents the trigger of a controller device, which has 
/// a single axis ranging between 0 and 1.
/// </summary>
/// <remarks>
/// The <see cref="Trigger"/> class provides configurations 
/// that can be used to modify the effective value of the 
/// trigger. If you don't need this functionality, consider 
/// using <see cref="SlimTrigger"/>, which is a lightweight 
/// alternative, that provides only the essential 
/// functionality. See <see cref="SlimTrigger"/> for more 
/// information.
/// </remarks>
/// <seealso cref="SlimTrigger"/>
/// <seealso cref="Joystick"/>
[DebuggerDisplay($"{nameof(Value)} = {{{nameof(Value)}}}")]
[Serializable]
public class Trigger : EventDispatcherObject
{


    #region Fields

    // Static PropertyChangedEventArgs fields, use for property changes.
    private static readonly PropertyChangedEventArgs s_EA_RawValue = new(nameof(RawValue));
    private static readonly PropertyChangedEventArgs s_EA_Value = new(nameof(Value));
    private static readonly PropertyChangedEventArgs s_EA_Delta = new(nameof(Delta));
    private static readonly PropertyChangedEventArgs s_EA_MovementSpeed = new(nameof(MovementSpeed));
    private static readonly PropertyChangedEventArgs s_EA_IsMoving = new(nameof(IsMoving));
    private static readonly PropertyChangedEventArgs s_EA_FrameTime = new(nameof(FrameTime));

    // Property back storage fields.
    private float _rawValue = 0f;  // Store for the value of RawValue property.
    private float _value = 0f;  // Store for the value of Value property.
    private float _delta = 0f;  // Store for the value of Delta property.
    private float _movementSpeed = 0f;  // Store for the value of MovementSpeed property.
    private bool _isMoving = false;  // Store for the value of IsMoving property.
    private TimeSpan _frameTime = TimeSpan.Zero;  // Store for the value of FrameTime property.
    private float _innerDeadZone = 0f;  // Store for the value of InnerDeadZone property.
    private float _outerDeadZone = 0f;  // Store for the value of OuterDeadZone property.
    private ModifierFunction? _modifierFunction = null;  // Store for the value of ModifierFunction property.
    private bool _isInverted = false;  // Store for the value of IsInverted property.

    // State keeping fields.
    private bool _isValid = false;  // Indicates if the trigger effective value is valid for the current property values.
    private bool _isValidating = false;  // Indicates if a validation operation is currently in progress.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a default instance of a <see cref="Trigger"/> 
    /// class.
    /// </summary>
    private Trigger()
        : base(EventDispatchMode.Deferred)
    {

    }


    /// <summary>
    /// Initializes a new instance of a <see cref="Trigger"/> 
    /// class, that supports state updating from external code.
    /// </summary>
    /// <param name="updateCallback">Variable that will be set 
    /// with a <see cref="TriggerUpdateCallback"/> delegate 
    /// that can be invoked to update the state of the 
    /// <see cref="Trigger"/> instance.</param>
    public Trigger(out TriggerUpdateCallback updateCallback)
        : this()
    {
        updateCallback = new TriggerUpdateCallback(UpdateState);
    }


    /// <summary>
    /// Initializes a new instance of a <see cref="Trigger"/> 
    /// class that has the specified immutable raw value.
    /// </summary>
    /// <param name="rawValue">Raw value of the trigger.</param>
    /// <exception cref="ArgumentException"><paramref name="rawValue"/> 
    /// is <see cref="float.NaN"/>.</exception>
    public Trigger(float rawValue)
        : this()
    {
        if (float.IsNaN(rawValue))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(rawValue)}' parameter.",
                nameof(rawValue));

        UpdateState(rawValue, TimeSpan.Zero);
    }


    /// <summary>
    /// Initializes a new instance of a <see cref="Trigger"/> 
    /// class that has an immutable raw value that is obtained 
    /// from the specified <see cref="SlimTrigger"/> object.
    /// </summary>
    /// <param name="trigger"><see cref="SlimTrigger"/> object 
    /// that will be used to initialize the <see cref="Trigger"/>.</param>
    /// <seealso cref="SlimTrigger"/>
    public Trigger(SlimTrigger trigger)
        : this(trigger.Value)
    {

    }

    #endregion Constructors


    #region Events

    /// <summary>
    /// It's invoked whenever the <see cref="Trigger"/> is 
    /// updated by external code.
    /// </summary>
    /// <seealso cref="OnUpdated()"/>
    public event EventHandler? Updated;


    /// <summary>
    /// It's invoked whenever the effective value of <see cref="Value"/> 
    /// property is changed.
    /// </summary>
    /// <seealso cref="Value"/>
    public event EventHandler? ValueChanged;


    /// <summary>
    /// It's invoked whenever the effective value of <see cref="IsMoving"/> 
    /// property is changed.
    /// </summary>
    /// <seealso cref="IsMoving"/>
    /// <seealso cref="OnIsMovingChanged()"/>
    public event EventHandler? IsMovingChanged;

    #endregion Events


    #region Properties

    /// <summary>
    /// Gets the raw value of the trigger. This is the normalized 
    /// value of the trigger (between 0 and 1) as reported by the 
    /// device.
    /// </summary>
    /// <seealso cref="Value"/>
    public float RawValue
    {
        get => _rawValue;
        private set => Invalidate(SetProperty(
            ref _rawValue, InputMath.Clamp01(value), s_EA_RawValue));
    }


    /// <summary>
    /// Gets the processed value of the trigger. This is the 
    /// normalized value of the trigger (between 0 and 1), 
    /// after it has been processed and all modifiers applied.
    /// </summary>
    /// <seealso cref="RawValue"/>
    public float Value
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _value;
        }
        private set
        {
            if (SetProperty(ref _value, InputMath.Clamp01(value), s_EA_Value))
            {
                OnValueChanged();
            }
        }
    }


    /// <summary>
    /// Gets the difference between the current and the previous value 
    /// of the trigger.
    /// </summary>
    /// <returns>A number between -1 and 1, indicating the current 
    /// effective value of the trigger, relative to the effective value
    /// it had before the most recent update operation.</returns>
    public float Delta
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _delta;
        }
        private set
        {
            SetProperty(ref _delta, InputMath.Clamp11(value), s_EA_Delta);
        }
    }


    /// <summary>
    /// Gets the estimated distance per second the trigger is being moved by,
    /// by considering its current a previous effective value.
    /// </summary>
    /// <returns>A number equal to or greater than 0, representing the estimated 
    /// distance the trigger is moving per second. If <see cref="FrameTime"/> 
    /// is <see cref="TimeSpan.Zero"/> and <see cref="Delta"/> is greater than 
    /// 0, <see cref="float.PositiveInfinity"/> is returned.</returns>
    /// <remarks>
    /// The number returned by this property represents the total distance the 
    /// trigger axis would travel within a second, if it kept moving at its 
    /// current speed. Its current speed is the trigger's delta distance (see 
    /// <see cref="Delta"/> property), divided by the number of seconds elapsed 
    /// between the two most recent update operations. Although the trigger 
    /// could not keep moving indeterminately because it is constrained to its 
    /// 0 to 1 boundaries, this property assumes as if it could.
    /// <br/><br/>
    /// When the time elapsed between the two most recent update operations is 
    /// zero (<see cref="TimeSpan.Zero"/>) while the delta distance is greater 
    /// than 0, this property returns <see cref="float.PositiveInfinity"/> to 
    /// indicate the trigger is moving at infinite speed and represent an 
    /// immediate movement.
    /// </remarks>
    /// <seealso cref="Delta"/>
    /// <seealso cref="FrameTime"/>
    public float MovementSpeed
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _movementSpeed;
        }
        private set
        {
            SetProperty(ref _movementSpeed, MathF.Max(value, 0f), s_EA_MovementSpeed);
        }
    }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the trigger is currently 
    /// being moved, by considering the two most recent update operations.
    /// </summary>
    /// <returns><see langword="true"/> if the trigger is being moved; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="IsMovingChanged"/>
    /// <seealso cref="Delta"/>
    /// <seealso cref="MovementSpeed"/>
    public bool IsMoving
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _isMoving;
        }
        private set
        {
            if (SetProperty(ref _isMoving, value, s_EA_IsMoving))
            {
                OnIsMovingChanged();
            }
        }
    }


    /// <summary>
    /// Gets the amount of time elapsed since the last time the 
    /// <see cref="Trigger"/> was updated from external code.
    /// </summary>
    public TimeSpan FrameTime
    {
        get => _frameTime;
        private set => SetProperty(ref _frameTime, value, s_EA_FrameTime);
    }


    /// <summary>
    /// Gets or sets the inner dead-zone of the trigger — that is, the raw 
    /// portion of the trigger in the beginning of the axis that is ignored 
    /// in the effective value of the trigger.
    /// </summary>
    /// <value>A number between 0 and 1, where 0 means no dead-zone is 
    /// applied and 1 means the full trigger axis range is ignored. The 
    /// default value is 0.</value>
    /// <exception cref="ArgumentException">The value being set to the 
    /// property is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="OuterDeadZone"/>
    /// <seealso cref="Value"/>
    public float InnerDeadZone
    {
        get => _innerDeadZone;
        set
        {
            if (float.IsNaN(value))
                throw new ArgumentException(
                    $"'{float.NaN}' is not a valid value for " +
                    $"'{nameof(InnerDeadZone)}' property.",
                    nameof(value));

            Invalidate(SetProperty(ref _innerDeadZone, InputMath.Clamp01(value)));
        }
    }


    /// <summary>
    /// Gets or sets the dead-zone of the outer end of the trigger — 
    /// that is, the raw portion of the trigger in the end of the axis 
    /// that is ignored in the effective value of the trigger.
    /// </summary>
    /// <value>A number between 0 and 1, where 0 means no dead-zone is 
    /// applied and 1 means the full trigger axis range is ignored. The 
    /// default value is 0.</value>
    /// <exception cref="ArgumentException">The value being set to the 
    /// property is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="InnerDeadZone"/>
    /// <seealso cref="Value"/>
    public float OuterDeadZone
    {
        get => _outerDeadZone;
        set
        {
            if (float.IsNaN(value))
                throw new ArgumentException(
                    $"'{float.NaN}' is not a valid value for '{nameof(value)}' parameter.",
                    nameof(value));

            Invalidate(SetProperty(ref _outerDeadZone, InputMath.Clamp01(value)));
        }
    }


    /// <summary>
    /// Gets or sets the custom function that is used to modify the 
    /// processed value of the <see cref="Trigger"/>. This function 
    /// affects the value of <see cref="Value"/> property.
    /// </summary>
    /// <value>A <see cref="XInputium.ModifierFunction"/> delegate 
    /// or <see langword="null"/> to use no function. 
    /// The default value is <see langword="null"/>.</value>
    /// <remarks>
    /// The input value of the function has already all of the base 
    /// modifiers of the <see cref="Trigger"/> applied — 
    /// for example, it has already the dead-zones applied.
    /// </remarks>
    /// <seealso cref="XInputium.ModifierFunction"/>
    public ModifierFunction? ModifierFunction
    {
        get => _modifierFunction;
        set => Invalidate(SetProperty(ref _modifierFunction, value));
    }


    /// <summary>
    /// Gets or sets a <see cref="bool"/> that indicates the 
    /// effective value of the axis is inverted (that is, when 
    /// its effective would be 0, it will be 1, and when it 
    /// would be 1, it will be 0).
    /// </summary>
    /// <seealso cref="Value"/>
    public bool IsInverted
    {
        get => _isInverted;
        set => Invalidate(SetProperty(ref _isInverted, value));
    }

    #endregion Properties


    #region Methods

    /// <summary>
    /// Creates a new <see cref="Trigger"/> instance that encapsulates the 
    /// specified <see cref="Trigger"/> instance and is automatically 
    /// updated when the specified trigger is updated.
    /// </summary>
    /// <param name="trigger"><see cref="Trigger"/> instance to 
    /// encapsulate.</param>
    /// <returns>The newly created <see cref="Trigger"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="trigger"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method returns a new <see cref="Trigger"/> instance that uses 
    /// the specified <paramref name="trigger"/> as the source of its raw 
    /// axis value and uses its time information. The returned 
    /// <see cref="Trigger"/> is automatically updated whenever the 
    /// specified <paramref name="trigger"/> is updated. Only time information 
    /// and the raw axis value are obtained from the underlying 
    /// <see cref="Trigger"/>.
    /// <br/><br/>
    /// This method can be useful in scenarios where you need to have more 
    /// that one <see cref="Trigger"/> instance associated with the same 
    /// physical trigger, with their own trigger settings.
    /// </remarks>
    public static Trigger Encapsulate(Trigger trigger)
    {
        if (trigger is null)
            throw new ArgumentNullException(nameof(trigger));

        Trigger instance = new(out TriggerUpdateCallback updateCallback);
        trigger.Updated += (sender, e) =>
        {
            if (sender is Trigger t)
            {
                updateCallback.Invoke(t.RawValue, t.FrameTime);
            }
        };
        updateCallback.Invoke(trigger.RawValue, trigger.FrameTime);
        return instance;
    }


    /// <summary>
    /// Gets the <see cref="string"/> representation of the 
    /// current <see cref="Trigger"/> instance.
    /// </summary>
    /// <returns>The <see cref="string"/> representation 
    /// of the <see cref="Trigger"/>.</returns>
    public override string ToString()
    {
        return $"{Value}";
    }


    /// <summary>
    /// Raises the <see cref="Updated"/> event.
    /// </summary>
    /// <seealso cref="Updated"/>
    protected virtual void OnUpdated()
    {
        RaiseEvent(() => Updated?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="ValueChanged"/> event.
    /// </summary>
    /// <seealso cref="ValueChanged"/>
    /// <seealso cref="Value"/>
    protected virtual void OnValueChanged()
    {
        RaiseEvent(() => ValueChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="IsMovingChanged"/> event.
    /// </summary>
    /// <seealso cref="IsMovingChanged"/>
    /// <seealso cref="IsMoving"/>
    protected virtual void OnIsMovingChanged()
    {
        RaiseEvent(() => IsMovingChanged?.Invoke(this, EventArgs.Empty));
    }


    private void UpdateState(float value, TimeSpan time)
    {
        // Validate parameters.
        if (float.IsNaN(value))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(value)}' parameter.",
                nameof(value));
        if (time < TimeSpan.Zero)
            time = TimeSpan.Zero;

        // Update properties.
        RawValue = value;
        FrameTime = time;

        // Call derived implementation before validation.
        OnUpdating();

        // Validate the trigger.
        float oldValue = _value;
        Validate();

        // Update post-validation properties. These properties are update here,
        // instead of on the validation method, because the validation method 
        // will not perform a validation when the raw value doesn't change,
        // and these properties need to be updated independently of value changes.
        Delta = oldValue - _value;
        IsMoving = Delta != 0f;
        MovementSpeed = Delta == 0f ? 0f
                    : FrameTime.TotalSeconds > 0d ? MathF.Abs(Delta) / (float)FrameTime.TotalSeconds
                    : float.PositiveInfinity;

        // Perform post-validation operations.
        OnUpdated();
        DispatchEvents();
    }


    /// <summary>
    /// It's called on every update to the <see cref="Trigger"/>, 
    /// before trigger validation. When overridden in a derived 
    /// class, performs operations that need to occur before 
    /// the trigger is validated.
    /// </summary>
    /// <remarks>
    /// This method allows inheritors to perform any operation 
    /// that must occur when the <see cref="Trigger"/> is updated, 
    /// that might invalidate the <see cref="Trigger"/>, forcing 
    /// a validation to occur as soon as possible.
    /// <br/><br/>
    /// Use <see cref="RawValue"/> and <see cref="FrameTime"/> 
    /// properties to obtain the current value of the trigger
    /// axis and the current update's frame time.
    /// </remarks>
    /// <seealso cref="Invalidate()"/>
    protected virtual void OnUpdating()
    {

    }


    /// <summary>
    /// Marks the effective value of the <see cref="Trigger"/> 
    /// — the value of <see cref="Value"/> property — as 
    /// outdated. The next time the getter method of <see cref="Value"/> 
    /// property is called, the property's value will be updated.
    /// </summary>
    /// <remarks>
    /// Inheritors can call this method if they are implementing 
    /// functionality that may affect the value of <see cref="Value"/> 
    /// property, that is not automatically applied. For instance, 
    /// if you are implementing a custom modifier property that 
    /// consumers can change, you would call this method whenever 
    /// your property's value changes.
    /// </remarks>
    /// <seealso cref="Validate()"/>
    /// <seealso cref="ApplyValueModifiers(float)"/>
    protected void Invalidate()
    {
        _isValid = false;
    }


    private void Invalidate(bool validate)
    {
        if (validate)
        {
            Invalidate();
        }
    }


    /// <summary>
    /// If the effective value of the trigger is outdated, 
    /// forces it to update and all modifiers to be applied.
    /// </summary>
    /// <returns><see langword="true"/> if the value of 
    /// <see cref="Value"/> property was updated and effectively 
    /// changed as a result of this operation; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NotSupportedException">The 
    /// <see cref="ApplyValueModifiers(float)"/> was overridden 
    /// in the derived class, and returned <see cref="float.NaN"/> 
    /// when called by <see cref="Validate()"/>.</exception>
    /// <remarks>
    /// You call this method when you need to force the value 
    /// of <see cref="Value"/> property to be updated with all 
    /// the modifiers. You usually call this method after changing 
    /// the value of a property that can affect the value of 
    /// <see cref="Value"/> property. Although the value of 
    /// <see cref="Value"/> property is automatically updated 
    /// as needed when its getter method is called, calling 
    /// <see cref="Validate()"/> method ensures it is 
    /// updated immediately, so any events that depend on this 
    /// will be triggered.
    /// </remarks>
    /// <seealso cref="Value"/>
    /// <seealso cref="Invalidate()"/>
    /// <seealso cref="ApplyValueModifiers(float)"/>
    public bool Validate()
    {
        if (_isValid || _isValidating)
            return false;

        try
        {
            _isValidating = true;
            float value = RawValue;

            // Apply modifiers to the effective value.
            value = ApplyValueModifiers(value);
            if (float.IsNaN(value))
                throw new NotSupportedException(
                    $"The return value of '{nameof(ApplyValueModifiers)}' method " +
                    $"cannot be '{float.NaN}'.");
            value = InputMath.Clamp01(value);

            // Update effective value.
            _isValid = true;
            if (value == _value)
            {
                return false;
            }
            else
            {
                float delta = value - _value;
                Value = value;
                return true;
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            DispatchEvents();
            _isValidating = false;
        }
    }


    /// <summary>
    /// When overridden in derived classes, receives the specified 
    /// axis raw value, applies any modifiers to it, and returns 
    /// the effective modified result.
    /// </summary>
    /// <param name="rawValue">Raw axis value to modify. This is a 
    /// value between 0 and 1.</param>
    /// <returns>A value between 0 and 1, which is the value of 
    /// <paramref name="rawValue"/> with all modifiers applied.</returns>
    /// <exception cref="NotSupportedException">The modifier function 
    /// specified in <see cref="ModifierFunction"/> property was 
    /// called and has returned <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// This method is called by <see cref="Validate()"/> method 
    /// and the <see cref="Value"/> property getter to apply the 
    /// modifiers to the trigger axis value. The base implementation 
    /// applies all of the base modifiers, so you need to call the 
    /// base implementation from your implementation to ensure the 
    /// base modifiers are correctly applied. Because 
    /// <paramref name="rawValue"/> is the raw value of the axis, 
    /// obtained from the underlying device, you have the ability 
    /// to apply modifications that require the raw value for them 
    /// to be applied, meaning you would call the base implementation 
    /// in the most appropriate moment for your needs.
    /// </remarks>
    /// <seealso cref="Value"/>
    /// <seealso cref="Validate()"/>
    protected virtual float ApplyValueModifiers(float rawValue)
    {
        // Apply dead-zones (inner and outer).
        rawValue = InputMath.ApplyDeadZone(rawValue, InnerDeadZone, OuterDeadZone);

        // Apply the modifier function.
        if (ModifierFunction is not null)
        {
            rawValue = ModifierFunction(rawValue);
            if (float.IsNaN(rawValue))
                throw new NotSupportedException($"The value returned by " +
                    $"the function at '{nameof(ModifierFunction)}' property " +
                    $"must not return '{float.NaN}'.");
            rawValue = InputMath.Clamp01(rawValue);
        }

        // Invert value.
        if (IsInverted)
        {
            rawValue = 1f - rawValue;
        }

        // Return the modified value.
        return rawValue;
    }


    /// <summary>
    /// Sets all writable properties in the current <see cref="Trigger"/> 
    /// with values from the corresponding properties in the specified 
    /// <see cref="Trigger"/> instance.
    /// </summary>
    /// <param name="trigger"><see cref="Trigger"/> instance to copy 
    /// the values from.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="trigger"/> is <see langword="null"/>.</exception>
    public virtual void CopyConfigurationFrom(Trigger trigger)
    {
        if (trigger is null)
            throw new ArgumentNullException(nameof(trigger));

        InnerDeadZone = trigger.InnerDeadZone;
        OuterDeadZone = trigger.OuterDeadZone;
        ModifierFunction = trigger.ModifierFunction;
        IsInverted = trigger.IsInverted;
    }


    /// <summary>
    /// Gets a new <see cref="DigitalButton"/> instance that activates 
    /// (presses) when the specified activation function returns 
    /// <see langword="true"/> for the current state of the <see cref="Trigger"/>.
    /// </summary>
    /// <param name="activationFunction">Function that will be called 
    /// on every update to the state of the <see cref="Trigger"/>, which 
    /// receives the current <see cref="Trigger"/> instance and the 
    /// <see cref="DigitalButton"/> as its parameters.</param>
    /// <returns>A new <see cref="DigitalButton"/> instance that is updated 
    /// automatically, and reports its state as pressed depending on the state 
    /// of the <see cref="Trigger"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activationFunction"/> is <see langword="null"/>.</exception>
    /// <seealso cref="DigitalButton"/>
    public DigitalButton ToDigitalButton(Func<Trigger, DigitalButton, bool> activationFunction)
    {
        if (activationFunction is null)
            throw new ArgumentNullException(nameof(activationFunction));

        DigitalButton button = new(out DigitalButtonUpdateCallback updateCallback);
        Updated += (sender, e) =>
        {
            if (sender is Trigger trigger)
            {
                updateCallback.Invoke(activationFunction(trigger, button), trigger.FrameTime);
            }
        };
        updateCallback.Invoke(activationFunction(this, button), FrameTime);

        return button;
    }


    /// <summary>
    /// Gets a new <see cref="DigitalButton"/> instance that activates 
    /// (presses) whenever the current <see cref="Trigger"/>'s axis 
    /// effective value is equal or greater than the specified activation 
    /// threshold and then deactivates when the effective axis value is 
    /// less than the specified deactivation threshold.
    /// </summary>
    /// <param name="activationThreshold">A number between 0 and 1 that 
    /// specifies the minimum value, over which the effective axis value 
    /// of the trigger will activate (press) the button.</param>
    /// <param name="deactivationThreshold">A number between 0 and 1 that 
    /// specifies the value under which the effective axis value 
    /// of the trigger will deactivate (release) the button, when the 
    /// button is activated.</param>
    /// <returns>A new <see cref="DigitalButton"/> instance that is updated 
    /// automatically, and reports its state as pressed depending on the state 
    /// of the <see cref="Trigger"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="activationThreshold"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="deactivationThreshold"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="DigitalButton"/>
    public DigitalButton ToDigitalButton(float activationThreshold, float deactivationThreshold)
    {
        if (float.IsNaN(activationThreshold))
            throw new ArgumentException($"{float.NaN} is not a valid " +
                $"value for '{nameof(activationThreshold)}' parameter.",
                nameof(activationThreshold));
        if (float.IsNaN(deactivationThreshold))
            throw new ArgumentException($"{float.NaN} is not a valid " +
                $"value for '{nameof(deactivationThreshold)}' parameter.",
                nameof(deactivationThreshold));

        return ToDigitalButton((trigger, button) => !button.IsPressed
            ? trigger.Value >= activationThreshold
            : trigger.Value >= deactivationThreshold);
    }


    /// <summary>
    /// Gets a new <see cref="DigitalButton"/> instance that activates 
    /// (presses) whenever the current <see cref="Trigger"/>'s axis 
    /// effective value is equal or greater than the specified threshold.
    /// </summary>
    /// <param name="activationThreshold">A number between 0 and 1 that 
    /// specifies the minimum value, over which the effective axis value 
    /// of the trigger will activate (press) the button.</param>
    /// <returns>A new <see cref="DigitalButton"/> instance that is updated 
    /// automatically, and reports its state as pressed depending on the state 
    /// of the <see cref="Trigger"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="activationThreshold"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="DigitalButton"/>
    public DigitalButton ToDigitalButton(float activationThreshold)
    {
        return ToDigitalButton(activationThreshold, activationThreshold);
    }

    #endregion Methods


}
