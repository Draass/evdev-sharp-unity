using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EvDevSharp.InteropStructs;
using static EvDevSharp.IoCtlRequest;

namespace EvDevSharp
{
    public unsafe class LinuxNativeMethods
    {
#if NETSTANDARD || NETFRAMEWORK
        [DllImport("libc", SetLastError = true)]
        public static extern int ioctl(IntPtr fd, ulong request, void* data);

        [DllImport("libc", SetLastError = true)]
        public static extern int ioctl(IntPtr fd, ulong request, [Out] byte[] data);
#elif NET6_0
        [DllImport("libc", SetLastError = true)]
        public static extern int ioctl(IntPtr fd, CULong request, void* data);

        [DllImport("libc", SetLastError = true)]
        public static extern int ioctl(IntPtr fd, CULong request, [Out] byte[] data);
#endif
    }
    
    public unsafe partial class EvDevDevice : IDisposable
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
            
#if NETSTANDARD || NETFRAMEWORK
            ulong EVUIOCGVERSION_LONG = 2147763457;
            if (LinuxNativeMethods.ioctl(fd, EVUIOCGVERSION_LONG, &version) == -1)
                throw new Win32Exception($"Unable to get evdev driver version for {path}");
#elif NET6_0
            if (LinuxNativeMethods.ioctl(fd, new CULong(EVIOCGVERSION), &version) == -1)
                throw new Win32Exception($"Unable to get evdev driver version for {path}");
#endif
            

            DriverVersion = new Version(version >> 16, (version >> 8) & 0xff, version & 0xff);

            var id = stackalloc ushort[4];

#if NETSTANDARD || NETFRAMEWORK
            ulong EVIOCGID_LONG = EVIOCGID;
            if (LinuxNativeMethods.ioctl(fd, EVIOCGID_LONG, id) == -1)
                throw new Win32Exception($"Unable to get evdev id for {path}");
#elif NET6_0
            if (LinuxNativeMethods.ioctl(fd, new CULong(EVIOCGID), id) == -1)
                throw new Win32Exception($"Unable to get evdev id for {path}");
#endif

            Id = new EvDevDeviceId
            {
                Bus = id[0],
                Vendor = id[1],
                Product = id[2],
                Version = id[3],
            };

            var str = stackalloc byte[256];
            
#if NETSTANDARD || NETFRAMEWORK
            ulong EVIOCGNAME_LONG = EVIOCGNAME(256);
            
            if (LinuxNativeMethods.ioctl(fd, EVIOCGNAME_LONG, str) == -1)
                throw new Win32Exception($"Unable to get evdev name for {path}");
#elif NET6_0
            if (LinuxNativeMethods.ioctl(fd, new CULong(EVIOCGNAME(256)), str) == -1)
                throw new Win32Exception($"Unable to get evdev name for {path}");
#endif


            Name = Marshal.PtrToStringAnsi(new IntPtr(str));

            // if (ioctl(fd, new CULong(EVIOCGUNIQ(256)), str) == -1)
            //     throw new Win32Exception($"Unable to get evdev unique ID for {path}");

            //TODO Add UId
            //UniqueId = Marshal.PtrToStringAnsi(new IntPtr(str));

            var bitCount = (int) EvDevKeyCode.KEY_MAX;
            var bits = new byte[bitCount / 8 + 1];

#if NETSTANDARD || NETFRAMEWORK
            ulong EVIOCGBIT_SYN_LONG = EVIOCGBIT(EvDevEventType.EV_SYN, bitCount);
            LinuxNativeMethods.ioctl(fd, EVIOCGBIT_SYN_LONG, bits);
#elif NET6_0
            LinuxNativeMethods.ioctl(fd, new CULong(EVIOCGBIT(EvDevEventType.EV_SYN, bitCount)), bits);
#endif

            var supportedEvents = DecodeBits(bits).Cast<EvDevEventType>().ToList();
            foreach (var evType in supportedEvents)
            {
                if (evType == EvDevEventType.EV_SYN)
                    continue;
                
                Array.Clear(bits, 0, bits.Length);
                
#if NETSTANDARD || NETFRAMEWORK
                ulong EVIOCGBIT_EVENT_LONG = EVIOCGBIT(evType, bitCount);
                
                LinuxNativeMethods.ioctl(fd, EVIOCGBIT_EVENT_LONG, bits);
#elif NET6_0
                LinuxNativeMethods.ioctl(fd, new CULong(EVIOCGBIT(evType, bitCount)), bits);
#endif
                
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
                        
#if NETSTANDARD || NETFRAMEWORK
                        ulong EVIOCGABS_LONG = EVIOCGABS(x);
                        LinuxNativeMethods.ioctl(fd, EVIOCGABS_LONG, &absInfo);
#elif NET6_0
                        LinuxNativeMethods.ioctl(fd, new CULong(EVIOCGABS(x)), &absInfo);
#endif
                        return absInfo;
                    });
            }

            Array.Clear(bits, 0, bits.Length);
            
#if NETSTANDARD || NETFRAMEWORK
            ulong EVIOCGPROP_LONG = EVIOCGPROP((int) EvDevProperty.INPUT_PROP_CNT);
            LinuxNativeMethods.ioctl(fd, EVIOCGPROP_LONG, bits);
#elif NET6_0
            LinuxNativeMethods.ioctl(fd, new CULong(EVIOCGPROP((int) EvDevProperty.INPUT_PROP_CNT)), bits);
#endif
            
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