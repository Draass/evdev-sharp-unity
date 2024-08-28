using System;
using System.Collections.Generic;
using EvDevSharp.InteropStructs;

namespace EvDevSharp
{
    public unsafe partial class EvDevDevice : IDisposable
    {
        public EvDevDeviceId Id { get; }
        
        // TODO Not implemented
        //public string? UniqueId { get; }
        
        public Version DriverVersion => _deviceData.DriverVersion;
       
        public string? Name => _deviceData.Name;
        
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