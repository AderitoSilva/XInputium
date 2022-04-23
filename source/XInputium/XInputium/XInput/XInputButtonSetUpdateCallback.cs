using System;

namespace XInputium.XInput;

/// <summary>
/// Callback delegate outputted by <see cref="XInputButtonSet"/> 
/// class's constructor, which can be invoked to update the 
/// state of the class.
/// </summary>
/// <param name="buttonsState">A bitwise combination of 
/// <see cref="XButtons"/> flags that specify what XInput 
/// buttons are pressed.</param>
/// <param name="time">Amount of time elapsed since the last 
/// state update operation.</param>
/// <seealso cref="XInputButtonSet"/>
public delegate void XInputButtonSetUpdateCallback(
    XButtons buttonsState, TimeSpan time);
