using System;
// ReSharper disable EventNeverSubscribedTo.Global
#pragma warning disable CS0067 // Event is never used

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

        public event Action<EvDevMouseDevice, EvDevEventArgs> OnSideMouseButtonPressed;
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnSideMouseButtonReleased;

        public event Action<EvDevMouseDevice, EvDevEventArgs> OnExtraMouseButtonPressed;
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnExtraMouseButtonReleased;

        public event Action<EvDevMouseDevice, EvDevEventArgs> OnForwardMouseButtonPressed;
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnForwardMouseButtonReleased;

        public event Action<EvDevMouseDevice, EvDevEventArgs> OnBackMouseButtonPressed;
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnBackMouseButtonReleased;

        public event Action<EvDevMouseDevice, EvDevEventArgs> OnTaskButtonPressed;
        public event Action<EvDevMouseDevice, EvDevEventArgs> OnTaskButtonReleased;

        public event Action<EvDevMouseDevice, EvDevEventArgs> OnToolMouseButton;

        public event Action<EvDevMouseDevice, EvDevEventArgs> OnWheelButton;
    }
}