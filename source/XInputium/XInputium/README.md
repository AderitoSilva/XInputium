**XInputium** is a .NET library that provides you out-of-the-box XInput integration into your application. In contrast with other existing .NET XInput libraries, XInputium introduces a full-fledged input system, and is designed to allow you to focus more on your game/application logic and less on input handling code.

## Key features
- **A dedicated Event System**, composed of dynamic events (events that are triggered accordingly to your custom specific conditions), as well as the regular .NET events.
- **Built-in input loop** — you don't need to worry about managing input state logic between application/game frames. Just call a single method each frame, and XInputium will do the rest.
- **Built-in Joystick and Trigger functionality**, like dead-zones, axis inversion, smoothing, sensitivity and many more.
- **Axis Modifier Functions** allow you to modify axes in any way you wish. Several built-in functions included, which you can creatively combine to do whatever your game or application requires.
- **Map joysticks or triggers to buttons**, using your own mapping criteria.
- **Compose layers of joysticks/triggers** for specific parts of your application. For instance, the same joystick might have different simultaneous configurations depending on the current game/application mode the user is at. 
- **XAML bindable** — bind your XAML controls to any property (ex. joystick radius), and it just works.