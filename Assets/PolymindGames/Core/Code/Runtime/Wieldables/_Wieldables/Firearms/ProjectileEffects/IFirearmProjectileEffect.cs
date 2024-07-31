using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public interface IFirearmProjectileEffect
    {
        void DoHitEffect(ref RaycastHit hit, Vector3 hitDirection, float speed, float travelledDistance);
        void DoHitEffect(Collision collision, float travelledDistance);

        void Attach();
        void Detach();
    }

    public sealed class DefaultFirearmProjectileEffect : IFirearmProjectileEffect
    {
        public static readonly DefaultFirearmProjectileEffect Instance = new();

        private DefaultFirearmProjectileEffect() { }
        
        public void DoHitEffect(ref RaycastHit hit, Vector3 hitDirection, float speed, float travelledDistance) { }
        public void DoHitEffect(Collision collision, float travelledDistance) { }
        public void Attach() { }
        public void Detach() { }
    }
}