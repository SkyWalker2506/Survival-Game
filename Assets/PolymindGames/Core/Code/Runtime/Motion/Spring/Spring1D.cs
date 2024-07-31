using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public sealed class Spring1D
    {
        private SpringSettings _settings;
        private float _acceleration;
        private float _targetValue;
        private float _value;
        private float _velocity;
        private bool _isIdle;
        
        private const float MAX_STEP_SIZE = 1f / 61f;
        private const float PRECISION = 0.0005f;


        public Spring1D() : this(SpringSettings.Default) { }

        public Spring1D(SpringSettings settings)
        {
            _settings = settings;
            _isIdle = true;
            _targetValue = 0f;
            _velocity = 0f;
            _acceleration = 0f;
        }

        public bool IsIdle => _isIdle;

        public void Adjust(SpringSettings settings)
        {
            _settings = settings;
            _isIdle = false;
        }

        /// <summary>
        /// Reset all values to initial states.
        /// </summary>
        public void Reset()
        {
            _isIdle = true;
            _value = 0f;
            _velocity = 0f;
            _acceleration = 0f;
        }

        /// <summary>
        /// Sets the target value in the middle of motion.
        /// This reuse the current velocity and interpolate the value smoothly afterwards.
        /// </summary>
        /// <param name="value">Target value</param>
        public void SetTargetValue(float value)
        {
            _targetValue = value;
            _isIdle = false;
        }

        /// <summary>
        /// Advance a step by deltaTime(seconds).
        /// </summary>
        /// <param name="deltaTime">Delta time since previous frame</param>
        /// <returns>Evaluated Value</returns>
        public float Evaluate(float deltaTime)
        {
            if (_isIdle)
                return 0f;

            float damp = _settings.Damping;
            float stf = _settings.Stiffness;
            float mass = _settings.Mass;

            float val = _value;
            float vel = _velocity;
            float acc = _acceleration;

            float stepSize = deltaTime * _settings.Speed;
            float maxStepSize = stepSize > MAX_STEP_SIZE ? MAX_STEP_SIZE : stepSize - 0.001f;
            float steps = (int)(stepSize / maxStepSize + 0.5f);

            for (var i = 0; i < steps; i++)
            {
                var dt = Math.Abs(i - (steps - 1)) < 0.001f ? stepSize - i * maxStepSize : maxStepSize;

                val += vel * dt + acc * (dt * dt * 0.5f);

                float calcAcc = (-stf * (val - _targetValue) + -damp * vel) / mass;

                vel += (acc + calcAcc) * (dt * 0.5f);
                acc = calcAcc;
            }

            _value = val;
            _velocity = vel;
            _acceleration = acc;

            if (Mathf.Abs(acc) < PRECISION)
                _isIdle = true;

            return _value;
        }
    }
}