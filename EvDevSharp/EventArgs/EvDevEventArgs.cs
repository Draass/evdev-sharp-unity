using System;

namespace EvDevSharp
{
    public class EvDevEventArgs : EventArgs
    {
        public EvDevEventArgs(int code, int value) =>
            (Code, Value) = (code, value);

        public int Code { get; set; }
        public int Value { get; set; }
    }
}