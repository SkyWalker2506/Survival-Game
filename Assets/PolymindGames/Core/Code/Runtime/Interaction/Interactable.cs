using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames
{
    [DisallowMultipleComponent]
    public sealed class Interactable : MonoBehaviour, IHoverableInteractable
    {
        [SerializeField, Range(0f, 10f), BeginGroup("Settings"), EndGroup]
        [Tooltip("For how many seconds should the Player hold the interact button to interact with this object.")]
        private float _holdDuration;

        [SerializeField, BeginGroup("Description")]
        [Tooltip("Interactable text (could be used as a name), shows up in the UI when looking at this object.")]
        private string _interactTitle;

        [SerializeField, Multiline, EndGroup]
        [Tooltip("Interactable description, shows up in the UI when looking at this object.")]
        private string _interactDescription;

        [SerializeField, BeginGroup("Effects"), EndGroup]
        private MaterialEffect _materialEffect;
        

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

        public bool InteractionEnabled => enabled;
        public float HoldDuration => _holdDuration;

        public event InteractEventHandler Interacted;
        public event HoverEventHandler HoverStarted;
        public event HoverEventHandler HoverEnded;
        public event UnityAction DescriptionChanged;

        public void OnInteract(ICharacter character)
        {
            Interacted?.Invoke(this, character);
        }
        
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