using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public interface IFirearmTrigger
    {
        bool IsTriggerHeld { get; }

        event UnityAction<float> Shoot;


        void HoldTrigger();
        void ReleaseTrigger();
        void Attach();
        void Detach();
    }

    public sealed class DefaultFirearmTrigger : IFirearmTrigger
    {
        public static readonly DefaultFirearmTrigger Instance = new();

        private DefaultFirearmTrigger() { }
        
        public bool IsTriggerHeld => false;

        public event UnityAction<float> Shoot
        {
            add { }
            remove { }
        }

        public void HoldTrigger() { }
        public void ReleaseTrigger() { }
        public void Attach() { }
        public void Detach() { }
    }
}