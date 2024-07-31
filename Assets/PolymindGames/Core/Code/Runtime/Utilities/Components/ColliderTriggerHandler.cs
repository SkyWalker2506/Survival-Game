using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames
{
    [RequireComponent(typeof(Collider))]
    public sealed class ColliderTriggerHandler : MonoBehaviour
    {
        [SerializeField, BeginGroup]
        private UnityEvent<Collider> _triggerEnter;

        [SerializeField, EndGroup]
        private UnityEvent<Collider> _triggerExit;

        
        public event UnityAction<Collider> TriggerEnter
        {
            add => _triggerEnter.AddListener(value);
            remove => _triggerEnter.RemoveListener(value);
        }

        public event UnityAction<Collider> TriggerExit
        {
            add => _triggerExit.AddListener(value);
            remove => _triggerExit.RemoveListener(value);
        }

        private void OnTriggerEnter(Collider other) => _triggerEnter.Invoke(other);
        private void OnTriggerExit(Collider other) => _triggerExit.Invoke(other);

#if UNITY_EDITOR
        private void Reset()
        {
            if (!TryGetComponent(out Collider col))
                col = gameObject.AddComponent<SphereCollider>();

            col.isTrigger = true;
        }
#endif
    }
}