using System;

namespace XInputium.XInput;

/// <summary>
/// Exposes constants that represent an XInput user index.
/// </summary>
/// <remarks>
/// XInput supports up to four devices simultaneously connected 
/// to the system, and each device is identified by a constant 
/// index, which is called User Index. 
/// <see cref="XInputUserIndex"/> enumeration provides constants 
/// that represent these user indexes.
/// <br/><br/>
/// You use an <see cref="XInputUserIndex"/> constant to relate 
/// to a specific XInput device in the system, when working with 
/// the <see cref="XInputDevice"/> and <see cref="XGamepad"/> 
/// classes. <see cref="XInputDevice"/> uses an 
/// <see cref="XInputUserIndex"/> constant to communicate with 
/// a physical XInput device. See <see cref="XInputDevice"/> 
/// for more information.
/// </remarks>
/// <seealso cref="XInputDevice"/>
public enum XInputUserIndex
{


    /// <summary>
    /// XInput device user One.
    /// </summary>
    One = 0,

    /// <summary>
    /// XInput device user Two.
    /// </summary>
    Two = 1,

    /// <summary>
    /// XInput device user Three.
    /// </summary>
    Three = 2,

    /// <summary>
    /// XInput device user Four.
    /// </summary>
    Four = 3,


}
