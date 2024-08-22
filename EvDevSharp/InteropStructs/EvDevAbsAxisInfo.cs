using System.Runtime.InteropServices;

namespace EvDevSharp.InteropStructs
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct EvDevAbsAxisInfo
    {
        public int Value;
        public int Min;
        public int Max;
        public int Fuzz;
        public int Flat;
        public int Resolution;
    }
}