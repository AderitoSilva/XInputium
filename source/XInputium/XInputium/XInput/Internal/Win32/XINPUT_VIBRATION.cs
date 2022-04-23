using System;
using System.Runtime.InteropServices;

namespace XInputium.XInput.Internal.Win32;

/// <summary>
/// Specifies motor speed levels for the vibration function 
/// of a controller.
/// </summary>
/// <remarks>
/// The left motor is the low-frequency rumble motor. The right 
/// motor is the high-frequency rumble motor. The two motors are 
/// not the same, and they create different vibration effects.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal struct XINPUT_VIBRATION
{


    /// <summary>
    /// Speed of the left motor. Valid values are in the range 0 to 65,535. 
    /// Zero signifies no motor use; 65,535 signifies 100 percent motor use.
    /// </summary>
    public ushort wLeftMotorSpeed;


    /// <summary>
    /// Speed of the right motor. Valid values are in the range 0 to 65,535. 
    /// Zero signifies no motor use; 65,535 signifies 100 percent motor use.
    /// </summary>
    public ushort wRightMotorSpeed;


}
