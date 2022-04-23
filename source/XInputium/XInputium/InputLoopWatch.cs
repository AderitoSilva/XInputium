using System;
using XInputium.Internal;

namespace XInputium;

/// <summary>
/// Provides the base for classes that provide time measurement 
/// features for input loops.
/// </summary>
/// <remarks>
/// The default implementation of <see cref="InputLoopWatch"/>
/// can be obtained using the <see cref="GetDefault()"/> static 
/// method, which uses the system counter to measure time.
/// </remarks>
public abstract class InputLoopWatch
{


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an <see cref="InputLoopWatch"/> 
    /// class.
    /// </summary>
    public InputLoopWatch()
    {

    }

    #endregion Constructors


    #region Methods

    /// <summary>
    /// When overridden in a derived class, stops time measurement 
    /// and resets the measured time to 0.
    /// </summary>
    /// <seealso cref="GetTime()"/>
    public abstract void Reset();


    /// <summary>
    /// When overridden in a derived class, gets the amount 
    /// of time elapsed since the last call to 
    /// <see cref="GetTime()"/> method.
    /// </summary>
    /// <returns>The amount of time elapsed since the last 
    /// call to <see cref="GetTime()"/> method. If this is 
    /// the first time <see cref="GetTime()"/> method is 
    /// called or if <see cref="Reset()"/> was called after 
    /// the last call to <see cref="GetTime()"/>, 
    /// <see cref="TimeSpan.Zero"/> is returned.</returns>
    /// <seealso cref="Reset()"/>
    public abstract TimeSpan GetTime();


    /// <summary>
    /// Gets the default <see cref="InputLoopWatch"/>, that uses the 
    /// system's counter to measure time.
    /// </summary>
    /// <returns>A new instance that is the default implementation of 
    /// <see cref="InputLoopWatch"/>.</returns>
    public static InputLoopWatch GetDefault()
    {
        return new DefaultInputLoopWatch();
    }

    #endregion Methods


}
