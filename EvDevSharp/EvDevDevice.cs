using System;
using System.Collections.Generic;
using EvDevSharp.InteropStructs;

namespace EvDevSharp
{
    public unsafe partial class EvDevDevice : IDisposable
    {
        // TODO: implement
        public EvDevDeviceId Id { get; }

        /// <summary>
        /// Get whether the device is currently monitoring for input events
        /// </summary>
        public bool IsMonitoring { get; private set; } = false;
        
        // TODO Not implemented
        //public string? UniqueId { get; }
        
        /// <summary>
        /// Get the driver version of the device
        /// </summary>
        public Version DriverVersion => _deviceData.DriverVersion;
       
        public string? Name => _deviceData.Name;
        
        /// <summary>
        /// Get the device path of the device. Should look like /dev/input/eventX
        /// </summary>
        public string DevicePath => _deviceData.DevicePath;
        
        public EvDevGuessedDeviceType GuessedDeviceType => _deviceData.GuessedDeviceType;
        
        public Dictionary<EvDevEventType, List<int>> RawEventCodes => _deviceData.RawEventCodes;
        
        public List<EvDevKeyCode>? Keys => _deviceData.Keys;
        
        public List<EvDevRelativeAxisCode>? RelativeAxises => _deviceData.RelativeAxises;
        
        public Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo>? AbsoluteAxises => _deviceData.AbsoluteAxises;
        
        public List<EvDevProperty> Properties => _deviceData.Properties;
        
        private EvDevDeviceData _deviceData;
        
        internal EvDevDevice(EvDevDeviceData data)
        {
            _deviceData = data;
        }
    }
}