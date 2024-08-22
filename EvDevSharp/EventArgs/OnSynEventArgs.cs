using System;

namespace EvDevSharp
{
    public class OnSynEventArgs : EventArgs
    {
        public OnSynEventArgs(EvDevSynCode code, int value) =>
            (Code, Value) = (code, value);

        public EvDevSynCode Code { get; set; }
        public int Value { get; set; }
    }
}