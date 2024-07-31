using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames
{
    [DisallowMultipleComponent]
    public sealed class TriggerInteractable : MonoBehaviour, IHoverableInteractable
    {
        [SerializeField, Range(0f, 10f), BeginGroup("Settings"), EndGroup]
        [Tooltip("How long should the player sit in the trigger of this object (in seconds) to interact with it.")]
        private float _holdDuration; 
        
        [SerializeField, BeginGroup("Description")]
        [Tooltip("Interactable text (could be used as a name), shows up in the UI when looking at this object.")]
        private string _interactTitle;

        [SerializeField, Multiline, EndGroup]
        [Tooltip("Interactable description, shows up in the UI when looking at this object.")]
        private string _interactDescription;
        
        [SerializeField, BeginGroup("Effects"), EndGroup]
        private MaterialEffect _materialEffect;
        

        public bool InteractionEnabled => false;
        public float HoldDuration => _holdDuration;
        
        public string Title
        {
            get => _interactTitle;
            set
            {
                _interactTitle = value;
                DescriptionChanged?.Invoke();
            }
        }

        public string Description
        {
            get => _interactDescription;
            set
            {
                _interactDescription = value;
                DescriptionChanged?.Invoke();
            }
        }
        
        public event InteractEventHandler Interacted;
        public event HoverEventHandler HoverStarted;
        public event HoverEventHandler HoverEnded;
        public event UnityAction DescriptionChanged;

        public void OnInteract(ICharacter character) => Interacted?.Invoke(this, character);
        
        public void OnHoverStart(ICharacter character)
        {
            HoverStarted?.Invoke(this, character);
            if (_materialEffect != null)
                _materialEffect.EnableEffect();
        }

        public void OnHoverEnd(ICharacter character)
        {
            HoverEnded?.Invoke(this, character);
            if (_materialEffect != null)
                _materialEffect.DisableEffect();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerConstants.CHARACTER && other.TryGetComponent(out ICharacter character))
                OnInteract(character);
        }
        
#if UNITY_EDITOR
        private void Reset()
        {
            gameObject.layer = LayerConstants.INTERACTABLE;
            if (_materialEffect == null && !gameObject.TryGetComponentInHierarchy(out _materialEffect))
                _materialEffect = gameObject.AddComponent<MaterialEffect>();
        }
#endif
    }
}
