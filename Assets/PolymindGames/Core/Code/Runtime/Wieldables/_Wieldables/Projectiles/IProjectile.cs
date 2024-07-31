using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public interface IProjectile : IMonoBehaviour
    {
        void Launch(ICharacter character, Vector3 origin, Vector3 velocity, IFirearmProjectileEffect effect, float gravity = 9.81f, bool instantStart = false, UnityAction hitCallback = null);
    }
}