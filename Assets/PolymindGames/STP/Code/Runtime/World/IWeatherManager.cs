using UnityEngine;

namespace PolymindGames.WorldManagement
{
    /// <summary>
    /// Interface for managing game weather.
    /// </summary>
    public interface IWeatherManager
    {
        float GlobalTemperature { get; }
        float GetTemperatureAtPoint(Vector3 point);
        
        public const float DEFAULT_TEMPERATURE_IN_CELSIUS = 20f;
    }
}