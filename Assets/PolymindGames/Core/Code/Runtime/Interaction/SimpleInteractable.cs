using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    [DisallowMultipleComponent]
    public sealed class SimpleInteractable : MonoBehaviour, IHoverableInteractable
    {
        [SerializeField, Range(0f, 10f), BeginGroup]
        [Tooltip("How time it takes to interact with this object. (e.g. for how many seconds should the Player hold the interact button).")]
        private float _holdDuration;

        [SerializeField, EndGroup]
        private MaterialEffect _materialEffect;

        private string _interactDescription;
        private string _interactTitle;


        public string Title
        {
            get => _interactTitle;
            set
            {
                if (ReferenceEquals(_interactTitle, value))
                    return;
                
                _interactTitle = value;
                DescriptionChanged?.Invoke();
            }
        }

        public string Description
        {
            get => _interactDescription;
            set
            {
                if (ReferenceEquals(_interactDescription, value))
                    return;
                
                _interactDescription = value;
                DescriptionChanged?.Invoke();
            }
        }

        public bool InteractionEnabled => enabled;
        public float HoldDuration => _holdDuration;

        public event HoverEventHandler HoverStarted;
        public event HoverEventHandler HoverEnded;
        public event InteractEventHandler Interacted;
        public event UnityAction DescriptionChanged;

        /// <summary>
        /// Called when a character interacts with this object.
        /// </summary>
        public void OnInteract(ICharacter character)
        {
            Interacted?.Invoke(this, character);
        }
        
        /// <summary>
        /// Called when a character starts looking at this object.
        /// </summary>
        public void OnHoverStart(ICharacter character)
        {
            if (_materialEffect != null)
                _materialEffect.EnableEffect();

            HoverStarted?.Invoke(this, character);
        }

        /// <summary>
        /// Called when a character stops looking at this object.
        /// </summary>
        public void OnHoverEnd(ICharacter character)
        {
            if (_materialEffect != null)
                _materialEffect.DisableEffect();

            HoverEnded?.Invoke(this, character);
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
