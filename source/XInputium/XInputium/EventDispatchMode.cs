using System;

namespace XInputium;

/// <summary>
/// Exposes constants the that represent the mode for 
/// dispatching events in an <see cref="EventDispatcherObject"/>.
/// </summary>
/// <seealso cref="EventDispatcherObject"/>
public enum EventDispatchMode
{


    /// <summary>
    /// Events are invoked immediately after they are raised.
    /// </summary>
    Immediate,

    /// <summary>
    /// Events get stacked when they are raised and are 
    /// invoked in sequence only when specified.
    /// </summary>
    Deferred,


}
