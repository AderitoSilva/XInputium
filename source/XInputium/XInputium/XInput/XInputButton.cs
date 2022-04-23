using System;

namespace XInputium.XInput;

/// <summary>
/// Implements a <see cref="DigitalButton"/> that represents 
/// an XInput button.
/// </summary>
/// <seealso cref="DigitalButton"/>
/// <seealso cref="XInputButtonSet"/>
/// <seealso cref="XButtons"/>
public class XInputButton : DigitalButton
{


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an <see cref="XInputButton"/> 
    /// class that represents the specified button and supports 
    /// state updating.
    /// </summary>
    /// <param name="button">An <see cref="XButtons"/> constant 
    /// that represents the XInput button that will be associated 
    /// with the <see cref="XInputButton"/>.</param>
    /// <param name="updateCallback">Variable that will be set with 
    /// a <see cref="DigitalButtonUpdateCallback"/> delegate that 
    /// you can invoke from your code to update the state of the 
    /// new <see cref="XInputButton"/> instance.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="button"/> is not a defined constant of an 
    /// <see cref="XButtons"/> enumeration or it is 
    /// <see cref="XButtons.None"/>.</exception>
    public XInputButton(XButtons button,
        out DigitalButtonUpdateCallback updateCallback)
        : base(out updateCallback)
    {
        if (button == XButtons.None || !Enum.IsDefined(button))
            throw new ArgumentException(
                $"'{button}' is not a valid value for '{nameof(button)}' " +
                $"parameter. A specific button constant is required.");

        Button = button;
    }


    /// <summary>
    /// Initializes a new instance of an <see cref="XInputButton"/> 
    /// class that represents the specified button and has the 
    /// specified immutable state.
    /// </summary>
    /// <param name="button">An <see cref="XButtons"/> constant 
    /// that represents the XInput button that will be associated 
    /// with the <see cref="XInputButton"/>.</param>
    /// <param name="isPressed"><see langword="true"/> to indicate 
    /// the button is currently being pressed or 
    /// <see langword="false"/> to indicate the button is currently 
    /// in the released state.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="button"/> is not a defined constant of an 
    /// <see cref="XButtons"/> enumeration or it is 
    /// <see cref="XButtons.None"/>.</exception>
    public XInputButton(XButtons button, bool isPressed)
        : base(isPressed)
    {
        if (button == XButtons.None || !Enum.IsDefined(button))
            throw new ArgumentException(
                $"'{button}' is not a valid value for '{nameof(button)}' " +
                $"parameter. A specific button constant is required.");

        Button = button;
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the <see cref="XButtons"/> constant that represents 
    /// the XInput button associated with the current 
    /// <see cref="XInputButton"/> instance.
    /// </summary>
    public XButtons Button { get; }

    #endregion Properties


    #region Methods

    /// <summary>
    /// Gets the <see cref="string"/> representation of the 
    /// current <see cref="XInputButton"/> instance.
    /// </summary>
    /// <returns>The <see cref="string"/> representation of 
    /// the current <see cref="XInputButton"/> instance.</returns>
    /// <seealso cref="Button"/>
    public override string ToString()
    {
        return Button.ToString();
    }

    #endregion Methods


}
