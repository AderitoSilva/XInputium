using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using XInputium;
using XInputium.Preview.Data.Poco;
using XInputium.ModifierFunctions;
using XInputium.XInput;

namespace XInputium.Preview;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{


    #region Fields and constants

    public static readonly List<ModifierFunctionPoco> ModifierFunctions;

    #endregion Fields and constants


    #region Constructors

    static MainWindow()
    {
        // Create the modifier functions we will use in the UI.
        ModifierFunctions = new()
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
        };
    }


    public MainWindow()
    {
        InitializeComponent();

        // Register window event handlers
        SourceInitialized += MainWindow_SourceInitialized;
        Loaded += MainWindow_Loaded;

        // Initialize input objects.
        DeviceManager = new();
        DeviceManager.DeviceConnected += DeviceManager_DeviceConnected;
        DeviceManager.DeviceDisconnected += DeviceManager_DeviceDisconnected;
        Gamepad = new(null);
        Gamepad.StateChanged += Gamepad_StateChanged;
    }

    #endregion Constructors


    #region Properties

#pragma warning disable CA1822 // Mark members as static
    public App App => (App)Application.Current;
#pragma warning restore CA1822 // Mark members as static


    #region Gamepad dependency property

    public XGamepad Gamepad
    {
        get => (XGamepad)GetValue(GamepadProperty);
        private set => SetValue(GamepadPropertyKey, value);
    }

    private static readonly DependencyPropertyKey GamepadPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(Gamepad),
            typeof(XGamepad), typeof(MainWindow),
            new PropertyMetadata(null));

    public static readonly DependencyProperty GamepadProperty
        = GamepadPropertyKey.DependencyProperty;

    #endregion


    #region DeviceManager dependency property

    public XInputDeviceManager DeviceManager
    {
        get => (XInputDeviceManager)GetValue(DeviceManagerProperty);
        private set => SetValue(DeviceManagerPropertyKey, value);
    }

    private static readonly DependencyPropertyKey DeviceManagerPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(DeviceManager),
            typeof(XInputDeviceManager), typeof(MainWindow),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DeviceManagerProperty
        = DeviceManagerPropertyKey.DependencyProperty;

    #endregion

    #endregion Properties


    #region Methods

    #region Initialization related methods

    private void InitializeLogic()
    {
        // Set default gamepad settings.
        Gamepad.LeftTrigger.InnerDeadZone = 0.15f;
        Gamepad.LeftTrigger.ModifierFunction = NonLinearFunctions.QuadraticEaseIn;
        Gamepad.RightTrigger.CopyConfigurationFrom(Gamepad.LeftTrigger);
        Gamepad.LeftJoystick.InnerDeadZone = 0.2f;
        Gamepad.LeftJoystick.RadiusModifierFunction = NonLinearFunctions.QuadraticEaseIn;
        Gamepad.LeftJoystick.SmoothingSamplePeriod = TimeSpan.FromMilliseconds(100);
        Gamepad.LeftJoystick.SmoothingFactor = 0.75f;
        Gamepad.RightJoystick.CopyConfigurationFrom(Gamepad.LeftJoystick);
    }


    private void InitializeUi()
    {

    }


    [Conditional("DEBUG")]
    private void InitializeDebug()
    {
        Gamepad.RegisterButtonPressedEvent(XButtons.B,
            (sender, e) => Debug.WriteLine($"Pressed button {e.Button}."));
        Gamepad.RegisterButtonReleasedEvent(XButtons.B,
            (sender, e) => Debug.WriteLine($"Released button {e.Button}."));
        Gamepad.RegisterButtonHoldEvent(XButtons.Y, TimeSpan.FromMilliseconds(2000),
            (sender, e) => Debug.WriteLine($"Held button {e.Button}."));

    }

    #endregion


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

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        CompositionTarget.Rendering += CompositionTarget_Rendering;
    }


    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeLogic();
        InitializeUi();
        InitializeDebug();
    }


    private void CompositionTarget_Rendering(object? sender, EventArgs e)
    {
        OnRendering();
    }


    private void DeviceManager_DeviceConnected(object? sender, XInputDeviceEventArgs e)
    {
        if (!Gamepad.IsConnected)
        {
            Gamepad.Device = DeviceManager.ConnectedDevices.FirstOrDefault();
        }
    }


    private void DeviceManager_DeviceDisconnected(object? sender, XInputDeviceEventArgs e)
    {

    }


    private void Gamepad_StateChanged(object? sender, EventArgs e)
    {
        OnGamepadStateChanged();
    }

    #endregion Event handlers


}
