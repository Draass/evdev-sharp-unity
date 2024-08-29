using System;
using System.Collections.Generic;
using EvDevSharp.InteropStructs;
// ReSharper disable UnusedMember.Global

namespace EvDevSharp.Wrappers.Mouse
{
    public partial class EvDevMouseDevice
    {
        public enum ButtonPressState
        {
            Pressed,
            Released
        }

        private readonly Dictionary<EvDevKeyCode, ButtonPressState> _mouseButtonsStates = new()
        {
            {EvDevKeyCode.BTN_LEFT, ButtonPressState.Released}, // same as EvDevKeyCode.BTN_MOUSE
            {EvDevKeyCode.BTN_RIGHT, ButtonPressState.Released},
            {EvDevKeyCode.BTN_MIDDLE, ButtonPressState.Released},
            {EvDevKeyCode.BTN_SIDE, ButtonPressState.Released},
            {EvDevKeyCode.BTN_EXTRA, ButtonPressState.Released},
            {EvDevKeyCode.BTN_FORWARD, ButtonPressState.Released},
            {EvDevKeyCode.BTN_BACK, ButtonPressState.Released},
            {EvDevKeyCode.BTN_TASK, ButtonPressState.Released},
            {EvDevKeyCode.BTN_TOOL_MOUSE, ButtonPressState.Released},
            {EvDevKeyCode.BTN_WHEEL, ButtonPressState.Released}
        };
        
        /// <summary>
        /// Gets the current state of the given button
        /// </summary>
        /// <param name="keyCode">KeyCode of the desired button</param>
        /// <returns>ButtonPressState of the given button. If button is not presented in possible button, Released is returned</returns>
        public ButtonPressState GetButtonPressState(EvDevKeyCode keyCode)
        {
            if(!_mouseButtonsStates.TryGetValue(keyCode, out var state))
                return ButtonPressState.Released;
            
            return state;
        }

        internal override void ProcessInput(InputEvent inputEvent)
        {
            base.ProcessInput(inputEvent);

            if ((EvDevEventType) inputEvent.type != EvDevEventType.EV_KEY) 
                return;
            
            switch ((EvDevKeyCode) inputEvent.code)
            {
                case EvDevKeyCode.BTN_LEFT:
                    HandleButtonClick(EvDevKeyCode.BTN_LEFT, OnLeftMouseButtonPressed, OnLeftMouseButtonReleased,
                        inputEvent);
                    break;

                case EvDevKeyCode.BTN_RIGHT:
                    HandleButtonClick(EvDevKeyCode.BTN_RIGHT, OnRightMouseButtonPressed, OnRightMouseButtonReleased,
                        inputEvent);
                    break;

                case EvDevKeyCode.BTN_MIDDLE:
                    HandleButtonClick(EvDevKeyCode.BTN_MIDDLE, OnMiddleMouseButtonPressed, OnMiddleMouseButtonReleased,
                        inputEvent);
                    break;

                case EvDevKeyCode.BTN_SIDE:
                    HandleButtonClick(EvDevKeyCode.BTN_SIDE, OnSideMouseButtonPressed, OnSideMouseButtonReleased,
                        inputEvent);
                    break;

                case EvDevKeyCode.BTN_EXTRA:
                    HandleButtonClick(EvDevKeyCode.BTN_EXTRA, OnExtraMouseButtonPressed, OnExtraMouseButtonReleased,
                        inputEvent);
                    break;

                case EvDevKeyCode.BTN_FORWARD:
                    HandleButtonClick(EvDevKeyCode.BTN_FORWARD, OnForwardMouseButtonPressed,
                        OnForwardMouseButtonReleased, inputEvent);
                    break;

                case EvDevKeyCode.BTN_BACK:
                    HandleButtonClick(EvDevKeyCode.BTN_BACK, OnBackMouseButtonPressed, OnBackMouseButtonReleased,
                        inputEvent);
                    break;

                case EvDevKeyCode.BTN_TASK:
                    HandleButtonClick(EvDevKeyCode.BTN_TASK, OnTaskButtonPressed, OnTaskButtonReleased, inputEvent);
                    break;

                case EvDevKeyCode.BTN_TOOL_MOUSE:
                    OnToolMouseButton?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                    break;

                case EvDevKeyCode.BTN_WHEEL:
                    OnWheelButton?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                    break;

                default:
                    Console.WriteLine(
                        $"EvDevMouseDevice: Unknown button code: {inputEvent.code}. Please add handling for this code.");
                    break;
            }
        }

        private void HandleButtonClick(
            EvDevKeyCode keyCode, 
            Action<EvDevMouseDevice, EvDevEventArgs>? pressedEvent, 
            Action<EvDevMouseDevice, EvDevEventArgs>? releasedEvent, 
            InputEvent inputEvent)
        {
            if (_mouseButtonsStates[keyCode] == ButtonPressState.Pressed)
            {
                releasedEvent?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                _mouseButtonsStates[keyCode] = ButtonPressState.Released;
            }
            else
            {
                pressedEvent?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                _mouseButtonsStates[keyCode] = ButtonPressState.Pressed;
            }
        }


        ~EvDevMouseDevice() => StopMonitoring();
    }
}