using System;
using System.ComponentModel;

namespace XInputium;

/// <summary>
/// Provides the base for classes that represent an 
/// input device, offering input device state update 
/// and comparison functionality.
/// </summary>
/// <typeparam name="TState">Type implementing 
/// <see cref="IInputDeviceState"/> interface, that 
/// is the type of the objects that represent the state 
/// an input device has in a specific moment in time.</typeparam>
/// <remarks>
/// Classes inheriting from <see cref="InputDevice{TState}"/> 
/// represent a physical input device. Instances of these 
/// classes provide consumers with the ability to get 
/// the current state of an input device and to keep 
/// track of that state. <see cref="InputDevice{TState}"/> 
/// compares these states for equality, and keeps the two 
/// most recent states, so consumers of derived classes can 
/// compare these states for differences for determining 
/// what has changed since the last state.
/// <br/><br/>
/// <see cref="InputDevice{TState}"/> provides the 
/// <see cref="Update()"/> method, which consumers should 
/// call to get the current state of the input device. 
/// This method should be called several times 
/// per second, usually once per each game/application 
/// frame, so the state of the input device is updated 
/// at a pace that looks to the user as being real-time. 
/// See <see cref="Update()"/> method for more information.
/// <br/><br/>
/// After you call <see cref="Update()"/> method, you can 
/// use its return value or the value of 
/// <see cref="HasStateChanged"/> property to determine if 
/// the current state of the input device has changed since 
/// the last call to the method. <see cref="CurrentState"/> 
/// property returns the state of the input device at the time 
/// of the last call to <see cref="Update()"/> method, and 
/// <see cref="PreviousState"/> property returns the state of 
/// the input device at the time of that method's 
/// second-to-last call. <see cref="HasStateChanged"/> property 
/// returns <see langword="true"/> if 
/// <see cref="CurrentState"/> and <see cref="PreviousState"/> 
/// are not identical.
/// <br/><br/>
/// <see cref="IsConnected"/> property returns a 
/// <see cref="bool"/> that indicates if the device was 
/// connected, at the time of the most recent call to 
/// <see cref="Update()"/> method.
/// <br/><br/>
/// All base members of the <see cref="InputDevice{TState}"/> 
/// class assume the current state of the device as the state 
/// the device had at the time of the most recent call to 
/// <see cref="Update()"/> method. If the last call to that 
/// method is not very recent, members of 
/// <see cref="InputDevice{TState}"/> may provide you with 
/// unreliable data. For instance, <see cref="IsConnected"/> 
/// property may report that the input device is connected, 
/// while it actually not connected anymore.
/// </remarks>
/// <seealso cref="IInputDeviceState"/>
public abstract class InputDevice<TState>
    : EventDispatcherObject
    where TState : notnull, IInputDeviceState
{


    #region Fields

    // Static PropertyChangedEventArgs to use for property value changes.
    private static readonly PropertyChangedEventArgs s_EA_HasStateChanged = new(nameof(HasStateChanged));
    private static readonly PropertyChangedEventArgs s_EA_IsConnected = new(nameof(IsConnected));
    private static readonly PropertyChangedEventArgs s_EA_LastUpdateId = new(nameof(LastUpdateId));

    // Property backing storage fields.
    private TState _previousState;  // Store for the value of PreviousState property.
    private TState _currentState;  // Store for the value of CurrentState property.
    private bool _isConnected = false;  // Store for the value of IsConnected property.
    private bool _hasStateChanged = false;  // Store for the value of HasStateChanged property.
    private long _lastUpdateId = 0L;  // Store for the value of LastUpdateId property.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an <see cref="InputDevice{TState}"/> 
    /// class.
    /// </summary>
    public InputDevice()
        : base(EventDispatchMode.Deferred)
    {
        _previousState = CreateStateInstance();
        _currentState = CreateStateInstance();
    }

    #endregion Constructors


    #region Events

    /// <summary>
    /// It's invoked every time the device state is updated, 
    /// through a call to <see cref="Update()"/> method.
    /// </summary>
    /// <seealso cref="Update()"/>
    /// <seealso cref="OnUpdated()"/>
    public event EventHandler? Updated;


    /// <summary>
    /// It's invoked whenever the most recent input device state 
    /// changes from not connected to connected.
    /// </summary>
    /// <seealso cref="Disconnected"/>
    /// <seealso cref="IsConnected"/>
    public event EventHandler? Connected;


    /// <summary>
    /// It's invoked whenever the most recent input device state 
    /// changes from connected to not connected.
    /// </summary>
    /// <seealso cref="Connected"/>
    /// <seealso cref="IsConnected"/>
    public event EventHandler? Disconnected;


    /// <summary>
    /// It's invoked whenever the known state of the device 
    /// changes.
    /// </summary>
    /// <remarks>
    /// The current state of the device is represented 
    /// by <see cref="CurrentState"/> property, and the state 
    /// it has before that is represented by 
    /// <see cref="PreviousState"/> property. Whenever the 
    /// states represented by these properties differ from 
    /// one another, the input device state is considered to 
    /// have changed, and <see cref="HasStateChanged"/> 
    /// property will return <see langword="true"/>. The 
    /// values of these properties is updated when you call 
    /// <see cref="Update()"/> method, and it is during 
    /// that call that the <see cref="StateChanged"/> method 
    /// can be invoked, if the states changed. When this 
    /// event is invoked, the value of 
    /// <see cref="HasStateChanged"/> property is always 
    /// <see langword="true"/>, at least, until the next call 
    /// to <see cref="Update()"/> method.
    /// <br/><br/>
    /// You subscribe to <see cref="StateChanged"/> event 
    /// when you need to get notified of changes to the current 
    /// state of the underlying input device.
    /// <br/><br/>
    /// When a device is connected or unconnected, it is 
    /// considered that the device state has changed, so the
    /// <see cref="StateChanged"/> event will be invoked as 
    /// well. This follows the logic above, about the differing 
    /// of <see cref="CurrentState"/> and 
    /// <see cref="PreviousState"/> properties' values.
    /// </remarks>
    /// <seealso cref="HasStateChanged"/>
    /// <seealso cref="CurrentState"/>
    /// <seealso cref="PreviousState"/>
    /// <seealso cref="Update()"/>
    public event EventHandler? StateChanged;

    #endregion Events


    #region Properties

    /// <summary>
    /// Gets the <typeparamref name="TState"/> object that 
    /// represents the second most recent known state of the 
    /// input device, which is the state that was know before 
    /// the last call to <see cref="Update()"/> method.
    /// </summary>
    /// <remarks>
    /// The value of <see cref="PreviousState"/> is only 
    /// updated when you call <see cref="Update()"/> method.
    /// When you call <see cref="Update()"/> method, the value
    /// of <see cref="CurrentState"/> becomes the value of 
    /// <see cref="PreviousState"/>, and 
    /// <see cref="CurrentState"/> value is updated with a 
    /// value that represents the current state of the 
    /// underlying input device.
    /// <br/><br/>
    /// For performance reasons, if <typeparamref name="TState"/> 
    /// is a reference type, the <typeparamref name="TState"/> 
    /// object that is the value of <see cref="PreviousState"/> 
    /// property is not always the same object, although the 
    /// state it represents may not change. You should not store 
    /// a reference to the value of <see cref="PreviousState"/> 
    /// property, and your <typeparamref name="TState"/> object 
    /// implementation should account for this. The same is true 
    /// for <see cref="CurrentState"/> property.
    /// </remarks>
    /// <seealso cref="CurrentState"/>
    /// <seealso cref="Update()"/>
    public TState PreviousState => _previousState;


    /// <summary>
    /// Gets the <typeparamref name="TState"/> object that 
    /// represents the most recent known state of the 
    /// input device.
    /// </summary>
    /// <remarks>
    /// The value of <see cref="CurrentState"/> is only 
    /// updated when you call <see cref="Update()"/> method.
    /// When you call <see cref="Update()"/> method, the value
    /// of <see cref="CurrentState"/> becomes the value of 
    /// <see cref="PreviousState"/>, and 
    /// <see cref="CurrentState"/> value is updated with a 
    /// value that represents the current state of the 
    /// underlying input device.
    /// <br/><br/>
    /// For performance reasons, if <typeparamref name="TState"/> 
    /// is a reference type, the <typeparamref name="TState"/> 
    /// object that is the value of <see cref="CurrentState"/> 
    /// property is not always the same object, although the 
    /// state it represents may not change. You should not store 
    /// a reference to the value of <see cref="CurrentState"/> 
    /// property, and your <typeparamref name="TState"/> object 
    /// implementation should account for this. The same is true 
    /// for <see cref="PreviousState"/> property.
    /// </remarks>
    /// <seealso cref="PreviousState"/>
    /// <seealso cref="Update()"/>
    public TState CurrentState => _currentState;


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the device 
    /// is currently connected, accordingly to its most recent 
    /// state update.
    /// </summary>
    /// <remarks>
    /// Although <see cref="IsConnected"/> may report the device 
    /// is currently connected, it might actually not be anymore. 
    /// This property reports the state of the device accordingly 
    /// to the information currently present in 
    /// <see cref="CurrentState"/> property, as this is the most
    /// recent state. This information is only updated when 
    /// <see cref="Update()"/> method is called. This is intended 
    /// behavior, so the reported state of the device can only 
    /// change when explicitly requested, through a formal state 
    /// update. Call <see cref="Update()"/> method to update the 
    /// current state.
    /// </remarks>
    /// <seealso cref="CurrentState"/>
    /// <seealso cref="Update()"/>
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (SetProperty(ref _isConnected, in value, s_EA_IsConnected))
            {
                if (_isConnected)
                {
                    OnConnected();
                }
                else
                {
                    OnDisconnected();
                }
            }
        }
    }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the 
    /// values of <see cref="CurrentState"/> and 
    /// <see cref="PreviousState"/> properties represent 
    /// different states, meaning the device state has 
    /// changed between the two most recent calls to 
    /// <see cref="Update()"/> method.
    /// </summary>
    /// <seealso cref="CurrentState"/>
    /// <seealso cref="PreviousState"/>
    /// <seealso cref="StateChanged"/>
    /// <seealso cref="Update()"/>
    public bool HasStateChanged
    {
        get => _hasStateChanged;
        private set => SetProperty(ref _hasStateChanged, in value, s_EA_HasStateChanged);
    }


    /// <summary>
    /// Gets a <see cref="long"/> that represents the ID of 
    /// the last state update operation.
    /// </summary>
    /// <remarks>
    /// <see cref="LastUpdateId"/> property value is a <see cref="long"/> 
    /// that is sequentially increased each time 
    /// <see cref="Update()"/> method is called. This value can 
    /// be used as an ID that represents the last state update 
    /// operation. Although <see cref="CurrentState"/>, 
    /// <see cref="PreviousState"/> and <see cref="HasStateChanged"/> 
    /// properties can be used to determine if the current state 
    /// has changed, <see cref="LastUpdateId"/> enables you to determine 
    /// if an update operation was performed, even if the device 
    /// state might had actually not changed.
    /// </remarks>
    /// <seealso cref="CurrentState"/>
    /// <seealso cref="HasStateChanged"/>
    public long LastUpdateId
    {
        get => _lastUpdateId;
        private set => SetProperty(ref _lastUpdateId, in value, s_EA_LastUpdateId);
    }

    #endregion Properties


    #region Methods

    /// <summary>
    /// Raises the <see cref="Updated"/> event.
    /// </summary>
    /// <seealso cref="Updated"/>
    /// <seealso cref="Update()"/>
    protected virtual void OnUpdated()
    {
        RaiseEvent(() => Updated?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="StateChanged"/> event.
    /// </summary>
    /// <seealso cref="StateChanged"/>
    /// <seealso cref="CurrentState"/>
    /// <seealso cref="PreviousState"/>
    protected virtual void OnStateChanged()
    {
        RaiseEvent(() => StateChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="Connected"/> event.
    /// </summary>
    /// <seealso cref="Connected"/>
    /// <seealso cref="IsConnected"/>
    protected virtual void OnConnected()
    {
        RaiseEvent(() => Connected?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="Disconnected"/> event.
    /// </summary>
    /// <seealso cref="Disconnected"/>
    /// <seealso cref="IsConnected"/>
    protected virtual void OnDisconnected()
    {
        RaiseEvent(() => Disconnected?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// When overridden in a derived class, creates a new 
    /// instance of a <typeparamref name="TState"/> object, 
    /// that represents an input device that is not connected 
    /// to the system and has no user input.
    /// </summary>
    /// <returns>The newly created <typeparamref name="TState"/> 
    /// instance.</returns>
    /// <remarks>
    /// This method is called by internal code to instantiate 
    /// <typeparamref name="TState"/> objects, that will be 
    /// used to represent the states of the input device.
    /// </remarks>
    /// <seealso cref="UpdateState(ref TState)"/>
    protected abstract TState CreateStateInstance();


    /// <summary>
    /// When overridden in a derived class, updates the 
    /// specified <typeparamref name="TState"/> with current 
    /// data from the underlying input device.
    /// </summary>
    /// <param name="state">Variable that is being updated 
    /// with current state data from the input device.</param>
    /// <remarks>
    /// If the underlying device is not connected to the 
    /// system or not available, <paramref name="state"/> must 
    /// be updated to reflect an empty state, to represent a 
    /// device that is not connected and has no user input.
    /// <br/><br/>
    /// This method is called by <see cref="Update()"/> method 
    /// to update the current state of the device. The state 
    /// information set by <see cref="UpdateState(ref TState)"/> 
    /// method will represent the current device state, which 
    /// can be obtained using <see cref="CurrentState"/> 
    /// property.
    /// </remarks>
    /// <seealso cref="Update()"/>
    /// <seealso cref="CurrentState"/>
    protected abstract void UpdateState(ref TState state);


    /// <summary>
    /// Updates the current state of the <see cref="InputDevice{TState}"/> 
    /// with current data from the underlying input device.
    /// </summary>
    /// <returns><see langword="true"/> of the current state of 
    /// the input device has changed since the last call to 
    /// <see cref="Update()"/> method.</returns>
    /// <remarks>
    /// This method updates the values of <see cref="CurrentState"/> 
    /// and <see cref="PreviousState"/> properties, as well as 
    /// <see cref="HasStateChanged"/> property. 
    /// <see cref="PreviousState"/> property gets the value of 
    /// <see cref="CurrentState"/> property, and its old value is 
    /// recycle, set with the most recent data from the underlying 
    /// input device, and set to <see cref="CurrentState"/> property.
    /// Both states are then compared for equality and, if they 
    /// differ, <see cref="HasStateChanged"/> property is set to 
    /// <see langword="true"/>.
    /// <br/><br/>
    /// Calling this method will also raise some events, as necessary, 
    /// like <see cref="Connected"/>, <see cref="Disconnected"/> 
    /// and <see cref="StateChanged"/> events. These events are raised 
    /// through a call to their respective triggering methods — in 
    /// this case, <see cref="OnConnected()"/>, 
    /// <see cref="OnDisconnected()"/> and 
    /// <see cref="OnStateChanged()"/> methods, respectively.
    /// <br/><br/>
    /// To update the current state with current data from the 
    /// underlying input device, <see cref="Update()"/> method 
    /// calls the inherited class implementation of 
    /// <see cref="UpdateState(ref TState)"/> method. You override 
    /// this method to provide a way for <see cref="Update()"/> 
    /// method to obtain the state of the input device.
    /// <br/><br/>
    /// Consumers of <see cref="InputDevice{TState}"/> derived 
    /// classes would call <see cref="Update()"/> method several 
    /// times per second to determine the current state of the 
    /// input device. This method would, usually, be called once 
    /// per game/application render frame.
    /// </remarks>
    /// <seealso cref="CurrentState"/>
    /// <seealso cref="PreviousState"/>
    /// <seealso cref="HasStateChanged"/>
    public bool Update()
    {
        // Update the current state.
        (_currentState, _previousState) = (_previousState, _currentState);  // Swap states.
        UpdateState(ref _currentState);

        IsConnected = CurrentState.IsConnected;
        HasStateChanged = CurrentState.IsConnected != PreviousState.IsConnected
            || !CurrentState.StateEquals(PreviousState);

        unchecked
        {
            long lastUpdateId = LastUpdateId;
            lastUpdateId++;
            LastUpdateId = lastUpdateId;
        }

        // Raise events as necessary.
        OnUpdated();
        if (HasStateChanged)
        {
            OnStateChanged();
        }

        // Dispatch events.
        DispatchEvents();

        // Return a boolean indicating if the state has changed since the last update.
        return HasStateChanged;
    }

    #endregion Methods


}
