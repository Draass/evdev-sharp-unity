using System;
using System.Collections.Generic;
using EvDevSharp.InteropStructs;

namespace EvDevSharp
{
    internal struct EvDevDeviceData
    {
        public string? Name { get; set; }
        public EvDevGuessedDeviceType GuessedDeviceType { get; set; }
        public Version DriverVersion { get; set; }
        public List<EvDevKeyCode>? Keys { get; set; }
        public List<EvDevRelativeAxisCode>? RelativeAxises { get; set; }
        public Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo>? AbsoluteAxises { get; set; }
        public string DevicePath { get; set; }
        public List<EvDevProperty> Properties { get; set; }
        public Dictionary<EvDevEventType, List<int>> RawEventCodes { get; set; }
        
        internal EvDevDeviceData(
            string? name, 
            EvDevGuessedDeviceType guessedDeviceType, 
            Version driverVersion, 
            List<EvDevKeyCode>? keys, 
            List<EvDevRelativeAxisCode>? relativeAxises, 
            Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo>? absoluteAxises, 
            string devicePath, List<EvDevProperty> properties, 
            Dictionary<EvDevEventType, List<int>> rawEventCodes)
        {
            Name = name;
            GuessedDeviceType = guessedDeviceType;
            DriverVersion = driverVersion;
            Keys = keys;
            RelativeAxises = relativeAxises;
            AbsoluteAxises = absoluteAxises;
            DevicePath = devicePath;
            Properties = properties;
            RawEventCodes = rawEventCodes;
        }
    }
}