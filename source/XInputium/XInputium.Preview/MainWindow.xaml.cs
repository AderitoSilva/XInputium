﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using XInputium.Preview.Data.Poco;
using XInputium.ModifierFunctions;
using XInputium.XInput;

namespace XInputium.Preview;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{


    #region Fields

    private static readonly ModifierFunctionPoco[] s_ModifierFunctions;

    #endregion Fields


    #region Constructors

    static MainWindow()
    {
        // Create the modifier functions that we will use in the UI.
        s_ModifierFunctions = new ModifierFunctionPoco[]
        {
            new ModifierFunctionPoco("None", null),
            new("Linear", NonLinearFunctions.Linear),
            new("Quadratic EaseIn", NonLinearFunctions.QuadraticEaseIn),
            new("Quadratic EaseOut", NonLinearFunctions.QuadraticEaseOut),
            new("Quadratic EaseInOut", NonLinearFunctions.QuadraticEaseInOut),
            new("Cubic EaseIn", NonLinearFunctions.CubicEaseIn),
            new("Cubic EaseOut", NonLinearFunctions.CubicEaseOut),
            new("Cubic EaseInOut", NonLinearFunctions.CubicEaseInOut),
            new("Quartic EaseIn", NonLinearFunctions.QuarticEaseIn),
            new("Quartic EaseOut", NonLinearFunctions.QuarticEaseOut),
            new("Quartic EaseInOut", NonLinearFunctions.QuarticEaseInOut),
            new("Quintic EaseIn", NonLinearFunctions.QuinticEaseIn),
            new("Quintic EaseOut", NonLinearFunctions.QuinticEaseOut),
            new("Quintic EaseInOut", NonLinearFunctions.QuinticEaseInOut),
            new("Sine EaseIn", NonLinearFunctions.SineEaseIn),
            new("Sine EaseOut", NonLinearFunctions.SineEaseOut),
            new("Sine EaseInOut", NonLinearFunctions.SineEaseInOut),
            new("Circular EaseIn", NonLinearFunctions.CircularEaseIn),
            new("Circular EaseOut", NonLinearFunctions.CircularEaseOut),
            new("Circular EaseInOut", NonLinearFunctions.CircularEaseInOut),
            new("Exponential EaseIn", NonLinearFunctions.ExponentialEaseIn),
            new("Exponential EaseOut", NonLinearFunctions.ExponentialEaseOut),
            new("Exponential EaseInOut", NonLinearFunctions.ExponentialEaseInOut),
            new("Bézier", NonLinearFunctions.Bezier),
            new("Reverse", CommonModifierFunctions.Reverse),
            new("Negate", CommonModifierFunctions.Negate),
            new("Scale 200%", CommonModifierFunctions.Scale(2f)),
            new("Scale 50%", CommonModifierFunctions.Scale(0.5f)),
            new("Boolean", CommonModifierFunctions.Boolean()),
            new("Quantize 1/1", CommonModifierFunctions.Quantize(1f)),
            new("Quantize 1/2", CommonModifierFunctions.Quantize(0.5f)),
            new("Quantize 1/4", CommonModifierFunctions.Quantize(0.25f)),
            new("Quantize 1/8", CommonModifierFunctions.Quantize(0.125f)),
            new("Zero", CommonModifierFunctions.Zero),
        };
    }


    public MainWindow()
    {
        InitializeComponent();

        // Register window event handlers
        SourceInitialized += (_, _) => CompositionTarget.Rendering += (_, _) => OnRendering();
        Loaded += MainWindow_Loaded;

        // Initialize input objects.
        DeviceManager = new();
        DeviceManager.DeviceConnected += DeviceManager_DeviceConnected;
        Gamepad = new(null);
        Gamepad.StateChanged += (_, _) => OnGamepadStateChanged();
    }

    #endregion Constructors


    #region Properties

    public static IReadOnlyList<ModifierFunctionPoco> ModifierFunctions => s_ModifierFunctions;


    #region Gamepad dependency property

