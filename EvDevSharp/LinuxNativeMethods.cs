using System;
using System.Runtime.InteropServices;

namespace EvDevSharp
{
    public static unsafe class LinuxNativeMethods
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
}