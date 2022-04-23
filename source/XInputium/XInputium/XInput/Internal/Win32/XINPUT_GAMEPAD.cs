using System;
using System.Runtime.InteropServices;

namespace XInputium.XInput.Internal.Win32;

/// <summary>
/// Describes the current state of the Xbox 360 Controller.
/// </summary>
/// <seealso cref="XINPUT_STATE"/>
[StructLayout(LayoutKind.Sequential)]
internal struct XINPUT_GAMEPAD
{


    /// <summary>
    /// Flags representing the device digital buttons.
    /// </summary>
    /// <seealso cref="XINPUT_GAMEPAD_wButtons"/>
    [MarshalAs(UnmanagedType.U2)]
    public XINPUT_GAMEPAD_wButtons wButtons;


    /// <summary>
    /// The current value of the left trigger analog control. 
    /// The value is between 0 and 255.
    /// </summary>
    public byte bLeftTrigger;


    /// <summary>
    /// The current value of the right trigger analog control. 
    /// The value is between 0 and 255.
    /// </summary>
    public byte bRightTrigger;


    /// <summary>
    /// Left thumbstick x-axis value. Each of the thumbstick axis members is a 
    /// signed value between -32768 and 32767 describing the position of the 
    /// thumbstick. A value of 0 is centered. Negative values signify down or to 
    /// the left. Positive values signify up or to the right. The constants 
    /// XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE or XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE 
    /// can be used as a positive and negative value to filter a thumbstick input.
    /// </summary>
    public short sThumbLX;


    /// <summary>
    /// Left thumbstick y-axis value. The value is between -32768 and 32767.
    /// </summary>
    public short sThumbLY;


    /// <summary>
    /// Right thumbstick x-axis value. The value is between -32768 and 32767.
    /// </summary>
    public short sThumbRX;


    /// <summary>
    /// Right thumbstick y-axis value. The value is between -32768 and 32767.
    /// </summary>
    public short sThumbRY;


}
