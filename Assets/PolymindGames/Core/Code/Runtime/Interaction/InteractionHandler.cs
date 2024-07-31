using System.Collections;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/player/modules-and-behaviours/interaction#interaction-handler-module")]
    public sealed class InteractionHandler : CharacterBehaviour, IInteractionHandlerCC
    {
        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The transform used in ray-casting.")]
        private Transform _view;

        [SerializeField, Range(0.01f, 25f), BeginGroup]
        [Tooltip("The max detection distance, anything out of range will be ignored.")]
        private float _distance = 2.5f; 

        [SerializeField, Range(0f, 25f), EndGroup]
        [Tooltip("If set to a value larger than 0, the detection method will be set to SphereCast instead of Raycast.")]
        private float _radius;

        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The trigger colliders interaction mode.")]
        private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide;

        [SerializeField, BeginGroup, EndGroup]
        private AudioDataSO _failedAudio;

        private IInteractable _interactable;
        private float _interactionProgress;
        private Transform _ignoredRoot;
        private RaycastHit _raycastHit;
        private IHoverable _hoverable;
        private bool _hasRaycastHit;


        public bool InteractionEnabled
        {
            get => enabled;
            set
            {
                if (enabled == value)
                    return;

                enabled = value;
                InteractionEnabledChanged?.Invoke(value);
            }
        }

        public IHoverable Hoverable
        {
            get => _hoverable;
            private set
            {
                if (_hoverable != value)
                {
                    _hoverable = value;
                    HoverableInViewChanged?.Invoke(value);
                }
            }
        }

        private float InteractProgress
        {
            get => _interactionProgress;
            set
            {
                _interactionProgress = value;
                InteractProgressChanged?.Invoke(_interactionProgress);
            }
        }

        public event UnityAction<IHoverable> HoverableInViewChanged;
        public event UnityAction<float> InteractProgressChanged;
        public event UnityAction<bool> InteractionEnabledChanged;
        

        #region Interaction
        public void StartInteraction()
        {
            if (_interactable != null)
                return;

            if (Hoverable is IInteractable { InteractionEnabled: true } interactable)
            {
                _interactable = interactable;
                
                if (_interactable.HoldDuration > 0.01f)
                    StartCoroutine(C_DelayedInteraction());
                else
                    _interactable?.OnInteract(Character);
            }
            else
                Character.AudioPlayer.PlaySafe(_failedAudio);
        }

        public void StopInteraction()
        {
            if (_interactable == null)
                return;

            StopAllCoroutines();

            InteractProgress = 0f;
            _interactable = null;
        }

        private IEnumerator C_DelayedInteraction()
        {
            float endTime = Time.time + _interactable.HoldDuration;

            while (true)
            {
                float time = Time.time;
                InteractProgress = 1 - (endTime - time) / _interactable.HoldDuration;

                if (time < endTime)
                    yield return null;
                else
                    break;
            }

            _interactable?.OnInteract(Character);
            StopInteraction();
        }
        #endregion

        #region Detection
        private void Awake() => enabled = false;
        
        protected override void OnBehaviourStart(ICharacter character) => _ignoredRoot = character.transform;
        
        private void FixedUpdate()
        {
            Ray ray = new Ray(_view.position, _view.forward);
            int prevColliderId = _raycastHit.colliderInstanceID;

            bool hit = PhysicsUtils.RaycastOptimized(ray, _distance, out _raycastHit, LayerConstants.INTERACTABLES_MASK, _ignoredRoot, _triggerInteraction);

            if (!hit && _radius > 0.001f)
                hit = PhysicsUtils.SphereCastOptimized(ray, _radius, _distance, out _raycastHit, LayerConstants.INTERACTABLES_MASK, _ignoredRoot, _triggerInteraction);

            if (hit)
            {
                if (prevColliderId != _raycastHit.colliderInstanceID)
                {
                    _hasRaycastHit = true;

                    // Hover End
                    Hoverable?.OnHoverEnd(Character);
                    StopInteraction();

                    var found = _raycastHit.collider.GetComponent<IHoverable>();

                    if (found != null)
                    {
                        if (found.enabled)
                            Hoverable = found;
                        else
                        {
                            Hoverable = null;
                            _raycastHit = default(RaycastHit);
                        }
                    }
                    else
                        Hoverable = null;

                    // Hover Start
                    Hoverable?.OnHoverStart(Character);
                }
            }
            else if (_hasRaycastHit)
            {
                _hasRaycastHit = false;

                // Hover End
                Hoverable?.OnHoverEnd(Character);
                StopInteraction();
                Hoverable = null;
            }
        }
        #endregion
    }
}