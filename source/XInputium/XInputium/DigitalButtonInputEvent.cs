using System;

namespace XInputium;

/// <summary>
/// Implements an <see cref="InputEvent"/> that can trigger 
/// events depending on the state of a <see cref="DigitalButton"/>
/// derived instance.
/// </summary>
/// <typeparam name="T">A <see cref="DigitalButton"/> or a 
/// type derived from <see cref="DigitalButton"/>, that is 
/// the button type associated with the event.</typeparam>
/// <seealso cref="InputEvent"/>
/// <seealso cref="DigitalButton"/>
public class DigitalButtonInputEvent<T> : InputEvent
    where T : notnull, DigitalButton
{


    #region Fields

    private readonly DigitalButtonInputEventArgs<T> _eventArgs;  // Cached event arguments for the event.

    private bool _isActive = false;  // Is the condition that triggered the event still true?
    private bool _wasPressed = false;  // Was the button pressed, on the last update?

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of a 
    /// <see cref="DigitalButtonInputEvent{T}"/> 
    /// class, that is associated with the specified 
    /// <typeparamref name="T"/>, uses the specified triggering mode, 
    /// and, depending on the triggering mode, must be held for the 
    /// specified duration to trigger an event.
    /// </summary>
    /// <param name="button"><typeparamref name="T"/> instance, of 
    /// which state will be used to determine when to trigger an event.</param>
    /// <param name="mode"><see cref="DigitalButtonInputEventMode"/> 
    /// constant that specifies when the event will be triggered.</param>
    /// <param name="holdDuration">If <paramref name="mode"/> is 
    /// <see cref="DigitalButtonInputEventMode.OnHold"/>, this specifies 
    /// the minimum amount of time the button must be held (pressed) 
    /// for the event to trigger; otherwise, <see cref="TimeSpan.Zero"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="button"/> 
    /// is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="mode"/> is not 
    /// a defined constant in a <see cref="DigitalButtonInputEventMode"/> 
    /// enumeration.</exception>
    public DigitalButtonInputEvent(T button,
        DigitalButtonInputEventMode mode, TimeSpan holdDuration)
        : base()
    {
        if (button is null)
            throw new ArgumentNullException((nameof(button)));
        if (!Enum.IsDefined(mode))
            throw new ArgumentException(
                $"'{mode}' is not a defined constant in a " +
                $"'{nameof(DigitalButtonInputEventMode)}' enumeration.",
                nameof(mode));

        Button = button;
        Mode = mode;
        HoldDuration = holdDuration > TimeSpan.Zero ? holdDuration : TimeSpan.Zero;

        _eventArgs = new(this, button);
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the <typeparamref name="T"/> instance associated 
    /// with the <see cref="DigitalButtonInputEvent{T}"/>.
    /// </summary>
    public T Button { get; }


    /// <summary>
    /// Gets the <see cref="DigitalButtonInputEventMode"/> constant 
    /// that specifies when the event is triggered.
    /// </summary>
    /// <seealso cref="DigitalButtonInputEventMode"/>
    public DigitalButtonInputEventMode Mode { get; }


    /// <summary>
    /// Gets a <see cref="TimeSpan"/> that specifies the 
    /// minimum amount of time that the associated 
    /// <see cref="DigitalButton"/> has to be held for, 
    /// for the event to be triggered, when <see cref="Mode"/> 
    /// is <see cref="DigitalButtonInputEventMode.OnHold"/>.
    /// </summary>
    /// <seealso cref="Mode"/>
    /// <seealso cref="DigitalButtonInputEventMode.OnHold"/>
    public TimeSpan HoldDuration { get; }

    #endregion Properties


    #region Methods

    /// <summary>
    /// Updates the event logic with the current state of the 
    /// associated <typeparamref name="T"/>. Overrides 
    /// <see cref="InputEvent.OnUpdate(TimeSpan)"/>.
    /// </summary>
    /// <param name="time">Amount of time elapsed since the 
    /// last update operation.</param>
    /// <seealso cref="InputEvent.OnUpdate(TimeSpan)"/>
    /// <seealso cref="InputEvent.Update(TimeSpan)"/>
    protected override void OnUpdate(TimeSpan time)
    {
        try
        {
            if (Mode == DigitalButtonInputEventMode.OnPress)
            {
                if (!_isActive && !_wasPressed && Button.IsPressed)
                {
                    _isActive = true;
                    Raise(this, _eventArgs);
                }
                else if (!Button.IsPressed)
                {
                    _isActive = false;
                }
            }
            else if (Mode == DigitalButtonInputEventMode.OnRelease)
            {
                if (!_isActive && _wasPressed && !Button.IsPressed)
                {
                    _isActive = true;
                    Raise(this, _eventArgs);
                }
                else if (Button.IsPressed)
                {
                    _isActive = false;
                }
            }
            else if (Mode == DigitalButtonInputEventMode.OnHold)
            {
                if (!_isActive && Button.IsHolding(HoldDuration))
                {
                    _isActive = true;
                    Raise(this, _eventArgs);
                }
                else if (!Button.IsPressed)
                {
                    _isActive = false;
                }
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            _wasPressed = Button.IsPressed;
        }
    }

    #endregion Methods


}
