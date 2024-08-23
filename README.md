# evdev-sharp
A library for consuming evdev-capable devices

## What is evdev?
From Linux kernel docs:

> Input subsystem is a collection of drivers that is designed to support all input devices under Linux. Most of the drivers reside in drivers/input, although quite a few live in drivers/hid and drivers/platform.

For more information refer to the Linux kernel [docs](https://www.kernel.org/doc/html/latest/input/input.html).

## How to use this library?
Each evdev device is represented with the `EvDevDevice` class. 
This class does not have a public constructor and in order to obtain device objects you must use the `EvDevDevice.GetDevices()` method.
This method will enumerate each `/dev/input/event*` file and creates an object for each one.

Each `EvDevDevice` instance contains properties such as device's buttons and axises. It also provides events that you can subscribe to.
In order to get events you must call the `StartMonitoring()` method on the instance. This method will constantly read the device's evdev file on a separate thread and will raise events accordingly.

## Requirements:
- C# 9.0 or later
- .Net Standard 2.1 or .Net Framework 4.8.1 (probably will try to downgrade in the future if it will be requested)
- For Unity - should work for any C# and .Net Framework/Standard compatible version, though it was tested only on Unity 2022.3.26f1
- Should work on any Linux version, but it was tested only on RedOs 8.0
- Application should be run as root in order to access evdev devices

## Installation:
### Clear C#
Just download it on NuGet and add it to your project 

Link: https://www.nuget.org/packages/EvDevSharpWrapper.
### Unity
There are basically 2 ways:
- Install NuGet for Unity and repeat the previous step
- Or just download/build dll and drop to the Assets folder

### Examples
The following code will list every evdev device on the system:

``` csharp
var devices = EvDevDevice.GetDevices().OrderBy(d => d.DevicePath).ToList();

for (int i = 0; i < devices.Count; i++)
{
    Console.WriteLine($"{i}) {devices[i].Name}");
}
```

The following code will get the first gamepad (joystick) on the system and will subscribe to its key events:

``` csharp
var gamepad = EvDevDevice.GetDevices()
    .First(d => d.GuessedDeviceType == EvDevGuessedDeviceType.GamePad);

gamepad.OnKeyEvent += (s, e) =>
{
    Console.WriteLine($"Button: {e.Key}\tState: {e.Value}");
};

gamepad.StartMonitoring();
```

