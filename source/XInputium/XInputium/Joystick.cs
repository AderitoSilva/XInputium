using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XInputium.Internal.Statistics;

namespace XInputium;

/// <summary>
/// Represents a controller joystick, with support for 
/// configuration of how its position is effectively 
/// represented.
/// </summary>
/// <remarks>
/// For a lightweight alternative to the <see cref="Joystick"/> 
/// class, that has only the essential functionality, you 
/// can use <see cref="SlimJoystick"/> structure.
/// </remarks>
/// <seealso cref="SlimJoystick"/>
/// <seealso cref="Trigger"/>
[DebuggerDisplay($"{nameof(X)} = {{{nameof(X)}}}, " +
    $"{nameof(Y)} = {{{nameof(Y)}}}")]
[Serializable]
public class Joystick : EventDispatcherObject
{


    #region Internal types

    [Serializable]
    private readonly struct JoystickSample
    {

        public static readonly JoystickSample Zero = new(0f, 0f, TimeSpan.Zero);

        public readonly float X;
        public readonly float Y;
        public readonly TimeSpan Time;

        public JoystickSample(float x, float y, TimeSpan time)
        {
            X = x;
            Y = y;
            Time = time;
        }

        public static JoystickSample operator +(JoystickSample x, JoystickSample y)
        {
            return new JoystickSample(x.X + y.X, x.Y + y.Y, x.Time + y.Time);
        }

        public static JoystickSample operator -(JoystickSample x, JoystickSample y)
        {
            return new JoystickSample(x.X - y.X, x.Y - y.Y, x.Time - y.Time);
        }

    }


    private sealed class JoystickSamples : SampledAverage<JoystickSample>
    {

        public JoystickSamples(int maxSampleLength)
            : base(maxSampleLength)
        {

        }


        protected override JoystickSample Zero { get; } = JoystickSample.Zero;


        protected override JoystickSample SumValue(JoystickSample x, JoystickSample y)
        {
            return x + y;
        }


        protected override JoystickSample SubtractValue(JoystickSample x, JoystickSample y)
        {
            return x - y;
        }


        protected override JoystickSample GetAverage(int samplesCount, JoystickSample totalSum)
        {
            if (samplesCount > 0)
            {
                JoystickSample average = new(totalSum.X / samplesCount,
                    totalSum.Y / samplesCount, totalSum.Time / samplesCount);
                return average;
            }
            else
            {
                return Zero;
            }
        }


    }

    #endregion Internal types


    #region Fields and constants

    // Constants used to provide some defaults.
    private const int MinSmoothingSamples = 2;  // Minimum number of smoothing samples to register, when using smoothing.
    private const int MaxSmoothingSamples = 500;  // Maximum number of smoothing samples to register, when using smoothing.
    private const int DefaultSmoothingSamples = 120;  // Initial number of samples to register, before adjustments.

    // Static PropertyChangedEventArgs for property changes. This is to avoid instantiating
    // a PropertyChangedEventArgs class whenever the value of a property changes.
    private static readonly PropertyChangedEventArgs s_EA_RawX = new(nameof(RawX));
    private static readonly PropertyChangedEventArgs s_EA_RawY = new(nameof(RawY));
    private static readonly PropertyChangedEventArgs s_EA_RawRadius = new(nameof(RawRadius));
    private static readonly PropertyChangedEventArgs s_EA_RawAngle = new(nameof(RawAngle));
    private static readonly PropertyChangedEventArgs s_EA_IsRawWithinCircle = new(nameof(IsRawWithinCircle));
    private static readonly PropertyChangedEventArgs s_EA_X = new(nameof(X));
    private static readonly PropertyChangedEventArgs s_EA_Y = new(nameof(Y));
    private static readonly PropertyChangedEventArgs s_EA_Angle = new(nameof(Angle));
    private static readonly PropertyChangedEventArgs s_EA_Radius = new(nameof(Radius));
    private static readonly PropertyChangedEventArgs s_EA_Delta = new(nameof(Delta));
    private static readonly PropertyChangedEventArgs s_EA_Direction = new(nameof(Direction));
    private static readonly PropertyChangedEventArgs s_EA_MovementSpeed = new(nameof(MovementSpeed));
    private static readonly PropertyChangedEventArgs s_EA_IsMoving = new(nameof(IsMoving));
    private static readonly PropertyChangedEventArgs s_EA_IsPushed = new(nameof(IsPushed));
    private static readonly PropertyChangedEventArgs s_EA_FrameTime = new(nameof(FrameTime));

    // Property backing storage fields.
    private float _rawX = 0f;  // Store for the value of RawX property.
    private float _rawY = 0f;  // Store for the value of RawY property.
    private float _rawRadius = 0f;  // Store for the value of RawRadius property.
    private float _rawAngle = 0f;  // Store for the value of RawAngle property.
    private bool _isRawWithinCircle = true;  // Store for the value of IsRawWithinBounds property.
    private float _x = 0f;  // Store for the value of X property.
    private float _y = 0f;  // Store for the value of Y property.
    private float _angle = 0f;  // Store for the value of Angle property.
    private float _radius = 0f;  // Store for the value of Radius property.
    private JoystickDelta _delta = JoystickDelta.Zero;  // Store for the value of Delta property.
    private JoystickDirection _direction = JoystickDirection.None;  // Store for the value of Direction property.
    private float _movementSpeed = 0f;  // Store for the value of MovementSpeed property.
    private bool _isMoving = false;  // Store for the value of IsMoving property.
    private bool _isPushed = false;  // Store for the value of IsPushed property.
    private TimeSpan _frameTime = TimeSpan.Zero;  // Store for the value of FameTime property.
    private bool _invertX = false;  // Store for the value of InvertX property.
    private bool _invertY = false;  // Store for the value of InvertX property.
    private float _innerDeadZone = 0f;  // Store for the value of InnerDeadZone property.
    private float _outerDeadZone = 0f;  // Store for the value of OuterDeadZone property.
    private ModifierFunction? _xModifierFunction = null;  // Store for the value of XModifierFunction property.
    private ModifierFunction? _yModifierFunction = null;  // Store for the value of YModifierFunction property.
    private ModifierFunction? _radiusModifierFunction = null;  // Store for the value of RadiusModifierFunction property.
    private ModifierFunction? _angleModifierFunction = null;  // Store for the value of AngleModifierFunction property.
    private TimeSpan _smoothingSamplePeriod = TimeSpan.Zero;  // Store for the value of SmoothingSamplePeriod property.
    private float _smoothingFactor = 0f;  // Store for the value of SmoothingFactor property.

    // State keeping fields.
    private bool _isValid = false;  // Stores the value that specifies if the effective joystick position is up to date.
    private bool _isValidating = false;  // Stores the value that indicates if a validation operation is currently in progress.
    private readonly JoystickSamples _smoothingSamples = new(DefaultSmoothingSamples);  // Stores the joystick samples, used for smoothing.

    #endregion Fields and constants


    #region Constructors

    /// <summary>
    /// Initializes a default instance of a <see cref="Joystick"/> 
    /// class.
    /// </summary>
    private Joystick()
        : base(EventDispatchMode.Deferred)
    {

    }


