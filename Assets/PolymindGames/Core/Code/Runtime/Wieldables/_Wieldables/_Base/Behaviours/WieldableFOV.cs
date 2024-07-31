using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    /// <summary>
    /// Controls the field of view (FOV) for both the camera and the view model of a wieldable object.
    /// </summary>
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable FOV")]
    public sealed class WieldableFOV : MonoBehaviour
    {
        [BeginGroup]
        [SerializeField, Range(0f, 100f), Delayed]
        private float _viewModelFOV = 55f;

        [SerializeField, Range(0.1f, 1f), EndGroup]
        private float _viewModelSize = 0.5f;

        private const float DEFAULT_EASE_DURATION = 0.5f;
        private IFOVHandlerCC _fovHandler;


        /// <summary>
        /// Gets the current FOV of the camera.
        /// </summary>
        public float CameraFOV => _fovHandler.CameraFOV;
        
        /// <summary>
        /// Gets the FOV of the view model.
        /// </summary>
        public float ViewModelFOV => _fovHandler.ViewModelFOV;

        /// <summary>
        /// Sets the size of the view model.
        /// </summary>
        /// <param name="size">The new size of the view model.</param>
        public void SetViewModelSize(float size) => _fovHandler.SetViewModelSize(size);
        
        /// <summary>
        /// Sets the FOV of the view model with a modifier.
        /// </summary>
        /// <param name="fovMod">The modifier to apply to the FOV.</param>
        public void SetViewModelFOV(float fovMod) => SetViewModelFOV(fovMod, DEFAULT_EASE_DURATION);
        
        /// <summary>
        /// Sets the FOV of the view model with a modifier over a specific duration.
        /// </summary>
        /// <param name="fovMod">The modifier to apply to the FOV.</param>
        /// <param name="duration">The duration over which to change the FOV.</param>
        /// <param name="delay">Optional delay before applying the FOV change.</param>
        public void SetViewModelFOV(float fovMod, float duration, float delay = 0f) => _fovHandler.SetViewModelFOV(_viewModelFOV * fovMod, duration, delay);

        /// <summary>
        /// Sets the FOV of the camera with a modifier.
        /// </summary>
        /// <param name="fovMod">The modifier to apply to the FOV.</param>
        public void SetCameraFOV(float fovMod) => SetCameraFOV(fovMod, DEFAULT_EASE_DURATION);
        
        /// <summary>
        /// Sets the FOV of the camera with a modifier over a specific duration.
        /// </summary>
        /// <param name="fovMod">The modifier to apply to the FOV.</param>
        /// <param name="duration">The duration over which to change the FOV.</param>
        /// <param name="delay">Optional delay before applying the FOV change.</param>
        public void SetCameraFOV(float fovMod, float duration, float delay = 0f) => _fovHandler.SetCameraFOV(fovMod, duration, delay);
        
        private void Awake()
        {
            var wieldable = GetComponentInParent<IWieldable>();

            if (wieldable.Character != null)
                _fovHandler = wieldable.Character.GetCC<IFOVHandlerCC>();
            else
                Debug.LogError("This behaviour requires a wieldable with a parent character.");
        }

        private void OnEnable()
        {
            if (_fovHandler != null)
            {
                SetViewModelFOV(1f, 0f);
                SetViewModelSize(_viewModelSize);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_fovHandler != null)
                SetViewModelFOV(1f);
        }
#endif
    }
}