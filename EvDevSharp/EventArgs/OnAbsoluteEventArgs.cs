using System;

namespace EvDevSharp
{
    public class OnAbsoluteEventArgs : EventArgs
    {
        public OnAbsoluteEventArgs(EvDevAbsoluteAxisCode axis, int value) =>
            (Axis, Value) = (axis, value);

        public EvDevAbsoluteAxisCode Axis { get; set; }
        public int Value { get; set; }
    }
}