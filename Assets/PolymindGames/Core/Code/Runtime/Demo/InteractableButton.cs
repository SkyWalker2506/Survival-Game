using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.Demo
{
    [DisallowMultipleComponent]
    public sealed class InteractableButton : MonoBehaviour, IHoverableInteractable
    {
        [SerializeField, BeginGroup("Description")]
        [Tooltip("Interactable text (could be used as a name), shows up in the UI when looking at this object.")]
        private string _interactTitle;

        [SerializeField, Multiline, EndGroup]
        [Tooltip("Interactable description, shows up in the UI when looking at this object.")]
        private string _interactDescription;
        
        [SerializeField, Range(0f, 10f), BeginGroup("Settings")]
        [Tooltip("For how many seconds should the Player hold the interact button to interact with this object.")]
        private float _holdDuration;

        [SerializeField, Range(0f, 10f)]
        private float _interactCooldown = 0.5f;

        [SerializeField, Range(0, 100), EndGroup]
        private int _interactCountLimit;

        [SerializeField, BeginGroup("Effects")]
        private AudioDataSO _interactAudio;
        
        [SerializeField, EndGroup]
        private MaterialEffect _materialEffect;
        
        private int _pressedTimes;
        private float _pressTimer;
        

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

        /// <summary>
        /// Called when a character interacts with this object.
        /// </summary>
        public void OnInteract(ICharacter character)
        {
            bool hasPressLimit = _interactCountLimit > 0;
            if (Time.time < _pressTimer || hasPressLimit && _pressedTimes >= _interactCountLimit)
                return;

            _pressTimer = Time.time + _interactCooldown;
            _pressedTimes++;
            
            if (_interactAudio != null)
                AudioManager.Instance.PlayClipAtPoint(_interactAudio.Clip, transform.position, _interactAudio.Volume, _interactAudio.Pitch);
            
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

        private void Start() { } 

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