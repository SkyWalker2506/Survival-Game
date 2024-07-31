using UnityEngine;

namespace PolymindGames
{
    public interface ISleepingPlace : IMonoBehaviour
    {
        Vector3 SleepPosition { get; }
        Vector3 SleepRotation { get; }
    }
}