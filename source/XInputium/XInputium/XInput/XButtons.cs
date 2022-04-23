using System;
using XBtn = XInputium.XInput.Internal.Win32.XINPUT_GAMEPAD_wButtons;

namespace XInputium.XInput;

/// <summary>
/// Exposes constants that represent a button of an 
/// XInput controller. These constants can be used 
/// as bitwise flags to represent several buttons.
/// </summary>
/// <seealso cref="XInputButton"/>
[Flags]
public enum XButtons
{


    /// <summary>
    /// No button. This is used to represent no buttons.
    /// </summary>
    None = 0,

    /// <summary>
    /// D-Pad Up. This is one of the directional buttons.
    /// </summary>
    DPadUp = XBtn.XINPUT_GAMEPAD_DPAD_UP,

    /// <summary>
    /// D-Pad Down. This is one of the directional buttons.
    /// </summary>
    DPadDown = XBtn.XINPUT_GAMEPAD_DPAD_DOWN,

    /// <summary>
    /// D-Pad Left. This is one of the directional buttons.
    /// </summary>
    DPadLeft = XBtn.XINPUT_GAMEPAD_DPAD_LEFT,

    /// <summary>
    /// D-Pad Right. This is one of the directional buttons.
    /// </summary>
    DPadRight = XBtn.XINPUT_GAMEPAD_DPAD_RIGHT,

    /// <summary>
    /// The Start button.
    /// </summary>
    Start = XBtn.XINPUT_GAMEPAD_START,

    /// <summary>
    /// The Back button.
    /// </summary>
    Back = XBtn.XINPUT_GAMEPAD_BACK,

    /// <summary>
    /// The LS (Left Stick) button.
    /// </summary>
    LS = XBtn.XINPUT_GAMEPAD_LEFT_THUMB,

    /// <summary>
    /// The RS (Right Stick) button.
    /// </summary>
    RS = XBtn.XINPUT_GAMEPAD_RIGHT_THUMB,

    /// <summary>
    /// The LB (Left Shoulder) button.
    /// </summary>
    LB = XBtn.XINPUT_GAMEPAD_LEFT_SHOULDER,

    /// <summary>
    /// The RB (Right Shoulder).
    /// </summary>
    RB = XBtn.XINPUT_GAMEPAD_RIGHT_SHOULDER,

    /// <summary>
    /// The A button.
    /// </summary>
    A = XBtn.XINPUT_GAMEPAD_A,

    /// <summary>
    /// The B button.
    /// </summary>
    B = XBtn.XINPUT_GAMEPAD_B,

    /// <summary>
    /// The X button.
    /// </summary>
    X = XBtn.XINPUT_GAMEPAD_X,

    /// <summary>
    /// The Y button.
    /// </summary>
    Y = XBtn.XINPUT_GAMEPAD_Y,

}
