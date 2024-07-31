using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    /// <summary>
    /// Controls the motion behavior of a wieldable object.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_1)]
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable Motion")]
    public sealed class WieldableMotion : MonoBehaviour, IWieldableMotion
    {
        [SerializeField, InLineEditor, BeginGroup, EndGroup]
        [Tooltip("The target transform for the motion.")]
        private Transform _targetTransform;

        [SerializeField, BeginGroup("Offset")]
        [Tooltip("The offset of the pivot.")]
        private Vector3 _pivotOffset;

        [SerializeField]
        [Tooltip("The offset of the position.")]
        private Vector3 _positionOffset;

        [SerializeField, EndGroup]
        [Tooltip("The offset of the rotation.")]
        private Vector3 _rotationOffset;

        [SerializeField, BeginGroup("Data"), EndGroup]
        [Tooltip("The motion profile data.")]
        private MotionProfile _motionProfile;

        private IFPSCharacter _character;
        

        /// <summary>
        /// Gets the shake handler associated with the wieldable motion.
        /// </summary>
        public IShakeHandler ShakeHandler => _character.ShakeHandler;
        
        /// <summary>
        /// Gets the head motion mixer associated with the wieldable motion.
        /// </summary>
        public IMotionMixer HeadMotionMixer => _character.HeadMotionMixer;
        
        /// <summary>
        /// Gets the hands motion mixer associated with the wieldable motion.
        /// </summary>
        public IMotionMixer HandsMotionMixer => _character.HandsMotionMixer;
        
        /// <summary>
        /// Gets the head motion data handler associated with the wieldable motion.
        /// </summary>
        public IMotionDataHandler HeadDataHandler => _character.HeadMotionDataHandler;
        
        /// <summary>
        /// Gets the hands motion data handler associated with the wieldable motion.
        /// </summary>
        public IMotionDataHandler HandsDataHandler => _character.HandsMotionDataHandler;

        /// <summary>
        /// Gets or sets the rotation offset of the wieldable motion.
        /// </summary>
        public Vector3 RotationOffset
        {
            get => _rotationOffset;
            set
            {
                _rotationOffset = value;
                
                if (gameObject.activeInHierarchy)
                    HandsMotionMixer.ResetMixer(_targetTransform, _pivotOffset, _positionOffset, value);
            }
        }

        /// <summary>
        /// Gets or sets the position offset of the wieldable motion.
        /// </summary>
        public Vector3 PositionOffset
        {
            get => _positionOffset;
            set
            {
                _positionOffset = value;
                
                if (gameObject.activeInHierarchy)
                    HandsMotionMixer.ResetMixer(_targetTransform, _pivotOffset, value, _rotationOffset);
            }
        }

        /// <summary>
        /// Sets the motion profile of the wieldable motion.
        /// </summary>
        /// <param name="profile">The motion profile to set.</param>
        public void SetProfile(MotionProfile profile)
        {
            _motionProfile = profile;
            
            if (gameObject.activeInHierarchy)
                HandsDataHandler.SetPreset(_motionProfile);
        }

        private void Awake()
        {
            var wieldable = GetComponentInParent<IWieldable>();
            _character = wieldable.Character as IFPSCharacter;

            if (_character == null)
                Debug.LogError("This behaviour requires an wieldable with an FPS-Character parent.", gameObject);
        }

        private void OnEnable()
        {
            HandsMotionMixer.ResetMixer(_targetTransform, _pivotOffset, _positionOffset, _rotationOffset);
            HandsDataHandler.SetPreset(_motionProfile);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_targetTransform == null)
                _targetTransform = transform;

            if (_character != null)
                OnEnable();
        }
#endif
    }
}