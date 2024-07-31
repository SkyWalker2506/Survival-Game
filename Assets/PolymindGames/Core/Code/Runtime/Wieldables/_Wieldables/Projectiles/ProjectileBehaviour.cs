using PolymindGames.WieldableSystem;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    public abstract class ProjectileBehaviour : MonoBehaviour, IProjectile
    {
        public abstract void Launch(ICharacter character, Vector3 origin, Vector3 velocity, IFirearmProjectileEffect effect,
            float gravity = 9.81f, bool instantStart = false, UnityAction hitCallback = null);
        
#if UNITY_EDITOR
        protected virtual void Reset()
        {
            gameObject.layer = LayerConstants.EFFECT;
        }
#endif
    }
}