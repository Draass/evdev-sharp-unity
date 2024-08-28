using System;
using System.Collections.Generic;
using EvDevSharp.InteropStructs;

namespace EvDevSharp.Wrappers.Mouse
{
    public partial class EvDevMouseDevice
    {
        enum ButtonPressState
        {
            Pressed,
            Released
        }

        private readonly Dictionary<EvDevKeyCode, ButtonPressState> _mouseButtonsStates = new();

        internal override void ProcessInput(InputEvent inputEvent)
        {
            base.ProcessInput(inputEvent);

            switch ((EvDevEventType) inputEvent.type)
            {
                case EvDevEventType.EV_KEY:

                    switch ((EvDevKeyCode) inputEvent.code)
                    {
                        case EvDevKeyCode.BTN_MOUSE:
                            if (_mouseButtonsStates[EvDevKeyCode.BTN_MOUSE] == ButtonPressState.Pressed)
                            {
                                OnLeftMouseButtonReleased?.Invoke(this,
                                    new EvDevEventArgs(inputEvent.code, inputEvent.value));
                                _mouseButtonsStates[EvDevKeyCode.BTN_MOUSE] = ButtonPressState.Released;
                            }
                            else
                            {
                                OnLeftMouseButtonPressed?.Invoke(this,
                                    new EvDevEventArgs(inputEvent.code, inputEvent.value));
                                _mouseButtonsStates[EvDevKeyCode.BTN_MOUSE] = ButtonPressState.Pressed;
                            }

                            break;
                        case EvDevKeyCode.BTN_RIGHT:
                            break;
                        case EvDevKeyCode.BTN_MIDDLE:
                            break;
                        case EvDevKeyCode.BTN_SIDE:
                            break;
                        case EvDevKeyCode.BTN_EXTRA:
                            break;
                        case EvDevKeyCode.BTN_FORWARD:
                            break;
                        case EvDevKeyCode.BTN_BACK:
                            break;
                        case EvDevKeyCode.BTN_TASK:
                            break;
                        case EvDevKeyCode.BTN_TOOL_MOUSE:
                            break;
                        case EvDevKeyCode.BTN_WHEEL:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
            }
        }

        ~EvDevMouseDevice() => StopMonitoring();
    }
}