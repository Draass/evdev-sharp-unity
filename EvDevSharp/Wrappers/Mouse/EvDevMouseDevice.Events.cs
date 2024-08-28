using System;

namespace EvDevSharp.Wrappers.Mouse
{
    public partial class EvDevMouseDevice
    {
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnLeftMouseButtonPressed;
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnLeftMouseButtonReleased;
        
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnRightMouseButtonPressed;
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnRightMouseButtonReleased;
        
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnMiddleMouseButtonPressed;
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnMiddleMouseButtonReleased;
    }
}