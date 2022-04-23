using System;

namespace XInputium;

/// <summary>
/// Represents the state of a physical input device.
/// </summary>
/// <remarks>
/// Types implementing <see cref="IInputDeviceState"/> 
/// interface can be used to represent input device 
/// states in an <see cref="InputDevice{TState}"/> 
/// derived class.
/// <br/><br/>
/// Although any object type can implement 
/// <see cref="IInputDeviceState"/> interface, these 
/// types are intended to be performance oriented, 
/// meaning they should avoid being too resource 
/// intensive as much as possible.
/// <br/><br/>
/// If your type implements <see cref="IInputDeviceState"/> 
/// and use your derived type as the input device state 
/// type in an <see cref="InputDevice{TState}"/> derived 
/// class, your type will be used by that class in 
/// different ways, depending on whether your type is 
/// a reference type of a value type. If your type is 
/// a reference type, only two instances of it will be 
/// ever created per each instance of 
/// <see cref="InputDevice{TState}"/> derived class, 
/// and your type's instances will be recycled as 
/// necessary to represent newer states. If your type 
/// is a value type, your type's objects will be copied 
/// by value on each state update. This is a measure to 
/// improve performance, avoiding the creation of 
/// unused objects in memory and avoiding the need for 
/// garbage collection by the CLR runtime.
/// </remarks>
/// <seealso cref="InputDevice{TState}"/>
public interface IInputDeviceState
{


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates the input 
    /// device is currently connected to the system and 
    /// available for usage.
    /// </summary>
    bool IsConnected { get; }


    /// <summary>
    /// Determines if the current <see cref="IInputDeviceState"/> 
    /// represents an input state that is identical to the input 
    /// state represented by the specified 
    /// <see cref="IInputDeviceState"/> object.
    /// </summary>
    /// <param name="state"><see cref="IInputDeviceState"/> derived 
    /// object to compare.</param>
    /// <returns><see langword="true"/> if the current object 
    /// represents an input state that is identical to that of 
    /// <paramref name="state"/>;
    /// otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method considers only the state represented by the 
    /// compared <see cref="IInputDeviceState"/> objects. It 
    /// doesn't consider other differences these objects may have 
    /// (like, references or properties not related to input state). 
    /// To compare equality between objects, the implementation of 
    /// an object's <see cref="object.Equals(object?)"/> method is 
    /// usually the preferable option.
    /// </remarks>
    bool StateEquals(IInputDeviceState state);


}
