using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace XInputium.XInput;

/// <summary>
/// Encapsulates a set of <see cref="XInputButton"/> objects 
/// that represent all of the buttons of an XInput controller, 
/// and provides the means to manage the state of all buttons.
/// </summary>
/// <seealso cref="XInputButton"/>
/// <seealso cref="XGamepad"/>
public class XInputButtonSet : EventDispatcherObject,
    IReadOnlyCollection<XInputButton>
{


    #region Internal types

    private sealed record ButtonAssociation(
        XInputButton Button, 
        DigitalButtonUpdateCallback UpdateCallback,
        DigitalButtonEventArgs<XInputButton> DigitalButtonEventArgs);

    #endregion Internal types


    #region Fields

    // Static PropertyChangedEventArgs to use for property value changes.
    private static readonly PropertyChangedEventArgs s_EA_Buttons = new(nameof(Buttons));
    private static readonly PropertyChangedEventArgs s_EA_PressedButtonsCount = new(nameof(PressedButtonsCount));
    private static readonly PropertyChangedEventArgs s_EA_IsAnyButtonPressed = new(nameof(IsAnyButtonPressed));

    // Property backing storage fields.
    private XButtons _buttons = XButtons.None;  // Store for the value of Buttons property.
    private int _pressedButtonsCount = 0;  // Store for the value of PressedButtonsCount property.
    private bool _isAnyButtonPressed = false;  // Store for the value of IsAnyButtonPressed property.

    // State keeping fields.
    private readonly Dictionary<XButtons, ButtonAssociation> _buttonAssociations = new(14);

    #endregion Fields


    #region Constructors

    private XInputButtonSet()
        : base(EventDispatchMode.Deferred)
    {
        DPadUp = CreateAndRegisterButton(XButtons.DPadUp);
        DPadDown = CreateAndRegisterButton(XButtons.DPadDown);
        DPadLeft = CreateAndRegisterButton(XButtons.DPadLeft);
        DPadRight = CreateAndRegisterButton(XButtons.DPadRight);
        Start = CreateAndRegisterButton(XButtons.Start);
        Back = CreateAndRegisterButton(XButtons.Back);
        A = CreateAndRegisterButton(XButtons.A);
        B = CreateAndRegisterButton(XButtons.B);
        X = CreateAndRegisterButton(XButtons.X);
        Y = CreateAndRegisterButton(XButtons.Y);
        LB = CreateAndRegisterButton(XButtons.LB);
        RB = CreateAndRegisterButton(XButtons.RB);
        LS = CreateAndRegisterButton(XButtons.LS);
        RS = CreateAndRegisterButton(XButtons.RS);
    }


    /// <summary>
    /// Initializes a new instance of an <see cref="XInputButtonSet"/> 
    /// class, that supports updating of the buttons state.
    /// </summary>
    /// <param name="updateCallback">A variable that will be set with an 
    /// <see cref="XInputButtonSetUpdateCallback"/> delegate, that can be 
    /// used to update the state of the <see cref="XInputButtonSet"/> 
    /// from external code.</param>
    public XInputButtonSet(out XInputButtonSetUpdateCallback updateCallback)
        : this()
    {
        updateCallback = new XInputButtonSetUpdateCallback(UpdateState);
    }


    /// <summary>
    /// Initializes a new instance of an <see cref="XInputButtonSet"/> 
    /// class that represents the immutable state of the 
    /// buttons determined by the specified button flags.
    /// </summary>
    /// <param name="buttonsState">Flags containing the 
    /// currently pressed buttons.</param>
    public XInputButtonSet(XButtons buttonsState)
        : this()
    {
        UpdateState(buttonsState, TimeSpan.Zero);
    }

    #endregion Constructors


    #region Events

    /// <summary>
    /// It's invoked whenever the pressed state of a button changes.
    /// </summary>
    /// <seealso cref="OnButtonStateChanged(DigitalButtonEventArgs{XInputButton})"/>
    public event DigitalButtonEventHandler<XInputButton>? ButtonStateChanged;

    #endregion Events


    #region Properties

    /// <summary>
    /// Gets the <see cref="XButtons"/> flags that specify 
    /// what buttons are currently being pressed.
    /// </summary>
    public XButtons Buttons
    {
        get => _buttons;
        private set => SetProperty(ref _buttons, in value, s_EA_Buttons);
    }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button D-Pad Up.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton DPadUp { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button D-Pad Down.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton DPadDown { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button D-Pad Left.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton DPadLeft { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button D-Pad Right.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton DPadRight { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button Start.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton Start { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button Back.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton Back { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button A.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton A { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button B.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton B { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button X.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton X { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button Y.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton Y { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button Left Shoulder.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton LB { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button Right Shoulder.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton RB { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button Left Stick.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton LS { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance that 
    /// represents the XInput button Right Stick.
    /// </summary>
    /// <seealso cref="XInputButton"/>
    public XInputButton RS { get; }


    /// <summary>
    /// Gets the <see cref="XInputButton"/> instance associated 
    /// with the specified <see cref="XButtons"/> constant.
    /// </summary>
    /// <param name="button"><see cref="XButtons"/> constant 
    /// representing the button to get the <see cref="XInputButton"/> 
    /// associate with it.</param>
    /// <returns>The <see cref="XInputButton"/> instance associated 
    /// with <paramref name="button"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is not a defined constant of an <see cref="XButtons"/> 
    /// enumeration.</exception>
    /// <seealso cref="XInputButton"/>
    public XInputButton this[XButtons button]
    {
        get
        {
            if (_buttonAssociations.TryGetValue(button,
                out ButtonAssociation? association))
            {
                return association.Button;
            }
            else
            {
                throw new ArgumentException(
                    $"'{button}' is not a valid value for '{nameof(button)}' parameter.",
                    nameof(button));
            }
        }
    }


    /// <summary>
    /// Gets the number of buttons that are currently being pressed.
    /// </summary>
    /// <seealso cref="IsAnyButtonPressed"/>
    public int PressedButtonsCount
    {
        get => _pressedButtonsCount;
        private set
        {
            if (SetProperty(ref _pressedButtonsCount,
                Math.Clamp(value, 0, _buttonAssociations.Count),
                s_EA_PressedButtonsCount))
            {
                IsAnyButtonPressed = _pressedButtonsCount > 0;
            }
        }
    }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if there is 
    /// currently any button being pressed.
    /// </summary>
    /// <seealso cref="PressedButtonsCount"/>
    public bool IsAnyButtonPressed
    {
        get => _isAnyButtonPressed;
        private set => SetProperty(ref _isAnyButtonPressed, 
            in value, s_EA_IsAnyButtonPressed);
    }


    int IReadOnlyCollection<XInputButton>.Count => _buttonAssociations.Count;

    #endregion Properties


    #region Methods

    private XInputButton CreateAndRegisterButton(XButtons button)
    {
        XInputButton xInputButton = new(button,
            out DigitalButtonUpdateCallback updateCallback);

        if (_buttonAssociations.TryAdd(button,
            new ButtonAssociation(xInputButton, updateCallback,
            new DigitalButtonEventArgs<XInputButton>(xInputButton))))
        {
            xInputButton.IsPressedChanged += Button_IsPressedChanged;
            return xInputButton;
        }
        else
        {
            throw new ArgumentException($"'{button}' is already created.");
        }
    }


    /// <summary>
    /// Raises the <see cref="ButtonStateChanged"/> event.
    /// </summary>
    /// <param name="e"><see cref="DigitalButtonEventArgs{XInputButton}"/> 
    /// instance containing information about the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="e"/> is <see langword="null"/>.</exception>
    /// <seealso cref="ButtonStateChanged"/>
    protected virtual void OnButtonStateChanged(DigitalButtonEventArgs<XInputButton> e)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        RaiseEvent(() => ButtonStateChanged?.Invoke(this, e));
    }


    private void UpdateState(XButtons buttonsState, TimeSpan time)
    {
        int pressedButtonsCount = 0;
        foreach (var buttonAssociation in _buttonAssociations.Values)
        {
            bool isButtonPressed = buttonsState.HasFlag(buttonAssociation.Button.Button);
            buttonAssociation.UpdateCallback.Invoke(isButtonPressed, time);
            if (isButtonPressed)
            {
                pressedButtonsCount++;
            }
        }
        PressedButtonsCount = pressedButtonsCount;
        Buttons = buttonsState;

        DispatchEvents();
    }


    /// <summary>
    /// Gets an enumerator that iterates through the 
    /// <see cref="XInputButton"/> instances of the 
    /// <see cref="XInputButtonSet"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate 
    /// through the buttons of the <see cref="XInputButtonSet"/>.</returns>
    public IEnumerator<XInputButton> GetEnumerator()
    {
        foreach (ButtonAssociation association in _buttonAssociations.Values)
        {
            yield return association.Button;
        }
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    /// <summary>
    /// Gets an enumerable that can be used to iterate through 
    /// all currently pressed buttons in the 
    /// <see cref="XInputButtonSet"/>.
    /// </summary>
    /// <returns>An enumerable that iterates through all 
    /// pressed buttons currently in the 
    /// <see cref="XInputButtonSet"/>.</returns>
    /// <seealso cref="IsPressed(XButtons)"/>
    public IEnumerable<XInputButton> GetPressedButtons()
    {
        foreach (XInputButton button in this)
        {
            if (button.IsPressed)
            {
                yield return button;
            }
        }
    }


    /// <summary>
    /// Determines if the specified button is currently being 
    /// pressed.
    /// </summary>
    /// <param name="button"><see cref="XButtons"/> constant 
    /// that specifies the button to check.</param>
    /// <returns><see langword="true"/> if <paramref name="button"/> 
    /// is currently being pressed; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is not a defined constant of an <see cref="XButtons"/> 
    /// or it is <see cref="XButtons.None"/>.</exception>
    /// <seealso cref="IsHolding(XButtons, TimeSpan)"/>
    public bool IsPressed(XButtons button)
    {
        if (button == XButtons.None || !Enum.IsDefined(button))
            throw new ArgumentException(
                $"'{button}' is not a valid value for '{nameof(button)}' " +
                $"parameter. A specific button constant is required.");

        return Buttons.HasFlag(button);
    }


    /// <summary>
    /// Determines if the specified button is currently being 
    /// held (pressed) for, at least, the specified duration.
    /// </summary>
    /// <param name="button"><see cref="XButtons"/> constant that 
    /// specifies the button to check.</param>
    /// <param name="duration">Minimum amount of time the button 
    /// must be held.</param>
    /// <returns><see langword="true"/> if <paramref name="button"/> 
    /// is being held for, at least, the amount of time specified 
    /// by <paramref name="duration"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="button"/> 
    /// is not a defined constant in an <see cref="XButtons"/> 
    /// enumeration or it is <see cref="XButtons.None"/>.</exception>
    /// <seealso cref="IsPressed(XButtons)"/>
    public bool IsHolding(XButtons button, TimeSpan duration)
    {
        if (IsPressed(button))
        {
            if (_buttonAssociations.TryGetValue(button,
                out ButtonAssociation? association))
            {
                return association.Button.IsHolding(duration);
            }
        }
        return false;
    }

    #endregion Methods


    #region Event handlers

    private void Button_IsPressedChanged(object? sender, EventArgs e)
    {
        if (sender is XInputButton button
            && _buttonAssociations.TryGetValue(button.Button, out ButtonAssociation? association))
        {
            OnButtonStateChanged(association.DigitalButtonEventArgs);
        }
    }

    #endregion Event handlers


}
