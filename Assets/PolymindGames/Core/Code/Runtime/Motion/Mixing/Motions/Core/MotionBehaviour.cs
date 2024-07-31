using System.Runtime.CompilerServices;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [RequireComponent(typeof(IMotionMixer))]
    public abstract class MotionBehaviour : CharacterBehaviour, IMixedMotion
    {
        [BeginGroup, EndGroup]
        [SerializeField, Range(0f, 10f)]
        private float _multiplier = 1f;

        protected readonly Spring3D PositionSpring = new();
        protected readonly Spring3D RotationSpring = new();

        private float _externalMultiplier = 1f;
        private bool _ignoreParentMultiplier;
        

        protected IMotionMixer MotionMixer { get; private set; }
        
        public bool IgnoreParentMultiplier
        {
            get => _ignoreParentMultiplier;
            set
            {
                _ignoreParentMultiplier = value;
                _externalMultiplier = value ? 1f : MotionMixer.WeightMultiplier;
            }
        }

        float IMixedMotion.Multiplier
        {
            get => _externalMultiplier;
            set
            {
                value = Mathf.Clamp01(value);
                _externalMultiplier = _ignoreParentMultiplier ? 1f : value;
            }
        }
        
        public float Multiplier
        {
            get => _multiplier;
            set => _multiplier = value;
        }

        protected float FinalMultiplier
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _multiplier * _externalMultiplier;
        }

        public abstract void UpdateMotion(float deltaTime);

        public Vector3 GetPosition(float deltaTime)
        {
            Vector3 value = PositionSpring.Evaluate(deltaTime);
            return value;
        }

        public Quaternion GetRotation(float deltaTime)
        {
            Vector3 value = RotationSpring.Evaluate(deltaTime);
            return Quaternion.Euler(value);
        }

        protected void SetTargetPosition(Vector3 target)
        {
            target *= _multiplier * _externalMultiplier;
            PositionSpring.SetTargetValue(target);
        }

        protected void SetTargetPosition(Vector3 target, float multiplier)
        {
            target *= _multiplier * multiplier * _externalMultiplier;
            PositionSpring.SetTargetValue(target);
        }

        protected void SetTargetRotation(Vector3 target)
        {
            target *= _multiplier * _externalMultiplier;
            RotationSpring.SetTargetValue(target);
        }

        protected void SetTargetRotation(Vector3 target, float multiplier)
        {
            target *= _multiplier * multiplier * _externalMultiplier;
            RotationSpring.SetTargetValue(target);
        }

        protected virtual void Awake()
        {
            PositionSpring.Adjust(GetDefaultPositionSpringSettings());
            RotationSpring.Adjust(GetDefaultRotationSpringSettings());
            MotionMixer = GetComponent<IMotionMixer>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            MotionMixer.AddMixedMotion(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            MotionMixer.RemoveMixedMotion(this);
        }

        protected virtual SpringSettings GetDefaultPositionSpringSettings() => SpringSettings.Default;
        protected virtual SpringSettings GetDefaultRotationSpringSettings() => SpringSettings.Default;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!Application.isPlaying || PositionSpring == null)
                return;

            PositionSpring.Adjust(GetDefaultPositionSpringSettings());
            RotationSpring.Adjust(GetDefaultRotationSpringSettings());
        }
#endif
    }
}