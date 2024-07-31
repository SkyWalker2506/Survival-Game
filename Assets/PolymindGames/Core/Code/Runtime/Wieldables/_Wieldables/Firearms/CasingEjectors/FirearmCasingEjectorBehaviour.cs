using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmCasingEjectorBehaviour : FirearmAttachmentBehaviour, IFirearmCasingEjector
    {
        [SerializeField, Range(0f, 10f), BeginGroup("Settings")]
        private float _ejectDuration;

        [SerializeField, SpaceArea(0f, 5f), EndGroup]
        private bool _ejectAnimation;

        private float _ejectionTimer;
        
        
        public float EjectDuration => _ejectDuration;
        public bool IsEjecting => _ejectionTimer > Time.time;

        public virtual void Eject()
        {
            if (_ejectAnimation)
                Wieldable.Animation.SetTrigger(WieldableAnimationConstants.EJECT);

            _ejectionTimer = Time.time + _ejectDuration;
        }

        protected virtual void OnEnable()
        {
            if (Firearm != null)
                Firearm.CasingEjector = this;
        }
    }
}