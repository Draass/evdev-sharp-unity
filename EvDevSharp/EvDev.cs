using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EvDevSharp.InteropStructs;
using EvDevSharp.Wrappers.Keyboard;
using EvDevSharp.Wrappers.Mouse;

namespace EvDevSharp
{
    /// <summary>
    /// Main class for handling evdev devices lifecycle
    /// </summary>
    public static unsafe class EvDev
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
                EvDevDeviceData deviceData = GetDeviceData(path);

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
                EvDevDeviceData deviceData = GetDeviceData(path);

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

        private static EvDevDeviceData GetDeviceData(string path)
        {
            using var eventFile = File.OpenRead(path);
            var fd = eventFile.SafeFileHandle.DangerousGetHandle();

            var bitCount = (int) EvDevKeyCode.KEY_MAX;
            var bits = new byte[bitCount / 8 + 1];

            EvDevGuessedDeviceType guessedDeviceType = EvDevGuessedDeviceType.Unknown;
            var deviceName = GetDeviceName();

            var version = GetDeviceDriverVersion(path, fd);

            var rawEventCodes = GetDeviceRawEventCodes();
            var absoluteAxises = GetDeviceAbsoluteAxises();
            var properties = GetDeviceProperties();
            var relativeAxises = GetDeviceRelativeAxises();
            var keyCodes = GetDeviceKeyCodes();

            if (GuessDeviceTypeByName(out var guessDeviceType))
                guessedDeviceType = guessDeviceType;
            else if (GuessDeviceTypeByKeys(out var deviceType))
                guessedDeviceType = deviceType;
            else
                guessedDeviceType = EvDevGuessedDeviceType.Unknown;

            return new EvDevDeviceData
            {
                Name = deviceName,
                GuessedDeviceType = guessedDeviceType,
                DriverVersion = version,
                Keys = keyCodes,
                RelativeAxises = relativeAxises,
                AbsoluteAxises = absoluteAxises,
                DevicePath = path,
                Properties = properties,
                RawEventCodes = rawEventCodes
            };

            #region LocalFunc

            string? GetDeviceName()
            {
                var str = stackalloc byte[256];

#if NETSTANDARD || NETFRAMEWORK
                ulong EVIOCGNAME_LONG = IoCtlRequest.EVIOCGNAME(256);

                if (LinuxNativeMethods.ioctl(fd, EVIOCGNAME_LONG, str) == -1)
                    throw new Win32Exception($"Unable to get evdev name for {path}");
#elif NET6_0
                if (LinuxNativeMethods.ioctl(fd, new CULong(IoCtlRequest.EVIOCGNAME(256)), str) == -1)
                    throw new Win32Exception($"Unable to get evdev name for {path}");
#endif


                var name = Marshal.PtrToStringAnsi(new IntPtr(str));
                return name;
            }

            List<EvDevRelativeAxisCode> GetDeviceRelativeAxises()
            {
                List<EvDevRelativeAxisCode>? evDevRelativeAxisCodes = new List<EvDevRelativeAxisCode>();

                if (rawEventCodes.TryGetValue(EvDevEventType.EV_REL, out var rel))
                    evDevRelativeAxisCodes = rel.Cast<EvDevRelativeAxisCode>().ToList();
                return evDevRelativeAxisCodes;
            }

            List<EvDevKeyCode> GetDeviceKeyCodes()
            {
                List<EvDevKeyCode>? evDevKeyCodes = new List<EvDevKeyCode>();

                if (rawEventCodes.TryGetValue(EvDevEventType.EV_KEY, out var keys))
                    evDevKeyCodes = keys.Cast<EvDevKeyCode>().ToList();
                return evDevKeyCodes;
            }

            List<EvDevProperty> GetDeviceProperties()
            {
                List<EvDevProperty> evDevProperties = new List<EvDevProperty>();
#if NETSTANDARD || NETFRAMEWORK
                ulong EVIOCGPROP_LONG = IoCtlRequest.EVIOCGPROP((int) EvDevProperty.INPUT_PROP_CNT);
                LinuxNativeMethods.ioctl(fd, EVIOCGPROP_LONG, bits);
#elif NET6_0
                LinuxNativeMethods.ioctl(fd, new CULong(IoCtlRequest.EVIOCGPROP((int) EvDevProperty.INPUT_PROP_CNT)),
                    bits);
#endif

                evDevProperties = DecodeBits(bits, (int) EvDevProperty.INPUT_PROP_CNT).Cast<EvDevProperty>().ToList();
                return evDevProperties;
            }

            Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo> GetDeviceAbsoluteAxises()
            {
                Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo>? evDevAbsAxisInfos = new();
                if (rawEventCodes.TryGetValue(EvDevEventType.EV_ABS, out var abs))
                {
                    evDevAbsAxisInfos = abs.ToDictionary(
                        x => (EvDevAbsoluteAxisCode) x,
                        x =>
                        {
                            var absInfo = default(EvDevAbsAxisInfo);

#if NETSTANDARD || NETFRAMEWORK
                            ulong EVIOCGABS_LONG = IoCtlRequest.EVIOCGABS(x);
                            LinuxNativeMethods.ioctl(fd, EVIOCGABS_LONG, &absInfo);
#elif NET6_0
                            LinuxNativeMethods.ioctl(fd, new CULong(IoCtlRequest.EVIOCGABS(x)), &absInfo);
#endif
                            return absInfo;
                        });
                }

                return evDevAbsAxisInfos;
            }

            Dictionary<EvDevEventType, List<int>> GetDeviceRawEventCodes()
            {
                Dictionary<EvDevEventType, List<int>> RawEventCodes = new();

                var supportedEvents = DecodeBits(bits).Cast<EvDevEventType>().ToList();
                foreach (var evType in supportedEvents)
                {
                    if (evType == EvDevEventType.EV_SYN)
                        continue;

                    Array.Clear(bits, 0, bits.Length);

#if NETSTANDARD || NETFRAMEWORK
                    ulong EVIOCGBIT_EVENT_LONG = IoCtlRequest.EVIOCGBIT(evType, bitCount);

                    LinuxNativeMethods.ioctl(fd, EVIOCGBIT_EVENT_LONG, bits);
#elif NET6_0
                    LinuxNativeMethods.ioctl(fd, new CULong(IoCtlRequest.EVIOCGBIT(evType, bitCount)), bits);
#endif

                    RawEventCodes[evType] = DecodeBits(bits);
                }

                return RawEventCodes;
            }

            bool GuessDeviceTypeByName(out EvDevGuessedDeviceType evDevGuessedDeviceType)
            {
                evDevGuessedDeviceType = EvDevGuessedDeviceType.Unknown;

                if (deviceName == null)
                    return false;

                // Often device name says what it is
                var isAbsolutePointingDevice = absoluteAxises?.ContainsKey(EvDevAbsoluteAxisCode.ABS_X) == true;

                var n = deviceName.ToLowerInvariant();
                if (n.Contains(("mouse")))
                {
                    evDevGuessedDeviceType = EvDevGuessedDeviceType.Mouse;
                    return true;
                }

                if (n.Contains("touchscreen")
                    && isAbsolutePointingDevice
                    && keyCodes?.Contains(EvDevKeyCode.BTN_TOUCH) == true)
                {
                    evDevGuessedDeviceType = EvDevGuessedDeviceType.TouchScreen;
                    return true;
                }

                if (n.Contains("tablet")
                    && isAbsolutePointingDevice
                    && keyCodes?.Contains(EvDevKeyCode.BTN_LEFT) == true)
                {
                    evDevGuessedDeviceType = EvDevGuessedDeviceType.Tablet;
                    return true;
                }

                if (n.Contains("touchpad")
                    && isAbsolutePointingDevice
                    && keyCodes?.Contains(EvDevKeyCode.BTN_LEFT) == true)
                {
                    evDevGuessedDeviceType = EvDevGuessedDeviceType.TouchPad;
                    return true;
                }

                if (n.Contains("keyboard")
                    && keyCodes != null)
                {
                    evDevGuessedDeviceType = EvDevGuessedDeviceType.Keyboard;
                    return true;
                }

                if (n.Contains("gamepad") || n.Contains("joystick")
                    && keyCodes != null)
                {
                    evDevGuessedDeviceType = EvDevGuessedDeviceType.GamePad;
                    return true;
                }

                return false;
            }

            bool GuessDeviceTypeByKeys(out EvDevGuessedDeviceType deviceType)
            {
                deviceType = EvDevGuessedDeviceType.Unknown;

                if (keyCodes?.Contains(EvDevKeyCode.BTN_TOUCH) == true
                    && properties.Contains(EvDevProperty.INPUT_PROP_DIRECT))
                {
                    deviceType = EvDevGuessedDeviceType.TouchScreen;
                    return true;
                }

                if (keyCodes?.Contains(EvDevKeyCode.BTN_SOUTH) == true)
                {
                    deviceType = EvDevGuessedDeviceType.GamePad;
                    return true;
                }

                if (keyCodes?.Contains(EvDevKeyCode.BTN_LEFT) == true &&
                    keyCodes?.Contains(EvDevKeyCode.BTN_RIGHT) == true)
                {
                    if (absoluteAxises != null)
                    {
                        if (absoluteAxises?.ContainsKey(EvDevAbsoluteAxisCode.ABS_X) == true)
                        {
                            if (properties.Contains(EvDevProperty.INPUT_PROP_DIRECT))
                            {
                                deviceType = EvDevGuessedDeviceType.Tablet;
                                return true;
                            }

                            {
                                deviceType = EvDevGuessedDeviceType.TouchPad;
                                return true;
                            }
                        }
                    }

                    if (relativeAxises?.Contains(EvDevRelativeAxisCode.REL_X) == true &&
                        relativeAxises.Contains(EvDevRelativeAxisCode.REL_Y))
                    {
                        deviceType = EvDevGuessedDeviceType.Mouse;
                        return true;
                    }
                }

                if (keyCodes != null)
                {
                    deviceType = EvDevGuessedDeviceType.Keyboard;
                    return true;
                }

                return false;
            }

            Version GetDeviceDriverVersion(string path, IntPtr fd)
            {
                int version = 0;

#if NETSTANDARD || NETFRAMEWORK
                ulong EVUIOCGVERSION_LONG = 2147763457;

                if (LinuxNativeMethods.ioctl(fd, EVUIOCGVERSION_LONG, &version) == -1)
                    throw new Win32Exception($"Unable to get evdev driver version for {path}");
#elif NET6_0
                if (LinuxNativeMethods.ioctl(fd, new CULong(IoCtlRequest.EVIOCGVERSION), &version) == -1)
                    throw new Win32Exception($"Unable to get evdev driver version for {path}");
#endif


                return new Version(version >> 16, (version >> 8) & 0xff, version & 0xff);
            }

            List<int> DecodeBits(byte[] arr, int? max = null)
            {
                var rv = new List<int>();
                max ??= arr.Length * 8;
                for (int idx = 0; idx < max; idx++)
                {
                    var b = arr[idx / 8];
                    var shift = idx % 8;
                    var v = (b >> shift) & 1;
                    if (v != 0)
                        rv.Add(idx);
                }

                return rv;
            }

            #endregion
        }
    }
}