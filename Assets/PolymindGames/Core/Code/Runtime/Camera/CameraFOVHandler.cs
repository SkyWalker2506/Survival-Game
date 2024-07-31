using System.Diagnostics;
using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames
{
    /// <summary>
    /// Handles the World & View Model FOV of a character's camera.
    /// </summary>
    [RequireCharacterComponent(typeof(IMotorCC))]
    public sealed class CameraFOVHandler : CharacterBehaviour, IFOVHandlerCC
    {
        [SerializeField, Range(30f, 90f), BeginGroup("Field Of View (View Model)")]
        private float _baseViewModelFOV = 60f;

        [SerializeField]
        private EaseType _viewModelEaseType = EaseType.QuadInOut;

        [SerializeField]
        private string _viewModelFOVEnabledProperty = "_FOVEnabled";

        [SerializeField, EndGroup]
        private string _viewModelFOVProperty = "_FOV";

        [SerializeField, NotNull, BeginGroup("Field Of View (Camera)")]
        private Camera _camera;

        [SerializeField]
        private EaseType _cameraEaseType = EaseType.QuadInOut;

        [SerializeField, Range(0.1f, 5f)]
        private float _airborneFOVMod = 1.05f;

        [SerializeField]
        private AnimationCurve _speedFOVMultiplier = new(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

        [SerializeField, EndGroup]
        [EditorButton(nameof(PingOptions))]
        private AnimationCurve _heightFOVMultiplier = new(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

        private ValueTween<float> _viewModelFOVTween;
        private ValueTween<float> _cameraFOVTween;
        private float _cameraFOVTweenMod = 1f;
        private float _cameraFOVMod = 1f;
        private int _viewModelFOVId;
        private IMotorCC _motor;


        public float CameraFOV => _camera.fieldOfView;
        public float ViewModelFOV => _viewModelFOVTween.CurrentValue;
        public float ViewModelSize => transform.localScale.x;

        public void SetViewModelSize(float size) => transform.localScale = Vector3.one * size;

        public void SetCameraFOV(float fovMultiplier, float duration = 0.3f, float delay = 0f)
        {
            _cameraFOVTweenMod = fovMultiplier;
            float targetFOV = GraphicsOptions.Instance.FieldOfView * fovMultiplier;
            _cameraFOVTween.SetTo(targetFOV)
                .SetDuration(duration)
                .Play(this, delay);
        }

        public void SetViewModelFOV(float fov, float duration = 0.01f, float delay = 0f)
        {
            _viewModelFOVTween.SetTo(fov)
                .SetDuration(duration)
                .Play(this, delay);
        }

        private void Awake()
        {
            // Initialize the view model FOV handling.
            var fieldOfViewSetting = GraphicsOptions.Instance.FieldOfView;
            fieldOfViewSetting.Changed += OnFOVSettingChanged;

            float cameraFOV = fieldOfViewSetting.Value;
            _camera.fieldOfView = cameraFOV;
            _cameraFOVTween = Tweens.Get(cameraFOV, cameraFOV)
                .SetEase(_cameraEaseType);

            float viewmodelFOV = _baseViewModelFOV;
            
            _viewModelFOVId = Shader.PropertyToID(_viewModelFOVProperty);
            Shader.SetGlobalFloat(_viewModelFOVEnabledProperty, 1f);
            
            _viewModelFOVTween = Tweens.Get(viewmodelFOV, viewmodelFOV)
                .SetEase(_viewModelEaseType);

            GraphicsOptions.Instance.FieldOfView.Changed += OnFOVSettingChanged; 
        }

        protected override void OnDestroy()
        {
            Tweens.Release(_cameraFOVTween);
            Tweens.Release(_viewModelFOVTween);
            Shader.SetGlobalFloat(_viewModelFOVEnabledProperty, 0f);

            var fieldOfViewSetting = GraphicsOptions.Instance.FieldOfView;
            fieldOfViewSetting.Changed -= OnFOVSettingChanged;
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            _motor = character.GetCC<IMotorCC>();
        }

        private void OnFOVSettingChanged(float fov)
        {
            float targetFOV = fov * _cameraFOVTweenMod;
            _cameraFOVTween
                .SetTo(targetFOV)
                .Play(this);
        }

        private void Update()
        {
            _cameraFOVMod = Mathf.Lerp(_cameraFOVMod, GetCameraFOVMod(), Time.deltaTime * 3f);
            _camera.fieldOfView = _cameraFOVTween.CurrentValue * _cameraFOVMod;

            Shader.SetGlobalFloat(_viewModelFOVId, _viewModelFOVTween.CurrentValue);
        }

        private float GetCameraFOVMod()
        {
            float multiplier = 1f;

            var velocity = _motor.Velocity;
            var horizontalVel = new Vector2(velocity.x, velocity.z);
            multiplier *= _speedFOVMultiplier.Evaluate(horizontalVel.magnitude);
            multiplier *= _heightFOVMultiplier.Evaluate(_motor.Height);

            if (!_motor.IsGrounded)
                multiplier *= _airborneFOVMod;

            return multiplier;
        }

        #region Editor
        [Conditional("UNITY_EDITOR")]
        private void PingOptions()
        {
#if UNITY_EDITOR
            UnityUtils.PingResourceAsset<GraphicsOptions>();
#endif
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying || !enabled)
                return;

            _viewModelFOVTween?.SetEase(_viewModelEaseType).SetFrom(_baseViewModelFOV);
            _viewModelFOVId = Shader.PropertyToID(_viewModelFOVProperty);
            Shader.SetGlobalFloat(_viewModelFOVEnabledProperty, 1f);
        }
#endif
        #endregion
    }
}