    /// <summary>
    /// Initializes a new instance of a <see cref="Joystick"/> 
    /// class, that supports state updating from external code.
    /// </summary>
    /// <param name="updateCallback">Variable that will be set 
    /// with a <see cref="JoystickUpdateCallback"/> delegate 
    /// that can be invoked to update the state of the 
    /// <see cref="Joystick"/> instance.</param>
    public Joystick(out JoystickUpdateCallback updateCallback)
        : this()
    {
        updateCallback = new JoystickUpdateCallback(UpdateRawPosition);
    }


    /// <summary>
    /// Initializes a new instance of a <see cref="Joystick"/> 
    /// class, that has a read-only raw position.
    /// </summary>
    /// <param name="x">A value between -1 and 1 representing the 
    /// horizontal axis, where -1 is the left and 1 is the right 
    /// position.</param>
    /// <param name="y">A value between -1 and 1 representing the 
    /// vertical axis, where -1 is the bottom and 1 is the top 
    /// position.</param>
    /// <exception cref="ArgumentException"><paramref name="x"/> 
    /// is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="y"/> 
    /// is <see cref="float.NaN"/>.</exception>
    public Joystick(float x, float y)
        : this()
    {
        if (float.IsNaN(x))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(x)}' parameter.",
                nameof(x));
        if (float.IsNaN(y))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(y)}' parameter.",
                nameof(y));

        UpdateRawPosition(x, y, TimeSpan.Zero);
    }


    /// <summary>
    /// Creates a new instance of a <see cref="Joystick"/> 
    /// class that has read-only raw coordinates obtained from 
    /// the coordinates of the specified <see cref="SlimJoystick"/> 
    /// object.
    /// </summary>
    /// <param name="joystick"><see cref="SlimJoystick"/> object 
    /// to obtain the raw coordinates from.</param>
    /// <seealso cref="SlimJoystick"/>
    public Joystick(SlimJoystick joystick)
        : this(joystick.X, joystick.Y)
    {

    }

    #endregion Constructors


    #region Events

    /// <summary>
    /// It's invoked whenever the state of the <see cref="Joystick"/>
    /// is updated from outside code.
    /// </summary>
    public event EventHandler? Updated;


    /// <summary>
    /// It's invoked whenever the value of <see cref="X"/> or 
    /// <see cref="Y"/> properties changes.
    /// </summary>
    /// <seealso cref="X"/>
    /// <seealso cref="Y"/>
    public event EventHandler? PositionChanged;


    /// <summary>
    /// It's invoked whenever the value of <see cref="Angle"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="Angle"/>
    public event EventHandler? AngleChanged;


    /// <summary>
    /// It's invoked whenever the value of <see cref="Radius"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="Radius"/>
    public event EventHandler? RadiusChanged;


    /// <summary>
    /// It's invoked whenever the value of <see cref="Direction"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="Direction"/>
    public event EventHandler? DirectionChanged;


    /// <summary>
    /// It's invoked whenever the value of <see cref="IsMoving"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="IsMoving"/>
    /// <seealso cref="OnIsMovingChanged()"/>
    public event EventHandler? IsMovingChanged;


    /// <summary>
    /// It's invoked whenever the value of <see cref="IsPushed"/> 
    /// property changes.
    /// </summary>
    /// <seealso cref="IsPushed"/>
    public event EventHandler? IsPushedChanged;

    #endregion Events


    #region Properties

    /// <summary>
    /// Gets the raw position of the joystick's X axis.
    /// </summary>
    /// <returns>A value between -1 and 1, where -1 represents the 
    /// left and 1 represents the right raw position of 
    /// the joystick.</returns>
    /// <remarks>
    /// <see cref="RawX"/> returns the raw position of the 
    /// joystick's horizontal axis as provided by the underlying 
    /// device. To get the effective position of the X axis, 
    /// use <see cref="X"/> property, which considers all of 
    /// the <see cref="Joystick"/>'s modifiers.
    /// </remarks>
    /// <seealso cref="RawY"/>
    /// <seealso cref="X"/>
    public float RawX
    {
        get => _rawX;
        private set => Invalidate(SetProperty(
            ref _rawX, InputMath.Clamp11(value), s_EA_RawX));
    }


    /// <summary>
    /// Gets the raw position of the joystick's Y axis.
    /// </summary>
    /// <returns>A value between -1 and 1, where -1 represents the 
    /// bottom and 1 represents the top raw position of 
    /// the joystick.</returns>
    /// <remarks>
    /// <see cref="RawY"/> returns the raw position of the 
    /// joystick's vertical axis as provided by the underlying 
    /// device. To get the effective position of the Y axis, 
    /// use <see cref="Y"/> property, which considers all of 
    /// the <see cref="Joystick"/>'s modifiers.
    /// </remarks>
    /// <seealso cref="RawX"/>
    /// <seealso cref="Y"/>
    public float RawY
    {
        get => _rawY;
        private set => Invalidate(SetProperty(
            ref _rawY, InputMath.Clamp11(value), s_EA_RawY));
    }


    /// <summary>
    /// Gets the raw radius of the joystick, which is 
    /// analogous to its distance from the center raw 
    /// position.
    /// </summary>
    /// <returns>A value between 0 and 1, where 0 is the center 
    /// of the joystick and 1 is its outer edge.</returns>
    /// <seealso cref="Radius"/>
    /// <seealso cref="Angle"/>
    /// <seealso cref="RawX"/>
    /// <seealso cref="RawY"/>
    public float RawRadius
    {
        get => _rawRadius;
        private set => Invalidate(SetProperty(
            ref _rawRadius, InputMath.Clamp01(value), s_EA_RawRadius));
    }


    /// <summary>
    /// Gets the normalized raw angle of the joystick.
    /// </summary>
    /// <returns>A value between 0 and 1, representing the 
    /// joystick's normalized raw angle.</returns>
    /// <remarks>
    /// In the context of the <see cref="Joystick"/> class, 
    /// a normalized angle is a value between 0 and 1 that 
    /// represents the angle. The normalized angle is 
    /// analogous to a quartz clock, where 0 is analogous 
    /// to 12 o´clock, and increases in clockwise orientation 
    /// until 1 is analogous to 12 o'clock again.
    /// <br/>
    /// The normalized angle can be useful when converting 
    /// it to other units, like degrees.
    /// <br/>
    /// You can convert between normalized angles and radians 
    /// using <see cref="InputMath.ConvertNormalToRadians(float)"/> 
    /// and <see cref="InputMath.ConvertRadiansToNormal(float)"/> 
    /// methods.
    /// </remarks>
    /// <seealso cref="RawRadius"/>
    /// <seealso cref="Angle"/>
    public float RawAngle
    {
        get => _rawAngle;
        private set => Invalidate(SetProperty(
            ref _rawAngle, InputMath.Clamp01(value), s_EA_RawAngle));
    }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates whether the 
    /// raw position of the joystick is within the full circular 
    /// area of the joystick.
    /// </summary>
    /// <returns><see langword="true"/> when the raw position 
    /// of the joystick would result in a radius lower than or 
    /// equal to 1; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This property will return <see langword="false"/> when 
    /// the raw position of the joystick, as reported by 
    /// <see cref="RawX"/> and <see cref="RawY"/> coordinates, 
    /// would result in a radius that is greater than 1. 
    /// Usually, when this property returns <see langword="false"/>, 
    /// it means the raw position would not be valid for a 
    /// circular joystick, meaning a device reporting such position 
    /// may be damaged or defective. Not that devices may not 
    /// be 100% accurate in their axis measurement, making it 
    /// possible for this property to return <see langword="false"/> 
    /// even on properly working devices.
    /// </remarks>
    public bool IsRawWithinCircle
    {
        get => _isRawWithinCircle;
        private set => Invalidate(SetProperty(
            ref _isRawWithinCircle, value, s_EA_IsRawWithinCircle));
    }


    /// <summary>
    /// Gets the effective value of the joystick's X axis.
    /// </summary>
    /// <returns>A value between -1 and 1, where -1 represents the 
    /// left and 1 represents the right effective position of 
    /// the joystick.</returns>
    /// <remarks>
    /// <see cref="X"/> property returns the effective position 
    /// of the joystick's horizontal axis. In the context of the 
    /// <see cref="Joystick"/> class, the effective position of 
    /// an axis is the position of the axis with all the modifiers 
    /// applied to the joystick. This contrasts with the raw 
    /// position, which represents the non-processed position of 
    /// the joystick as provided by the underlying device. You 
    /// can use <see cref="RawX"/> to get the raw position of the 
    /// X axis.
    /// </remarks>
    /// <seealso cref="Y"/>
    /// <seealso cref="RawX"/>
    public float X
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _x;
        }
        private set
        {
            if (SetProperty(ref _x, InputMath.Clamp11(value), s_EA_X))
            {
                OnPositionChanged();
            }
        }
    }


    /// <summary>
    /// Gets the effective value of the joystick's Y axis.
    /// </summary>
    /// <returns>A value between -1 and 1, where -1 represents the 
    /// bottom and 1 represents the top effective position of 
    /// the joystick.</returns>
    /// <remarks>
    /// <see cref="Y"/> property returns the effective position 
    /// of the joystick's vertical axis. In the context of the 
    /// <see cref="Joystick"/> class, the effective position of 
    /// an axis is the position of the axis with all the modifiers 
    /// applied to the joystick. This contrasts with the raw 
    /// position, which represents the non-processed position of 
    /// the joystick as provided by the underlying device. You 
    /// can use <see cref="RawY"/> to get the raw position of the 
    /// Y axis.
    /// </remarks>
    /// <seealso cref="X"/>
    /// <seealso cref="RawY"/>
    public float Y
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _y;
        }
        private set
        {
            if (SetProperty(ref _y, InputMath.Clamp11(value), s_EA_Y))
            {
                OnPositionChanged();
            }
        }
    }


    /// <summary>
    /// Gets the effective normalized angle of the joystick.
    /// </summary>
    /// <returns>A value between 0 and 1, representing the 
    /// joystick's effective normalized angle.</returns>
    /// <remarks>
    /// In the context of the <see cref="Joystick"/> class, 
    /// a normalized angle is a value between 0 and 1 that 
    /// represents the angle. The normalized angle is 
    /// analogous to a quartz clock, where 0 is analogous 
    /// to 12 o´clock, and increases in clockwise orientation 
    /// until 1 is analogous to 12 o'clock again.
    /// <br/>
    /// The normalized angle can be useful when converting 
    /// it to other units, like degrees.
    /// <br/>
    /// You can convert between normalized angles and radians 
    /// using <see cref="InputMath.ConvertNormalToRadians(float)"/> 
    /// and <see cref="InputMath.ConvertRadiansToNormal(float)"/> 
    /// methods.
    /// </remarks>
    /// <seealso cref="Radius"/>
    public float Angle
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _angle;
        }
        private set
        {
            if (SetProperty(ref _angle, InputMath.Clamp01(value), s_EA_Angle))
            {
                OnAngleChanged();
            }
        }
    }


    /// <summary>
    /// Gets the effective radius of the joystick, which is 
    /// analogous to its distance from the center effective 
    /// position.
    /// </summary>
    /// <returns>A value between 0 and 1, where 0 is the center 
    /// of the joystick and 1 is its outer edge.</returns>
    /// <seealso cref="RawRadius"/>
    /// <seealso cref="Angle"/>
    public float Radius
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _radius;
        }
        private set
        {
            if (SetProperty(ref _radius, InputMath.Clamp01(value), s_EA_Radius))
            {
                OnRadiusChanged();
            }
        }
    }


    /// <summary>
    /// Gets a <see cref="JoystickDelta"/> object that represents 
    /// the difference between the current and the previous joystick 
    /// effective position.
    /// </summary>
    /// <returns>A <see cref="JoystickDelta"/> object representing 
    /// the movement delta of the joystick.</returns>
    /// <remarks>
    /// This property can be useful to determine if the joystick has 
    /// moved and by how much it has moved. The returned 
    /// <see cref="JoystickDelta"/> object contains the relative 
    /// coordinates of the current joystick effective position, relative 
    /// to the effective position it had before the most recent update.
    /// </remarks>
    /// <seealso cref="IsPushed"/>
    public JoystickDelta Delta
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _delta;
        }
        private set
        {
            SetProperty(ref _delta, value, s_EA_Delta);
        }
    }


    /// <summary>
    /// Gets a <see cref="XInputium.JoystickDirection"/> constant 
    /// that represents the current effective direction of 
    /// the joystick.
    /// </summary>
    /// <returns>An <see cref="XInputium.JoystickDirection"/> constant 
    /// representing the direction of the joystick's angle. 
    /// If the joystick radius is 0, <see cref="JoystickDirection.None"/> 
    /// is returned.</returns>
    /// <seealso cref="XInputium.JoystickDirection"/>
    /// <seealso cref="Angle"/>
    /// <seealso cref="Radius"/>
    public JoystickDirection Direction
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _direction;
        }
        private set
        {
            if (SetProperty(ref _direction, value, s_EA_Direction))
            {
                OnDirectionChanged();
            }
        }
    }


    /// <summary>
    /// Gets the estimated distance per second the joystick is being moved by,
    /// by considering its current a previous effective position.
    /// </summary>
    /// <returns>A number equal to or greater than 0, representing the estimated 
    /// distance the joystick is moving per second. If <see cref="FrameTime"/> 
    /// is <see cref="TimeSpan.Zero"/> and <see cref="Delta"/> reports a 
    /// movement distance greater than 0, <see cref="float.PositiveInfinity"/> 
    /// is returned.</returns>
    /// <remarks>
    /// The number returned by this property represents the total distance the 
    /// joystick would travel within a second, if it kept moving at its current 
    /// speed. Its current speed is the joystick's delta distance (see 
    /// <see cref="Delta"/> property), divided by the number of seconds elapsed 
    /// between the two most recent update operations. Although the joystick 
    /// could not keep moving indeterminately because it is constrained to its 
    /// -1 to 1 boundaries, this property assumes as if it could.
    /// <br/><br/>
    /// When the time elapsed between the two most recent update operations is 
    /// zero (<see cref="TimeSpan.Zero"/>) while the delta distance is greater 
    /// than 0, this property returns <see cref="float.PositiveInfinity"/> to 
    /// indicate the joystick is moving at infinite speed and represent an 
    /// immediate movement.
    /// </remarks>
    /// <seealso cref="Delta"/>
    /// <seealso cref="FrameTime"/>
    /// <seealso cref="IsPushed"/>
    public float MovementSpeed
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _movementSpeed;
        }
        private set
        {
            SetProperty(ref _movementSpeed, MathF.Max(value, 0f), s_EA_MovementSpeed);
        }
    }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the joystick is 
    /// currently being moved, considering the two most recent 
    /// update operations.
    /// </summary>
    /// <returns><see langword="true"/> if the joystick is being moved;
    /// otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="Delta"/>
    /// <seealso cref="MovementSpeed"/>
    /// <seealso cref="IsPushed"/>
    public bool IsMoving
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _isMoving;
        }
        private set
        {
            if (SetProperty(ref _isMoving, value, s_EA_IsMoving))
            {
                OnIsMovingChanged();
            }
        }
    }


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates the joystick's 
    /// effective position is currently not at its center position, 
    /// meaning it is being pushed by the user.
    /// </summary>
    /// <returns><see langword="true"/> if <see cref="Radius"/> is 
    /// greater than 0; otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="Radius"/>
    public bool IsPushed
    {
        get
        {
            if (!_isValid)
            {
                Validate();
            }
            return _isPushed;
        }
        private set
        {
            if (SetProperty(ref _isPushed, value, s_EA_IsPushed))
            {
                OnIsPushedChanged();
            }
        }
    }


    /// <summary>
    /// Gets the amount of time elapsed since the last time the 
    /// <see cref="Joystick"/> raw position was updated.
    /// </summary>
    public TimeSpan FrameTime
    {
        get => _frameTime;
        private set => SetProperty(ref _frameTime, value, s_EA_FrameTime);
    }


    /// <summary>
    /// Gets or sets a <see cref="bool"/> that specifies if the 
    /// value of the X axis must be inverted.
    /// </summary>
    /// <value><see langword="true"/> to invert the value of 
    /// the X axis or <see langword="false"/> to not change it. 
    /// The default value is <see langword="false"/>.</value>
    /// <remarks>
    /// When this property is set to <see langword="true"/>, the 
    /// value of the X axis will be inverted, meaning a value that 
    /// would be -1 will be 1, and a value that would be 1 will 
    /// be -1.
    /// </remarks>
    /// <seealso cref="InvertY"/>
    /// <seealso cref="X"/>
    public bool InvertX
    {
        get => _invertX;
        set => Invalidate(SetProperty(ref _invertX, value));
    }


    /// <summary>
    /// Gets or sets a <see cref="bool"/> that specifies if the 
    /// value of the Y axis must be inverted.
    /// </summary>
    /// <value><see langword="true"/> to invert the value of 
    /// the Y axis or <see langword="false"/> to not change it. 
    /// The default value is <see langword="false"/>.</value>
    /// <remarks>
    /// When this property is set to <see langword="true"/>, the 
    /// value of the T axis will be inverted, meaning a value that 
    /// would be -1 will be 1, and a value that would be 1 will 
    /// be -1.
    /// </remarks>
    /// <seealso cref="InvertX"/>
    /// <seealso cref="Y"/>
    public bool InvertY
    {
        get => _invertY;
        set => Invalidate(SetProperty(ref _invertY, value));
    }


    /// <summary>
    /// Gets or sets the inner circular dead-zone of the joystick.
    /// </summary>
    /// <value>A value between 0 and 1, representing the portion 
    /// of the joystick's inner area that will be effectively 
    /// ignored. The default value is 0.</value>
    /// <exception cref="ArgumentException">The value being set 
    /// to the property is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="OuterDeadZone"/>
    public float InnerDeadZone
    {
        get => _innerDeadZone;
        set
        {
            if (float.IsNaN(value))
                throw new ArgumentException(
                    $"'{float.NaN}' is not a valid value for " +
                    $"'{nameof(InnerDeadZone)}' property.",
                    nameof(value));

            Invalidate(SetProperty(ref _innerDeadZone, value));
        }
    }


    /// <summary>
    /// Gets or sets the outer circular dead-zone of the joystick.
    /// </summary>
    /// <value>A value between 0 and 1, representing the portion 
    /// of the joystick's outer area that will be effectively 
    /// ignored. The default value is 0.</value>
    /// <exception cref="ArgumentException">The value being set 
    /// to the property is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="InnerDeadZone"/>
    public float OuterDeadZone
    {
        get => _outerDeadZone;
        set
        {
            if (float.IsNaN(value))
                throw new ArgumentException(
                    $"'{float.NaN}' is not a valid value for " +
                    $"'{nameof(OuterDeadZone)}' property.",
                    nameof(value));

            Invalidate(SetProperty(ref _outerDeadZone, value));
        }
    }


    /// <summary>
    /// Gets or sets the <see cref="ModifierFunction"/> delegate 
    /// that is used to modify the X axis.
    /// </summary>
    /// <value>A <see cref="ModifierFunction"/> delegate or 
    /// <see langword="null"/> to specify no modifier function. 
    /// The default value is <see langword="null"/>.</value>
    /// <seealso cref="X"/>
    public ModifierFunction? XModifierFunction
    {
        get => _xModifierFunction;
        set => Invalidate(SetProperty(ref _xModifierFunction, value));
    }


    /// <summary>
    /// Gets or sets the <see cref="ModifierFunction"/> delegate 
    /// that is used to modify the Y axis.
    /// </summary>
    /// <value>A <see cref="ModifierFunction"/> delegate or 
    /// <see langword="null"/> to specify no modifier function. 
    /// The default value is <see langword="null"/>.</value>
    /// <seealso cref="Y"/>
    public ModifierFunction? YModifierFunction
    {
        get => _yModifierFunction;
        set => Invalidate(SetProperty(ref _yModifierFunction, value));
    }


    /// <summary>
    /// Gets or sets the <see cref="ModifierFunction"/> delegate 
    /// that is used to modify the effective radius of the 
    /// joystick.
    /// </summary>
    /// <value>A <see cref="ModifierFunction"/> delegate or 
    /// <see langword="null"/> to specify no modifier function. 
    /// The default value is <see langword="null"/>.</value>
    /// <seealso cref="Radius"/>
    public ModifierFunction? RadiusModifierFunction
    {
        get => _radiusModifierFunction;
        set => Invalidate(SetProperty(ref _radiusModifierFunction, value));
    }


    /// <summary>
    /// Gets or sets the <see cref="ModifierFunction"/> delegate 
    /// that is used to modify the effective angle of the 
    /// joystick.
    /// </summary>
    /// <value>A <see cref="ModifierFunction"/> delegate or 
    /// <see langword="null"/> to specify no modifier function. 
    /// The default value is <see langword="null"/>.</value>
    /// <seealso cref="Angle"/>
    public ModifierFunction? AngleModifierFunction
    {
        get => _angleModifierFunction;
        set => Invalidate(SetProperty(ref _angleModifierFunction, value));
    }


    /// <summary>
    /// Gets or sets the maximum amount of time a smoothing 
    /// sample is kept in memory for joystick smoothing 
    /// calculation.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/> object representing 
    /// positive time, where <see cref="TimeSpan.Zero"/> 
    /// disables smoothing. The default value is 
    /// <see cref="TimeSpan.Zero"/>.</value>
    /// <remarks>
    /// This property specifies how long the most recent 
    /// joystick position samples are kept in memory for 
    /// use in joystick smoothing calculation. The longer 
    /// the period, the more smoothing is applied and, 
    /// consequently, the more the fastest joystick movements 
    /// are slowed down. A <see cref="TimeSpan.Zero"/> means 
    /// no time frame will be considered, meaning smoothing 
    /// doesn't occur. Usually, for smoothing of fast and
    /// unstable movements, just a few milliseconds would 
    /// be the ideal value for this property.
    /// <br/><br/>
    /// To specify how much of the calculated smoothing is 
    /// effectively applied to the joystick, use 
    /// <see cref="SmoothingFactor"/> property.
    /// <br/><br/>
    /// Joystick smoothing is not applicable to all use cases. 
    /// Some use cases might include a game or application 
    /// that requires the user to move an object in screen 
    /// with high precision or to minimize accidental 
    /// steering in a car driving game.
    /// </remarks>
    /// <seealso cref="SmoothingFactor"/>
    public TimeSpan SmoothingSamplePeriod
    {
        get => _smoothingSamplePeriod;
        set
        {
            if (value < TimeSpan.Zero)
                value = TimeSpan.Zero;

            if (SetProperty(ref _smoothingSamplePeriod, value))
            {
                if (value == TimeSpan.Zero)
                {
                    _smoothingSamples.Clear();
                }
                Invalidate();
            }
        }
    }


    /// <summary>
    /// Gets or sets how much of the computed smoothing is 
    /// applied to the joystick.
    /// </summary>
    /// <value>A number between 0 and 1, where 0 means no 
    /// smoothing is applied and 1 means full smoothing. 
    /// The default value is 0.</value>
    /// <exception cref="ArgumentException">The value being 
    /// set to the property is <see cref="float.NaN"/>.</exception>
    /// <remarks>
    /// This property specifies how much smoothing is applied. 
    /// While <see cref="SmoothingSamplePeriod"/> specifies the 
    /// amount of time newer registered samples are kept in 
    /// memory for the smoothing algorithm to determine the 
    /// smoothed coordinates calculation, 
    /// <see cref="SmoothingFactor"/> specifies how much of 
    /// that smoothed coordinates are used for the effective 
    /// position.
    /// </remarks>
    /// <seealso cref="SmoothingSamplePeriod"/>
    public float SmoothingFactor
    {
        get => _smoothingFactor;
        set
        {
            if (float.IsNaN(value))
                throw new ArgumentException(
                    $"'{float.NaN}' is not a valid value for " +
                    $"'{nameof(SmoothingFactor)}' property.",
                    nameof(value));

            Invalidate(SetProperty(ref _smoothingFactor, InputMath.Clamp01(value)));
        }
    }

    #endregion Properties


    #region Methods

    /// <summary>
    /// Gets a new <see cref="Joystick"/> instance that encapsulates 
    /// the specified <see cref="Joystick"/> instance, and that has 
    /// its raw joystick position automatically updated whenever the 
    /// encapsulated instance is updated.
    /// </summary>
    /// <param name="joystick"><see cref="Joystick"/> instance to 
    /// encapsulate.</param>
    /// <returns>The newly created <see cref="Joystick"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="joystick"/>
    /// is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method allows you to create a new <see cref="Joystick"/> instance, 
    /// but use another <see cref="Joystick"/> instance as the source of 
    /// its raw state information. This is useful in scenarios where you need 
    /// to have different <see cref="Joystick"/> instances that represent the 
    /// same physical joystick, but need to have different settings depending 
    /// on your application's state. The returned <see cref="Joystick"/> 
    /// instance is automatically updated whenever the underlying 
    /// <paramref name="joystick"/> is updated. Only the underlying joystick's 
    /// raw position and time information is obtained; the effective position 
    /// of the returned <see cref="Joystick"/> is determined depending on the 
    /// obtained raw position and the settings you specify on the new 
    /// <see cref="Joystick"/>.
    /// </remarks>
    public static Joystick Encapsulate(Joystick joystick)
    {
        if (joystick is null)
            throw new ArgumentNullException(nameof(joystick));

        Joystick instance = new(out JoystickUpdateCallback updateCallback);
        joystick.Updated += (sender, e) =>
        {
            if (sender is Joystick j)
            {
                updateCallback(j.RawX, j.RawY, j.FrameTime);
            }
        };
        updateCallback(joystick.RawX, joystick.RawY, joystick.FrameTime);
        return instance;
    }


    /// <summary>
    /// Converts the specified normalized angle to a 
    /// <see cref="JoystickDirection"/> constant that represents 
    /// the direction of the angle.
    /// </summary>
    /// <param name="normalAngle">A value between 0 and 1, that 
    /// specifies the normalized angle to convert.</param>
    /// <returns>A <see cref="JoystickDirection"/> constant 
    /// that specifies the direction of the angle.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="normalAngle"/> is <see cref="float.NaN"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JoystickDirection ConvertNormalAngleToJoystickDirection(
        float normalAngle)
    {
        if (float.IsNaN(normalAngle))
            throw new ArgumentException($"'{float.NaN}' is not a valid value " +
                $"for '{nameof(normalAngle)}' parameter.",
                nameof(normalAngle));

        normalAngle = MathF.Abs(normalAngle);
        normalAngle -= MathF.Floor(normalAngle);

        if (normalAngle >= 0.875f || normalAngle < 0.125f)
            return JoystickDirection.Up;
        else if (normalAngle >= 0.125f && normalAngle < 0.375f)
            return JoystickDirection.Right;
        else if (normalAngle >= 0.375f && normalAngle < 0.625f)
            return JoystickDirection.Down;
        else if (normalAngle >= 0.625f && normalAngle < 0.875f)
            return JoystickDirection.Left;
        else
            return JoystickDirection.None;
    }


    /// <summary>
    /// Gets the <see cref="string"/> representation of the 
    /// current <see cref="Joystick"/> instance.
    /// </summary>
    /// <returns>The <see cref="string"/> representation of 
    /// the <see cref="Joystick"/></returns>
    public override string ToString()
    {
        return $"{X}, {Y}";
    }


    /// <summary>
    /// It's called once on every update and raises the 
    /// <see cref="Updated"/> event.
    /// </summary>
    /// <seealso cref="Updated"/>
    protected virtual void OnUpdated()
    {
        RaiseEvent(() => Updated?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="PositionChanged"/> event.
    /// </summary>
    /// <seealso cref="PositionChanged"/>
    /// <seealso cref="X"/>
    /// <seealso cref="Y"/>
    protected virtual void OnPositionChanged()
    {
        RaiseEvent(() => PositionChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="AngleChanged"/> event.
    /// </summary>
    /// <seealso cref="AngleChanged"/>
    /// <seealso cref="Angle"/>
    protected virtual void OnAngleChanged()
    {
        RaiseEvent(() => AngleChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="RadiusChanged"/> event.
    /// </summary>
    /// <seealso cref="RadiusChanged"/>
    /// <seealso cref="Radius"/>
    protected virtual void OnRadiusChanged()
    {
        RaiseEvent(() => RadiusChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="DirectionChanged"/> event.
    /// </summary>
    /// <seealso cref="DirectionChanged"/>
    /// <seealso cref="Direction"/>
    protected virtual void OnDirectionChanged()
    {
        RaiseEvent(() => DirectionChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="IsMovingChanged"/> event.
    /// </summary>
    /// <seealso cref="IsMovingChanged"/>
    /// <seealso cref="IsMoving"/>
    protected virtual void OnIsMovingChanged()
    {
        RaiseEvent(() => IsMovingChanged?.Invoke(this, EventArgs.Empty));
    }


    /// <summary>
    /// Raises the <see cref="IsPushedChanged"/> event.
    /// </summary>
    /// <seealso cref="IsPushedChanged"/>
    /// <seealso cref="IsPushed"/>
    protected virtual void OnIsPushedChanged()
    {
        RaiseEvent(() => IsPushedChanged?.Invoke(this, EventArgs.Empty));
    }


    private void UpdateRawPosition(float x, float y, TimeSpan time)
    {
        // Validate parameters.
        if (float.IsNaN(x))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(x)}' parameter.",
                nameof(x));
        if (float.IsNaN(y))
            throw new ArgumentException(
                $"'{float.NaN}' is not a valid value for '{nameof(y)}' parameter.",
                nameof(y));
        if (time < TimeSpan.Zero)
            time = TimeSpan.Zero;

        // Update properties.
        RawX = x;
        RawY = y;
        FrameTime = time;

        // Update raw radius.
        InputMath.ConvertToPolar(RawX, RawY, out float rawAngle, out float rawRadius);
        rawAngle = InputMath.ConvertRadiansToNormal(rawAngle);
        RawAngle = rawAngle;
        RawRadius = rawRadius;
        IsRawWithinCircle = rawRadius <= 1f;

        // Call inherited implementation.
        OnUpdating();

        // Register smoothing samples.
        RegisterSmoothingSample(in x, in y, in time);

        // Validate the joystick.
        float oldX = _x;
        float oldY = _y;
        Validate();

        // Update post-validation properties. These properties are update here,
        // instead of on the validation method, because the validation method 
        // will not perform a validation when the raw position doesn't change,
        // and these properties need to be updated independently of position changes.
        Delta = JoystickDelta.FromJoystickPosition(oldX, oldY, _x, _y);
        MovementSpeed = Delta.Distance == 0f ? 0f
            : FrameTime.TotalMilliseconds > 0d
            ? Delta.Distance / (float)FrameTime.TotalSeconds
            : float.PositiveInfinity;
        IsMoving = Delta.Distance > 0f;

        // Perform post-validation operations.
        OnUpdated();
        DispatchEvents();
    }


    /// <summary>
    /// It's called on every update to the <see cref="Joystick"/>, 
    /// before joystick validation. When overridden by a derived 
    /// class, performs inheritors custom operations that need to 
    /// occur before the validation is performed.
    /// </summary>
    /// <remarks>
    /// This method allows inheritors to perform any operation 
    /// that must occur when the <see cref="Joystick"/> is updated, 
    /// that might invalidate the <see cref="Joystick"/>, forcing 
    /// a validation to occur as soon as possible.
    /// <br/><br/>
    /// Use <see cref="RawX"/>, <see cref="RawY"/>, <see cref="RawAngle"/>,
    /// <see cref="RawRadius"/> and <see cref="FrameTime"/> properties 
    /// to get the new raw position of the joystick and the current 
    /// update's frame time.
    /// </remarks>
    /// <seealso cref="Invalidate()"/>
    protected virtual void OnUpdating()
    {

    }


    /// <summary>
    /// Marks the effective position of the <see cref="Joystick"/> 
    /// as outdated. The next time a getter method of one of the 
    /// properties that are associated with the position of the 
    /// <see cref="Joystick"/> is called, the <see cref="Joystick"/> 
    /// will be validated.
    /// </summary>
    /// <remarks>
    /// Inheritors can call this method if they are implementing 
    /// functionality that may affect th effective position of the 
    /// <see cref="Joystick"/> — the value of <see cref="X"/> 
    /// or <see cref="Y"/> properties — that is not automatically 
    /// applied. For instance, if you are implementing a custom 
    /// modifier property that consumers can change, you would call 
    /// this method whenever your property's value changes.
    /// </remarks>
    /// <seealso cref="Validate()"/>
    protected void Invalidate()
    {
        _isValid = false;
    }


    private void Invalidate(bool validate)
    {
        if (validate)
        {
            Invalidate();
        }
    }


    /// <summary>
    /// If the effective position of the joystick is outdated, 
    /// forces it to get updated and all modifiers to be applied.
    /// </summary>
    /// <returns><see langword="true"/> if the effective position 
    /// of the joystick was updated and effectively 
    /// changed as a result of this operation; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NotSupportedException">The 
    /// <see cref="ApplyCartesianModifiers(ref float, ref float)"/> 
    /// was overridden in the derived class, and outputted a
    /// <see cref="float.NaN"/> value when called by 
    /// <see cref="Validate()"/>.</exception>
    /// <exception cref="NotSupportedException">The 
    /// <see cref="ApplyPolarModifiers(ref float, ref float)"/> was 
    /// overridden in the derived class, and outputted a
    /// <see cref="float.NaN"/> value when called by 
    /// <see cref="Validate()"/>.</exception>
    /// <remarks>
    /// You call this method when you need to force the effective 
    /// position of the joystick to be updated with all the modifiers. 
    /// You usually call this method after changing the value of a 
    /// property that can affect the joystick's effective position. 
    /// Although the effective position is automatically updated 
    /// as needed when the getter method of some properties is 
    /// called, calling <see cref="Validate()"/> method ensures it 
    /// is updated immediately, so any events that depend on this 
    /// will be triggered.
    /// </remarks>
    /// <seealso cref="X"/>
    /// <seealso cref="Y"/>
    /// <seealso cref="Invalidate()"/>
    /// <seealso cref="ApplyCartesianModifiers(ref float, ref float)"/>
    /// <seealso cref="ApplyPolarModifiers(ref float, ref float)"/>
    public bool Validate()
    {
        if (_isValid || _isValidating)
            return false;

        try
        {
            _isValidating = true;
            float x = RawX;
            float y = RawY;

            // Apply smoothing, if smoothing is enabled.
            if (SmoothingFactor > 0f && SmoothingSamplePeriod > TimeSpan.Zero)
            {
                x = InputMath.Clamp11(InputMath.Interpolate(
                    x, _smoothingSamples.Average.X, SmoothingFactor));
                y = InputMath.Clamp11(InputMath.Interpolate(
                    y, _smoothingSamples.Average.Y, SmoothingFactor));
            }

            // Apply Cartesian modifiers to the effective position.
            ApplyCartesianModifiers(ref x, ref y);
            if (float.IsNaN(x))
                throw new NotSupportedException(
                    $"The output value 'x' of '{nameof(ApplyCartesianModifiers)}' method " +
                    $"cannot be '{float.NaN}'.");
            if (float.IsNaN(y))
                throw new NotSupportedException(
                    $"The output value 'y' of '{nameof(ApplyCartesianModifiers)}' method " +
                    $"cannot be '{float.NaN}'.");
            x = InputMath.Clamp11(x);
            y = InputMath.Clamp11(y);

            // Apply polar modifiers to the effective position.
            InputMath.ConvertToPolar(x, y, out float angle, out float radius);
            angle = InputMath.ConvertRadiansToNormal(angle);  // Convert angle from radians to 0-1 range.
            ApplyPolarModifiers(ref angle, ref radius);
            if (float.IsNaN(angle))
                throw new NotSupportedException(
                    $"The output value 'angle' of '{nameof(ApplyPolarModifiers)}' method " +
                    $"cannot be '{float.NaN}'.");
            if (float.IsNaN(radius))
                throw new NotSupportedException(
                    $"The output value 'radius' of '{nameof(ApplyPolarModifiers)}' method " +
                    $"cannot be '{float.NaN}'.");
            angle = InputMath.Clamp01(angle);
            radius = InputMath.Clamp01(radius);
            if (radius == 0f)
                angle = 0f;
            InputMath.ConvertToCartesian(
                InputMath.ConvertNormalToRadians(angle), radius, out x, out y);

            // Update effective position.
            _isValid = true;
            if (x == _x && y == _y)
            {
                return false;
            }
            else
            {
                X = x;
                Y = y;
                Angle = angle;
                Radius = radius;
                Direction = radius == 0f ? JoystickDirection.None
                    : ConvertNormalAngleToJoystickDirection(angle);
                IsPushed = radius > 0f;
                return true;
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            DispatchEvents();
            _isValidating = false;
        }
    }


    /// <summary>
    /// When overridden in derived classes, receives the specified 
    /// joystick raw Cartesian coordinates and applies any modifiers 
    /// to them.
    /// </summary>
    /// <param name="rawX">Reference to the raw X axis value to modify. 
    /// This is a value between -1 and 1.</param>
    /// <param name="rawY">Reference to the raw Y axis value to modify. 
    /// This is a value between -1 and 1.</param>
    /// <remarks>
    /// This method is called by <see cref="Validate()"/> method to 
    /// apply the modifiers to the axes position. The base 
    /// implementation applies all of the base Cartesian modifiers, so 
    /// you need to call the base implementation from your own 
    /// implementation to ensure the base modifiers are correctly 
    /// applied. Because <paramref name="rawX"/> and 
    /// <paramref name="rawY"/> represent the raw values of the X and 
    /// Y axes, obtained from the underlying device, you have the 
    /// ability to apply modifications that require the raw axes' 
    /// value for them to be applied, meaning you would call the base 
    /// implementation in the most appropriate moment for your needs.
    /// </remarks>
    /// <seealso cref="X"/>
    /// <seealso cref="Y"/>
    /// <seealso cref="Validate()"/>
    /// <seealso cref="ApplyPolarModifiers(ref float, ref float)"/>
    protected virtual void ApplyCartesianModifiers(ref float rawX, ref float rawY)
    {
        // Invert each axis, if inversion is enabled.
        if (InvertX)
        {
            rawX = -rawX;
        }
        if (InvertY)
        {
            rawY = -rawY;
        }

        // Apply the X modifier function.
        if (XModifierFunction is not null)
        {
            rawX = XModifierFunction(rawX);
            if (float.IsNaN(rawX))
                throw new NotSupportedException($"The value returned by " +
                    $"the function at '{nameof(XModifierFunction)}' property " +
                    $"must not return '{float.NaN}'.");
            rawX = InputMath.Clamp11(rawX);
        }
        // Apply the Y modifier function.
        if (YModifierFunction is not null)
        {
            rawY = YModifierFunction(rawY);
            if (float.IsNaN(rawY))
                throw new NotSupportedException($"The value returned by " +
                    $"the function at '{nameof(YModifierFunction)}' property " +
                    $"must not return '{float.NaN}'.");
            rawY = InputMath.Clamp11(rawY);
        }
    }


    /// <summary>
    /// When overridden in derived classes, receives the specified 
    /// joystick raw polar coordinates and applies any modifiers 
    /// to them.
    /// </summary>
    /// <param name="normalAngle">Reference to the normalized angle 
    /// to modify. This is a value between 0 and 1.</param>
    /// <param name="radius">Reference to the radius to modify. 
    /// This is a value between 0 and 1. See <see cref="Radius"/> 
    /// property for more information about normalized 
    /// <see cref="Joystick"/> angles.</param>
    /// <remarks>
    /// This method is called by <see cref="Validate()"/> method to 
    /// apply the modifiers to the joystick position in polar 
    /// coordinates, after a call to 
    /// <see cref="ApplyCartesianModifiers(ref float, ref float)"/> 
    /// method.
    /// <br/>
    /// The base implementation applies all of the base polar 
    /// modifiers, so you need to call the base implementation from 
    /// your own implementation to ensure the base modifiers are 
    /// correctly applied.
    /// </remarks>
    /// <seealso cref="Angle"/>
    /// <seealso cref="Radius"/>
    /// <seealso cref="Validate()"/>
    /// <seealso cref="ApplyCartesianModifiers(ref float, ref float)"/>
    protected virtual void ApplyPolarModifiers(ref float normalAngle, ref float radius)
    {
        // Apply dead-zones (inner and outer).
        radius = InputMath.ApplyDeadZone(radius, InnerDeadZone, OuterDeadZone);

        // Apply the radius modifier function.
        if (RadiusModifierFunction is not null)
        {
            radius = RadiusModifierFunction(radius);
            if (float.IsNaN(radius))
                throw new NotSupportedException($"The value returned by " +
                    $"the function at '{nameof(RadiusModifierFunction)}' property " +
                    $"must not return '{float.NaN}'.");
            radius = InputMath.Clamp01(radius);
        }
        // Apply the angle modifier function.
        if (AngleModifierFunction is not null)
        {
            normalAngle = AngleModifierFunction(normalAngle);
            if (float.IsNaN(normalAngle))
                throw new NotSupportedException($"The value returned by " +
                    $"the function at '{nameof(AngleModifierFunction)}' property " +
                    $"must not return '{float.NaN}'.");
            normalAngle = InputMath.Clamp01(normalAngle);
        }
    }


    private void RegisterSmoothingSample(in float x, in float y, in TimeSpan time)
    {
        if (SmoothingFactor > 0f && SmoothingSamplePeriod > TimeSpan.Zero)
        {
            // Add a new sample to the smoothing samples.
            _smoothingSamples.Add(new JoystickSample(x, y, time));

            // Adjust sample length based on average frame time.
            if (_smoothingSamples.Count >= MinSmoothingSamples
                && _smoothingSamples.Average.Time > TimeSpan.Zero)
            {
                int sampleLength = (int)Math.Ceiling(SmoothingSamplePeriod.TotalMilliseconds
                    / _smoothingSamples.Average.Time.TotalMilliseconds);
                sampleLength = Math.Clamp(sampleLength, MinSmoothingSamples, MaxSmoothingSamples);
                if (sampleLength != _smoothingSamples.MaxSampleLength)
                {
                    _smoothingSamples.SetMaxSampleLength(sampleLength);
                }
            }

            // Remove all excess samples, if any.
            _smoothingSamples.RemoveWhile(sample =>
                _smoothingSamples.TotalSum.Time > SmoothingSamplePeriod
                || _smoothingSamples.TotalSum.Time == TimeSpan.Zero);

            // Invalidate joystick.
            Invalidate(_smoothingSamples.Average.X != x || _smoothingSamples.Average.Y != y);
        }
    }


    /// <summary>
    /// Sets all writable properties in the current <see cref="Joystick"/>
    /// instance with values obtained from the corresponding properties 
    /// in the specified <see cref="Joystick"/> instance.
    /// </summary>
    /// <param name="joystick"><see cref="Joystick"/> instance to copy 
    /// the configuration from.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="joystick"/> is <see langword="null"/>.</exception>
    public virtual void CopyConfigurationFrom(Joystick joystick)
    {
        if (joystick is null)
            throw new ArgumentNullException(nameof(joystick));

        InvertX = joystick.InvertX;
        InvertY = joystick.InvertY;
        InnerDeadZone = joystick.InnerDeadZone;
        OuterDeadZone = joystick.OuterDeadZone;
        XModifierFunction = joystick.XModifierFunction;
        YModifierFunction = joystick.YModifierFunction;
        AngleModifierFunction = joystick.AngleModifierFunction;
        RadiusModifierFunction = joystick.RadiusModifierFunction;
        SmoothingFactor = joystick.SmoothingFactor;
        SmoothingSamplePeriod = joystick.SmoothingSamplePeriod;
    }


    /// <summary>
    /// Gets a new <see cref="DigitalButton"/> instance that activates 
    /// (presses) when the specified activation function returns 
    /// <see langword="true"/> for the current state of the 
    /// <see cref="Joystick"/>.
    /// </summary>
    /// <param name="activationFunction">Function that will be called 
    /// on every update to the state of the <see cref="Joystick"/>, which 
    /// receives the current <see cref="Joystick"/> instance and the 
    /// <see cref="DigitalButton"/> as its parameters.</param>
    /// <returns>A new <see cref="DigitalButton"/> instance that is updated 
    /// automatically, and reports its state as pressed depending on the state 
    /// of the <see cref="Joystick"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="activationFunction"/> is <see langword="null"/>.</exception>
    /// <seealso cref="DigitalButton"/>
    public DigitalButton ToDigitalButton(
        Func<Joystick, DigitalButton, bool> activationFunction)
    {
        if (activationFunction is null)
            throw new ArgumentNullException(nameof(activationFunction));

        DigitalButton button = new(out DigitalButtonUpdateCallback updateCallback);
        Updated += (sender, e) =>
        {
            if (sender is Joystick joystick)
            {
                updateCallback.Invoke(activationFunction(joystick, button), joystick.FrameTime);
            }
        };
        updateCallback.Invoke(activationFunction(this, button), FrameTime);

        return button;
    }


    /// <summary>
    /// Gets a new <see cref="DigitalButton"/> instance that activates 
    /// (presses) whenever the current <see cref="Joystick"/>'s effective 
    /// direction matches the specified direction and the joystick's 
    /// effective radius reaches the specified activation threshold,
    /// and then deactivates if the effective radius gets under the 
    /// specified deactivation threshold.
    /// </summary>
    /// <param name="direction">A <see cref="JoystickDirection"/> constant 
    /// that specifies the effective direction the joystick must have for 
    /// the button to be activated.</param>
    /// <param name="activationThreshold">A number between the 0 and 1 
    /// inclusive range, that specifies the minimum value over which 
    /// the joystick's effective radius must reach for the button 
    /// to get activated.</param>
    /// <param name="deactivationThreshold">A number between the 0 and 1 
    /// inclusive range, that specifies the value under which the 
    /// joystick's effective radius must be for the button 
    /// to get deactivated, when the button is activated. Usually, 
    /// this is a value equal to or lower than 
    /// <paramref name="activationThreshold"/>.</param>
    /// <returns>A new <see cref="DigitalButton"/> that is automatically 
    /// updated whenever the state of the current <see cref="Joystick"/> 
    /// is updated, and that is activated (pressed) whenever the state of 
    /// the joystick matches the specified conditions.</returns>
    /// <exception cref="ArgumentException"><paramref name="direction"/>
    /// is not a defined constant in a <see cref="JoystickDirection"/> 
    /// enumeration.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="activationThreshold"/> is <see cref="float.NaN"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="deactivationThreshold"/> is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="DigitalButton"/>
    public DigitalButton ToDigitalButton(JoystickDirection direction,
        float activationThreshold, float deactivationThreshold)
    {
        if (!Enum.IsDefined(direction))
            throw new ArgumentException($"'{direction}' is not a defined " +
                $"constant in a '{nameof(JoystickDirection)}' enumeration.",
                nameof(direction));
        if (float.IsNaN(activationThreshold))
            throw new ArgumentException($"'{float.NaN}' is not a valid " +
                $"value for '{nameof(activationThreshold)}' parameter.",
                nameof(activationThreshold));
        if (float.IsNaN(deactivationThreshold))
            throw new ArgumentException($"'{float.NaN}' is not a valid " +
                $"value for '{nameof(deactivationThreshold)}' parameter.",
                nameof(deactivationThreshold));

        return ToDigitalButton((joystick, button) =>
        {
            if (!button.IsPressed)
                return joystick.Radius >= activationThreshold && joystick.Direction == direction;
            else
                return joystick.Radius >= deactivationThreshold && joystick.Direction == direction;
        });
    }


    /// <summary>
    /// Gets a new <see cref="DigitalButton"/> instance that activates 
    /// (presses) whenever the current <see cref="Joystick"/>'s effective 
    /// direction matches the specified direction and the joystick's 
    /// effective radius reaches the specified activation threshold.
    /// </summary>
    /// <param name="direction">A <see cref="JoystickDirection"/> constant 
    /// that specifies the effective direction the joystick must have for 
    /// the button to be activated.</param>
    /// <param name="activationThreshold">A number between the 0 and 1 
    /// inclusive range, that specifies the minimum value over which 
    /// the joystick's effective radius must reach for the button 
    /// to get activated.</param>
    /// <returns>A new <see cref="DigitalButton"/> that is automatically 
    /// updated whenever the state of the current <see cref="Joystick"/> 
    /// is updated, and that is activated (pressed) whenever the state of 
    /// the joystick matches the specified conditions.</returns>
    /// <exception cref="ArgumentException"><paramref name="direction"/>
    /// is not a defined constant in a <see cref="JoystickDirection"/> 
    /// enumeration.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="activationThreshold"/> is <see cref="float.NaN"/>.</exception>
    /// <seealso cref="DigitalButton"/>
    public DigitalButton ToDigitalButton(JoystickDirection direction,
        float activationThreshold)
    {
        return ToDigitalButton(direction, activationThreshold, activationThreshold);
    }

    #endregion Methods


}
