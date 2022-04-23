using System;
using System.Diagnostics;

namespace XInputium.Internal;

/// <summary>
/// Implements an <see cref="InputLoopWatch"/> that uses 
/// the system counter to measure time.
/// </summary>
/// <seealso cref="InputLoopWatch"/>
internal class DefaultInputLoopWatch : InputLoopWatch
{


    #region Fields

    private readonly Stopwatch _stopwatch;  // Stopwatch used to measure time in the current instance.

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of a <see cref="DefaultInputLoopWatch"/> 
    /// class.
    /// </summary>
    public DefaultInputLoopWatch()
        : base()
    {
        _stopwatch = new Stopwatch();
    }

    #endregion Constructors


    #region Methods

    /// <summary>
    /// Stops time measurement and resets the measured time 
    /// to 0.
    /// Overrides <see cref="InputLoopWatch.Reset()"/>.
    /// </summary>
    /// <seealso cref="GetTime()"/>
    public override void Reset()
    {
        _stopwatch.Reset();
    }


    /// <summary>
    /// Gets the amount of time elapsed since the last call to 
    /// <see cref="GetTime()"/> method. 
    /// Overrides <see cref="InputLoopWatch.GetTime()"/>.
    /// </summary>
    /// <returns>The amount of time elapsed since the last call 
    /// to <see cref="GetTime()"/> method.</returns>
    /// <seealso cref="Reset()"/>
    public override TimeSpan GetTime()
    {
        TimeSpan time = _stopwatch.Elapsed;
        _stopwatch.Restart();
        return time;
    }

    #endregion Methods


}
