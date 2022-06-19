using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace XInputium;

/// <summary>
/// Represents a logical input device, that can abstract different input 
/// devices of the same type. This is an abstract class.
/// </summary>
/// <typeparam name="TDevice">Type deriving from 
/// <see cref="InputDevice{TState}"/>, that's the type of the 
/// input devices that can be abstracted by this class.</typeparam>
/// <typeparam name="TState">Type of the device state, which 
/// is a type that derives from <see cref="IInputDeviceState"/>.</typeparam>
/// <remarks>
/// You update the state of the <see cref="LogicalInputDevice{TDevice, TState}"/> 
/// instance by updating the state of the underlying input device 
/// (the input device set at <see cref="Device"/> property), by 
/// calling its <see cref="InputDevice{TState}.Update()"/> method.
/// <br/><br/>
/// <see cref="LogicalInputDevice{TDevice, TState}"/> inherits from 
/// <see cref="InputObject"/> and defers event dispatching. All 
/// <see cref="LogicalInputDevice{TDevice, TState}"/> events are 
/// raised only immediately after you call 
/// <see cref="InputDevice{TState}.Update()"/> method on the 
/// underlying input device. Any property changes and event 
/// invocations will occur immediately before 
/// <see cref="InputDevice{TState}.Update()"/> method returns. 
/// Usually, you call <see cref="InputDevice{TState}.Update()"/> 
/// method once per each game frame or UI render loop iteration, 
/// several times per second.
/// </remarks>
/// <seealso cref="InputDevice{TState}"/>
/// <seealso cref="IInputDeviceState"/>
/// <seealso cref="InputObject"/>
public abstract class LogicalInputDevice<TDevice, TState>
    : InputObject
    where TDevice : InputDevice<TState>
    where TState : notnull, IInputDeviceState
{


    #region Fields

    // Static PropertyChangedEventArgs fields for property value changes.
    private static readonly PropertyChangedEventArgs s_EA_Device = new(nameof(Device));
    private static readonly PropertyChangedEventArgs s_EA_IsConnected = new(nameof(IsConnected));
    private static readonly PropertyChangedEventArgs s_EA_HasStateChanged = new(nameof(HasStateChanged));
    private static readonly PropertyChangedEventArgs s_EA_FrameTime = new(nameof(FrameTime));
    private static readonly PropertyChangedEventArgs s_EA_IsEnabled = new(nameof(IsEnabled));

    // Property backing storage fields.
    private TDevice? _device;  // Store for the value of Device property.
    private bool _isConnected = false;  // Store for the value of IsConnected property.
    private bool _hasStateChanged = false;  // Store for the value of HasStateChanged property.
    private TimeSpan _frameTime = TimeSpan.Zero;  // Store for the value of FrameTime property.
    private bool _isEnabled = true;  // Store for the value of IsEnabled property.

    // State keeping fields.
    private readonly InputLoopWatch _loopWatch;  // Watch used to measure time between updates.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of a 
    /// <see cref="LogicalInputDevice{TDevice, TState}"/> class, 
    /// that measures time using the specified 
    /// <see cref="InputLoopWatch"/>.
    /// </summary>
    /// <param name="watch"><see cref="InputLoopWatch"/> based 
    /// instance that will be used to measure time between input 
    /// loops iterations. If you pass <see langword="null"/> to 
    /// this parameter, the default watch will be used.</param>
    public LogicalInputDevice(InputLoopWatch? watch)
        : base(EventDispatchMode.Deferred)
    {
        _loopWatch = watch ?? InputLoopWatch.GetDefault();
        _loopWatch.Reset();
    }

    #endregion Constructors


    #region Events

    /// <summary>
    /// It's invoked whenever an update operation is performed 
    /// on the underlying input device.
    /// </summary>
    /// <remarks>
    /// Update operations are performed by calling 
    /// <see cref="InputDevice{TState}.Update()"/> method 
    /// on the input device set at <see cref="Device"/> property.
    /// </remarks>
    /// <seealso cref="OnUpdated()"/>
    public event EventHandler? Updated;


    /// <summary>
    /// It's invoked whenever the value of <see cref="Device"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="Device"/>
    /// <seealso cref="OnDeviceChanged()"/>
    public event EventHandler? DeviceChanged;


    /// <summary>
    /// It's invoked whenever the value of <see cref="IsConnected"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="IsConnected"/>
    /// <seealso cref="OnIsConnectedChanged()"/>
    /// <seealso cref="Connected"/>
    /// <seealso cref="Disconnected"/>
    public event EventHandler? IsConnectedChanged;


    /// <summary>
    /// It's invoked when the underlying <typeparamref name="TDevice"/>
    /// changes to it's connected state.
    /// </summary>
    /// <seealso cref="OnConnected()"/>
    /// <seealso cref="Disconnected"/>
    /// <seealso cref="IsConnectedChanged"/>
    /// <seealso cref="IsConnected"/>
    public event EventHandler? Connected;


    /// <summary>
    /// It's invoked when the underlying <typeparamref name="TDevice"/>
    /// changes to it's unconnected state, or when <see cref="Device"/> 
    /// property is set to <see langword="null"/>.
    /// </summary>
    /// <seealso cref="OnDisconnected()"/>
    /// <seealso cref="Connected"/>
    /// <seealso cref="IsConnectedChanged"/>
    /// <seealso cref="IsConnected"/>
    public event EventHandler? Disconnected;


    /// <summary>
    /// It's invoked whenever the device input state changes.
    /// </summary>
    /// <seealso cref="OnStateChanged()"/>
    public event EventHandler? StateChanged;


    /// <summary>
    /// It's invoked whenever the value of <see cref="IsEnabled"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="IsEnabled"/>
    /// <seealso cref="OnIsEnabledChanged()"/>
    public event EventHandler? IsEnabledChanged;

    #endregion Events


    #region Properties

    /// <summary>
    /// Gets or sets the <typeparamref name="TDevice"/> used by the 
    /// current <see cref="LogicalInputDevice{TDevice, TState}"/> instance.
    /// </summary>
    public TDevice? Device
    {
        get => _device;
        set
        {
            if (ReferenceEquals(_device, value))
                return;

            Reset();

            // Detach existing device.
            DetachCurrentDevice();

            // Attach new device.
            if (value is not null)
            {
                AttachDevice(value);
            }

            // Notify event listeners.
            OnPropertyChanged(s_EA_Device);
            OnDeviceChanged();
            UpdateInputObject(TimeSpan.Zero);
        }
    }


    /// <summary>
    /// Gets a <see cref="bool"/> indicating if the underlying 
    /// input device is currently connected.
    /// </summary>
    /// <remarks>
    /// This method always returns <see langword="false"/> 
    /// when <see cref="Device"/> is set to <see langword="null"/>. 
    /// The value of this property is updated after a device 
    /// update operation.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Device))]
    public bool IsConnected
    {
        get => _isConnected && Device is not null;
        private set
        {
            if (SetProperty(ref _isConnected, value, s_EA_IsConnected))
            {
                OnIsConnectedChanged();
            }
        }
    }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the 
    /// device state has changed since the last device update 
    /// operation.
    /// </summary>
    public bool HasStateChanged
    {
        get => _hasStateChanged;
        private set => SetProperty(ref _hasStateChanged, value, s_EA_HasStateChanged);
    }


    /// <summary>
    /// Gets the amount of time elapsed between the two 
    /// most recent device update operations.
    /// </summary>
    /// <remarks>
    /// This property can be used to get the time between 
    /// update operations method. This is usually regarded 
    /// as the frame time.
    /// </remarks>
    public TimeSpan FrameTime
    {
        get => _frameTime;
        private set => SetProperty(ref _frameTime, value, s_EA_FrameTime);
    }


    /// <summary>
    /// Gets or sets a <see cref="bool"/> that indicates the 
    /// <see cref="LogicalInputDevice{TDevice, TState}"/> is 
    /// enabled, meaning it will update its state and trigger 
    /// events based on state changes from the underlying 
    /// input device.
    /// </summary>
    /// <value><see langword="true"/> to enabled input updating, 
    /// or <see langword="false"/> to disable it. 
    /// The default is <see langword="true"/>.</value>
    /// <remarks>
    /// This property allows you to disable input device state 
    /// updates on the <see cref="LogicalInputDevice{TDevice, TState}"/>. 
    /// When you set this property to <see langword="false"/>, 
    /// events will not be triggered and the current state will 
    /// not be updated until you set this property to 
    /// <see langword="true"/> again, even if you update the 
    /// underlying input device.
    /// <br/><br/>
    /// <see cref="IsEnabled"/> can be useful when you wish to 
    /// stop receiving input information but don't want to change 
    /// the logic of your code to accommodate for that.
    /// </remarks>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (SetProperty(ref _isEnabled, value, s_EA_IsEnabled))
            {
                if (!_isEnabled)
                {
                    Reset(true);
                }
                OnIsEnabledChanged();
            }
        }
    }

    #endregion Properties


    #region Methods

    /// <summary>
    /// Raises the <see cref="Updated"/> event.
    /// </summary>
    /// <seealso cref="Updated"/>
    protected virtual void OnUpdated()
    {
        RaiseEvent(() => Updated?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="DeviceChanged"/> event.
    /// </summary>
    /// <seealso cref="DeviceChanged"/>
    /// <seealso cref="Device"/>
    protected virtual void OnDeviceChanged()
    {
        RaiseEvent(() => DeviceChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="IsConnectedChanged"/> event.
    /// </summary>
    /// <seealso cref="IsConnectedChanged"/>
    /// <seealso cref="IsConnected"/>
    /// <seealso cref="OnConnected()"/>
    /// <seealso cref="OnDisconnected()"/>
    protected virtual void OnIsConnectedChanged()
    {
        RaiseEvent(() => IsConnectedChanged?.Invoke(this, EventArgs.Empty));
        if (IsConnected)
        {
            OnConnected();
        }
        else
        {
            OnDisconnected();
        }
    }


    /// <summary>
    /// Raises the <see cref="Connected"/> event.
    /// </summary>
    /// <seealso cref="IsConnected"/>
    /// <seealso cref="OnDisconnected()"/>
    /// <seealso cref="OnIsConnectedChanged()"/>
    protected virtual void OnConnected()
    {
        RaiseEvent(() => Connected?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="Disconnected"/> event.
    /// </summary>
    /// <seealso cref="IsConnected"/>
    /// <seealso cref="OnConnected()"/>
    /// <seealso cref="OnIsConnectedChanged()"/>
    protected virtual void OnDisconnected()
    {
        RaiseEvent(() => Disconnected?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="StateChanged"/> event.
    /// </summary>
    /// <seealso cref="StateChanged"/>
    protected virtual void OnStateChanged()
    {
        RaiseEvent(() => StateChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="IsEnabledChanged"/> event.
    /// </summary>
    /// <seealso cref="IsEnabledChanged"/>
    /// <seealso cref="IsEnabled"/>
    protected virtual void OnIsEnabledChanged()
    {
        RaiseEvent(() => IsEnabledChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Removes the current device from being attached to the 
    /// current instance.
    /// </summary>
    private void DetachCurrentDevice()
    {
        if (_device is not null)
        {
            SetDeviceState();
            _device.Updated -= Device_Updated;
            _device.Connected -= Device_Connected;
            _device.Disconnected -= Device_Disconnected;
        }
        _device = null;
    }


    /// <summary>
    /// Attaches the specified device to the current instance.
    /// </summary>
    /// <param name="device">Device to attach.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="device"/> is <see langword="null"/>.</exception>
    [MemberNotNull(nameof(_device))]
    private void AttachDevice(TDevice device)
    {
        if (device is null)
            throw new ArgumentNullException(nameof(device));

        _device = device;
        IsConnected = _device.IsConnected;
        _device.Updated += Device_Updated;
        _device.Connected += Device_Connected;
        _device.Disconnected += Device_Disconnected;
        UpdateLogicalState();
        SetDeviceState();
    }


    /// <summary>
    /// Resets the <see cref="LogicalInputDevice{TDevice, TState}"/> to 
    /// its no-device state.
    /// </summary>
    /// <param name="keepConnected">Optional. Indicates if the device 
    /// should keep reporting is current connection status.</param>
    /// <remarks>
    /// This method is called when the value of <see cref="Device"/> 
    /// property is set to <see langword="null"/>, turning the 
    /// <see cref="LogicalInputDevice{TDevice, TState}"/> into a 
    /// no-device instance.
    /// </remarks>
    /// <seealso cref="Device"/>
    private void Reset(bool keepConnected = false)
    {
        HasStateChanged = false;
        IsConnected = keepConnected && IsConnected;

        _loopWatch.Reset();

        ResetLogicalState();
    }


    /// <summary>
    /// When overridden in an inherited class, resets the 
    /// <see cref="LogicalInputDevice{TDevice, TState}"/> to 
    /// its no-device state.
    /// </summary>
    /// <remarks>
    /// This method is called when the value of <see cref="Device"/> 
    /// property is set to <see langword="null"/>, turning the 
    /// <see cref="LogicalInputDevice{TDevice, TState}"/> into a 
    /// no-device instance.
    /// </remarks>
    /// <seealso cref="Device"/>
    protected virtual void ResetLogicalState()
    {

    }


    /// <summary>
    /// When overridden in derived classes, updates the state 
    /// of the <see cref="LogicalInputDevice{TDevice, TState}"/> 
    /// instance based on the current physical device state.
    /// </summary>
    /// <exception cref="InvalidOperationException">There 
    /// is no device currently set. <see cref="Device"/> property 
    /// is <see langword="null"/>.</exception>
    /// <seealso cref="SetDeviceState()"/>
    /// <seealso cref="ResetLogicalState()"/>
    protected abstract void UpdateLogicalState();


    /// <summary>
    /// When overridden in derived classes, forwards any state 
    /// changes to the <see cref="LogicalInputDevice{TDevice, TState}"/> 
    /// instance based on the current logical state.
    /// </summary>
    /// <exception cref="InvalidOperationException">There 
    /// is no device currently set. <see cref="Device"/> property 
    /// is <see langword="null"/>.</exception>
    /// <remarks>
    /// You would override this method to set the device state, 
    /// like vibration motors speed and similar. This method is 
    /// called after a call to <see cref="UpdateLogicalState()"/> 
    /// method, which performs the inverse operation.
    /// </remarks>
    /// <seealso cref="UpdateLogicalState()"/>
    protected virtual void SetDeviceState()
    {
        if (Device is null)
            throw new InvalidOperationException(
                "There is no device currently set.");
    }


    /// <summary>
    /// Updates the state of the 
    /// <see cref="LogicalInputDevice{TDevice, TState}"/> instance 
    /// with data from the underlying <typeparamref name="TDevice"/>, 
    /// and dispatches all enqueued events.
    /// </summary>
    /// <returns><see langword="true"/> if the device state was 
    /// changed since the last call to this method; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method updates the 
    /// <see cref="LogicalInputDevice{TDevice, TState}"/> instance 
    /// with current state data from the underlying 
    /// <typeparamref name="TDevice"/> (the device set in 
    /// <see cref="Device"/> property), forwards any state changes 
    /// to the device, and dispatches all enqueued events.
    /// <br/><br/>
    /// This method is automatically invoked on every update 
    /// operation performed on the underlying input device. You 
    /// should not call this method. Instead, call the 
    /// <see cref="InputDevice{TState}.Update()"/> method of the 
    /// input device currently set at <see cref="Device"/> property.
    /// </remarks>
    /// <seealso cref="Device"/>
    /// <seealso cref="InputDevice{TState}.Update()"/>
    [MemberNotNullWhen(true, nameof(Device))]
    private bool UpdateInternal()
    {
        // Update frame time.
        TimeSpan frameTime = _loopWatch.GetTime();
        if (frameTime < TimeSpan.Zero)
            frameTime = TimeSpan.Zero;
        FrameTime = frameTime;

        // Check if we have a device.
        if (Device is null)
        {
            OnUpdated();
            UpdateInputObject(frameTime);  // Although we have no device, there may be enqueued events to dispatch.
            return false;
        }

        // Get device state.
        bool hasStateChanged = Device.HasStateChanged && IsEnabled;
        if (IsEnabled)
        {
            HasStateChanged = hasStateChanged;
            UpdateLogicalState();  // Although the device state may not have changed, we need to update elapsed times.
            if (Device.HasStateChanged)
            {
                OnStateChanged();
            }
        }

        // Set device state.
        SetDeviceState();

        // Update input object.
        OnUpdated();
        UpdateInputObject(frameTime);

        // Return a boolean value indicating if the device state has
        // changed since the last call to this method.
        return hasStateChanged;
    }


    /// <summary>
    /// Updates the state of the 
    /// <see cref="LogicalInputDevice{TDevice, TState}"/> instance 
    /// with data from the underlying <typeparamref name="TDevice"/>, 
    /// and dispatches all enqueued events.
    /// </summary>
    /// <returns><see langword="true"/> if the device state was 
    /// changed since the last time the device was updated, and 
    /// <see cref="IsEnabled"/> is <see langword="true"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method updates the 
    /// <see cref="LogicalInputDevice{TDevice, TState}"/> instance 
    /// with current state data from the underlying 
    /// <typeparamref name="TDevice"/> (the device set in 
    /// <see cref="Device"/> property), forwards any state changes 
    /// to the device, and dispatches all enqueued events.
    /// <br/><br/>
    /// As an alternative to calling this method, you may opt to 
    /// call the <see cref="InputDevice{TState}.Update()"/> method
    /// directly.
    /// </remarks>
    /// <seealso cref="Device"/>
    /// <seealso cref="InputDevice{TState}.Update()"/>
    [MemberNotNullWhen(true, nameof(Device))]
    public bool Update()
    {
        if (Device is not null)
        {
            Device.Update();
            return HasStateChanged;
        }
        return false;
    }

    #endregion Methods


    #region Event handlers

    private void Device_Updated(object? sender, EventArgs e)
    {
        UpdateInternal();
    }


    private void Device_Connected(object? sender, EventArgs e)
    {
        IsConnected = true;
    }


    private void Device_Disconnected(object? sender, EventArgs e)
    {
        IsConnected = false;
    }

    #endregion Event handlers


}
