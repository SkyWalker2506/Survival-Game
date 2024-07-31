using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmRecoilStockBehaviour : FirearmAttachmentBehaviour, IFirearmRecoilStock
    {
        [SerializeField, Range(0f, 1f), BeginGroup("Settings")]
        private float _hipfireAccuracyKick = 0.075f;

        [SerializeField, Range(0f, 1f)]
        private float _hipfireAccuracyRecover = 0.2f;

        [SerializeField, Range(0f, 1f)]
        private float _aimAccuracyKick = 0.05f;

        [SerializeField, Range(0f, 1f)]
        private float _aimAccuracyRecover = 0.275f;

        [SerializeField, Range(0f, 1f)]
        private float _recoilHeatRecover = 0.35f;
        
        [SerializeField, Range(0f, 1f), EndGroup]
        private float _recoilHeatRecoverDelay = 0.15f;
        
        
        public float RecoilHeatRecover => _recoilHeatRecover;
        public float RecoilHeatRecoverDelay => _recoilHeatRecoverDelay;
        public float HipfireAccuracyKick => _hipfireAccuracyKick;
        public float HipfireAccuracyRecover => _hipfireAccuracyRecover;
        public float AimAccuracyKick => _aimAccuracyKick;
        public float AimAccuracyRecover => _aimAccuracyRecover;
        protected float RecoilMultiplier { get; private set; } = 1f;

        public abstract void DoRecoil(bool isAiming, float heatValue, float triggerValue);
        public virtual void SetRecoilMultiplier(float multiplier) => RecoilMultiplier = multiplier;

        protected virtual void OnEnable()
        {
            if (Firearm != null)
                Firearm.RecoilStock = this;
        }
    }
}