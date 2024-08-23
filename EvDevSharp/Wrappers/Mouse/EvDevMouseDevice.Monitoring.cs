using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EvDevSharp.InteropStructs;

namespace EvDevSharp.Wrappers.Mouse
{
    public enum ButtonPressState
    {
        Pressed,
        Released
    }
    
    public partial class EvDevMouseDevice
    {
        protected Task? monitoringTask;
        protected CancellationTokenSource? cts;

        private Dictionary<EvDevKeyCode, ButtonPressState> mouseButtons = new();
        
        /// <summary>
        /// This method starts to read the device's event file on a separate thread and will raise events accordingly.
        /// </summary>
        public override void StartMonitoring()
        {
            if (cts is not null && !cts.IsCancellationRequested)
                return;

            cts = new();
            monitoringTask = Task.Run(Monitor);

            void Monitor()
            {
                InputEvent inputEvent;
                int size = Marshal.SizeOf(typeof(InputEvent));
                byte[] buffer = new byte[size];

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                using var eventFile = File.OpenRead(DevicePath);
                while (!cts.Token.IsCancellationRequested)
                {
                    eventFile.Read(buffer, 0, size);
                    inputEvent = (InputEvent) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(InputEvent))!;
                    switch ((EvDevEventType) inputEvent.type)
                    {
                        // case EvDevEventType.EV_SYN:
                        //     OnSynEvent?.Invoke(this,
                        //         new OnSynEventArgs((EvDevSynCode) inputEvent.code, inputEvent.value));
                        //     break;
                        case EvDevEventType.EV_KEY:

                            switch ((EvDevKeyCode) inputEvent.code)
                            {
                                // case EvDevKeyCode.BTN_1:
                                //     break;
                                // case EvDevKeyCode.BTN_2:
                                //     break;
                                // case EvDevKeyCode.BTN_3:
                                //     break;
                                // case EvDevKeyCode.BTN_4:
                                //     break;
                                // case EvDevKeyCode.BTN_5:
                                //     break;
                                // case EvDevKeyCode.BTN_6:
                                //     break;
                                // case EvDevKeyCode.BTN_7:
                                //     break;
                                // case EvDevKeyCode.BTN_8:
                                //     break;
                                // case EvDevKeyCode.BTN_9:
                                //     break;
                                case EvDevKeyCode.BTN_MOUSE:
                                    if (mouseButtons[EvDevKeyCode.BTN_MOUSE] == ButtonPressState.Pressed)
                                    {
                                        OnLeftMouseButtonReleased?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                                        mouseButtons[EvDevKeyCode.BTN_MOUSE] = ButtonPressState.Released;
                                    }
                                    else
                                    {
                                        OnLeftMouseButtonPressed?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                                        mouseButtons[EvDevKeyCode.BTN_MOUSE] = ButtonPressState.Pressed;
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
                            
                            // OnKeyEvent?.Invoke(this,
                            //     new OnKeyEventArgs((EvDevKeyCode) inputEvent.code, (EvDevKeyValue) inputEvent.value));
                            break;

                        // case EvDevEventType.EV_REL:
                        //     OnRelativeEvent?.Invoke(this,
                        //         new OnRelativeEventArgs((EvDevRelativeAxisCode) inputEvent.code, inputEvent.value));
                        //     break;
                        //
                        // case EvDevEventType.EV_ABS:
                        //     OnAbsoluteEvent?.Invoke(this,
                        //         new OnAbsoluteEventArgs((EvDevAbsoluteAxisCode) inputEvent.code, inputEvent.value));
                        //     break;
                        //
                        // case EvDevEventType.EV_MSC:
                        //     OnMiscellaneousEvent?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                        //     break;
                        //
                        // case EvDevEventType.EV_SW:
                        //     OnSwitchEvent?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                        //     break;
                        //
                        // case EvDevEventType.EV_LED:
                        //     OnLedEvent?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                        //     break;
                        //
                        // case EvDevEventType.EV_SND:
                        //     OnSoundEvent?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                        //     break;
                        //
                        // case EvDevEventType.EV_REP:
                        //     OnAutoRepeatEvent?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                        //     break;
                        //
                        // case EvDevEventType.EV_FF:
                        //     OnForceFeedbackEvent?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                        //     break;
                        //
                        // case EvDevEventType.EV_PWR:
                        //     OnPowerEvent?.Invoke(this, new EvDevEventArgs(inputEvent.code, inputEvent.value));
                        //     break;
                    }
                }

                handle.Free();
            }
        }

        /// <summary>
        /// This method cancels event file reading for this device.
        /// </summary>
        public void StopMonitoring()
        {
            cts?.Cancel();
            monitoringTask?.Wait();
        }

        public void Dispose() => StopMonitoring();

        ~EvDevMouseDevice() => StopMonitoring();
    }
}