using UnityEngine;

namespace PolymindGames.ResourceGathering
{
    public interface IGatherable : IMonoBehaviour
    {
        float Health { get; }
        bool IsAlive { get; }
        GatherableDefinition Definition { get; }

        event DamageReceivedDelegate DamageReceived;

        void ReceiveDamage(float damage, in DamageArgs args);
        void EnableCollision(bool enable);

        Vector3 GetGatherPosition();
    }
}