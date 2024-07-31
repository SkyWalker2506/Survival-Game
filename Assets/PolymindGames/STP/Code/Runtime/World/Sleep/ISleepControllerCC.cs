using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WorldManagement
{
    /// <summary>
    /// Controls character sleep behavior.
    /// </summary>
    public interface ISleepControllerCC : ICharacterComponent
    {
        /// <summary> Gets a value indicating whether the character is currently sleeping. </summary>
        bool SleepActive { get; }

        /// <summary> Gets the last sleep position for respawning purposes. </summary>
        Vector3 LastSleepPosition { get; }

        /// <summary> Gets the last sleep rotation for respawning purposes. </summary>
        Quaternion LastSleepRotation { get; }

        /// <summary> Event triggered when sleep starts. </summary>
        event UnityAction<int> SleepStart;

        /// <summary> Event triggered when sleep ends. </summary>
        event UnityAction<int> SleepEnd;

        /// <summary> Tries to initiate sleep at the specified sleeping place. </summary>
        /// <param name="sleepingPlace">The sleeping place to use for sleeping.</param>
        bool TrySleep(ISleepingPlace sleepingPlace);
    }

}