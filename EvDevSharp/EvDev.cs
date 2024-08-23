using System;
using System.Collections.Generic;

namespace EvDevSharp
{
    /// <summary>
    /// 
    /// </summary>
    public static class EvDev
    {
        private static Dictionary<Type, List<EvDevDevice>> RegisteredDevices = new();
        
        /// <summary>
        /// Registers all devices
        /// </summary>
        public static void RegisterDevices()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Registers all devices of given type
        /// </summary>
        public static void RegisterDevices<T>() where T : EvDevDevice
        {
            throw new System.NotImplementedException();
        }
        
        /// <summary>
        /// Unregisters all devices
        /// </summary>
        public static void UnregisterDevices()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Unregisters all devices of given type
        /// </summary>
        public static void UnregisterDevices<T>() where T : EvDevDevice
        {
            throw new System.NotImplementedException();
        }
        
        /// <summary>
        /// Unregisters given device
        /// </summary>
        public static void UnregisterDevice(EvDevDevice device)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Get all registered devices
        /// </summary>
        public static EvDevDevice[] GetRegisteredDevices()
        {
            throw new System.NotImplementedException();
        }
        
        /// <summary>
        /// Gets all registered devices of given type
        /// </summary>
        public static T[] GetRegisteredDevices<T>() where T : EvDevDevice
        {
            throw new System.NotImplementedException();
        }
    }
}