using System;
using System.ComponentModel;

namespace XInputium.XInput;

/// <summary>
/// Represents an XInput game controller and provides the 
/// means to obtain and change the state of a controller, 
/// as well as changing what controller device is 
/// providing state information. Inherits from 
/// <see cref="LogicalInputDevice{TDevice, TState}"/>.
/// </summary>
/// <remarks>
/// <see cref="XGamepad"/> class provides you high-level 
/// access to an underlying XInput controller. It uses an 
/// <see cref="XInputDevice"/> to communicate with the 
/// underlying device and is an abstraction layer over the 
/// <see cref="XInputDevice"/>, adding additional functionality.
/// <br/><br/>
/// You can use <see cref="XInputDevice"/> instances to get 
/// and set the state of an XInput controller, without using 
/// <see cref="XGamepad"/> class. However, 
/// <see cref="XGamepad"/> adds additional features that 
/// can simplify your work flow, at the cost of adding 
/// more complexity. If you don't need these additional 
/// features, consider using <see cref="XInputDevice"/>, as 
/// it is a more lightweight alternative to 
/// <see cref="XGamepad"/>.
/// <br/><br/>
/// An <see cref="XGamepad"/> instance represents a logical 
/// game controller device, while an <see cref="XInputDevice"/> 
/// instance represents a specific physical device. This means 
/// you can switch the underlying <see cref="XInputDevice"/> 
/// of an <see cref="XGamepad"/> instance to allow that 
/// instance to use a different underlying physical device. 
/// You do this by setting the value of 
/// <see cref="LogicalInputDevice{TDevice, TState}.Device"/> 
/// property with a different <see cref="XInputDevice"/> 
/// instance. The advantage of this behavior is that you will 
/// keep any configurations you have made to the 
/// <see cref="XGamepad"/> (for example, joystick dead-zones) 
/// and you don't need to manage your own logic for event 
/// registering and unregistering when you need to switch 
/// between physical XInput devices.
/// </remarks>
/// <seealso cref="LogicalInputDevice{TDevice, TState}"/>
/// <seealso cref="XInputDevice"/>
/// <seealso cref="InputObject"/>
public class XGamepad
    : LogicalInputDevice<XInputDevice, XInputDeviceState>
{


    #region Fields

    /// <summary>
    /// The speed of a device motor when it is fully stopped.
    /// </summary>
    /// <seealso cref="LeftMotorSpeed"/>
    /// <seealso cref="RightMotorSpeed"/>
    public static readonly float StoppedMotorSpeed = 0f;


    // Static PropertyChangedEventArgs to use for property value changes.
    private static readonly PropertyChangedEventArgs s_EA_IsVibrationEnabled = new(nameof(IsVibrationEnabled));
    private static readonly PropertyChangedEventArgs s_EA_LeftMotorSpeed = new(nameof(LeftMotorSpeed));
    private static readonly PropertyChangedEventArgs s_EA_RightMotorSpeed = new(nameof(RightMotorSpeed));
    private static readonly PropertyChangedEventArgs s_EA_VibrationFactor = new(nameof(VibrationFactor));

    // Property backing storage fields.
    private bool _isVibrationEnabled = true;  // Store for the value of IsVibrationEnabled property.
    private float _leftMotorSpeed = StoppedMotorSpeed;  // Store for the value of LeftMotorSpeed property.
    private float _rightMotorSpeed = StoppedMotorSpeed;  // Store for the value of RightMotorSpeed property.
    private float _vibrationFactor = 1f;  // Store for the value of VibrationFactor property.

    // Callbacks used to update instances of the triggers, joysticks and buttons.
    private readonly XInputButtonSetUpdateCallback _buttonsUpdateCallback;
    private readonly JoystickUpdateCallback _leftJoystickUpdateCallback;
    private readonly JoystickUpdateCallback _rightJoystickUpdateCallback;
    private readonly TriggerUpdateCallback _leftTriggerUpdateCallback;
    private readonly TriggerUpdateCallback _rightTriggerUpdateCallback;

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an <see cref="XGamepad"/> 
    /// class, that uses the specified <see cref="XInputDevice"/> 
    /// instance to communicate with the controller device and 
    /// that measures time using the specified 
    /// <see cref="InputLoopWatch"/>.
    /// </summary>
    /// <param name="device"><see cref="XInputDevice"/> instance 
    /// that will be used to perform communication with the 
    /// underlying device. <see langword="null"/> can be 
    /// used to specify no device.</param>
    /// <param name="watch"><see cref="InputLoopWatch"/> instance 
    /// that will be used to measure time within the 
    /// <see cref="XGamepad"/>. Use <see langword="null"/> to 
    /// specify the use of the default watch.</param>
    /// <remarks>
    /// If you pass <see langword="null"/> to <paramref name="device"/>, 
    /// no <see cref="XInputDevice"/> will be used, making the 
    /// <see cref="XGamepad"/> instance unable to communicate with 
    /// the underlying controller device. You can change the 
    /// underlying device later, by setting the value of 
    /// <see cref="LogicalInputDevice{TDevice, TState}.Device"/> 
    /// property.
    /// </remarks>
    public XGamepad(XInputDevice? device, InputLoopWatch? watch)
        : base(watch)
    {
        Buttons = new XInputButtonSet(out _buttonsUpdateCallback);
        LeftJoystick = new Joystick(out _leftJoystickUpdateCallback);
        RightJoystick = new Joystick(out _rightJoystickUpdateCallback);
        LeftTrigger = new Trigger(out _leftTriggerUpdateCallback);
        RightTrigger = new Trigger(out _rightTriggerUpdateCallback);

        Buttons.ButtonStateChanged += Buttons_ButtonStateChanged;
        LeftJoystick.PositionChanged += LeftJoystick_PositionChanged;
        RightJoystick.PositionChanged += RightJoystick_PositionChanged;
        LeftTrigger.ValueChanged += LeftTrigger_ValueChanged;
        RightTrigger.ValueChanged += RightTrigger_ValueChanged;

        Device = device;
    }


    /// <summary>
    /// Initializes a new instance of an <see cref="XGamepad"/> 
    /// class, that uses the specified <see cref="XInputDevice"/> 
    /// instance to communicate with the controller device.
    /// </summary>
    /// <param name="device"><see cref="XInputDevice"/> instance 
    /// that will be used to perform communication with the 
    /// underlying device. <see langword="null"/> can be 
    /// used to specify no device.</param>
    /// <remarks>
    /// If you pass <see langword="null"/> to <paramref name="device"/>, 
    /// no <see cref="XInputDevice"/> will be used, making the 
    /// <see cref="XGamepad"/> instance unable to communicate with 
    /// the underlying controller device. You can change the 
    /// underlying device later, by setting the value of 
    /// <see cref="LogicalInputDevice{TDevice, TState}.Device"/> 
    /// property.
    /// </remarks>
    public XGamepad(XInputDevice? device)
        : this(device, null)
    {

    }


    /// <summary>
    /// Initializes a new instance of an <see cref="XGamepad"/> 
    /// class that uses the first available connected controller 
    /// as its <see cref="XInputDevice"/> or, if no controller 
    /// is available, uses no underlying device.
    /// </summary>
    public XGamepad()
        : this(XInputDevice.GetFirstConnectedDevice(), null)
    {

    }


    /// <summary>
    /// Initializes a new instance of an <see cref="XGamepad"/> 
    /// class, that uses an <see cref="XInputDevice"/> that is 
    /// associated with the specified user index.
    /// </summary>
    /// <param name="userIndex"><see cref="XInputUserIndex"/> 
    /// constant that specifies the index of the XInput 
    /// controller.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="userIndex"/> is not a defined constant 
    /// of an <see cref="XInputUserIndex"/> enumeration.</exception>
    public XGamepad(XInputUserIndex userIndex)
        : this(new XInputDevice(userIndex), null)
    {

    }

    #endregion Constructors


    #region Events

    /// <summary>
    /// It's invoked whenever the pressed state of an XInput button 
    /// changes.
    /// </summary>
    /// <seealso cref="ButtonPressed"/>
    /// <seealso cref="ButtonReleased"/>
    /// <seealso cref="OnButtonStateChanged(DigitalButtonEventArgs{XInputButton})"/>
    /// <seealso cref="Buttons"/>
    public event DigitalButtonEventHandler<XInputButton>? ButtonStateChanged;


    /// <summary>
    /// It's invoked whenever an XInput button is pressed — 
    /// that is, its state is changed from released to pressed.
    /// </summary>
    /// <seealso cref="ButtonReleased"/>
    /// <seealso cref="ButtonStateChanged"/>
    /// <seealso cref="OnButtonPressed(DigitalButtonEventArgs{XInputButton})"/>
    public event DigitalButtonEventHandler<XInputButton>? ButtonPressed;


    /// <summary>
    /// It's invoked whenever an XInput button is released — 
    /// that is, its state changed from pressed to released.
    /// </summary>
    /// <seealso cref="ButtonPressed"/>
    /// <seealso cref="ButtonStateChanged"/>
    /// <seealso cref="OnButtonReleased(DigitalButtonEventArgs{XInputButton})"/>
    public event DigitalButtonEventHandler<XInputButton>? ButtonReleased;


    /// <summary>
    /// It's invoked whenever the effective position of the 
    /// left joystick changes.
    /// </summary>
    /// <seealso cref="RightJoystickMove"/>
    public event EventHandler? LeftJoystickMove;


    /// <summary>
    /// It's invoked whenever the effective position of the 
    /// right joystick changes.
    /// </summary>
    /// <seealso cref="LeftJoystickMove"/>
    public event EventHandler? RightJoystickMove;


    /// <summary>
    /// It's invoked whenever the effective position of the 
    /// left trigger changes.
    /// </summary>
    /// <seealso cref="RightTriggerMove"/>
    public event EventHandler? LeftTriggerMove;


    /// <summary>
    /// It's invoked whenever the effective position of the 
    /// right trigger changes.
    /// </summary>
    /// <seealso cref="LeftTriggerMove"/>
    public event EventHandler? RightTriggerMove;

    #endregion Events


    #region Properties

    /// <summary>
    /// Gets or sets a <see cref="bool"/> that enables or disables the vibration 
    /// of the gamepad.
    /// </summary>
    /// <value><see langword="true"/> to enable vibration, or <see langword="false"/> 
    /// to disallow any vibration and stop any currently rotating motor. 
    /// The default is <see langword="true"/>.</value>
    /// <remarks>
    /// The value of this property will be effectively applied on the device 
    /// during the next call to 
    /// <see cref="LogicalInputDevice{TDevice, TState}.Update()"/> method.
    /// </remarks>
    /// <seealso cref="LeftMotorSpeed"/>
    /// <seealso cref="RightMotorSpeed"/>
    public bool IsVibrationEnabled
    {
        get => _isVibrationEnabled;
        set
        {
            SetProperty(ref _isVibrationEnabled, in value, s_EA_IsVibrationEnabled);
        }
    }


    /// <summary>
    /// Gets or sets the left motor rotation speed.
    /// </summary>
    /// <value>A number between the 0 and 1 inclusive range, that specifies 
    /// the motor rotation speed, where 0 means the motor is stopped and 1 
    /// means the motor is running at full speed; if you specify 
    /// <see cref="float.NaN"/>, it will also stop the motor, being recognized 
    /// as 0. The default value is <see cref="StoppedMotorSpeed"/>.</value>
    /// <remarks>
    /// The value of this property will be effectively applied on the device 
    /// during the next call to 
    /// <see cref="LogicalInputDevice{TDevice, TState}.Update()"/> method. 
    /// <br/><br/>
    /// Note that the device motor will only rotate when the value of 
    /// <see cref="IsVibrationEnabled"/> property is <see langword="true"/>. 
    /// While <see cref="IsVibrationEnabled"/> property is set to 
    /// <see langword="false"/>, the device motors will remain stopped, but 
    /// <see cref="LeftMotorSpeed"/> will still report the value you set to 
    /// it. To get the actual device left motor speed, use 
    /// <see cref="XInputDevice.LeftMotorSpeed"/> property.
    /// </remarks>
    /// <seealso cref="RightMotorSpeed"/>
    /// <seealso cref="IsVibrationEnabled"/>
    public float LeftMotorSpeed
    {
        get => _leftMotorSpeed;
        set
        {
            if (float.IsNaN(value))
                value = 0f;
            SetProperty(ref _leftMotorSpeed, in value, s_EA_LeftMotorSpeed);
        }
    }


    /// <summary>
    /// Gets or sets the left motor rotation speed.
    /// </summary>
    /// <value>A number between the 0 and 1 inclusive range, that specifies 
    /// the motor rotation speed, where 0 means the motor is stopped and 1 
    /// means the motor is running at full speed; if you specify 
    /// <see cref="float.NaN"/>, it will also stop the motor, being recognized 
    /// as 0. The default value is <see cref="StoppedMotorSpeed"/>.</value>
    /// <remarks>
    /// The value of this property will be effectively applied on the device 
    /// during the next call to 
    /// <see cref="LogicalInputDevice{TDevice, TState}.Update()"/> method.
    /// <br/><br/>
    /// Note that the device motor will only rotate when the value of 
    /// <see cref="IsVibrationEnabled"/> property is <see langword="true"/>. 
    /// While <see cref="IsVibrationEnabled"/> property is set to 
    /// <see langword="false"/>, the device motors will remain stopped, but 
    /// <see cref="RightMotorSpeed"/> will still report the value you set to 
    /// it. To get the actual device left motor speed, use 
    /// <see cref="XInputDevice.RightMotorSpeed"/> property.
    /// </remarks>
    /// <seealso cref="LeftMotorSpeed"/>
    /// <seealso cref="IsVibrationEnabled"/>
    public float RightMotorSpeed
    {
        get => _rightMotorSpeed;
        set
        {
            if (float.IsNaN(value))
                value = 0f;
            SetProperty(ref _rightMotorSpeed, in value, s_EA_RightMotorSpeed);
        }
    }


    /// <summary>
    /// Gets or sets the multiplier for the device motors speed.
    /// </summary>
    /// <value>A number equal to or greater than 0, by which 
    /// <see cref="LeftMotorSpeed"/> and <see cref="RightMotorSpeed"/> 
    /// will be multiplied. The default is 1.</value>
    /// <exception cref="ArgumentException">The value being set to the property 
    /// is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// The value of this property will be effectively applied on the device 
    /// during the next call to 
    /// <see cref="LogicalInputDevice{TDevice, TState}.Update()"/> method.
    /// </remarks>
    /// <seealso cref="IsVibrationEnabled"/>
    /// <seealso cref="LeftMotorSpeed"/>
    /// <seealso cref="RightMotorSpeed"/>
    public float VibrationFactor
    {
        get => _vibrationFactor;
        set
        {
            if (float.IsNaN(value))
                throw new ArgumentException(
                    $"'{nameof(value)}' cannot be '{float.NaN}'.",
                    nameof(value));
            SetProperty(ref _vibrationFactor, MathF.Max(value, 0f), s_EA_VibrationFactor);
        }
    }


    /// <summary>
    /// Gets the <see cref="XInputButtonSet"/> that encapsulates 
    /// information about the state of the controller's buttons.
    /// </summary>
    /// <seealso cref="XInputButtonSet"/>
    /// <seealso cref="XInputButton"/>
    public XInputButtonSet Buttons { get; }


    /// <summary>
    /// Gets the <see cref="Joystick"/> instance that 
    /// encapsulates information about the current state of 
    /// the controllers's left joystick.
    /// </summary>
    /// <seealso cref="RightJoystick"/>
    public Joystick LeftJoystick { get; }


    /// <summary>
    /// Gets the <see cref="Joystick"/> instance that 
    /// encapsulates information about the current state of 
    /// the controllers's right joystick.
    /// </summary>
    /// <seealso cref="LeftJoystick"/>
    public Joystick RightJoystick { get; }


    /// <summary>
    /// Gets the <see cref="Trigger"/> instance that 
    /// encapsulates information about the current state of 
    /// the controllers's left trigger.
    /// </summary>
    /// <seealso cref="RightTrigger"/>
    public Trigger LeftTrigger { get; }


    /// <summary>
    /// Gets the <see cref="Trigger"/> instance that 
    /// encapsulates information about the current state of 
    /// the controllers's right trigger.
    /// </summary>
    /// <seealso cref="LeftTrigger"/>
    public Trigger RightTrigger { get; }

    #endregion Properties


    #region Methods

    #region Event raising related methods

    /// <summary>
    /// Raises the <see cref="ButtonStateChanged"/> event.
    /// </summary>
    /// <param name="e">Information about the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="e"/> is <see langword="null"/>.</exception>
    /// <seealso cref="ButtonStateChanged"/>
    protected virtual void OnButtonStateChanged(
        DigitalButtonEventArgs<XInputButton> e)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        RaiseEvent(() => ButtonStateChanged?.Invoke(this, e));
    }


    /// <summary>
    /// Raises the <see cref="ButtonPressed"/> event.
    /// </summary>
    /// <param name="e">Information about the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="e"/> is <see langword="null"/>.</exception>
    /// <seealso cref="ButtonPressed"/>
    protected virtual void OnButtonPressed(
        DigitalButtonEventArgs<XInputButton> e)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        RaiseEvent(() => ButtonPressed?.Invoke(this, e));
    }


    /// <summary>
    /// Raises the <see cref="ButtonReleased"/> event.
    /// </summary>
    /// <param name="e">Information about the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="e"/> is <see langword="null"/>.</exception>
    /// <seealso cref="ButtonReleased"/>
    protected virtual void OnButtonReleased(
        DigitalButtonEventArgs<XInputButton> e)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        RaiseEvent(() => ButtonReleased?.Invoke(this, e));
    }


    /// <summary>
    /// Raises the <see cref="LeftJoystickMove"/> 
    /// event.
    /// </summary>
    /// <seealso cref="LeftJoystickMove"/>
    protected virtual void OnLeftJoystickMove()
    {
        RaiseEvent(() => LeftJoystickMove?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="RightJoystickMove"/> 
    /// event.
    /// </summary>
    /// <seealso cref="RightJoystickMove"/>
    protected virtual void OnRightJoystickMove()
    {
        RaiseEvent(() => RightJoystickMove?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="LeftTriggerMove"/> 
    /// event.
    /// </summary>
    /// <seealso cref="LeftTriggerMove"/>
    protected virtual void OnLeftTriggerMove()
    {
        RaiseEvent(() => LeftTriggerMove?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="RightTriggerMove"/> 
    /// event.
    /// </summary>
    /// <seealso cref="RightTriggerMove"/>
    protected virtual void OnRightTriggerMove()
    {
        RaiseEvent(() => RightTriggerMove?.Invoke(this, EventArgs.Empty));
    }

    #endregion


    #region Input state related methods

    /// <summary>
    /// Resets the <see cref="XGamepad"/> to its no-device state. 
    /// Overrides 
    /// <see cref="LogicalInputDevice{TDevice, TState}.ResetLogicalState()"/>.
    /// </summary>
    /// <seealso cref="UpdateLogicalState()"/>
    protected override void ResetLogicalState()
    {
        _buttonsUpdateCallback.Invoke(XButtons.None, TimeSpan.Zero);
        _leftJoystickUpdateCallback.Invoke(0f, 0f, TimeSpan.Zero);
        _rightJoystickUpdateCallback.Invoke(0f, 0f, TimeSpan.Zero);
        _leftTriggerUpdateCallback.Invoke(0f, TimeSpan.Zero);
        _rightTriggerUpdateCallback.Invoke(0f, TimeSpan.Zero);

        LeftMotorSpeed = StoppedMotorSpeed;
        RightMotorSpeed = StoppedMotorSpeed;
    }


    /// <summary>
    /// Updates the logical state of the <see cref="XGamepad"/> 
    /// based on the current device state. Overrides 
    /// <see cref="LogicalInputDevice{TDevice, TState}.UpdateLogicalState()"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">No device is set. 
    /// <see cref="LogicalInputDevice{TDevice, TState}.Device"/> property 
    /// is <see langword="null"/>.</exception>
    /// <seealso cref="ResetLogicalState()"/>
    /// <seealso cref="SetDeviceState()"/>
    protected override void UpdateLogicalState()
    {
        XInputDevice? device = Device;
        if (device is null)
            throw new InvalidOperationException("No input device is set.");
        if (!device.IsConnected)
            return;

        _buttonsUpdateCallback.Invoke(device.CurrentState.Buttons, FrameTime);
        _leftJoystickUpdateCallback.Invoke(
            device.CurrentState.LeftJoystick.X, device.CurrentState.LeftJoystick.Y, FrameTime);
        _rightJoystickUpdateCallback.Invoke(
            device.CurrentState.RightJoystick.X, device.CurrentState.RightJoystick.Y, FrameTime);
        _leftTriggerUpdateCallback.Invoke(device.CurrentState.LeftTrigger.Value, FrameTime);
        _rightTriggerUpdateCallback.Invoke(device.CurrentState.RightTrigger.Value, FrameTime);
    }


    /// <summary>
    /// Sets the physical device state, based on the current logical 
    /// state. More specifically, sets the device's motors rotation 
    /// speed. Overrides 
    /// <see cref="LogicalInputDevice{TDevice, TState}.SetDeviceState()"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">No device is set. 
    /// <see cref="LogicalInputDevice{TDevice, TState}.Device"/> 
    /// is <see langword="null"/>.</exception>
    /// <seealso cref="UpdateLogicalState()"/>
    protected override void SetDeviceState()
    {
        if (Device is null)
            throw new InvalidOperationException("No device is set.");

        // Set the device motors speed, as necessary.
        float leftMotorSpeed = LeftMotorSpeed * VibrationFactor;
        float rightMotorSpeed = RightMotorSpeed * VibrationFactor;
        leftMotorSpeed = InputMath.Clamp01(leftMotorSpeed);
        rightMotorSpeed = InputMath.Clamp01(rightMotorSpeed);
        if (leftMotorSpeed != Device.LeftMotorSpeed
            || rightMotorSpeed != Device.RightMotorSpeed
            || (!IsVibrationEnabled && (Device.LeftMotorSpeed > 0f || Device.RightMotorSpeed > 0f)))
        {
            if (!IsVibrationEnabled)
            {
                // The vibration is disabled. Stop the motors.
                leftMotorSpeed = StoppedMotorSpeed;
                rightMotorSpeed = StoppedMotorSpeed;
            }
            // Update the device motors speed.
            Device.SetMotorsSpeed(leftMotorSpeed, rightMotorSpeed);
            // Synchronize properties with the current raw values from the
            // device, but only when the vibration is enabled, so they don't
            // get changed to 0 when the vibration is disabled.
            if (IsVibrationEnabled && VibrationFactor == 1f)
            {
                LeftMotorSpeed = Device.LeftMotorSpeed;
                RightMotorSpeed = Device.RightMotorSpeed;
            }
        }
    }

    #endregion


    #region InputEvent registration related methods    

    /// <summary>
    /// Registers and returns a <see cref="DigitalButtonInputEvent{T}"/> 
    /// that is triggered when the specified button changes its state 
    /// from released to pressed, meaning the user has just tapped 
    /// the button.
    /// </summary>
    /// <param name="button"><see cref="XButtons"/> constant that 
    /// specifies the XInput button to listen for.</param>
    /// <param name="callback">Callback that will be called when 
    /// the event is triggered.</param>
    /// <returns>The new <see cref="DigitalButtonInputEvent{T}"/> 
    /// that was registered.</returns>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is not a defined constant in an <see cref="XButtons"/> 
    /// enumeration.</exception>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is <see cref="XButtons.None"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    public DigitalButtonInputEvent<XInputButton> RegisterButtonPressedEvent(
        XButtons button, DigitalButtonInputEventHandler<XInputButton> callback)
    {
        if (button == XButtons.None || !Enum.IsDefined(button))
            throw new ArgumentException(
                $"'{button}' is not a valid value for '{nameof(button)}' " +
                $"parameter. A specific button constant is required.");
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        return this.RegisterButtonPressedEvent(Buttons[button], callback);
    }


    /// <summary>
    /// Registers and returns a <see cref="DigitalButtonInputEvent{T}"/> 
    /// that is triggered when the specified button changes its state 
    /// from pressed to released.
    /// </summary>
    /// <param name="button"><see cref="XButtons"/> constant that 
    /// specifies the XInput button to listen for.</param>
    /// <param name="callback">Callback that will be called when 
    /// the event is triggered.</param>
    /// <returns>The new <see cref="DigitalButtonInputEvent{T}"/> that 
    /// was registered.</returns>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is not a defined constant in an <see cref="XButtons"/> 
    /// enumeration.</exception>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is <see cref="XButtons.None"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <seealso cref="RegisterButtonPressedEvent(XButtons, DigitalButtonInputEventHandler{XInputButton})"/>
    /// <seealso cref="RegisterButtonHoldEvent(XButtons, TimeSpan, DigitalButtonInputEventHandler{XInputButton})"/>
    public DigitalButtonInputEvent<XInputButton> RegisterButtonReleasedEvent(
        XButtons button, DigitalButtonInputEventHandler<XInputButton> callback)
    {
        if (button == XButtons.None || !Enum.IsDefined(button))
            throw new ArgumentException(
                $"'{button}' is not a valid value for '{nameof(button)}' " +
                $"parameter. A specific button constant is required.");
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        return this.RegisterButtonReleasedEvent(Buttons[button], callback);
    }


    /// <summary>
    /// Registers and returns a <see cref="DigitalButtonInputEvent{T}"/> 
    /// that is triggered once when the specified button is held by 
    /// the specified duration.
    /// </summary>
    /// <param name="button"><see cref="XButtons"/> constant that 
    /// specifies the XInput button to listen for.</param>
    /// <param name="holdDuration">The amount of time the user must 
    /// hold down the button for the event to fire. If you specify 
    /// <see cref="TimeSpan.Zero"/>, this event will behave like a 
    /// pressed event.</param>
    /// <param name="callback">Callback that will be called when 
    /// the event is triggered.</param>
    /// <returns>The new <see cref="DigitalButtonInputEvent{T}"/> that 
    /// was registered.</returns>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is not a defined constant in an <see cref="XButtons"/> 
    /// enumeration.</exception>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is <see cref="XButtons.None"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The returned registered <see cref="DigitalButtonInputEvent{T}"/> 
    /// will be triggered once when the user presses and holds the button 
    /// for the specified amount of time. Once the event is triggered, 
    /// it will only be triggered again after the user releases the 
    /// button and repeats the same action (pressing and holding the 
    /// button for <paramref name="holdDuration"/>).
    /// </remarks>
    /// <seealso cref="RegisterButtonPressedEvent(XButtons, DigitalButtonInputEventHandler{XInputButton})"/>
    /// <seealso cref="RegisterButtonReleasedEvent(XButtons, DigitalButtonInputEventHandler{XInputButton})"/>
    public DigitalButtonInputEvent<XInputButton> RegisterButtonHoldEvent(
        XButtons button, TimeSpan holdDuration,
        DigitalButtonInputEventHandler<XInputButton> callback)
    {
        if (button == XButtons.None || !Enum.IsDefined(button))
            throw new ArgumentException(
                $"'{button}' is not a valid value for '{nameof(button)}' " +
                $"parameter. A specific button constant is required.");
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        return this.RegisterButtonHoldEvent(Buttons[button], holdDuration, callback);
    }


    /// <summary>
    /// Registers and returns a <see cref="RepeatDigitalButtonInputEvent{T}"/> 
    /// that is triggered repeatedly while the specified button is held, and 
    /// uses the specified acceleration parameters for acceleration or 
    /// deceleration of repeat delay times.
    /// </summary>
    /// <param name="button"><see cref="XButtons"/> constant that 
    /// specifies the XInput button to listen for.</param>
    /// <param name="initialDelay">Amount of time the button must be 
    /// held for the repeating to start.</param>
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
    /// <param name="callback">Callback that will be called when 
    /// the event is triggered.</param>
    /// <returns>The new <see cref="RepeatDigitalButtonInputEvent{T}"/> that 
    /// was registered.</returns>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is not a defined constant in an <see cref="XButtons"/> 
    /// enumeration.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is <see cref="XButtons.None"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="accelerationRatio"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="accelerationRatio"/> is equal to or lower than 0.</exception>
    public RepeatDigitalButtonInputEvent<XInputButton> RegisterButtonRepeatEvent(
        XButtons button, TimeSpan initialDelay, TimeSpan repeatDelay,
        float accelerationRatio, TimeSpan minRepeatDelay, TimeSpan maxRepeatDelay,
        DigitalButtonInputEventHandler<XInputButton> callback)
    {
        if (button == XButtons.None || !Enum.IsDefined(button))
            throw new ArgumentException(
                $"'{button}' is not a valid value for '{nameof(button)}' " +
                $"parameter. A specific button constant is required.");
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        return this.RegisterButtonRepeatEvent(
            Buttons[button], initialDelay, repeatDelay,
            accelerationRatio, minRepeatDelay, maxRepeatDelay, callback);
    }


    /// <summary>
    /// Registers and returns a <see cref="RepeatDigitalButtonInputEvent{T}"/> 
    /// that is triggered repeatedly while the specified button is held.
    /// </summary>
    /// <param name="button"><see cref="XButtons"/> constant that 
    /// specifies the XInput button to listen for.</param>
    /// <param name="initialDelay">Amount of time the button must be 
    /// held for the repeating to start.</param>
    /// <param name="repeatDelay">Amount of time to wait between each 
    /// repeat.</param>
    /// <param name="callback">Callback that will be called when 
    /// the event is triggered.</param>
    /// <returns>The new <see cref="RepeatDigitalButtonInputEvent{T}"/> that 
    /// was registered.</returns>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is not a defined constant in an <see cref="XButtons"/> 
    /// enumeration.</exception>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is <see cref="XButtons.None"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.</exception>
    public RepeatDigitalButtonInputEvent<XInputButton> RegisterButtonRepeatEvent(
        XButtons button, TimeSpan initialDelay, TimeSpan repeatDelay,
        DigitalButtonInputEventHandler<XInputButton> callback)
    {
        return RegisterButtonRepeatEvent(button, initialDelay, repeatDelay,
            1f, TimeSpan.Zero, TimeSpan.MaxValue,
            callback);
    }

    #endregion

    #endregion Methods


    #region Event handlers

    private void Buttons_ButtonStateChanged(object? sender, DigitalButtonEventArgs<XInputButton> e)
    {
        OnButtonStateChanged(e);
        if (e.Button.IsPressed)
        {
            OnButtonPressed(e);
        }
        else
        {
            OnButtonReleased(e);
        }
    }


    private void LeftJoystick_PositionChanged(object? sender, EventArgs e)
    {
        OnLeftJoystickMove();
    }


    private void RightJoystick_PositionChanged(object? sender, EventArgs e)
    {
        OnRightJoystickMove();
    }


    private void LeftTrigger_ValueChanged(object? sender, EventArgs e)
    {
        OnLeftTriggerMove();
    }


    private void RightTrigger_ValueChanged(object? sender, EventArgs e)
    {
        OnRightTriggerMove();
    }

    #endregion Event handlers


}
