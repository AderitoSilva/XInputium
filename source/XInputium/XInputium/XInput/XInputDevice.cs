using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XInputium.XInput.Internal.Win32;

namespace XInputium.XInput;

/// <summary>
/// Represents an XInput controller device.
/// </summary>
/// <remarks>
/// <see cref="XInputDevice"/> provides the means to 
/// communicate with an XInput device that is connected to 
/// the system. You instantiate an <see cref="XInputDevice"/> 
/// class to communicate with a single XInput device that 
/// may or may not be connected at a specific XInput user 
/// index.
/// <br/><br/>
/// <see cref="XInputDevice"/> provides several static 
/// members that provide you with ways to determine what 
/// XInput devices are available on the system or to get 
/// and set their state.
/// <br/><br/>
/// <see cref="XInputDevice"/> is designed to be lightweight 
/// and, thus, provides only core functionality to work 
/// with XInput devices. For an alternative that offers a 
/// more complete and comprehensive set of features, 
/// consider using <see cref="XGamepad"/> class.
/// <see cref="XGamepad"/> abstracts and extends an 
/// <see cref="XInputDevice"/> instance to provide 
/// more advanced features, at the expense of being more 
/// resource intensive than <see cref="XInputDevice"/>. 
/// See <see cref="XGamepad"/> for more information.
/// <br/><br/>
/// <see cref="XInputDevice"/> class is the only type in
/// the <see cref="XInputium"/> namespace that performs 
/// external XInput API calls. This means this is the 
/// primary class in the namespace to work with XInput.
/// </remarks>
/// <seealso cref="InputDevice{TState}"/>
/// <seealso cref="XInputDeviceState"/>
/// <seealso cref="XGamepad"/>
[DebuggerDisplay($"{nameof(UserIndex)} = {{{nameof(UserIndex)}}}")]
public class XInputDevice : InputDevice<XInputDeviceState>
{


    #region Fields

    private static readonly XInputUserIndex[] s_UserIndexes = Enum.GetValues<XInputUserIndex>();

    private static readonly PropertyChangedEventArgs s_EA_LeftMotorSpeed = new(nameof(LeftMotorSpeed));
    private static readonly PropertyChangedEventArgs s_EA_RightMotorSpeed = new(nameof(RightMotorSpeed));

