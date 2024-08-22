using System;

namespace EvDevSharp
{
    public sealed partial class EvDevDevice : IDisposable
    {
        public delegate void OnSynEventHandler(object sender, OnSynEventArgs e);

        ///<summary>This event corresponds to evdev EV_SYN event type.</summary>
        public event OnSynEventHandler? OnSynEvent;

        public delegate void OnKeyEventHandler(object sender, OnKeyEventArgs e);

        ///<summary>This event corresponds to evdev EV_KEY event type.</summary>
        public event OnKeyEventHandler? OnKeyEvent;

        public delegate void OnRelativeEventHandler(object sender, OnRelativeEventArgs e);

        ///<summary>This event corresponds to evdev EV_REL event type.</summary>
        public event OnRelativeEventHandler? OnRelativeEvent;

        public delegate void OnAbsoluteEventHandler(object sender, OnAbsoluteEventArgs e);

        ///<summary>This event corresponds to evdev EV_ABS event type.</summary>
        public event OnAbsoluteEventHandler? OnAbsoluteEvent;

        public delegate void OnMiscellaneousEventHandler(object sender, EvDevEventArgs e);

        ///<summary>This event corresponds to evdev EV_MSC event type.</summary>
        public event OnMiscellaneousEventHandler? OnMiscellaneousEvent;

        public delegate void OnSwitchEventHandler(object sender, EvDevEventArgs e);

        ///<summary>This event corresponds to evdev EV_SW event type.</summary>
        public event OnSwitchEventHandler? OnSwitchEvent;

        public delegate void OnLedEventHandler(object sender, EvDevEventArgs e);

        ///<summary>This event corresponds to evdev EV_LED event type.</summary>
        public event OnLedEventHandler? OnLedEvent;

        public delegate void OnSoundEventHandler(object sender, EvDevEventArgs e);

        ///<summary>This event corresponds to evdev EV_SND event type.</summary>
        public event OnSoundEventHandler? OnSoundEvent;

        public delegate void OnAutoRepeatEventHandler(object sender, EvDevEventArgs e);

        ///<summary>This event corresponds to evdev EV_REP event type.</summary>
        public event OnAutoRepeatEventHandler? OnAutoRepeatEvent;

        public delegate void OnForceFeedbackEventHandler(object sender, EvDevEventArgs e);

        ///<summary>This event corresponds to evdev EV_FF event type.</summary>
        public event OnForceFeedbackEventHandler? OnForceFeedbackEvent;

        public delegate void OnPowerEventHandler(object sender, EvDevEventArgs e);

        ///<summary>This event corresponds to evdev EV_PWR event type.</summary>
        public event OnPowerEventHandler? OnPowerEvent;

        public delegate void OnForceFeedbackStatusEventHandler(object sender, EvDevEventArgs e);

        ///<summary>This event corresponds to evdev EV_FF_STATUS event type.</summary>
        public event OnForceFeedbackStatusEventHandler? OnForceFeedbackStatusEvent;
    }
}