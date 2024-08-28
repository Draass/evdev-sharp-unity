using System;
using EvDevSharp.Wrappers.Keyboard;
using EvDevSharp.Wrappers.Mouse;

namespace EvDevSharp
{
    internal static class EvDevExtensions
    {
        /// <summary>
        /// Converts a given <see cref="Type"/> to a <see cref="EvDevGuessedDeviceType"/>
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">deviceType must be derived from EvDevDevice</exception>
        public static EvDevGuessedDeviceType ToGuessedDeviceType(Type deviceType)
        {
            if (!typeof(EvDevDevice).IsAssignableFrom(deviceType))
            {
                throw new ArgumentException("deviceType must be derived from EvDevDevice", nameof(deviceType));
            }

            return deviceType switch
            {
                Type t when t == typeof(EvDevKeyboardDevice) => EvDevGuessedDeviceType.Keyboard,
                Type t when t == typeof(EvDevMouseDevice) => EvDevGuessedDeviceType.Mouse,
                _ => EvDevGuessedDeviceType.Unknown
            };
        }
    }
}