using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EvDevSharp.InteropStructs;

namespace EvDevSharp.Wrappers.Mouse
{
    public partial class EvDevDeviceBase
    {
        private const string InputPath = "/dev/input/";
        private const string InputPathSearchPattern = "event*";
        
        public EvDevDeviceId Id { get; }
        public string? UniqueId { get; }
        public Version DriverVersion { get; }
        public string? Name { get; }
        public string DevicePath { get; }
        public EvDevGuessedDeviceType GuessedDeviceType { get; set; }
        public Dictionary<EvDevEventType, List<int>> RawEventCodes { get; } = new();
        public List<EvDevKeyCode>? Keys { get; set; }
        public List<EvDevRelativeAxisCode>? RelativeAxises { get; set; }
        public Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo>? AbsoluteAxises { get; set; }
        public List<EvDevProperty> Properties { get; set; }


        public EvDevDevice(string path)
        {
            using var eventFile = File.OpenRead(path);
            var fd = eventFile.SafeFileHandle.DangerousGetHandle();
            
            DevicePath = path;
            
            int version = 0;
            
            ulong EVUIOCGVERSION_LONG = 2147763457;
            if (LinuxNativeMethods.ioctl(fd, EVUIOCGVERSION_LONG, &version) == -1)
                throw new Win32Exception($"Unable to get evdev driver version for {path}");

            DriverVersion = new Version(version >> 16, (version >> 8) & 0xff, version & 0xff);

            var id = stackalloc ushort[4];

            ulong EVIOCGID_LONG = EVIOCGID;
            if (LinuxNativeMethods.ioctl(fd, EVIOCGID_LONG, id) == -1)
                throw new Win32Exception($"Unable to get evdev id for {path}");

            Id = new EvDevDeviceId
            {
                Bus = id[0],
                Vendor = id[1],
                Product = id[2],
                Version = id[3],
            };

            var str = stackalloc byte[256];
            ulong EVIOCGNAME_LONG = EVIOCGNAME(256);
            
            if (LinuxNativeMethods.ioctl(fd, EVIOCGNAME_LONG, str) == -1)
                throw new Win32Exception($"Unable to get evdev name for {path}");

            Name = Marshal.PtrToStringAnsi(new IntPtr(str));

            // if (ioctl(fd, new CULong(EVIOCGUNIQ(256)), str) == -1)
            //     throw new Win32Exception($"Unable to get evdev unique ID for {path}");

            //TODO Add UId
            //UniqueId = Marshal.PtrToStringAnsi(new IntPtr(str));

            var bitCount = (int) EvDevKeyCode.KEY_MAX;
            var bits = new byte[bitCount / 8 + 1];

            ulong EVIOCGBIT_SYN_LONG = EVIOCGBIT(EvDevEventType.EV_SYN, bitCount);
            LinuxNativeMethods.ioctl(fd, EVIOCGBIT_SYN_LONG, bits);
            var supportedEvents = DecodeBits(bits).Cast<EvDevEventType>().ToList();
            foreach (var evType in supportedEvents)
            {
                if (evType == EvDevEventType.EV_SYN)
                    continue;
                Array.Clear(bits, 0, bits.Length);
                ulong EVIOCGBIT_EVENT_LONG = EVIOCGBIT(evType, bitCount);
                LinuxNativeMethods.ioctl(fd, EVIOCGBIT_EVENT_LONG, bits);
                RawEventCodes[evType] = DecodeBits(bits);
            }

            if (RawEventCodes.TryGetValue(EvDevEventType.EV_KEY, out var keys))
                Keys = keys.Cast<EvDevKeyCode>().ToList();

            if (RawEventCodes.TryGetValue(EvDevEventType.EV_REL, out var rel))
                RelativeAxises = rel.Cast<EvDevRelativeAxisCode>().ToList();

            if (RawEventCodes.TryGetValue(EvDevEventType.EV_ABS, out var abs))
            {
                AbsoluteAxises = abs.ToDictionary(
                    x => (EvDevAbsoluteAxisCode) x,
                    x =>
                    {
                        var absInfo = default(EvDevAbsAxisInfo);
                        ulong EVIOCGABS_LONG = EVIOCGABS(x);
                        LinuxNativeMethods.ioctl(fd, EVIOCGABS_LONG, &absInfo);
                        return absInfo;
                    });
            }

            Array.Clear(bits, 0, bits.Length);
            ulong EVIOCGPROP_LONG = EVIOCGPROP((int) EvDevProperty.INPUT_PROP_CNT);
            LinuxNativeMethods.ioctl(fd, EVIOCGPROP_LONG, bits);
            Properties = DecodeBits(bits, (int) EvDevProperty.INPUT_PROP_CNT).Cast<EvDevProperty>().ToList();

            GuessedDeviceType = GuessDeviceType();
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

        private EvDevGuessedDeviceType GuessDeviceType()
        {
            if (Name != null)
            {
                // Often device name says what it is
                var isAbsolutePointingDevice = AbsoluteAxises?.ContainsKey(EvDevAbsoluteAxisCode.ABS_X) == true;

                var n = Name.ToLowerInvariant();
                if (n.Contains("touchscreen")
                    && isAbsolutePointingDevice
                    && Keys?.Contains(EvDevKeyCode.BTN_TOUCH) == true)
                    return EvDevGuessedDeviceType.TouchScreen;

                if (n.Contains("tablet")
                    && isAbsolutePointingDevice
                    && Keys?.Contains(EvDevKeyCode.BTN_LEFT) == true)
                    return EvDevGuessedDeviceType.Tablet;

                if (n.Contains("touchpad")
                    && isAbsolutePointingDevice
                    && Keys?.Contains(EvDevKeyCode.BTN_LEFT) == true)
                    return EvDevGuessedDeviceType.TouchPad;

                if (n.Contains("keyboard")
                    && Keys != null)
                    return EvDevGuessedDeviceType.Keyboard;

                if (n.Contains("gamepad") || n.Contains("joystick")
                    && Keys != null)
                    return EvDevGuessedDeviceType.GamePad;
            }

            if (Keys?.Contains(EvDevKeyCode.BTN_TOUCH) == true
                && Properties.Contains(EvDevProperty.INPUT_PROP_DIRECT))
                return EvDevGuessedDeviceType.TouchScreen;

            if (Keys?.Contains(EvDevKeyCode.BTN_SOUTH) == true)
                return EvDevGuessedDeviceType.GamePad;

            if (Keys?.Contains(EvDevKeyCode.BTN_LEFT) == true && Keys?.Contains(EvDevKeyCode.BTN_RIGHT) == true)
            {
                if (AbsoluteAxises != null)
                {
                    if (AbsoluteAxises?.ContainsKey(EvDevAbsoluteAxisCode.ABS_X) == true)
                    {
                        if (Properties.Contains(EvDevProperty.INPUT_PROP_DIRECT))
                            return EvDevGuessedDeviceType.Tablet;
                        return EvDevGuessedDeviceType.TouchPad;
                    }
                }

                if (RelativeAxises?.Contains(EvDevRelativeAxisCode.REL_X) == true &&
                    RelativeAxises.Contains(EvDevRelativeAxisCode.REL_Y))
                    return EvDevGuessedDeviceType.Mouse;
            }

            if (Keys != null)
                return EvDevGuessedDeviceType.Keyboard;

            return EvDevGuessedDeviceType.Unknown;
        }
    }
    }
}