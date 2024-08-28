using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EvDevSharp.InteropStructs;

namespace EvDevSharp
{
    internal static unsafe class EvDevDeviceGuesser
    {
        internal static EvDevDeviceData GetDeviceData(string path)
        {
            using var eventFile = File.OpenRead(path);
            var fd = eventFile.SafeFileHandle.DangerousGetHandle();

            var bitCount = (int) EvDevKeyCode.KEY_MAX;
            var bits = new byte[bitCount / 8 + 1];

            EvDevGuessedDeviceType guessedDeviceType = EvDevGuessedDeviceType.Unknown;
            var deviceName = GetDeviceName(fd, path);

            var version = GetDeviceDriverVersion(path, fd);

            var rawEventCodes = GetDeviceRawEventCodes(bits, bitCount, fd);
            var absoluteAxises = GetDeviceAbsoluteAxises(rawEventCodes, fd);
            var properties = GetDeviceProperties(fd, bits);
            var relativeAxises = GetDeviceRelativeAxises(rawEventCodes);
            var keyCodes = GetDeviceKeyCodes(rawEventCodes);

            if (GuessDeviceTypeByName(out var guessDeviceType, deviceName, absoluteAxises, keyCodes))
                guessedDeviceType = guessDeviceType;
            else if (GuessDeviceTypeByKeys(out var deviceType, keyCodes, properties, absoluteAxises, relativeAxises))
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
        }

        private static Dictionary<EvDevEventType, List<int>> GetDeviceRawEventCodes(byte[]? bits, int bitCount, IntPtr fd)
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

        private static List<EvDevProperty> GetDeviceProperties(IntPtr fd, byte[]? bits)
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

        private static bool GuessDeviceTypeByKeys(out EvDevGuessedDeviceType deviceType, List<EvDevKeyCode>? keyCodes, List<EvDevProperty>? properties, Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo>? absoluteAxises, List<EvDevRelativeAxisCode>? relativeAxises)
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

        private static List<int> DecodeBits(byte[] arr, int? max = null)
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

        private static bool GuessDeviceTypeByName(out EvDevGuessedDeviceType evDevGuessedDeviceType, string? deviceName, Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo>? absoluteAxises, List<EvDevKeyCode>? keyCodes)
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

        private static Version GetDeviceDriverVersion(string path, IntPtr fd)
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

        private static Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo> GetDeviceAbsoluteAxises(Dictionary<EvDevEventType, List<int>>? rawEventCodes, IntPtr fd)
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

        private static List<EvDevKeyCode> GetDeviceKeyCodes(Dictionary<EvDevEventType, List<int>>? rawEventCodes)
        {
            List<EvDevKeyCode>? evDevKeyCodes = new List<EvDevKeyCode>();

            if (rawEventCodes.TryGetValue(EvDevEventType.EV_KEY, out var keys))
                evDevKeyCodes = keys.Cast<EvDevKeyCode>().ToList();
            return evDevKeyCodes;
        }

        private static List<EvDevRelativeAxisCode> GetDeviceRelativeAxises(Dictionary<EvDevEventType, List<int>>? rawEventCodes)
        {
            List<EvDevRelativeAxisCode>? evDevRelativeAxisCodes = new List<EvDevRelativeAxisCode>();

            if (rawEventCodes.TryGetValue(EvDevEventType.EV_REL, out var rel))
                evDevRelativeAxisCodes = rel.Cast<EvDevRelativeAxisCode>().ToList();
            return evDevRelativeAxisCodes;
        }

        private static string? GetDeviceName(IntPtr fd, string path)
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
    }
}