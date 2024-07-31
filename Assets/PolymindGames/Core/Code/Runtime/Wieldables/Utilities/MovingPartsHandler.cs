using PolymindGames.ProceduralMotion;
using UnityEngine;
using System;

namespace PolymindGames.WieldableSystem
{
    [Serializable]
    public sealed class MovingPartsHandler
    {
        [Serializable]
        private struct MovingPart
        {
            public Transform Transform;
            public Vector3 PositionOffset;
            public Vector3 RotationOffset;
        }

        [SerializeField, Range(0f, 10f)]
        private float _moveDelay = 0.1f;

        [SerializeField, Range(0f, 10f)]
        private float _easeDuration = 0.1f;

        [SerializeField, Range(0f, 10f)]
        private float _stopDuration;

        [SerializeField]
        private EaseType _easeType = EaseType.SineIn;

        [SerializeField, ReorderableList(ListStyle.Lined, elementLabel: "Part", Foldable = true)]
        private MovingPart[] _movingParts = Array.Empty<MovingPart>();

        private float _moveTimer = -1f;
        private float _easeTime;
        private bool _isMoving;


        public void StartMoving()
        {
            _moveTimer = Time.time + _moveDelay;
            _isMoving = true;
        }
        
        public void StopMoving()
        {
            _moveTimer = Time.time;
            _isMoving = false;
        }

        public void UpdateMoving()
        {
            if (_isMoving)
            {
                if (_moveTimer < Time.time)
                {
                    _easeTime = Mathf.Clamp01(1 - (_easeDuration - (Time.time - _moveTimer)) / _easeDuration);
                    Lerp(Mathf.Clamp01(Easer.Apply(_easeType, _easeTime)));
                }
            }
            else
            {
                if (_easeTime > 0f)
                {
                    _easeTime -= Time.deltaTime / _stopDuration;
                    Lerp(Mathf.Clamp01(Easer.Apply(_easeType, _easeTime)));
                }
            }
        }

        private void Lerp(float t)
        {
            if (t < 0.01f)
                return;
            
            for (int i = 0; i < _movingParts.Length; i++)
            {
                Transform transform = _movingParts[i].Transform;
                transform.position +=  transform.TransformDirection(_movingParts[i].PositionOffset * t);
                transform.rotation *= Quaternion.Euler(_movingParts[i].RotationOffset * t);
            }
        }
    }
}