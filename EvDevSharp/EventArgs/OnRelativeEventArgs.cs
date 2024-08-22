using System;

namespace EvDevSharp
{
    public class OnRelativeEventArgs : EventArgs
    {
        public OnRelativeEventArgs(EvDevRelativeAxisCode axis, int value) =>
            (Axis, Value) = (axis, value);

        public EvDevRelativeAxisCode Axis { get; set; }
        public int Value { get; set; }
    }
}