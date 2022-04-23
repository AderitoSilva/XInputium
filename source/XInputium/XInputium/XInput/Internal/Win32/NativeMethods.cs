using System;
using System.Runtime.InteropServices;
using System.Security;

namespace XInputium.XInput.Internal.Win32;

/// <summary>
/// Provides static Win32 native methods for internal 
/// interoperation.
/// </summary>
[SuppressUnmanagedCodeSecurity]
internal static class NativeMethods
{


    #region Fields and constants

    private const string Dll_XInput910 = "Xinput9_1_0.dll";

    #endregion Fields and constants


    #region Methods

    /// <summary>
    /// Retrieves the current state of the specified controller.
    /// </summary>
    /// <param name="dwUserIndex">Index of the user's controller. 
    /// Can be a value from 0 to 3.</param>
    /// <param name="pState">Reference to an <see cref="XINPUT_STATE"/> 
    /// structure that receives the current state of the controller.</param>
    /// <returns>
    /// If the function succeeds, the return value is 
    /// <see cref="Win32ErrorCodes.ERROR_SUCCESS"/>.
    /// If the controller is not connected, the return value is 
    /// <see cref="Win32ErrorCodes.ERROR_DEVICE_NOT_CONNECTED"/>.
    /// If the function fails, the return value is an error code defined in Winerror.h. 
    /// The function does not use SetLastError to set the calling thread's last-error code.
    /// </returns>
    /// <remarks>
    /// When <see cref="XInputGetState(uint, ref XINPUT_STATE)"/> is used to retrieve 
    /// controller data, the left and right triggers are each reported separately. 
    /// For legacy reasons, when DirectInput retrieves controller data, the two triggers 
    /// share the same axis. The legacy behavior is noticeable in the current 
    /// Game Device Control Panel, which uses DirectInput for controller state.
    /// </remarks>
    [DllImport(Dll_XInput910, EntryPoint = "XInputGetState", SetLastError = false)]
    internal static extern uint XInputGetState(uint dwUserIndex, ref XINPUT_STATE pState);


    /// <summary>
    /// Sends data to a connected controller. This function is used 
    /// to activate the vibration function of a controller.
    /// </summary>
    /// <param name="dwUserIndex">Index of the user's controller. 
    /// Can be a value from 0 to 3.</param>
    /// <param name="pVibration">Reference to an <see cref="XINPUT_VIBRATION"/> 
    /// structure containing the vibration information to send to the controller.</param>
    /// <returns>
    /// If the function succeeds, the return value is 
    /// <see cref="Win32ErrorCodes.ERROR_SUCCESS"/>.
    /// If the controller is not connected, the return value is 
    /// <see cref="Win32ErrorCodes.ERROR_DEVICE_NOT_CONNECTED"/>.
    /// If the function fails, the return value is an error code defined in Winerror.h. 
    /// The function does not use SetLastError to set the calling thread's last-error code.
    /// </returns>
    [DllImport(Dll_XInput910, EntryPoint = "XInputSetState", SetLastError = false)]
    internal static extern uint XInputSetState(uint dwUserIndex, ref XINPUT_VIBRATION pVibration);

    #endregion Methods


}
