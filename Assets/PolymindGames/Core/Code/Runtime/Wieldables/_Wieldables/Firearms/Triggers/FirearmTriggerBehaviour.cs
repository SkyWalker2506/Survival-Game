using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmTriggerBehaviour : FirearmAttachmentBehaviour, IFirearmTrigger
    {
        public bool IsTriggerHeld { get; protected set; }

        public event UnityAction<float> Shoot;


        public virtual void HoldTrigger()
        {
            if (!IsTriggerHeld)
                TapTrigger();

            IsTriggerHeld = true;
        }

        public virtual void ReleaseTrigger()
        {
            IsTriggerHeld = false;
        }

        protected virtual void TapTrigger() { }
        protected void RaiseShootEvent(float value) => Shoot?.Invoke(value);

        protected virtual void OnEnable()
        {
            if (Firearm != null)
                Firearm.Trigger = this;
        }
    }
}