# evdev-sharp
A library for consuming evdev-capable devices

## What is evdev?
From Linux kernel docs:

> Input subsystem is a collection of drivers that is designed to support all input devices under Linux. 
> Most of the drivers reside in drivers/input, although quite a few live in drivers/hid and drivers/platform.

For more information refer to the Linux kernel [docs](https://www.kernel.org/doc/html/latest/input/input.html).

## How to use this library?
Each evdev device is represented with the `EvDevDevice` class. 
This class does not have a public constructor and in order to obtain device objects you must use the `EvDev.GetRegisteredDevices()` method.

First you should register devices. This can be done in two ways:

``` csharp
// This method will register all available devices and begin monitoring them
EvDev.RegisterDevices();
```
There is also a generic version of this method:
``` csharp
// This method will register only devices of given type and begin monitoring them
// In this case only mouse and keyboard devices will be registered
Evdev.RegisterDevices<EvDevMouseDevice>();
Evdev.RegisterDevices<EvDevKeyboardDevice>();
```

Each `EvDevDevice` instance contains properties such as device's buttons and axises. It also provides events that you can subscribe to.
If you want to stop receiving events, you must call the `StopMonitoring()` method on the instance.

There are also wrappers for EvDevDevices of specific types (keyboard, mouse, touchpad, etc.). 
These wrappers contain wrapper events that you can subscribe to, but you can still use generic events.

## Requirements:
- C# 9.0 or later
- .Net Standard 2.1 or .Net Framework 4.8.1 (probably will try to downgrade in the future if it will be requested)
- For Unity - should work for any C# and .Net Framework/Standard compatible version, though it was tested only on Unity 2022.3.26f1
- Should work on any Linux version, but it was tested only on Linux 6 distro version
- Application should be run as root in order to access evdev devices

## Warning
This library is still in development and may (will) have some bugs. It is used in production, so bugs probably will be
detected and fixed, but if you encountered bug or strange behavior, please report it. You can create an issue or write me
directly on draas.games@gmail.com

## Installation:
### Non-Unity
[Just get it on NuGet](https://www.nuget.org/packages/EvDevSharpWrapper) and add it to your project or get dll from releases section on github

### Unity
There are basically 2 ways:
- Install NuGet for Unity and repeat the previous step
- Or just download/build dll and drop to the Assets folder

## Examples
The following code will list every evdev device on the system:

``` csharp
var devices = EvDev.GetRegisteredDevices().OrderBy(d => d.DevicePath).ToList();

for (int i = 0; i < devices.Count; i++)
{
    Console.WriteLine($"{i}) {devices[i].Name}");
}
```

The following code will get the first mouse on the system and will subscribe to its key events:

``` csharp
EvDev.RegisterDevices<EvDevMouseDevice>();

// Assume at least one mouse device registered
var mouse = EvDev.GetRegisteredDevices<EvDevMouseDevice>().First();

mouse.OnLeftMouseButtonPressed += (s, e) =>
{
    Console.WriteLine($"Left mouse button on {s.Name} pressed");
};

// Or you can use generic event subscription
mouse.OnKeyEvent((s, e) =>
{
    Console.WriteLine($"Button: {e.Key}\tState: {e.Value}");
});
```