    private float _leftMotorSpeed = 0f;  // Store for the value of LeftMotorSpeed property.
    private float _rightMotorSpeed = 0f;  // Store for the value of RightMotorSpeed property.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="XInputDevice"/>
    /// class that can be used to communicate with an XInput 
    /// device at the specified XInput user index.
    /// </summary>
    /// <param name="userIndex"><see cref="XInputUserIndex"/> 
    /// constant that specifies the XInput user index of the 
    /// XInput device.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="userIndex"/> is not a defined constant 
    /// of an <see cref="XInputUserIndex"/> enumeration.</exception>
    /// <seealso cref="XInputUserIndex"/>
    public XInputDevice(XInputUserIndex userIndex)
        : base()
    {
        if (!Enum.IsDefined(userIndex))
            throw new ArgumentException(
                $"'{userIndex}' is not a defined constant of an " +
                $"'{nameof(XInputUserIndex)}' enumeration.",
                nameof(userIndex));

        UserIndex = userIndex;
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the user index for the current <see cref="XInputDevice"/>.
    /// </summary>
    /// <seealso cref="XInputUserIndex"/>
    public XInputUserIndex UserIndex { get; }


    /// <summary>
    /// Gets or sets the rotation speed of the device's left motor.
    /// </summary>
    /// <value>A value between 0 and 1, where 0 means the motor is 
    /// stopped and 1 means the motor is at its full rotation speed.</value>
    /// <exception cref="ArgumentException">The value being set to the 
    /// property is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// The value of this property is based on the last call 
    /// to <see cref="SetMotorsSpeed(float, float)"/> method. 
    /// Because of how the internal XInput API works, there is not way 
    /// to determine the current rotation speed of a device's motor. 
    /// If you set the motor rotation speed outside the current 
    /// <see cref="XInputDevice"/> instance or if you instantiate 
    /// the <see cref="XInputDevice"/> when the device's motor 
    /// is not stopped, this property will not return an accurate 
    /// value until the next time you call 
    /// <see cref="SetMotorsSpeed(float, float)"/> method.
    /// <br/><br/>
    /// Setting the motor speed using this property will require 
    /// an internal call to the XInput API. If you intend to set 
    /// the rotation speeds of both motors, consider using 
    /// <see cref="SetMotorsSpeed(float, float)"/> method, to 
    /// avoid needing to make an API call for each motor. 
    /// <see cref="SetMotorsSpeed(float, float)"/> sets the speed 
    /// of both motors at once, using one single XInput API call.
    /// </remarks>
    /// <seealso cref="RightMotorSpeed"/>
    /// <seealso cref="SetMotorsSpeed(float, float)"/>
    public float LeftMotorSpeed
    {
        get => _leftMotorSpeed;
        set => SetMotorsSpeed(value, RightMotorSpeed);
    }


    /// <summary>
    /// Gets or sets the rotation speed of the device's right motor.
    /// </summary>
    /// <value>A value between 0 and 1, where 0 means the motor is 
    /// stopped and 1 means the motor is at its full rotation speed.</value>
    /// <exception cref="ArgumentException">The value being set to the 
    /// property is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// The value of this property is based on the last call 
    /// to <see cref="SetMotorsSpeed(float, float)"/> method. 
    /// Because of how the internal XInput API works, there is not way 
    /// to determine the current rotation speed of a device's motor. 
    /// If you set the motor rotation speed outside the current 
    /// <see cref="XInputDevice"/> instance or if you instantiate 
    /// the <see cref="XInputDevice"/> when the device's motor 
    /// is not stopped, this property will not return an accurate 
    /// value until the next time you call 
    /// <see cref="SetMotorsSpeed(float, float)"/> method.
    /// <br/><br/>
    /// Setting the motor speed using this property will require 
    /// an internal call to the XInput API. If you intend to set 
    /// the rotation speeds of both motors, consider using 
    /// <see cref="SetMotorsSpeed(float, float)"/> method, to 
    /// avoid needing to make an API call for each motor. 
    /// <see cref="SetMotorsSpeed(float, float)"/> sets the speed 
    /// of both motors at once, using one single XInput API call.
    /// </remarks>
    /// <seealso cref="LeftMotorSpeed"/>
    /// <seealso cref="SetMotorsSpeed(float, float)"/>
    public float RightMotorSpeed
    {
        get => _rightMotorSpeed;
        set => SetMotorsSpeed(LeftMotorSpeed, value);
    }

    #endregion Properties


    #region Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ConvertFromUserIndex(in XInputUserIndex userIndex)
    {
        return (uint)userIndex;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static XButtons ConvertToXButtons(in XINPUT_GAMEPAD_wButtons buttons)
    {
        return (XButtons)buttons;
    }


    /// <summary>
    /// Creates a new <see cref="XInputDeviceState"/> object. 
    /// Overrides <see cref="InputDevice{TState}.CreateStateInstance()"/>.
    /// </summary>
    /// <returns>A new <see cref="XInputDeviceState"/> object.</returns>
    /// <seealso cref="XInputDeviceState"/>
    protected override XInputDeviceState CreateStateInstance()
    {
        return XInputDeviceState.Empty;
    }


    /// <summary>
    /// Gets the current state of the XInput device associated 
    /// with the current <see cref="XInputDevice"/>. Overrides 
    /// <see cref="InputDevice{TState}.UpdateState(ref TState)"/>
    /// </summary>
    /// <returns>An <see cref="XInputDeviceState"/> that represents 
    /// the current state of the XInput device currently connected 
    /// at <see cref="UserIndex"/>. If no device is connected at 
    /// <see cref="UserIndex"/>, returns 
    /// <see cref="XInputDeviceState.Empty"/>.</returns>
    /// <seealso cref="GetState(XInputUserIndex)"/>
    /// <seealso cref="XInputDeviceState"/>
    protected override void UpdateState(ref XInputDeviceState state)
    {
        state = GetState(UserIndex);
    }


    /// <summary>
    /// Sets the rotation speed of both device's motors.
    /// </summary>
    /// <param name="leftMotorSpeed">Left motor rotation speed. 
    /// A value between 0 and 1, where 0 means the motor is 
    /// stopped and 1 means the motor is rotating at its full 
    /// rotation speed.</param>
    /// <param name="rightMotorSpeed">Right motor rotation speed. 
    /// A value between 0 and 1, where 0 means the motor is 
    /// stopped and 1 means the motor is rotating at its full 
    /// rotation speed.</param>
    /// <returns><see langword="true"/> if the device is connected 
    /// and the motors rotation speed was successfully set; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="leftMotorSpeed"/>
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="rightMotorSpeed"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// Although <see cref="InputDevice{TState}.IsConnected"/> 
    /// may report the device is currently connected, it might 
    /// not be anymore. This property reports the state of the 
    /// device accordingly to the information currently in 
    /// <see cref="InputDevice{TState}.CurrentState"/> property. 
    /// This information is only updated when 
    /// <see cref="InputDevice{TState}.Update()"/> method is 
    /// called. This is intended behavior, so the reported state 
    /// of the device only changes when explicitly asked, through 
    /// a formal state update. See 
    /// <see cref="InputDevice{TState}.IsConnected"/> property 
    /// for more information.
    /// </remarks>
    /// <seealso cref="SetState(XInputUserIndex, float, float)"/>
    public bool SetMotorsSpeed(float leftMotorSpeed, float rightMotorSpeed)
    {
        if (SetState(UserIndex, leftMotorSpeed, rightMotorSpeed))
        {
            SetProperty(ref _leftMotorSpeed, leftMotorSpeed, s_EA_LeftMotorSpeed);
            SetProperty(ref _rightMotorSpeed, rightMotorSpeed, s_EA_RightMotorSpeed);
            return true;
        }
        return false;
    }


    /// <summary>
    /// Gets an <see cref="XInputDeviceState"/> that represents 
    /// the current state of the XInput controller device that 
    /// is connected at the specified XInput user index.
    /// </summary>
    /// <param name="userIndex"><see cref="XInputUserIndex"/> 
    /// constant that specifies the user index where the device 
    /// can be connected.</param>
    /// <returns>An <see cref="XInputDeviceState"/> state that 
    /// represents the current state of the device. If no device 
    /// was found at <paramref name="userIndex"/>, 
    /// <see cref="XInputDeviceState.IsEmpty"/> is returned.</returns>
    /// <exception cref="ArgumentException"><paramref name="userIndex"/> 
    /// is not a defined constant in an <see cref="XInputUserIndex"/> 
    /// enumeration.</exception>
    /// <remarks>
    /// XInput only supports up to four controller devices connected 
    /// simultaneously, and they are represented by an user index. 
    /// <see cref="XInputUserIndex"/> enumeration provides constants 
    /// that represent these user indexes. This method can get the 
    /// state of any device at the specified <paramref name="userIndex"/>.
    /// </remarks>
    /// <seealso cref="XInputDeviceState"/>
    public static XInputDeviceState GetState(XInputUserIndex userIndex)
    {
        if (!Enum.IsDefined(userIndex))
            throw new ArgumentException(
                $"'{userIndex}' is not a defined constant of " +
                $"an '{nameof(XInputUserIndex)}' enumeration.",
                nameof(userIndex));

        uint dwUserIndex = ConvertFromUserIndex(userIndex);
        XINPUT_STATE state = new();
        Win32ErrorCodes result = (Win32ErrorCodes)NativeMethods
            .XInputGetState(dwUserIndex, ref state);
        if (result == Win32ErrorCodes.ERROR_SUCCESS)
        {
            static float ConvertThumbToFloat(short axis)
            {
                return ((float)axis) / (axis >= 0 ? 32767 : 32768);
            }
            static float ConvertTriggerToFloat(byte axis)
            {
                return ((float)axis) / byte.MaxValue;
            }
            return new XInputDeviceState(
                isConnected: true,
                buttons: ConvertToXButtons(in state.Gamepad.wButtons),
                leftJoystick: new(ConvertThumbToFloat(state.Gamepad.sThumbLX),
                                   ConvertThumbToFloat(state.Gamepad.sThumbLY)),
                rightJoystick: new(ConvertThumbToFloat(state.Gamepad.sThumbRX),
                                    ConvertThumbToFloat(state.Gamepad.sThumbRY)),
                leftTrigger: new(ConvertTriggerToFloat(state.Gamepad.bLeftTrigger)),
                rightTrigger: new(ConvertTriggerToFloat(state.Gamepad.bRightTrigger))
            );
        }
        else if (result == Win32ErrorCodes.ERROR_DEVICE_NOT_CONNECTED)
        {
            return XInputDeviceState.Empty;
        }
        else
        {
            // Here, some error occurred, and 'result' has the Win32 error
            // code that specifies the error. However, for now, we are just
            // returning an empty state, as if the device wasn't connected.
            return XInputDeviceState.Empty;
            // TODO Consider throwing an Exception when getting the device state.
            //throw new Win32Exception((int)result);
        }
    }


    /// <summary>
    /// Sets the state of the XInput device that is connected 
    /// at the specified XInput user index.
    /// </summary>
    /// <param name="userIndex"><see cref="XInputUserIndex"/> 
    /// constant that specifies the XInput user index of the 
    /// connected XInput device.</param>
    /// <param name="leftMotorSpeed">Rotation speed of the left 
    /// motor. This is a value between 0 and 1, where 0 means 
    /// the motor is stopped and 1 means the motor is at its 
    /// full rotation speed.</param>
    /// <param name="rightMotorSpeed">Rotation speed of the right 
    /// motor. This is a value between 0 and 1, where 0 means 
    /// the motor is stopped and 1 means the motor is at its 
    /// full rotation speed.</param>
    /// <returns><see langword="true"/> if there is an XInput 
    /// device connected at <paramref name="userIndex"/> and 
    /// the state was successfully set; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="userIndex"/> is not a defined constant 
    /// of an <see cref="XInputUserIndex"/> enumeration.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="leftMotorSpeed"/> is 
    /// <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="rightMotorSpeed"/> is 
    /// <see cref="float.NaN"/>.</exception>
    public static bool SetState(XInputUserIndex userIndex,
        float leftMotorSpeed, float rightMotorSpeed)
    {
        // Validate parameters.
        if (!Enum.IsDefined(userIndex))
            throw new ArgumentException(
                $"'{userIndex}' is not a defined constant of " +
                $"an '{nameof(XInputUserIndex)}' enumeration.",
                nameof(userIndex));
        if (float.IsNaN(leftMotorSpeed))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(leftMotorSpeed)}' parameter.",
                nameof(leftMotorSpeed));
        if (float.IsNaN(rightMotorSpeed))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(rightMotorSpeed)}' parameter.",
                nameof(rightMotorSpeed));

        leftMotorSpeed = Math.Clamp(leftMotorSpeed, 0, 1);
        rightMotorSpeed = Math.Clamp(rightMotorSpeed, 0, 1);

        // Set the controller motors speed.
        XINPUT_VIBRATION vibration = new()
        {
            wLeftMotorSpeed = Math.Clamp((ushort)(leftMotorSpeed * ushort.MaxValue), ushort.MinValue, ushort.MaxValue),
            wRightMotorSpeed = Math.Clamp((ushort)(rightMotorSpeed * ushort.MaxValue), ushort.MinValue, ushort.MaxValue),
        };

        Win32ErrorCodes result = (Win32ErrorCodes)NativeMethods
            .XInputSetState(ConvertFromUserIndex(userIndex), ref vibration);

        // If the motors speed was successfully changed, update our store fields.
        if (result == Win32ErrorCodes.ERROR_SUCCESS)
        {
            return true;
        }
        else if (result == Win32ErrorCodes.ERROR_DEVICE_NOT_CONNECTED)
        {
            return false;
        }
        else
        {
            // Here, a Win32 error occurred. For now, we're just returning false.
            return false;
            // TODO Throw an Exception when a Win32 error occurs when setting the XInput device state.
            //throw new Win32Exception((int)result);
        }
    }


    /// <summary>
    /// Determines if an XInput device is connected at the specified 
    /// XInput user index.
    /// </summary>
    /// <param name="userIndex"><see cref="XInputUserIndex"/> constant 
    /// that specifies the XInput user index of the device to check.</param>
    /// <returns><see langword="true"/> if there is a device connected 
    /// at <paramref name="userIndex"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="userIndex"/> 
    /// is not a defined constant in an 
    /// <see cref="XInputUserIndex"/> enumeration.</exception>
    /// <seealso cref="GetConnectedDeviceIndexes()"/>
    /// <seealso cref="IsAnyDeviceConnected()"/>
    public static bool IsDeviceConnected(XInputUserIndex userIndex)
    {
        if (!Enum.IsDefined(userIndex))
            throw new ArgumentException(
                $"'{userIndex}' is not a defined constant of an " +
                $"'{nameof(XInputUserIndex)}' enumeration.",
                nameof(userIndex));

        XINPUT_STATE state = new();
        Win32ErrorCodes result = (Win32ErrorCodes)NativeMethods
            .XInputGetState(ConvertFromUserIndex(userIndex), ref state);
        return result == Win32ErrorCodes.ERROR_SUCCESS;
    }


    /// <summary>
    /// Gets an enumerable that can iterate through the XInput user 
    /// indexes of all of the XInput devices that are currently 
    /// connected to the system.
    /// </summary>
    /// <returns>A enumerable that can be used to iterate through 
    /// the XInput user indexes of the devices that are currently 
    /// connected to the system.</returns>
    /// <remarks>
    /// You can use XInput user index to get the current state of 
    /// the associated XInput device using 
    /// <see cref="GetState(XInputUserIndex)"/> method or you 
    /// can create an <see cref="XInputDevice"/> instance to 
    /// continuously obtain the state of the XInput device 
    /// connected at the user index.
    /// </remarks>
    /// <seealso cref="IsDeviceConnected(XInputUserIndex)"/>
    /// <seealso cref="IsAnyDeviceConnected()"/>
    /// <seealso cref="XInputUserIndex"/>
    public static IEnumerable<XInputUserIndex> GetConnectedDeviceIndexes()
    {
        for (int i = 0; i < s_UserIndexes.Length; i++)
        {
            if (IsDeviceConnected(s_UserIndexes[i]))
            {
                yield return s_UserIndexes[i];
            }
        }
    }


    /// <summary>
    /// Determines if there is any XInput controller device 
    /// currently connected to the system.
    /// </summary>
    /// <returns><see langword="true"/> if there is, at least, 
    /// one XInput device connected to the system; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="GetConnectedDeviceIndexes()"/>
    /// <seealso cref="IsDeviceConnected(XInputUserIndex)"/>
    public static bool IsAnyDeviceConnected()
    {
        return GetConnectedDeviceIndexes().Any();
    }


    /// <summary>
    /// Tries to find a connected XInput device and returns 
    /// an <see cref="XInputDevice"/> instance that can be used 
    /// to communicate the with the first device that was found.
    /// </summary>
    /// <returns>A new <see cref="XInputDevice"/> instance that 
    /// represents the first XInput device that was found 
    /// connected to the system, in order of XInput user index. 
    /// If no connected device was found, returns 
    /// <see langword="null"/>.</returns>
    /// <seealso cref="IsAnyDeviceConnected()"/>
    /// <seealso cref="IsDeviceConnected(XInputUserIndex)"/>
    /// <seealso cref="GetConnectedDeviceIndexes()"/>
    public static XInputDevice? GetFirstConnectedDevice()
    {
        foreach (var userIndex in GetConnectedDeviceIndexes())
        {
            return new XInputDevice(userIndex);
        }
        return null;
    }

    #endregion Methods


}
