# XInputium
**XInputium** is a .NET library that provides you out-of-the-box XInput integration into your application. In contrast with other existing .NET XInput libraries, XInputium introduces a full fledged input system, and is designed to allow you to focus more on your game/application logic and less on input handling code.

## Key features
- **A dedicated Event System**, composed of dynamic events (events that are triggered accordingly to your custom specific conditions), as well as the regular .NET events.
- **Built-in input loop** — you don't need to worry about managing input state logic between application/game frames. Just call a single method each frame and XInputium will do the rest.
- **Built-in Joystick and Trigger functionality**, like dead-zones, axis inversion, smoothing, sensitivity and many more.
- **Axis Modifier Functions** allow you to modify axes in any way you wish. Several built-in functions included, which you can creatively combine to do whatever your game or application requires.
- **Map joysticks or triggers to buttons**, using your own mapping criteria.
- **Compose layers of joysticks/triggers** for specific parts of your application. For instance, the same joystick might have different simultaneous configurations depending on the current game/application mode the user is at. 
- **XAML bindable** — bind your XAML controls to any property (ex. joystick radius), and it just works.

## Getting started
The following example code shows how you can consume XInputium:
```c#
    XGamepad gamepad = new();
    gamepad.ButtonPressed += (s, e) => Debug.WriteLine($"Button {e.Button} was pressed.");
    
    // Call this on every app/game frame.
    gamepad.Update();
```
`XGamepad` represents a logical controller device, and is the main class you would use to consume most of XInputium features. The only thing you need to do is to update the device state on every app/game frame, as shown in the previous example. Once you call `gamepad.Update()`, any consequent events will trigger.

XInputium is fully documented, and essential information in the remarks sections is included where necessary. The library is designed to be as easy to use as possible, while still providing powerful features. A Wiki page and Reference Documentation are planned for the near future, so keep an eye out.

## Demo application — XInputium Preview

The provided XInputium demo application — XInputium Preview — lets you see XInputium in action and allows you to test-drive some of its features. It is a WPF application, made mostly in XAML, that shows how you would use XInputium in your own applications or games. XInputium Preview can also be useful to diagnose your XInput compatible device.

![XInputium Preview — XInputium feature preview application](/assets/images/XInputiumPreview.png "XInputium Preview — XInputium feature preview application")

## System requirements
* XInputium is built on **.NET 6.0**.
* XInputium uses **XInput 9.1.0** API.
* Currently, only **Windows 7 or later** is supported. Support for additional platforms may be implemented in the future.

