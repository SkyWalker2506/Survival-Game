using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames
{
    [RequireCharacterComponent(typeof(IMotorCC))]
    [DefaultExecutionOrder(ExecutionOrderConstants.AFTER_DEFAULT_1)]
    public sealed class CameraHeightController : CharacterBehaviour
    {
        [SerializeField, Range(0f, 100f), BeginGroup]
        private float _eyeHeight = 1.7f;

        [SerializeField, Range(5f, 50f), SpaceArea]
        [Tooltip("How fast should the camera adjust to the current Y position. (up - down)")]
        private float _yLerpSpeed = 20f;

        [SerializeField]
        private EaseType _heightChangeEase = EaseType.QuadInOut;

        [SerializeField, Range(0.001f, 10f), EndGroup]
        private float _heightChangeDuration = 0.33f;
        
        private Transform _cachedTransform;
        private ValueTween<float> _tween;
        private float _lastYPosition;
        private float _lerpSpeed;
        private IMotorCC _motor;


        protected override void OnBehaviourStart(ICharacter character)
        {
            _motor = character.GetCC<IMotorCC>();
            _cachedTransform = transform;
            _cachedTransform.localPosition = new Vector3(0f, _eyeHeight, 0f);

            _tween = Tweens.Get(_eyeHeight, _eyeHeight, _heightChangeDuration) 
                .SetEase(_heightChangeEase);

            _lastYPosition = _motor.transform.position.y - _motor.DefaultHeight + _eyeHeight * 2f;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Tweens.Release(_tween);
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            _motor.HeightChanged += SetHeight;
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _motor.HeightChanged -= SetHeight;
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;

            _lerpSpeed = Mathf.Lerp(_lerpSpeed, _motor.IsGrounded ? _yLerpSpeed : 100f, deltaTime * 10f);
            _lastYPosition = Mathf.Lerp(_lastYPosition, _motor.transform.position.y - _motor.DefaultHeight + _eyeHeight * 2f, _lerpSpeed * deltaTime);

            float eyeHeight = _lastYPosition + (_tween.CurrentValue - _eyeHeight);
            Vector3 pos = _cachedTransform.position;
            _cachedTransform.position = new Vector3(pos.x, eyeHeight, pos.z);
        }

        private void SetHeight(float height)
        {
            _tween.SetTo(height)
                .Play(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_tween != null)
            {
                _tween.SetDuration(_heightChangeDuration);
                _tween.SetEase(_heightChangeEase);
            }
        }
#endif
    }
}