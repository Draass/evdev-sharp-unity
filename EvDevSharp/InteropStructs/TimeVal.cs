using System.Runtime.InteropServices;

namespace EvDevSharp.InteropStructs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TimeVal
    {
        public nint tv_sec;
        public nint tv_usec;
    }
}