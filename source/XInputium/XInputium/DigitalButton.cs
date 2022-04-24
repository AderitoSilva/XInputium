using System;
using System.ComponentModel;

namespace XInputium;

/// <summary>
/// Implements an <see cref="EventDispatcherObject"/> that 
/// represents a button that has two states — pressed and 
/// released — and provides the means to measure elapsed 
/// time between those states.
/// </summary>
/// <seealso cref="EventDispatcherObject"/>
/// <seealso cref="Trigger"/>
/// <seealso cref="Joystick"/>
[Serializable]
public class DigitalButton : EventDispatcherObject
{


    #region Fields

    // Static PropertyChangedEventArgs to use for property value changes.
    private static readonly PropertyChangedEventArgs s_EA_IsPressed = new(nameof(IsPressed));
    private static readonly PropertyChangedEventArgs s_EA_Duration = new(nameof(Duration));

    // Property backing storage fields.
    private bool _isPressed = false;  // Store for the value of IsPressed property.
    private TimeSpan _duration = TimeSpan.Zero;  // Store for the value of Duration property.

    #endregion Fields


    #region Events

    /// <summary>
    /// It's invoked whenever the value of <see cref="IsPressed"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="IsPressed"/>
    /// <seealso cref="OnIsPressedChanged()"/>
    public event EventHandler? IsPressedChanged;


    /// <summary>
    /// It's invoked when the value of <see cref="IsPressed"/> 
    /// property was changed from <see langword="false"/> to 
    /// <see langword="true"/>, indicating the button has just 
    /// began being pressed.
    /// </summary>
    /// <seealso cref="Released"/>
    /// <seealso cref="IsPressedChanged"/>
    /// <seealso cref="IsPressed"/>
    public event EventHandler? Pressed;


    /// <summary>
    /// It's invoked when the value of <see cref="IsPressed"/> 
    /// property was changed from <see langword="true"/> to 
    /// <see langword="false"/>, indicating the button has 
    /// just been released.
    /// </summary>
    /// <seealso cref="Pressed"/>
    /// <seealso cref="IsPressedChanged"/>
    /// <seealso cref="IsPressed"/>
    public event EventHandler? Released;


    /// <summary>
    /// Its invoked whenever the value of <see cref="Duration"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="Duration"/>
    /// <seealso cref="OnDurationChanged()"/>
    public event EventHandler? DurationChanged;

    #endregion Events


    #region Constructors

    private DigitalButton()
        : base(EventDispatchMode.Immediate)
    {

    }


    /// <summary>
    /// Initializes a new instance of a <see cref="DigitalButton"/>
    /// class that supports state updating.
    /// </summary>
    /// <param name="updateCallback">A variable that will be set with 
    /// a <see cref="DigitalButtonUpdateCallback"/> delegate that you 
    /// can invoked from your code to update the state of the new 
    /// <see cref="DigitalButton"/> instance.</param>
    public DigitalButton(out DigitalButtonUpdateCallback updateCallback)
        : this()
    {
        updateCallback = new DigitalButtonUpdateCallback(UpdateState);
    }


    /// <summary>
    /// Initializes a new instance of a <see cref="DigitalButton"/> 
    /// class that has the specified immutable state.
    /// </summary>
    /// <param name="isPressed"><see langword="true"/> to indicate 
    /// the button is currently being pressed or <see langword="false"/> 
    /// to indicate the button is in the released state.</param>
    public DigitalButton(bool isPressed)
        : this()
    {
        UpdateState(isPressed, TimeSpan.Zero);
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets a <see cref="bool"/> that indicates of the button 
    /// represented by the <see cref="DigitalButton"/> is 
    /// currently being pressed.
    /// </summary>
    /// <seealso cref="Duration"/>
    /// <seealso cref="IsPressedChanged"/>
    /// <seealso cref="OnIsPressedChanged()"/>
    /// <seealso cref="IsHolding(TimeSpan)"/>
    public bool IsPressed
    {
        get => _isPressed;
        private set
        {
            if (SetProperty(ref _isPressed, in value, s_EA_IsPressed))
            {
                OnIsPressedChanged();
            }
        }
    }


    /// <summary>
    /// Gets a <see cref="TimeSpan"/> object representing the 
    /// amount of time elapsed since the last time the value 
    /// of <see cref="IsPressed"/> property was changed, allowing 
    /// for the button press or release duration to be obtained.
    /// </summary>
    /// <seealso cref="IsPressed"/>
    public TimeSpan Duration
    {
        get => _duration;
        private set
        {
            if (_duration < TimeSpan.Zero)
                value = TimeSpan.Zero;
            if (SetProperty(ref _duration, in value, s_EA_Duration))
            {
                OnDurationChanged();
            }
        }
    }

    #endregion Properties


    #region Methods

    /// <summary>
    /// Gets the <see cref="string"/> representation of the 
    /// current <see cref="DigitalButton"/> instance.
    /// </summary>
    /// <returns>The <see cref="string"/> representation 
    /// of the <see cref="DigitalButton"/> instance.</returns>
    public override string ToString()
    {
        return IsPressed ? "Pressed" : "Released";
    }


    /// <summary>
    /// Raises the <see cref="IsPressedChanged"/> event and, 
    /// depending on the value of <see cref="IsPressed"/> 
    /// property, raises either the 
    /// <see cref="Pressed"/> or <see cref="Released"/> 
    /// events.
    /// </summary>
    /// <seealso cref="IsPressedChanged"/>
    /// <seealso cref="IsPressed"/>
    /// <seealso cref="Pressed"/>
    /// <seealso cref="Released"/>
    protected virtual void OnIsPressedChanged()
    {
        IsPressedChanged?.Invoke(this, EventArgs.Empty);
        if (IsPressed)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Released?.Invoke(this, EventArgs.Empty);
        }
    }


    /// <summary>
    /// Raises the <see cref="DurationChanged"/> event.
    /// </summary>
    /// <seealso cref="DurationChanged"/>
    /// <seealso cref="Duration"/>
    protected virtual void OnDurationChanged()
    {
        DurationChanged?.Invoke(this, EventArgs.Empty);
    }


    private void UpdateState(bool isPressed, TimeSpan time)
    {
        // We're increasing the duration first, so the old button state
        // can have its duration completely accounted for and the
        // DurationChanged event is raised before the state is changed.
        // We're doing it this way because this method's time parameter
        // is telling us the time since the past, so it counts for the
        // past state, which the current state will be after this method
        // returns.
        if (time > TimeSpan.Zero)
        {
            Duration += time;
        }

        // Update the state of the button.
        bool wasPressed = IsPressed;
        IsPressed = isPressed;
        if (isPressed != wasPressed)
        {
            // The button state just changed. Let's start counting time from 0 again.
            Duration = TimeSpan.Zero;
        }
    }


    /// <summary>
    /// Determines if the button is being held (pressed) for, 
    /// at least, the specified duration.
    /// </summary>
    /// <param name="duration">Minimum amount of time the button 
    /// needs to be held.</param>
    /// <returns><see langword="true"/> if the button is being 
    /// held for a duration equal to or greater than 
    /// <paramref name="duration"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="IsPressed"/>
    public bool IsHolding(TimeSpan duration)
    {
        return IsPressed && Duration >= duration;
    }

    #endregion Methods


}
