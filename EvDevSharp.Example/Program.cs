using EvDevSharp;
using EvDevSharp.Example;

if (UnixEnvironment.GetEffectiveUserId() != UnixEnvironment.RootUserId)
{
    Console.WriteLine("This program requires root access.");
    return;
}

EvDev.RegisterDevices();

var devices = EvDev.GetRegisteredDevices().OrderBy(d => d.DevicePath).ToList();

if (!devices.Any())
{
    Console.WriteLine("No device was found.");
    return;
}

for (int i = 0; i < devices.Count; i++)
{
    Console.WriteLine($"{i}) {devices[i].Name}");
}

Console.WriteLine();

bool inputOk = false;
int key;
do
{
    Console.Write($"Select a device [0-{devices.Count - 1}]: ");
    inputOk = int.TryParse(Console.ReadLine(), out key);
} while (!inputOk || key < 0 || key >= devices.Count);

devices[key].OnKeyEvent += (s, e) =>
{
    Console.WriteLine($"Key: {e.Key}\tValue: {e.Value}");
};

devices[key].OnRelativeEvent += (s, e) =>
{
    Console.WriteLine($"Rel Axis: {e.Axis}\tValue: {e.Value}");
};

devices[key].OnAbsoluteEvent += (s, e) =>
{
    Console.WriteLine($"Abs Axis: {e.Axis}\tValue: {e.Value}");
};

Console.WriteLine("Monitoring the device (press enter to exit)...");

devices[key].StartMonitoring();
Console.ReadLine();
devices[key].StopMonitoring();