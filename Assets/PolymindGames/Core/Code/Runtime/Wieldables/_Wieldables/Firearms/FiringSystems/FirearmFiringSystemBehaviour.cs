using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmFiringSystemBehaviour : FirearmAttachmentBehaviour, IFirearmFiringSystem
    {
        [SerializeField, Range(0, 100), BeginGroup, EndGroup]
        private int _ammoPerShot = 1;
        
        
        public int AmmoPerShot => _ammoPerShot;

        public abstract void DryFire();
        public abstract void Shoot(float accuracy, IFirearmProjectileEffect projectileEffect, float value);

        protected virtual void OnEnable()
        {
            if (Firearm != null)
                Firearm.FiringSystem = this;
        }
    }
}