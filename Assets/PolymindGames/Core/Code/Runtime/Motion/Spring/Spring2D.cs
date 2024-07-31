using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public sealed class Spring2D
    {
        private SpringSettings _settings;
        private Vector2 _acceleration;
        private Vector2 _targetValue;
        private Vector2 _value;
        private Vector2 _velocity;
        private bool _isIdle;
        
        private const float MAX_STEP_SIZE = 1f / 61f;
        private const float PRECISION = 0.0005f;


        public Spring2D() : this(SpringSettings.Default) { }

        public Spring2D(SpringSettings settings)
        {
            _settings = settings;
            _isIdle = true;
            _targetValue = Vector2.zero;
            _velocity = Vector2.zero;
            _acceleration = Vector2.zero;
        }

        public SpringSettings Settings => _settings;
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
            _value = Vector2.zero;
            _velocity = Vector2.zero;
            _acceleration = Vector2.zero;
        }

        /// <summary>
        /// Sets the target value in the middle of motion.
        /// This reuse the current velocity and interpolate the value smoothly afterwards.
        /// </summary>
        /// <param name="value">Target value</param>
        public void SetTargetValue(Vector2 value)
        {
            _targetValue = value;
            _isIdle = false;
        }

        /// <summary>
        /// Sets the target value in the middle of motion.
        /// This reuse the current velocity and interpolate the value smoothly afterwards.
        /// </summary>
        public void SetTargetValue(float x, float y)
        {
            _targetValue.x = x;
            _targetValue.y = y;
            _isIdle = false;
        }

        /// <summary>
        /// Advance a step by deltaTime(seconds).
        /// </summary>
        /// <param name="deltaTime">Delta time since previous frame</param>
        /// <returns>Evaluated Value</returns>
        public Vector2 Evaluate(float deltaTime)
        {
            if (_isIdle)
                return Vector2.zero;

            float damp = _settings.Damping;
            float stf = _settings.Stiffness;
            float mass = _settings.Mass;

            Vector2 val = _value;
            Vector2 vel = _velocity;
            Vector2 acc = _acceleration;

            float stepSize = deltaTime * _settings.Speed;
            float maxStepSize = stepSize > MAX_STEP_SIZE ? MAX_STEP_SIZE : stepSize - 0.001f;
            float steps = (int)(stepSize / maxStepSize + 0.5f);

            for (var i = 0; i < steps; i++)
            {
                var dt = Math.Abs(i - (steps - 1)) < 0.001f ? stepSize - i * maxStepSize : maxStepSize;

                val += vel * dt + acc * (dt * dt * 0.5f);

                Vector2 calcAcc = (-stf * (val - _targetValue) + -damp * vel) / mass;

                vel += (acc + calcAcc) * (dt * 0.5f);
                acc = calcAcc;
            }

            _value = val;
            _velocity = vel;
            _acceleration = acc;

            if (Mathf.Abs(acc.x) < PRECISION && Mathf.Abs(acc.y) < PRECISION)
                _isIdle = true;

            return _value;
        }
    }
}