    public XGamepad Gamepad
    {
        get => (XGamepad)GetValue(GamepadProperty);
        private set => SetValue(s_GamepadPropertyKey, value);
    }

    private static readonly DependencyPropertyKey s_GamepadPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(Gamepad),
            typeof(XGamepad), typeof(MainWindow),
            new PropertyMetadata(null));

    public static readonly DependencyProperty GamepadProperty
        = s_GamepadPropertyKey.DependencyProperty;

    #endregion


    #region DeviceManager dependency property

    public XInputDeviceManager DeviceManager
    {
        get => (XInputDeviceManager)GetValue(DeviceManagerProperty);
        private set => SetValue(s_DeviceManagerPropertyKey, value);
    }

    private static readonly DependencyPropertyKey s_DeviceManagerPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(DeviceManager),
            typeof(XInputDeviceManager), typeof(MainWindow),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DeviceManagerProperty
        = s_DeviceManagerPropertyKey.DependencyProperty;

    #endregion

    #endregion Properties


    #region Methods

    private void SetGamepadDefaultConfiguration()
    {
        // Setup triggers.
        Gamepad.LeftTrigger.InnerDeadZone = 0.15f;
        Gamepad.LeftTrigger.ModifierFunction = NonLinearFunctions.QuadraticEaseIn;
        Gamepad.RightTrigger.CopyConfigurationFrom(Gamepad.LeftTrigger);

        // Setup joysticks.
        Gamepad.LeftJoystick.InnerDeadZone = 0.2f;
        Gamepad.LeftJoystick.RadiusModifierFunction = NonLinearFunctions.QuadraticEaseIn;
        Gamepad.LeftJoystick.SmoothingSamplePeriod = TimeSpan.FromMilliseconds(100d);
        Gamepad.LeftJoystick.SmoothingFactor = 0.75f;
        Gamepad.RightJoystick.CopyConfigurationFrom(Gamepad.LeftJoystick);
    }


    [Conditional("DEBUG")]
    private void InitializeDebug()
    {
        // Code in this method is for simple testing purposes during debug.

        Gamepad.RegisterButtonPressedEvent(XButtons.B,
            (sender, e) => Debug.WriteLine($"Pressed button {e.Button}."));
        Gamepad.RegisterButtonReleasedEvent(XButtons.B,
            (sender, e) => Debug.WriteLine($"Released button {e.Button}."));
        Gamepad.RegisterButtonHoldEvent(XButtons.Y, TimeSpan.FromMilliseconds(2000),
            (sender, e) => Debug.WriteLine($"Held button {e.Button} for " +
            $"{((DigitalButtonInputEvent<XInputButton>)e.Event).HoldDuration.TotalMilliseconds} milliseconds."));
        Gamepad.RegisterButtonRepeatEvent(XButtons.DPadUp,
            TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(200),
            1.05f, TimeSpan.FromMilliseconds(80), TimeSpan.MaxValue,
            (sender, e) => Debug.WriteLine($"[{Environment.TickCount64}] {e.Button} keystroke " +
                $"({((RepeatDigitalButtonInputEvent<XInputButton>)e.Event).RepeatCount} repeats)."));

    }



    protected virtual void OnRendering()
    {
        DeviceManager.Update();
    }


    protected virtual void OnGamepadStateChanged()
    {
        // Set the controller motors's speed based on the values of its triggers.
        Gamepad.LeftMotorSpeed = Gamepad.LeftTrigger.Value;
        Gamepad.RightMotorSpeed = Gamepad.RightTrigger.Value;
    }

    #endregion Methods


    #region Event handlers

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        SetGamepadDefaultConfiguration();
        InitializeDebug();
    }


    private void DeviceManager_DeviceConnected(object? sender, XInputDeviceEventArgs e)
    {
        if (!Gamepad.IsConnected)
        {
            Gamepad.Device = DeviceManager.ConnectedDevices.FirstOrDefault();
        }
    }

    #endregion Event handlers


}
