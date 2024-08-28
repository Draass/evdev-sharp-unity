using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EvDevSharp.Wrappers.Keyboard;
using EvDevSharp.Wrappers.Mouse;

namespace EvDevSharp
{
    /// <summary>
    /// Main class for handling evdev devices lifecycle
    /// </summary>
    public static class EvDev
    {
        private const string INPUT_PATH = "/dev/input/";
        private const string INPUT_PATH_SEARCH_PATTERN = "event*";

        private static Dictionary<Type, List<EvDevDevice>> RegisteredDevices = new();

        /// <summary>
        /// Enumerates all the Linux event files and generates a <c>EvDevDevice</c> object for each file.
        /// </summary>
        /// <exception cref="System.PlatformNotSupportedException">Current OS is not Linux</exception>
        public static void RegisterDevices()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                throw new PlatformNotSupportedException();

            var devicesPaths = Directory.GetFiles(INPUT_PATH, INPUT_PATH_SEARCH_PATTERN);

            Parallel.ForEach(devicesPaths, path =>
            {
                EvDevDeviceData deviceData = EvDevDeviceGuesser.GetDeviceData(path);

                var device = CreateDeviceOfType(deviceData);

                if (RegisteredDevices.TryGetValue(device.GetType(), out var registeredDevices))
                {
                    if (registeredDevices.Exists(d => d.DevicePath == device.DevicePath))
                    {
                        // Device already registered
                        return;
                    }

                    registeredDevices.Add(device);
                }
                else
                {
                    RegisteredDevices.Add(device.GetType(), new List<EvDevDevice> {device});
                }

                device.StartMonitoring();
            });
        }

        /// <summary>
        /// Enumerates all the Linux event files and generates a <c>EvDevDevice</c> object for each file of given type.
        /// </summary>
        /// <exception cref="System.PlatformNotSupportedException"></exception>
        public static void RegisterDevices<T>() where T : EvDevDevice
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                throw new PlatformNotSupportedException();

            var devicesPaths = Directory.GetFiles(INPUT_PATH, INPUT_PATH_SEARCH_PATTERN);

            Parallel.ForEach(devicesPaths, path =>
            {
                EvDevDeviceData deviceData = EvDevDeviceGuesser.GetDeviceData(path);

                if (deviceData.GuessedDeviceType != EvDevExtensions.ToGuessedDeviceType(typeof(T)))
                    return;

                var device = CreateDeviceOfType(deviceData);

                if (RegisteredDevices.TryGetValue(device.GetType(), out var registeredDevices))
                {
                    if (registeredDevices.Exists(d => d.DevicePath == device.DevicePath))
                    {
                        // Device already registered
                        return;
                    }

                    registeredDevices.Add(device);
                }
                else
                {
                    RegisteredDevices.Add(device.GetType(), new List<EvDevDevice> {device});
                }

                device.StartMonitoring();
            });
        }

        /// <summary>
        /// Unregisters all devices
        /// </summary>
        public static void UnregisterDevices()
        {
            foreach (var devices in RegisteredDevices.Values)
            {
                devices.ForEach(d => d.Dispose());
            }

            RegisteredDevices.Clear();
        }

        /// <summary>
        /// Unregisters all devices of given type
        /// </summary>
        public static void UnregisterDevices<T>() where T : EvDevDevice
        {
            if (!RegisteredDevices.TryGetValue(typeof(T), out var devices))
            {
                // No devices of given type registered
                return;
            }

            devices.ForEach(d => d.Dispose());

            RegisteredDevices.Remove(typeof(T));
        }

        /// <summary>
        /// Unregisters given device
        /// </summary>
        public static void UnregisterDevice(EvDevDevice device)
        {
            device.Dispose();

            if (RegisteredDevices.TryGetValue(device.GetType(), out var devices))
            {
                devices.Remove(device);
            }
        }

        /// <summary>
        /// Get all registered devices
        /// </summary>
        public static EvDevDevice[] GetRegisteredDevices()
        {
            return RegisteredDevices.Values
                .SelectMany(x => x)
                .ToArray();
        }

        /// <summary>
        /// Gets all registered devices of given type
        /// </summary>
        /// <returns>
        /// Array of registered devices of given type. If there are none, return empty array
        /// </returns>>
        public static T[] GetRegisteredDevices<T>() where T : EvDevDevice
        {
            return RegisteredDevices.TryGetValue(typeof(T), out var devices)
                ? devices.Cast<T>().ToArray()
                : Array.Empty<T>();
        }

        private static EvDevDevice CreateDeviceOfType(EvDevDeviceData deviceData)
        {
            EvDevDevice device = null;

            switch (deviceData.GuessedDeviceType)
            {
                case EvDevGuessedDeviceType.Keyboard:
                    device = new EvDevKeyboardDevice(deviceData);
                    break;
                case EvDevGuessedDeviceType.Mouse:
                    device = new EvDevMouseDevice(deviceData);
                    break;
                default:
                    device = new EvDevDevice(deviceData);
                    break;
            }

            return device;
        }
    }
}