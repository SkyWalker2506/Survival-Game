using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolymindGames.BuildingSystem
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/interaction/interactable/demo-interactables")]
    [RequireComponent(typeof(BoxCollider), typeof(IHoverableInteractable))]
    public sealed class Door : MonoBehaviour, IDamageReceiver
    {
        [SerializeField, Range(0f, 1000f), BeginGroup("Settings")]
        private float _damageRequiredToOpen = 30f;

        [SerializeField, Range(0f, 3f), EndGroup]
        [Tooltip("Open/Close door time cooldown")]
        private float _interactCooldown = 0.5f;

        [SerializeField, BeginGroup("Animation")]
        [Tooltip("Open rotation offset (how much should this door rotate when it opens).")]
        private Vector3 _openRotation;

        [SerializeField, Range(0.1f, 30f)]
        [Tooltip("Open animation rotation speed.")]
        private float _animationSpeed = 1f;

        [SerializeField, Range(0f, 10f)]
        [Tooltip("How much time should the locked animation last.")]
        private float _lockedAnimationDuration = 1f;

        [SerializeField, Range(0f, 100f)]
        [Tooltip("How much should the locked animation move the door.")]
        private float _lockedAnimationRange = 18f;

        [SerializeField, EndGroup]
        [Tooltip("Locked animation randomness.")]
        private Vector2 _lockedRandomRotation;

        [SerializeField, InLineEditor, BeginGroup("Audio")]
        [Tooltip("Audio to play when the door opens")]
        private AudioDataSO _openAudio;

        [SerializeField, InLineEditor, EndGroup]
        [Tooltip("Audio to play when the door opens")]
        private AudioDataSO _closeAudio;

        private IHoverableInteractable _interactable;
        private Quaternion _closedRotation;
        private BoxCollider _collider;
        private float _interactTimer;
        private bool _isLocked;

        private const string CLOSED_TITLE = "Open";
        private const string OPEN_TITLE = "Close";


        private void Awake()
        {
            _closedRotation = transform.localRotation;
            _collider = GetComponent<BoxCollider>();
            _interactable = GetComponent<IHoverableInteractable>();
            _interactable.Interacted += TryToggleDoor;
            _interactable.Title = CLOSED_TITLE;
        }
        
        private void TryToggleDoor(IInteractable interactable, ICharacter character)
        {
            if (Time.time > _interactTimer)
                ToggleDoor(character.transform);
        }

        private void ToggleDoor(Transform source)
        {
            StopAllCoroutines();
            StartCoroutine(!_isLocked ? C_DoAnimation(source) : C_DoLockedAnimation());
            _interactTimer = Time.time + _interactCooldown;
        }

        private IEnumerator C_DoAnimation(Transform source)
        {
            bool wasOpen = Quaternion.Angle(_closedRotation, transform.localRotation) > 0.5f;
            
            bool isOpen = !wasOpen;
            _interactable.Title = isOpen ? OPEN_TITLE : CLOSED_TITLE;
            
            Quaternion targetRotation = _closedRotation;
            bool characterIsInFront = Vector3.Dot(source.forward, transform.forward) > 0f;

            if (!wasOpen)
            {
                Vector3 modelEulerAngles = transform.localEulerAngles;
                targetRotation = Quaternion.Euler(characterIsInFront ? modelEulerAngles + _openRotation : modelEulerAngles - _openRotation);
            }
            
            bool shouldMove = Quaternion.Angle(targetRotation, transform.localRotation) > 0.5f;
            
            // Do move animation
            if (shouldMove)
            {
                // Audio
                var audioData = wasOpen ? _openAudio : _closeAudio;
                AudioManager.Instance.PlayClipAtPoint(audioData.Clip, transform.position, audioData.Volume);

                while (Quaternion.Angle(targetRotation, transform.transform.localRotation) > 0.5f)
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * _animationSpeed);

                    int count = PhysicsUtils.OverlapBoxOptimized(transform.TransformPoint(_collider.center), _collider.size, transform.rotation, out var colliders, 1 << LayerConstants.CHARACTER, QueryTriggerInteraction.Ignore);

                    if (count > 0)
                    {
                        if (colliders[0].TryGetComponent<ICharacter>(out var character))
                            character.GetCC<IMotorCC>().AddForce(characterIsInFront ? -transform.forward * 0.5f : transform.forward * 0.5f, ForceMode.VelocityChange, true);
                    }

                    yield return null;
                }
            }
        }

        private IEnumerator C_DoLockedAnimation()
        {
            float stopTime = Time.time + _lockedAnimationDuration;
            float range = _lockedAnimationRange;
            float currentVelocity = 0f;

            Quaternion localRotation = transform.localRotation;

            Vector2 randomRotationRange = _lockedRandomRotation;
            Quaternion randomRotation = Quaternion.Euler(new Vector2(
                Random.Range(-randomRotationRange.x, randomRotationRange.x),
                Random.Range(-randomRotationRange.y, randomRotationRange.y)));

            while (Time.time < stopTime)
            {
                transform.localRotation = localRotation * randomRotation * Quaternion.Euler(0, Random.Range(-range, range), 0f);
                range = Mathf.SmoothDamp(range, 0f, ref currentVelocity, stopTime - Time.time);

                yield return null;
            }
        }
        
#if UNITY_EDITOR
        private void Reset()
        {
            if (GetComponent<IHoverableInteractable>() == null)
                gameObject.AddComponent<SimpleInteractable>();
        }
#endif

		#region Damage
        ICharacter IDamageReceiver.Character => null;
        
        DamageResult IDamageReceiver.ReceiveDamage(float damage) => DamageResult.Ignored;
        DamageResult IDamageReceiver.ReceiveDamage(float damage, in DamageArgs args)
        {
            if (damage < _damageRequiredToOpen)
                return DamageResult.Ignored;

            bool isOpen = Quaternion.Angle(_closedRotation, transform.localRotation) > 0.5f;
            if (!isOpen)
                ToggleDoor(args.Source.transform);

            return DamageResult.Normal;
        }
        #endregion
    }
}