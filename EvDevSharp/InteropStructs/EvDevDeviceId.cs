namespace EvDevSharp.InteropStructs
{
    public struct EvDevDeviceId
    {
        public ushort Bus { get; set; }
        public ushort Vendor { get; set; }
        public ushort Product { get; set; }
        public ushort Version { get; set; }

        public override string ToString()
    {
        return $"Bus: 0x{Bus:x} Vendor: 0x{Vendor:x} Product: 0x{Product:x} Version: 0x{Version:x}";
    }
    }
}