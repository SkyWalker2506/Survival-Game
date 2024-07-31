using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmProjectileEffectBehaviour : FirearmAttachmentBehaviour, IFirearmProjectileEffect
    {
        public abstract void DoHitEffect(ref RaycastHit hit, Vector3 hitDirection, float speed, float travelledDistance);
        public abstract void DoHitEffect(Collision collision, float travelledDistance);

        protected virtual void OnEnable()
        {
            if (Firearm != null)
                Firearm.ProjectileEffect = this;
        }
    }
}