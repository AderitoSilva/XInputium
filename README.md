# XInputium
 The elemental XInput library for .NET.

**XInputium** is a .NET library that provides you out-of-the-box XInput integration into your application. In contrast with other existing .NET XInput libraries, XInputium provides a full fledged input system, and offers, at least, three different ways for communicating with XInput compatible devices, each with its own flavor.

## Key features
These are some of the key features of XInputium:
- 3+ different ways of consuming the API, allowing you to choose what best fits your needs and taste.
- A comprehensive Input System built around XInput.
- A full Event System composed of dynamic events (events that are triggered accordingly to specific parameters), as well as the traditional .NET events.
- Built-in input loop — you don't need to worry about input loop logic between application or game frames. Just call a single method each frame and XInputium will do the rest.
- Built-in Joystick and Trigger functionality, like dead-zones, axis inversion, smoothing, sensitivity and many more.
- Axis Modifier Functions allow you to modify axes in any way you wish. Several built-in functions included, which you can creatively combine to do whatever your game or application needs.
- Make joysticks or triggers work as buttons in any way you wish.
- Create layers of joysticks or triggers for different parts of your application. You don't need to change your own code logic to adapt to the device. 
- XAML bindable — bind your XAML controls to any property (ex. joystick radius) and it just works.

## How to use
### The `XInputDeviceState` way — just the essential features, the most lightweight
`XInputDeviceState` simply represents the state of a controller device.
```c#
    // Call this on every app/game frame.
    XInputDeviceState state = XInputDevice.GetState(XInputUserIndex.One);
```

### The `XInputDevice` way — more features than the above, but almost as lightweight
`XInputDevice` represents a controller device, and introduces basic events and contained device state management.
```c#
    XInputDevice device = XInputDevice.GetFirstConnectedDevice();

    // Call this on every app/game frame.
    if (device.Update())
    {
        Debug.WriteLine("The state has changed!");
    }
```

### The `XGamepad` way — the most feature rich and the easiest to work with
`XGamepad` represents a logical controller device, and encapsulates an `XInputDevice` class that can be changed at any time. `XGamepad` is a set-and-forget class.
```c#
    XGamepad gamepad = new();  // This constructor uses the first connected device it finds.
    gamepad.ButtonPressed += (s, e) => Debug.WriteLine($"Button {e.Button} was pressed.");
    
    // Call this on every app/game frame. This will update the device state and trigger any events.
    gamepad.Device?.Update();
```

## Demo application

The provided XInputium demo application — XInputium Preview — lets you see XInputium in action and allows you to test-drive some of its features. It is a WPF application, made mostly in XAML, that shows how you would use XInputium in your own apps or games. This application can also be useful to diagnose your XInput compatible device.

![XInputium Preview — XInputium feature preview application](/assets/images/XInputiumPreview.png "XInputium Preview — XInputium feature preview application")

## System requirements
XInputium is built on **.NET 6.0** and can run on **Windows 7 or newer**.
XInputium uses **XInput 9.1.0** API, which is included  on Windows since Windows Vista.
