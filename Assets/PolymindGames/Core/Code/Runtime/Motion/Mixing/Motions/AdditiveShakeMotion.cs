using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/Motion/Additive Shake Motion")]
    public sealed class AdditiveShakeMotion : MotionBehaviour
    {
        [SerializeField, BeginGroup("Interpolation")]
        private SpringSettings _slowPositionSpring = new(15f, 150f, 1.1f, 1f);

        [SerializeField]
        private SpringSettings _slowRotationSpring = new(15f, 150f, 1.1f, 1f);

        [SerializeField]
        private SpringSettings _fastPositionSpring = new(15f, 150f, 1.1f, 1f);

        [SerializeField, EndGroup]
        private SpringSettings _fastRotationSpring = new(15f, 150f, 1.1f, 1f);
        
        private readonly List<Shake> _positionShakes = new();
        private readonly List<Shake> _rotationShakes = new();
        private SpringType _positionSpringMode = SpringType.FastSpring;
        private SpringType _rotationSpringMode = SpringType.FastSpring;

        private static readonly Stack<Shake> s_ShakesPool = CreateShakesPool(16);

        private const float POSITION_FORCE_MOD = 0.03f;
        private const float ROTATION_FORCE_MOD = 3f;
        

        public void AddShake(ShakeData shake, SpringType springType = SpringType.FastSpring)
        {
            if (!shake.IsPlayable)
                return;

            AddPositionShake(shake.Shake.PositionShake, shake.Multiplier, springType);
            AddRotationShake(shake.Shake.RotationShake, shake.Multiplier, springType);
        }

        public void AddPositionShake(ShakeSettings3D settings, float multiplier = 1f, SpringType springType = SpringType.FastSpring)
        {
            if (settings.Duration < 0.01f)
                return;

            if (s_ShakesPool.TryPop(out var shake))
            {
                if (springType != _positionSpringMode)
                {
                    _positionSpringMode = springType;
                    PositionSpring.Adjust(_positionSpringMode == SpringType.SlowSpring ? _slowPositionSpring : _fastPositionSpring);
                }
                
                shake.Init(in settings, multiplier * POSITION_FORCE_MOD);
                _positionShakes.Add(shake);
                SetTargetPosition(EvaluateShakes(_positionShakes));
            }
        }

        public void AddRotationShake(ShakeSettings3D settings, float multiplier = 1f, SpringType springType = SpringType.FastSpring)
        {
            if (settings.Duration < 0.01f)
                return;

            if (s_ShakesPool.TryPop(out var shake))
            {
                if (springType != _rotationSpringMode)
                {
                    _rotationSpringMode = springType;
                    RotationSpring.Adjust(_rotationSpringMode == SpringType.SlowSpring ? _slowRotationSpring : _fastRotationSpring);
                }

                shake.Init(in settings, multiplier * ROTATION_FORCE_MOD);
                _rotationShakes.Add(shake);
                SetTargetRotation(EvaluateShakes(_rotationShakes));
            }
        }

        public override void UpdateMotion(float deltaTime)
        {
            if (PositionSpring.IsIdle && RotationSpring.IsIdle)
                return;

            SetTargetPosition(EvaluateShakes(_positionShakes));
            SetTargetRotation(EvaluateShakes(_rotationShakes));
        }

        protected override SpringSettings GetDefaultPositionSpringSettings() => _fastPositionSpring;
        protected override SpringSettings GetDefaultRotationSpringSettings() => _fastRotationSpring;

        private static Vector3 EvaluateShakes(List<Shake> shakes)
        {
            int i = 0;
            Vector3 value = default(Vector3);

            while (i < shakes.Count)
            {
                var shake = shakes[i];
                value += shake.Evaluate();

                if (shake.IsDone)
                {
                    shakes.RemoveAt(i);
                    s_ShakesPool.Push(shake);
                }
                else
                    i++;
            }

            return value;
        }
        
        private static Stack<Shake> CreateShakesPool(int capacity)
        {
            var shakesPool = new Stack<Shake>(capacity);
            for (int i = 0; i < capacity; i++)
                shakesPool.Push(new Shake());

            return shakesPool;
        }

        #region Internal
        private sealed class Shake
        {
            private float _duration;
            private float _endTime;
            private float _speed;
            private float _xAmplitude;
            private float _yAmplitude;
            private float _zAmplitude;

            private static readonly AnimationCurve s_DecayCurve =
                AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            

            public void Init(in ShakeSettings3D settings, float amplitude = 1f)
            {
                float xSign = Random.Range(0, 100) > 50 ? 1f : -1f;
                float ySign = Random.Range(0, 100) > 50 ? 1f : -1f;
                float zSign = Random.Range(0, 100) > 50 ? 1f : -1f;

                _xAmplitude = xSign * amplitude * settings.XAmplitude;
                _yAmplitude = ySign * amplitude * settings.YAmplitude;
                _zAmplitude = zSign * amplitude * settings.ZAmplitude;

                _duration = settings.Duration;
                _speed = settings.Speed;
                _endTime = Time.fixedTime + _duration;
            }

            public bool IsDone => Time.time > _endTime;

            public Vector3 Evaluate()
            {
                float time = Time.fixedTime;
                float timer = (_endTime - time) * _speed;
                float decay = s_DecayCurve.Evaluate(1f - (_endTime - time) / _duration);

                return new Vector3(Mathf.Sin(timer) * _xAmplitude * decay,
                    Mathf.Cos(timer) * _yAmplitude * decay,
                    Mathf.Sin(timer) * _zAmplitude * decay);
            }
        }
        #endregion
    }
}