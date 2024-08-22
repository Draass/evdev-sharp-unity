namespace EvDevSharp
{
    internal static class IoCtlRequest
    {
        private const int _IOC_SIZEBITS = 14;
        private const int _IOC_DIRBITS = 2;
        private const int _IOC_NRBITS = 8;
        private const int _IOC_TYPEBITS = 8;

        private const int _IOC_NRMASK = ((1 << _IOC_NRBITS) - 1);
        private const int _IOC_TYPEMASK = ((1 << _IOC_TYPEBITS) - 1);
        private const int _IOC_SIZEMASK = ((1 << _IOC_SIZEBITS) - 1);
        private const int _IOC_DIRMASK = ((1 << _IOC_DIRBITS) - 1);

        private const int _IOC_NRSHIFT = 0;
        private const int _IOC_TYPESHIFT = (_IOC_NRSHIFT + _IOC_NRBITS);
        private const int _IOC_SIZESHIFT = (_IOC_TYPESHIFT + _IOC_TYPEBITS);
        private const int _IOC_DIRSHIFT = (_IOC_SIZESHIFT + _IOC_SIZEBITS);

        private const int _IOC_NONE = 0;
        private const int _IOC_WRITE = 1;
        private const int _IOC_READ = 2;


        private static uint _IOC(long dir, long type, long nr, long size) =>
            (uint) (((dir) << _IOC_DIRSHIFT) |
                    ((type) << _IOC_TYPESHIFT) |
                    ((nr) << _IOC_NRSHIFT) |
                    ((size) << _IOC_SIZESHIFT));

        private static uint _IO(long type, long nr) => _IOC(_IOC_NONE, (type), (nr), 0);
        private static uint _IOR(long type, long nr, long size) => _IOC(_IOC_READ, (type), (nr), size);
        private static uint _IOW(long type, long nr, long size) => _IOC(_IOC_WRITE, (type), (nr), size);

        public static uint EVIOCGNAME(long len) => _IOC(_IOC_READ, 'E', 0x06, len);
        public static uint EVIOCGPHYS(long len) => _IOC(_IOC_READ, 'E', 0x07, len);
        public static uint EVIOCGUNIQ(long len) => _IOC(_IOC_READ, 'E', 0x08, len);
        public static uint EVIOCGPROP(long len) => _IOC(_IOC_READ, 'E', 0x09, len);
        public static uint EVIOCGBIT(EvDevEventType ev, long len) => _IOC(_IOC_READ, 'E', 0x20 + ((long) ev), len);
        public static uint EVIOCGABS(int abs) => _IOR('E', 0x40 + (abs), 24);

        public const uint EVIOCGVERSION = 2147763457;
        public const uint EVIOCGID = 2148025602;
        public const uint EVIOCGREP = 2148025603;
        public const uint EVIOCSREP = 1074283779;
        public const uint EVIOCGKEYCODE = 2148025604;
        public const uint EVIOCGKEYCODE_V2 = 2150122756;
        public const uint EVIOCSKEYCODE = 1074283780;
        public const uint EVIOCSKEYCODE_V2 = 1076380932;
    }
}