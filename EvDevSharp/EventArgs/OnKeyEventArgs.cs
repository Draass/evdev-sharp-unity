using System;

namespace EvDevSharp
{
    public class OnKeyEventArgs : EventArgs
    {
        public OnKeyEventArgs(EvDevKeyCode key, EvDevKeyValue value) =>
            (Key, Value) = (key, value);

        public EvDevKeyCode Key { get; set; }
        public EvDevKeyValue Value { get; set; }
    }
}