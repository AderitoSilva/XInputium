using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace XInputium.XInput;

/// <summary>
/// Provides the means to determine the state of all supported 
/// users of XInput devices in the system, and manages a group 
/// of <see cref="XInputDevice"/> instances that represent the 
/// underlying XInput devices.
/// </summary>
/// <seealso cref="XInputDevice"/>
public class XInputDeviceManager
    : IEnumerable<XInputDevice>, IReadOnlyCollection<XInputDevice>,
     INotifyPropertyChanged
{


    #region Fields and constants

    private const int DevicesCount = 4;  // Number of XInput devices supported by this class.

    private static readonly PropertyChangedEventArgs s_EA_ = new(null);
    private static readonly PropertyChangedEventArgs s_EA_IsAnyDeviceConnected = new(nameof(IsAnyDeviceConnected));

    private readonly HashSet<XInputDevice> _connectedDevices  // Stores the currently connected devices.
        = new(DevicesCount);

    private bool _isAnyDeviceConnected = false;  // Store for the value of IsAnyDeviceConnected property.

    #endregion Fields and constants


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an 
    /// <see cref="XInputDeviceManager"/> class.
    /// </summary>
    public XInputDeviceManager()
    {
        UserOne = new(XInputUserIndex.One);
        UserTwo = new(XInputUserIndex.Two);
        UserThree = new(XInputUserIndex.Three);
        UserFour = new(XInputUserIndex.Four);

        foreach (XInputDevice device in this)
        {
            XInputDeviceEventArgs eventArgs = new(device);
            device.Connected += (sender, e) => Device_Connected(sender, eventArgs);
            device.Disconnected += (sender, e) => Device_Disconnected(sender, eventArgs);
            device.StateChanged += (sender, e) => Device_StateChanged(sender, eventArgs);
        }
    }

    #endregion Constructors


    #region Events

    /// <summary>
    /// It's invoked whenever an <see cref="XInputDevice"/> in the 
    /// <see cref="XInputDeviceManager"/> is connected.
    /// </summary>
    /// <seealso cref="DeviceDisconnected"/>
    /// <seealso cref="DeviceStateChanged"/>
    public event EventHandler<XInputDeviceEventArgs>? DeviceConnected;


    /// <summary>
    /// It's invoked whenever an <see cref="XInputDevice"/> in the 
    /// <see cref="XInputDeviceManager"/> is unconnected.
    /// </summary>
    /// <seealso cref="DeviceDisconnected"/>
    /// <seealso cref="DeviceStateChanged"/>
    public event EventHandler<XInputDeviceEventArgs>? DeviceDisconnected;


    /// <summary>
    /// It's invoked whenever the state of an <see cref="XInputDevice"/> 
    /// in the <see cref="XInputDeviceManager"/> changes.
    /// </summary>
    /// <seealso cref="DeviceConnected"/>
    /// <seealso cref="DeviceDisconnected"/>
    public event EventHandler<XInputDeviceEventArgs>? DeviceStateChanged;


    /// <summary>
    /// It's invoked whenever a value of a property in the 
    /// <see cref="XInputDeviceManager"/> changes. Implements 
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/>.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged.PropertyChanged"/>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion Events


    #region Properties

    /// <summary>
    /// Gets the <see cref="XInputDevice"/> instance 
    /// associated with the XINput device at the 
    /// XInput user index <see cref="XInputUserIndex.One"/>.
    /// </summary>
    public XInputDevice UserOne { get; }


    /// <summary>
    /// Gets the <see cref="XInputDevice"/> instance 
    /// associated with the XINput device at the 
    /// XInput user index <see cref="XInputUserIndex.Two"/>.
    /// </summary>
    public XInputDevice UserTwo { get; }


    /// <summary>
    /// Gets the <see cref="XInputDevice"/> instance 
    /// associated with the XINput device at the 
    /// XInput user index <see cref="XInputUserIndex.Three"/>.
    /// </summary>
    public XInputDevice UserThree { get; }


    /// <summary>
    /// Gets the <see cref="XInputDevice"/> instance 
    /// associated with the XINput device at the 
    /// XInput user index <see cref="XInputUserIndex.Four"/>.
    /// </summary>
    public XInputDevice UserFour { get; }


    int IReadOnlyCollection<XInputDevice>.Count => DevicesCount;


    /// <summary>
    /// Gets a collection that contains the <see cref="XInputDevice"/>
    /// instances that represent currently connected XInput devices, 
    /// accordingly to the device's last state update.
    /// </summary>
    /// <seealso cref="IsAnyDeviceConnected"/>
    public IReadOnlyCollection<XInputDevice> ConnectedDevices => _connectedDevices;


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if there 
    /// are any <see cref="XInputDevice"/> connected, in 
    /// the <see cref="XInputDeviceManager"/>.
    /// </summary>
    /// <remarks>
    /// The value returned by this property is based on 
    /// the state each XInput device had at the time it 
    /// was last updated. Calling <see cref="Update()"/> 
    /// method updates all XInput devices of the 
    /// <see cref="XInputDeviceManager"/>.
    /// </remarks>
    /// <seealso cref="ConnectedDevices"/>
    public bool IsAnyDeviceConnected
    {
        get => _isAnyDeviceConnected;
        private set
        {
            if (value != _isAnyDeviceConnected)
            {
                _isAnyDeviceConnected = value;
                OnPropertyChanged(s_EA_IsAnyDeviceConnected);
            }
        }
    }


    /// <summary>
    /// Gets the <see cref="XInputDevice"/> instance that represents 
    /// an XInput device that can be connected at the specified 
    /// XInput user index.
    /// </summary>
    /// <param name="userIndex"><see cref="XInputUserIndex"/> 
    /// constant that specifies the XInput user index of the 
    /// XInput device to obtain.</param>
    /// <returns>The <see cref="XInputDevice"/> instance associated 
    /// with <paramref name="userIndex"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="userIndex"/> 
    /// is not a defined constant in an <see cref="XInputUserIndex"/> 
    /// enumeration.</exception>
    /// <seealso cref="XInputUserIndex"/>
    public XInputDevice this[XInputUserIndex userIndex]
    {
        get
        {
            if (!Enum.IsDefined(userIndex))
                throw new ArgumentException(
                    $"'{userIndex}' is not a defined constant of " +
                    $"an '{nameof(XInputUserIndex)}' enumeration.",
                    nameof(userIndex));

            return userIndex switch
            {
                XInputUserIndex.One => UserOne,
                XInputUserIndex.Two => UserTwo,
                XInputUserIndex.Three => UserThree,
                XInputUserIndex.Four => UserFour,
                _ => throw new NotSupportedException(
                    $"'{userIndex}' is not a supported value for " +
                    $"'{nameof(userIndex)}' parameter.")
            };
        }
    }

    #endregion Properties


    #region Methods

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="e"><see cref="PropertyChangedEventArgs"/> 
    /// instance containing information about the event. You can 
    /// pass <see langword="null"/> to specify no specific property.</param>
    /// <seealso cref="PropertyChanged"/>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs? e)
    {
        PropertyChanged?.Invoke(this, e ?? s_EA_);
    }


    /// <summary>
    /// Invokes the <see cref="DeviceConnected"/> event.
    /// </summary>
    /// <param name="e"><see cref="XInputDeviceEventArgs"/> instance
    /// containing information about the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="e"/> is <see langword="null"/>.</exception>
    /// <seealso cref="DeviceConnected"/>
    protected virtual void OnDeviceConnected(XInputDeviceEventArgs e)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        DeviceConnected?.Invoke(this, e);
    }


    /// <summary>
    /// Invokes the <see cref="DeviceDisconnected"/> event.
    /// </summary>
    /// <param name="e"><see cref="XInputDeviceEventArgs"/> instance
    /// containing information about the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="e"/> is <see langword="null"/>.</exception>
    /// <seealso cref="DeviceDisconnected"/>
    protected virtual void OnDeviceDisconnected(XInputDeviceEventArgs e)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        DeviceDisconnected?.Invoke(this, e);
    }


    /// <summary>
    /// Invokes the <see cref="DeviceStateChanged"/> event.
    /// </summary>
    /// <param name="e"><see cref="XInputDeviceEventArgs"/> instance
    /// containing information about the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="e"/> is <see langword="null"/>.</exception>
    /// <seealso cref="DeviceStateChanged"/>
    protected virtual void OnDeviceStateChanged(XInputDeviceEventArgs e)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        DeviceStateChanged?.Invoke(this, e);
    }


    /// <summary>
    /// Updates the state of the <see cref="XInputDevice"/> 
    /// instances in the <see cref="XInputDeviceManager"/>.
    /// </summary>
    /// <returns><see langword="true"/> if any device reported 
    /// a state change; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Call this method to update the states of all 
    /// <see cref="XInputDevice"/> instances in the 
    /// <see cref="XInputDeviceManager"/> at once.
    /// </remarks>
    public bool Update()
    {
        bool hasStateChanged = false;
        hasStateChanged |= UserOne.Update();
        hasStateChanged |= UserTwo.Update();
        hasStateChanged |= UserThree.Update();
        hasStateChanged |= UserFour.Update();

        return hasStateChanged;
    }


    /// <summary>
    /// Gets an enumerator that iterates though all of the devices 
    /// of the <see cref="XInputDeviceManager"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate though 
    /// all of the devices of the <see cref="XInputDeviceManager"/>.</returns>
    public IEnumerator<XInputDevice> GetEnumerator()
    {
        yield return UserOne;
        yield return UserTwo;
        yield return UserThree;
        yield return UserFour;
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion Methods


    #region Event handlers

    private void Device_Connected(object? sender, XInputDeviceEventArgs e)
    {
        _connectedDevices.Add(e.Device);
        IsAnyDeviceConnected = _connectedDevices.Count > 0;

        OnDeviceConnected(e);
    }


    private void Device_Disconnected(object? sender, XInputDeviceEventArgs e)
    {
        _connectedDevices.Remove(e.Device);
        IsAnyDeviceConnected = _connectedDevices.Count > 0;

        OnDeviceDisconnected(e);
    }


    private void Device_StateChanged(object? sender, XInputDeviceEventArgs e)
    {
        OnDeviceStateChanged(e);
    }

    #endregion Event handlers


}
