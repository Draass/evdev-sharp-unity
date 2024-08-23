using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EvDevSharp.Wrappers.Mouse;

namespace EvDevSharp
{
    /// <summary>
    /// 
    /// </summary>
    public static class EvDev
    {
        private const string InputPath = "/dev/input/";
        private const string InputPathSearchPattern = "event*";
        
        private static Dictionary<Type, List<EvDevDevice>> RegisteredDevices = new();
        
        /// <summary>
        /// Enumerates all the Linux event files and generates a <c>EvDevDevice</c> object for each file.
        /// </summary>
        /// <exception cref="System.PlatformNotSupportedException"></exception>
        public static void RegisterDevices()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                throw new PlatformNotSupportedException();
            
            var devices = Directory.GetFiles(InputPath, InputPathSearchPattern)
                .AsParallel()
                .Select(path => new EvDevDevice(path));
            
            foreach (var device in devices)
            {
                if (RegisteredDevices.TryGetValue(device.GetType(), out var registeredDevices))
                {
                    if (registeredDevices.Exists(d => d.DevicePath == device.DevicePath))
                    {
                        // Device already registered
                        continue;
                    }
                    
                    registeredDevices.Add(device);
                }
                else
                {
                    RegisteredDevices.Add(device.GetType(), new List<EvDevDevice> { device });
                }
            }
        }

        /// <summary>
        /// Enumerates all the Linux event files and generates a <c>EvDevDevice</c> object for each file of given type.
        /// </summary>
        /// <exception cref="System.PlatformNotSupportedException"></exception>
        public static void RegisterDevices<T>() where T : EvDevDevice
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                throw new PlatformNotSupportedException();
            
            var devices = Directory.GetFiles(InputPath, InputPathSearchPattern)
                .AsParallel()
                .Select(path => new EvDevDevice(path));
            
            foreach (var device in devices)
            {
                if(device is not T)
                    continue;
                
                if (RegisteredDevices.TryGetValue(typeof(T), out var registeredDevices))
                {
                    if (registeredDevices.Exists(d => d.DevicePath == device.DevicePath))
                    {
                        // Device already registered
                        continue;
                    }
                    
                    registeredDevices.Add(device);
                }
                else
                {
                    RegisteredDevices.Add(typeof(T), new List<EvDevDevice> { device });
                }
            }
        }
        
        /// <summary>
        /// Unregisters all devices
        /// </summary>
        public static void UnregisterDevices()
        {
            foreach (var devices in RegisteredDevices.Values)
            {
                devices.ForEach(d => d.StopMonitoring());
            }
            
            RegisteredDevices.Clear();
        }

        /// <summary>
        /// Unregisters all devices of given type
        /// </summary>
        public static void UnregisterDevices<T>() where T : EvDevDevice
        {
            if(!RegisteredDevices.TryGetValue(typeof(T), out var devices))
            {
                // No devices of given type registered
                return;
            }
            
            devices.ForEach(d => d.StopMonitoring());
            
            RegisteredDevices.Remove(typeof(T));
        }
        
        /// <summary>
        /// Unregisters given device
        /// </summary>
        public static void UnregisterDevice(EvDevDevice device)
        {
            device.StopMonitoring();

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
    }
}