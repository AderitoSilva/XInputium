using System;
using System.Runtime.InteropServices;

namespace XInputium.XInput.Internal.Win32;

/// <summary>
/// Represents the state of a controller.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct XINPUT_STATE
{


    /// <summary>
    /// State packet number. The packet number indicates whether there have 
    /// been any changes in the state of the controller. If the 
    /// <see cref="dwPacketNumber"/> member is the same in sequentially returned
    /// <see cref="XINPUT_STATE"/> structures, the controller state has not changed.
    /// </summary>
    public uint dwPacketNumber;


    /// <summary>
    /// <see cref="XINPUT_GAMEPAD"/> structure containing the current state 
    /// of an Xbox 360 Controller.
    /// </summary>
    /// <seealso cref="XINPUT_GAMEPAD"/>
    public XINPUT_GAMEPAD Gamepad;


}
