using System;

namespace XInputium;

/// <summary>
/// Encapsulates information about an event that was triggered 
/// by an <see cref="InputEvent"/> and is associated with 
/// a specific <see cref="DigitalButton"/>.
/// </summary>
/// <typeparam name="T"><see cref="DigitalButton"/> or a 
/// type deriving from <see cref="DigitalButton"/>. This is 
/// the type of the button associated with the event.</typeparam>
/// <seealso cref="DigitalButtonInputEventHandler{T}"/>
/// <seealso cref="DigitalButtonInputEvent{T}"/>
/// <seealso cref="DigitalButton"/>
/// <seealso cref="InputEvent"/>
public class DigitalButtonInputEventArgs<T> : InputEventArgs
    where T : notnull, DigitalButton
{


    #region Constructors

    /// <summary>
    /// Initializes a new instance of a 
    /// <see cref="DigitalButtonInputEventArgs{T}"/> class
    /// that is associated with the specified 
    /// <see cref="InputEvent"/> and <typeparamref name="T"/> 
    /// button.
    /// </summary>
    /// <param name="inputEvent"><see cref="InputEvent"/> 
    /// associated with the event.</param>
    /// <param name="button"><typeparamref name="T"/> instance 
    /// representing the button associated with event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputEvent"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="button"/> is <see langword="null"/>.</exception>
    public DigitalButtonInputEventArgs(
        InputEvent inputEvent, T button)
        : base(inputEvent)
    {
        if (button is null)
            throw new ArgumentNullException(nameof(button));

        Button = button;
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the <typeparamref name="T"/> button instance 
    /// associated with the event.
    /// </summary>
    public T Button { get; }

    #endregion Properties


}
