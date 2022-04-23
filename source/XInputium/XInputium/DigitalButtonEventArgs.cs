using System;

namespace XInputium;

/// <summary>
/// Encapsulates information about an event that is associated 
/// with a <see cref="DigitalButton"/> or a type that 
/// derived from <see cref="DigitalButton"/>.
/// </summary>
/// <typeparam name="T"><see cref="DigitalButton"/> or a type 
/// that derives from <see cref="DigitalButton"/>. This is the 
/// type of the button associated with the event.</typeparam>
/// <seealso cref="DigitalButtonEventHandler{T}"/>
/// <seealso cref="DigitalButton"/>
[Serializable]
public class DigitalButtonEventArgs<T> : EventArgs
    where T : notnull, DigitalButton
{


    #region Constructors

    /// <summary>
    /// Initializes a new instance of a <see cref="DigitalButtonEventArgs{T}"/> 
    /// class, that has the specified <typeparamref name="T"/> 
    /// button.
    /// </summary>
    /// <param name="button"><typeparamref name="T"/> instance 
    /// that represents the button associated with the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="button"/> is <see langword="null"/>.</exception>
    public DigitalButtonEventArgs(T button)
    {
        if (button is null)
            throw new ArgumentNullException(nameof(button));

        Button = button;
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the <typeparamref name="T"/> instance that represents the 
    /// button associated with the event.
    /// </summary>
    public T Button { get; }

    #endregion Properties


}
