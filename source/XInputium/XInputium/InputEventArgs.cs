using System;

namespace XInputium;

/// <summary>
/// Encapsulates information about an event that was 
/// triggered by an <see cref="InputEvent"/> instance.
/// </summary>
/// <seealso cref="InputEventHandler"/>
/// <seealso cref="InputEvent"/>
/// <seealso cref="EventArgs"/>
public class InputEventArgs : EventArgs
{


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an <see cref="InputEventArgs"/> 
    /// class, that is associated with the specified 
    /// <see cref="InputEvent"/>.
    /// </summary>
    /// <param name="inputEvent"><see cref="InputEvent"/> 
    /// that is triggering the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputEvent"/> is <see langword="null"/>.</exception>
    public InputEventArgs(InputEvent inputEvent)
    {
        if (inputEvent is null)
            throw new ArgumentNullException(nameof(inputEvent));

        Event = inputEvent;
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the <see cref="InputEvent"/> instance that 
    /// triggered the event.
    /// </summary>
    /// <seealso cref="InputEvent"/>
    public InputEvent Event { get; }

    #endregion Properties


}
