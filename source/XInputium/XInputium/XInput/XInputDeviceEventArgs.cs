using System;

namespace XInputium.XInput;

/// <summary>
/// Encapsulates information about an event associated 
/// with an <see cref="XInputDevice"/> instance.
/// </summary>
/// <seealso cref="XInputDevice"/>
/// <seealso cref="EventArgs"/>
public class XInputDeviceEventArgs : EventArgs
{


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an 
    /// <see cref="XInputDeviceEventArgs"/> class, 
    /// that is associated with the specified 
    /// <see cref="XInputDevice"/> instance.
    /// </summary>
    /// <param name="device"><see cref="XInputDevice"/> instance 
    /// associated with the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="device"/> is <see langword="null"/>.</exception>
    public XInputDeviceEventArgs(XInputDevice device)
    {
        if (device is null)
            throw new ArgumentNullException(nameof(device));

        Device = device;
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the <see cref="XInputDevice"/> instance 
    /// associated with the event.
    /// </summary>
    /// <seealso cref="XInputDevice"/>
    public XInputDevice Device { get; }

    #endregion Properties


}
