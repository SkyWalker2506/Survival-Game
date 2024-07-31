using System.Collections.Generic;
using UnityEngine;
using System;

namespace PolymindGames.WieldableSystem
{
    /// <summary>
    /// Represents the wieldable representation of carrying an object.
    /// </summary>
    [AddComponentMenu("Polymind Games/Wieldables/Tools/Carriable")]
    public sealed class WieldableCarriable : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.1f), BeginGroup, EndGroup]
        private float _speedDecreasePerWeightUnit = 0.01f;
        
        [SerializeField, NotNull, BeginGroup("References")]
        [Tooltip("Motion component for controlling movement.")]
        private WieldableMotion _wieldableMotion;
        
        [SerializeField, NotNull]
        [Tooltip("Animator component for controlling animations.")]
        private WieldableAnimator _wieldableAnimator;

        [SerializeField, NotNull]
        [Tooltip("The socket for the left hand.")]
        private Transform _leftHandSocket;

        [SerializeField, NotNull, EndGroup]
        [Tooltip("The socket for the right hand.")]
        private Transform _rightHandSocket;

        private const float MIN_MOVEMENT_SPEED = 0.5f;
        
        private readonly List<CarriablePickup> _pickups = new();
        private WieldableCarrySettings _settings;
        private IWieldable _wieldable;
        private float _weight;


        public IWieldable Wieldable => _wieldable ??= GetComponent<IWieldable>();
        public int CarryCount => _pickups.Count;

        /// <summary>
        /// Adds a carriable item to this wieldable item.
        /// </summary>
        /// <param name="pickup">The carriable pickup item to add.</param>
        public void AddCarriable(CarriablePickup pickup)
        {
            _pickups.Add(pickup);
            _weight = pickup.Rigidbody.mass;
            
            var settings = pickup.Definition.WieldableSettings;
            
            if (_settings != settings)
                SetSettings(settings);

            var parent = GetTransformForSocket(settings.TargetSocket);
            var offset = settings.Offsets[_pickups.Count - 1];
            var position = parent.TransformPoint(offset.PositionOffset);
            var rotation = parent.rotation * Quaternion.Euler(offset.RotationOffset);
            
            var pickupTransform = pickup.transform;
            pickupTransform.SetParent(parent);
            pickupTransform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// Removes a carriable item from this wieldable item.
        /// </summary>
        /// <param name="pickup">The carriable pickup item to remove.</param>
        public void RemoveCarriable(CarriablePickup pickup)
        {
            if (_pickups.Remove(pickup))
            {
                if (pickup != null)
                    pickup.transform.SetParent(null);

                if (CarryCount == 0)
                {
                    _wieldableAnimator.SetTrigger(WieldableAnimationConstants.HOLSTER);
                    _weight = 0f;
                }
            }
        }

        /// <summary>
        /// Retrieves the position and rotation offsets of a pickup at the specified index.
        /// </summary>
        /// <param name="index">The index of the pickup.</param>
        /// <returns>A tuple containing the position and rotation offsets.</returns>
        public (Vector3 position, Vector3 rotation) GetOffsetsAtIndex(int index)
        {
            if (_pickups.Count <= index)
                return default((Vector3 position, Vector3 rotation));

            var pickupTrs = _pickups[index].transform;
            return (pickupTrs.localPosition, pickupTrs.localEulerAngles);
        }

        /// <summary>
        /// Refreshes the position and rotation of the pickups.
        /// </summary>
        public void RefreshVisuals()
        {
            if (_settings == null)
                return;
            
            SetSettings(_settings);
            
            for (int i = 0; i < _pickups.Count; i++)
            {
                // Retrieve the parent transform for the target socket.
                var parent = GetTransformForSocket(_settings.TargetSocket);

                // Retrieve the offset settings for the pickup.
                var offset = _settings.Offsets[i];

                // Calculate the final position and rotation of the pickup based on the parent's transform and offset settings.
                var position = parent.TransformPoint(offset.PositionOffset);
                var rotation = parent.rotation * Quaternion.Euler(offset.RotationOffset);

                // Apply the calculated position and rotation to the pickup.
                _pickups[i].transform.SetPositionAndRotation(position, rotation);
            }
        }

        /// <summary>
        /// Sets the wieldable carry settings.
        /// </summary>
        /// <param name="settings">The settings to apply.</param>
        private void SetSettings(WieldableCarrySettings settings)
        {
            _settings = settings;
            _wieldableAnimator.Animator.runtimeAnimatorController = settings.Animator;
            _wieldableMotion.SetProfile(settings.Motion);
            _wieldableMotion.PositionOffset = settings.PositionOffset;
            _wieldableMotion.RotationOffset = settings.RotationOffset;
        }

        /// <summary>
        /// Gets the transform associated with the specified socket.
        /// </summary>
        /// <param name="socket">The socket to get the transform for.</param>
        /// <returns>The transform associated with the specified socket.</returns>
        private Transform GetTransformForSocket(WieldableCarrySettings.Socket socket) => socket switch
        {
            WieldableCarrySettings.Socket.LeftHand => _leftHandSocket,
            WieldableCarrySettings.Socket.RightHand => _rightHandSocket,
            _ => throw new ArgumentOutOfRangeException(nameof(socket), socket, null)
        };

        private void Awake()
        {
            if (Wieldable is IMovementSpeedHandler speedHandler)
                speedHandler.SpeedModifier.AddModifier(GetSpeed);
        }

        private float GetSpeed()
        {
            return Mathf.Max(1 - _weight * _speedDecreasePerWeightUnit * _pickups.Count, MIN_MOVEMENT_SPEED);
        }
    }
}