using PolymindGames.ProceduralMotion;
using System.Diagnostics;
using UnityEngine.Events;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolymindGames.MovementSystem
{
    using Debug = UnityEngine.Debug;
    
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterControllerMotor : MonoBehaviour, IMotorCC, ISaveableComponent
    {
        [SerializeField, Range(0f, 100f), BeginGroup]
        [Tooltip("The strength of the gravity.")]
        private float _gravity = 21f;

        [SerializeField, Range(20f,  160f)]
        [Tooltip("The mass of this character.")]
        private float _mass = 80f;

        [SerializeField, Range(0f, 100f), EndGroup]
        private float _groundingForce = 3f;

        [SerializeField, Range(20f, 90f), BeginGroup("Sliding")]
        [Tooltip("The angle at which the character will start to slide.")]
        private float _slideThreshold = 32f;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("The max sliding speed.")]
        private float _slideSpeed = 0.1f;

        [SerializeField, EndGroup]
        [Tooltip("Lowers/Increases the moving speed of the character when moving on sloped surfaces (i.e. lower speed when walking up a hill).")]
        private AnimationCurve _slopeSpeedMod;

        [SerializeField, Range(0f, 10f), BeginGroup("Misc")]
        [Tooltip("A force that will be applied to any rigidbody this motor will collide with.")]
        private float _pushForce = 1f;

        [SerializeField, EndGroup]
        [Tooltip("Layers that are considered obstacles.")]
        [EditorButton(nameof(MoveToSpawnPoint), ValidateMethodName = nameof(CanMoveToSpawnPoint))]
        private LayerMask _collisionMask;

        private RaycastHit _raycastHit;
        private CharacterController _cController;
        private CollisionFlags _collisionFlags;
        private Transform _cachedTransform;
        private MotionInputCallback _motionInput;
        private float _lastGroundedChangeTime;
        private float _defaultStepOffset;
        private float _defaultHeight;
        private float _lastYRotation;
        private float _turnSpeed;
        private bool _disableSnapToGround;
        private bool _applyGravity;
        private bool _isGrounded = true;
        private bool _snapToGround;
        private Vector3 _externalForce;
        private Vector3 _groundNormal;
        private Vector3 _simulatedVelocity;
        private Vector3 _slideVelocity;
        private Vector3 _velocity;


        public float LastGroundedChangeTime => _lastGroundedChangeTime;
        public float Gravity => _gravity;
        public Vector3 Velocity => _velocity;
        public Vector3 SimulatedVelocity => _simulatedVelocity;
        public Vector3 GroundNormal => _groundNormal;
        public float TurnSpeed => _turnSpeed;
        public CollisionFlags CollisionFlags => _collisionFlags;
        public LayerMask CollisionMask => _collisionMask;
        public float GroundSurfaceAngle => Vector3.Angle(Vector3.up, _groundNormal);
        public float DefaultHeight => _defaultHeight;
        public float SlopeLimit => _cController.slopeLimit;
        public float Radius => _cController.radius;
        public bool IsGrounded => _isGrounded;

        public float Height
        {
            get => _cController.height;
            set
            {
                if (Mathf.Abs(_cController.height - value) < 0.01f)
                    return;

                _cController.height = value;
                _cController.center = Vector3.up * (value * 0.5f);
                HeightChanged?.Invoke(value);
            }
        }

        public event UnityAction Teleported;
        public event UnityAction<bool> GroundedChanged;
        public event UnityAction<float> FallImpact;
        public event UnityAction<float> HeightChanged;

        public void SetMotionInput(MotionInputCallback motionInput) => _motionInput = motionInput;

        public void Teleport(Vector3 position, Quaternion rotation, bool resetMotor = false)
        {
            _cController.enabled = false;

            rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
            position = new Vector3(position.x, position.y + _cController.skinWidth, position.z);
            _cachedTransform.SetPositionAndRotation(position, rotation);

            if (resetMotor)
                ResetMotor();

            _cController.enabled = true;

            Teleported?.Invoke();
        }

        public void AddForce(Vector3 force, ForceMode mode, bool snapToGround = false)
        {
            _externalForce += mode switch
            {
                ForceMode.Force => force * (1f / _mass),
                ForceMode.Acceleration => force * Time.deltaTime,
                ForceMode.Impulse => force * (1f / _mass),
                ForceMode.VelocityChange => force,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };

            _disableSnapToGround = !snapToGround;
        }

        public void ResetMotor()
        {
            _groundNormal = _slideVelocity = Vector3.zero;
            _velocity = _simulatedVelocity = Vector3.zero;
            _isGrounded = true;
        }

        public float GetSlopeSpeedMultiplier()
        {
            if (!_isGrounded)
                return 1f;

            // Make sure to lower the speed when ascending steep surfaces.
            float surfaceAngle = GroundSurfaceAngle;
            if (surfaceAngle > 5f)
            {
                bool isAscendingSlope = Vector3.Dot(_groundNormal, _simulatedVelocity) < 0f;

                if (isAscendingSlope)
                    return _slopeSpeedMod.Evaluate(surfaceAngle / SlopeLimit);
            }

            return 1f;
        }

        public bool CanSetHeight(float targetHeight)
        {
            if (Mathf.Abs(Height - targetHeight) < 0.01f)
                return true;

            if (Height < targetHeight)
                return !DoCollisionCheck(true, targetHeight - Height + 0.1f);

            return true;
        }

        private void OnEnable() => _cController.enabled = true;
        private void OnDisable() => _cController.enabled = false;

        private void Awake()
        {
            _cachedTransform = transform;
            _cController = GetComponent<CharacterController>();
            _defaultStepOffset = _cController.stepOffset;
            _defaultHeight = _cController.height;
            _lastYRotation = transform.localEulerAngles.y;
        }
        
        private void Update()
        {
            if (_motionInput == null)
                return;

            var deltaTime = Time.deltaTime;
            var wasGrounded = _isGrounded;
            var groundingForce = 0f;

            _simulatedVelocity = _motionInput.Invoke(_simulatedVelocity, out _applyGravity, out _snapToGround);

            if (!_disableSnapToGround && wasGrounded && _snapToGround)
            {
                // Sliding...
                _simulatedVelocity += GetSlidingVelocity(deltaTime);

                // Grounding force...
                groundingForce = GetGroundingTranslation();
            }

            // Apply a gravity force...
            if (_applyGravity)
                _simulatedVelocity.y -= _gravity * deltaTime;

            // Apply the external force...
            if (_externalForce != Vector3.zero)
            {
                _simulatedVelocity += _externalForce;
                _disableSnapToGround = false;
                _externalForce = Vector3.zero;
            }

            // Move the controller...
            Vector3 translation = _simulatedVelocity * deltaTime;
            translation.y -= groundingForce;
            _collisionFlags = _cController.Move(translation);

            _velocity = _cController.velocity;
            _isGrounded = _cController.isGrounded;
            
            if (wasGrounded != _isGrounded)
            {
                GroundedChanged?.Invoke(_isGrounded);
                _lastGroundedChangeTime = Time.time;

                // Raise fall impact event...
                if (!wasGrounded && _isGrounded)
                {
                    FallImpact?.Invoke(Mathf.Abs(_simulatedVelocity.y));
                    _simulatedVelocity.y = 0f;
                }
            }

            // Calculate turn velocity...
            float currentYRot = _cachedTransform.localEulerAngles.y;
            _turnSpeed = Mathf.Abs(currentYRot - _lastYRotation);
            _lastYRotation = currentYRot;
        }

        private float GetGroundingTranslation()
        {
            // Predict next world position
            float distanceToGround = 0.001f;

            var ray = new Ray(_cachedTransform.position, Vector3.down);
            if (PhysicsUtils.RaycastOptimized(ray, _defaultStepOffset, out _raycastHit, _collisionMask))
                distanceToGround = _raycastHit.distance;
            else
                _applyGravity = true;

            return distanceToGround * _groundingForce;
        }

        private Vector3 GetSlidingVelocity(float deltaTime)
        {
            if (GroundSurfaceAngle > _slideThreshold)
            {
                Vector3 slideDirection = _groundNormal + Vector3.down;
                _slideVelocity += slideDirection * (_slideSpeed * deltaTime);
            }
            else
                _slideVelocity = Vector3.Lerp(_slideVelocity, Vector3.zero, deltaTime * 10f);

            return _slideVelocity;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _groundNormal = hit.normal;

            // make sure we hit a non kinematic rigidbody
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic) return;

            // make sure we only push desired layer(s)
            var bodyLayerMask = 1 << body.gameObject.layer;
            if ((bodyLayerMask & _collisionMask.value) == 0) return;

            // We don't want to push objects below us
            if (hit.moveDirection.y < -0.3f) return;

            // Calculate push direction from move direction, horizontal motion only
            Vector3 pushDir = new(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

            // Apply the push and take strength into account
            body.AddForce(pushDir * _pushForce, ForceMode.Impulse);
        }

        private bool DoCollisionCheck(bool checkAbove, float maxDistance)
        {
            Vector3 rayOrigin = _cachedTransform.position + (checkAbove ? Vector3.up * _cController.height / 2 : Vector3.up * _cController.height);
            Vector3 rayDirection = checkAbove ? Vector3.up : Vector3.down;

            return Physics.SphereCast(new Ray(rayOrigin, rayDirection), _cController.radius, maxDistance, _collisionMask, QueryTriggerInteraction.Ignore);
        }

        #region Save & Load
        [Serializable]
        private sealed class SaveData
        {
            public float Height;
            public Vector3 Velocity;
            public Vector3 SimulatedVelocity;
            public Vector3 Position;
            public Quaternion Rotation;
            public bool IsGrounded;
        }

        void ISaveableComponent.LoadMembers(object data)
        {
            var saveData = (SaveData)data;

            _cController.height = 0f;
            Height = saveData.Height;

            _velocity = saveData.Velocity;
            _simulatedVelocity = saveData.SimulatedVelocity;
            _isGrounded = saveData.IsGrounded;
            
            Teleport(saveData.Position, saveData.Rotation, false);
        }

        object ISaveableComponent.SaveMembers() =>
            new SaveData
            {
                Height = _cController.height,
                Velocity = _velocity,
                SimulatedVelocity = _simulatedVelocity,
                Position = _cachedTransform.position,
                Rotation = _cachedTransform.rotation,
                IsGrounded = _isGrounded
            };

        #endregion

        #region Editor
        private bool CanMoveToSpawnPoint()
        {
#if UNITY_EDITOR
            return !UnityUtils.IsAssetOnDisk(this);
#else
            return false;
#endif
        }

        [Conditional("UNITY_EDITOR")]
        private void MoveToSpawnPoint()
        {
#if UNITY_EDITOR
            Tweens.DebugAllActive();
            
            // Search for random spawn point.
            var gameMode = FindObjectOfType<GameMode>(true);

            if (gameMode == null)
            {
                Debug.LogError("No game mode component found in the scene.");
                return;
            }

            var (position, rotation) = gameMode.GetRandomSpawnPoint();

            if (Application.isPlaying)
                Teleport(position, rotation);
            else
            {
                Undo.RecordObject(transform, "MoveSpawnPoint");
                EditorUtility.SetDirty(transform);
                transform.SetPositionAndRotation(position, rotation);
            }
#endif
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_cController == null)
                return;

            _defaultHeight = _cController.height;
            _defaultStepOffset = _cController.stepOffset;
        }
#endif
        #endregion
    }
}