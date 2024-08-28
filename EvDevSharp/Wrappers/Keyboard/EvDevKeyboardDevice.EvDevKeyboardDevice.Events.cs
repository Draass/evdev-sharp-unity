using System;

namespace EvDevSharp.Wrappers.Keyboard
{
    public partial class EvDevKeyboardDevice
    {
        public event Action<EvDevKeyboardDevice, EvDevEventArgs> OnKeyPressed;
        public event Action<EvDevKeyboardDevice, EvDevEventArgs> OnKeyReleased;
    }
}