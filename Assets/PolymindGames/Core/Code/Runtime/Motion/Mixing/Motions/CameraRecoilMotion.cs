using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public sealed class CameraRecoilMotion : IMixedMotion
    {
        private readonly ILookHandlerCC _lookHandler;
        private readonly Spring2D _spring;
        
        private SpringSettings _recoilSpring;
        private SpringSettings _recoverySpring;
        private float _interpolation;
        private float _inverseDuration;
        private bool _recoilActive;
        private bool _recoveryActive;
        private Vector2 _springValue;
        private Vector2 _currentRotation;
        private Vector2 _startRotation;
        private Vector2 _targetRotation;
        private float _xControlled;
        private float _yControlled;


        public CameraRecoilMotion(ICharacter character)
        {
            _recoverySpring = SpringSettings.Default;
            _recoilSpring = SpringSettings.Default;
            _spring = new Spring2D(SpringSettings.Default);
            _lookHandler = character.GetCC<ILookHandlerCC>();
            _interpolation = 1f;
        }

        public void AddRecoil(Vector2 recoilToAdd, float duration)
        {
            if (_recoveryActive)
                _spring.Adjust(_recoilSpring);

            _startRotation = _springValue;
            _currentRotation = _springValue;

            if (!_recoveryActive)
                _targetRotation += recoilToAdd;
            else
                _targetRotation = _startRotation + recoilToAdd;

            _interpolation = 0f;
            _inverseDuration = 1f / Mathf.Max(duration, 0.001f);

            _recoveryActive = false;
            _recoilActive = true;

            if (!_recoilActive)
                _lookHandler.SetAdditiveLookInput(GetRecoil);
        }

        public void SetRecoilSprings(ref SpringSettings recoilSpring, in SpringSettings recoverySpring)
        {
            _recoilSpring = recoilSpring;
            _recoverySpring = recoverySpring;
            _spring.Adjust(_recoveryActive ? _recoverySpring : _recoilSpring);
        }

        private Vector2 GetRecoil()
        {
            if (!_recoilActive)
                return Vector2.zero;

            float deltaTime = Time.deltaTime;

            if (!_recoveryActive)
            {
                var internalMoveDelta = _lookHandler.LookDelta;
                if (internalMoveDelta.x > 0f)
                    _xControlled += internalMoveDelta.x;

                _yControlled += internalMoveDelta.y;

                if (_interpolation >= 1.35f && _targetRotation.x < 0f)
                {
                    if (_xControlled > Mathf.Abs(_targetRotation.x))
                        OnRecoilStop();

                    _recoveryActive = true;
                    float targetX = Mathf.Clamp(-_xControlled, _targetRotation.x, 0f);
                    float targetY = Mathf.Clamp(-_yControlled, -Mathf.Abs(_targetRotation.y / 2f), Mathf.Abs(_targetRotation.y / 2f));

                    _spring.Adjust(_recoverySpring);
                    _spring.SetTargetValue(targetX, targetY);
                }
                else
                {
                    _interpolation = Mathf.Min(_interpolation + deltaTime * _inverseDuration, 2f);
                    float interpolation = Mathf.Clamp01(_interpolation);
                    _currentRotation = Vector2.Lerp(_startRotation, _targetRotation, interpolation);
                    _spring.SetTargetValue(_currentRotation);
                }
            }

            _springValue = _spring.Evaluate(deltaTime);

            if (_recoveryActive && _spring.IsIdle)
                OnRecoilStop();

            return _springValue;
        }

        private void OnRecoilStop()
        {
            _currentRotation = Vector2.zero;
            _targetRotation = Vector2.zero;
            _recoilActive = false;
            _recoveryActive = false;
            _lookHandler.SetAdditiveLookInput(null);
            _spring.Reset();
            _spring.Adjust(_recoilSpring);

            _xControlled = 0f;
            _yControlled = 0f;
        }

        #region Ignore
        public float Multiplier { get; set; }
        public void UpdateMotion(float deltaTime) { }
        public Vector3 GetPosition(float deltaTime) => Vector3.zero;
        public Quaternion GetRotation(float deltaTime) => Quaternion.identity;
        #endregion
    }
}