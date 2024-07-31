using PolymindGames.ProceduralMotion;
using PolymindGames.InventorySystem;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames.BuildingSystem
{
    public sealed class CharacterConstructableBuilder : CharacterBehaviour, IConstructableBuilderCC
    {
        [SerializeField, Range(0f, 5f), BeginGroup("Detection Settings")]
        [Tooltip("The cooldown time for updating constructable detection.")]
        private float _updateCooldown = 0.2f;
        
        [SerializeField, Range(0f, 10f)]
        [Tooltip("The maximum distance at which constructables are detected.")]
        private float _maxDetectionDistance = 7f;

        [SerializeField, Range(0f, 120f), EndGroup]
        [Tooltip("The maximum angle for detecting constructables.")]
        private float _maxDetectionAngle = 20f;
        
        [SerializeField, Range(0f, 5f), BeginGroup, EndGroup]
        [Tooltip("The duration for canceling the constructable.")]
        private float _cancelPreviewDuration = 0.35f;

        [SerializeField, BeginGroup("Effects")]
        private ShakeData _addMaterialShake;
        
        [SerializeField, EndGroup]
        private AudioDataSO _invalidAddMaterialAudio;

        private List<IItemContainer> _containers;
        private IConstructable _constructableInView;
        private Transform _characterTransform;
        private float _cancelPreviewProgress;
        private bool _detectionEnabled;
        private float _updateTimer;


        public IConstructable ConstructableInView
        {
            get => _constructableInView;
            private set
            {
                if (!ReferenceEquals(_constructableInView, value))
                {
                    _constructableInView = value;
                    ConstructableChanged?.Invoke(_constructableInView);
                }
            }
        }

        public bool DetectionEnabled
        {
            get => _detectionEnabled;
            set
            {
                if (_detectionEnabled != value)
                {
                    _detectionEnabled = value;

                    if (value)
                        enabled = true;
                    else
                    {
                        enabled = false;
                        ConstructableInView = null;
                    }
                }
            }
        }
        
        private float CancelPreviewProgress
        {
            get => _cancelPreviewProgress;
            set
            {
                _cancelPreviewProgress = value;
                CancelConstructableProgressChanged?.Invoke(value);
            }
        }

        public event UnityAction<float> CancelConstructableProgressChanged;
        public event UnityAction<IConstructable> ConstructableChanged;
        public event UnityAction<IConstructable> BuildMaterialAdded;

        public void StartCancellingPreview()
        {
            if (_constructableInView == null)
                return;

            StartCoroutine(C_DestroyConstructable(_constructableInView));
        }

        public void StopCancellingPreview()
        {
            if (CancelPreviewProgress == 0f)
                return;

            StopAllCoroutines();
            CancelPreviewProgress = 0f;
        }

        public bool TryAddMaterialFromPlayer()
        {
            if (_constructableInView == null)
                return false;
            
            _containers ??= Character.Inventory.GetContainersWithoutTags();
            foreach (var container in _containers)
            {
                foreach (var slot in container.Slots)
                {
                    if (TryAddMaterialFromSlot(_constructableInView, slot))
                    {
                        BuildMaterialAdded?.Invoke(_constructableInView);
                        HandleSuccessfulAddMaterial();
                        return true;
                    }
                }
            }

            HandleFailedAddMaterial();
            return false;
        }

        public bool TryAddMaterial(BuildMaterialDefinition buildMaterial)
        {
            if (_constructableInView == null)
                return false;
            
            if (_constructableInView.TryAddMaterial(buildMaterial))
            {
                BuildMaterialAdded?.Invoke(_constructableInView);
                HandleSuccessfulAddMaterial();
                return true;
            }
            
            HandleFailedAddMaterial();
            return false;
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            _characterTransform = character.transform;
        }

        private void FixedUpdate()
        {
            if (_updateTimer > Time.fixedTime)
                return;

            int count = PhysicsUtils.OverlapSphereOptimized(_characterTransform.position, _maxDetectionDistance, out var cols, LayerConstants.STRUCTURE_MASK, QueryTriggerInteraction.Collide);
                
            ConstructableInView = count > 0 ? GetClosestConstructable(cols.AsSpan(0, count)) : null;
            _updateTimer = Time.fixedTime + _updateCooldown;
        }

        private static bool TryAddMaterialFromSlot(IConstructable constructable, ItemSlot slot)
        {
            if (slot.HasItem && slot.Item.Definition.TryGetDataOfType(out BuildMaterialData buildData))
            {
                if (constructable.TryAddMaterial(buildData.BuildMaterial))
                {
                    slot.Item.StackCount--;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the closest constructable object among the provided colliders.
        /// </summary>
        /// <param name="colliders">A span of colliders to search through.</param>
        /// <returns>The closest constructable object, or null if no constructable object is found.</returns>
        private IConstructable GetClosestConstructable(ReadOnlySpan<Collider> colliders)
        {
            // Initialize variables to keep track of the closest constructable object and its rank.
            IConstructable closestConstructable = null;
            float closestRank = float.MaxValue;
            
            // Loop through each collider in the provided span.
            foreach (var col in colliders)
            {
                if (col.TryGetComponent(out IConstructable constructable))
                {
                    if (constructable.IsConstructed)
                        continue;
                    
                    // Calculate the position and direction from the character to the collider.
                    Vector3 playerPosition = _characterTransform.position;
                    Vector3 position = col.transform.position;
                    Vector3 direction = position - playerPosition;
                    
                    // Calculate the squared distance between the character and the collider.
                    float distance = (playerPosition - position).sqrMagnitude;
                    
                    // If the distance is greater than the maximum detection distance squared, skip to the next collider.
                    if (distance > _maxDetectionDistance * _maxDetectionDistance)
                        continue;

                    // Calculate the angle between the direction to the collider and the character's forward direction.
                    float angle = Vector3.Angle(direction, _characterTransform.forward);
                    
                    // If the angle is greater than the maximum detection angle, skip to the next collider.
                    if (angle > _maxDetectionAngle)
                        continue;

                    // Calculate the rank of the current collider based on distance and angle.
                    float rank = distance + angle;
                    
                    // If the rank of the current collider is greater than the closest rank found so far, skip to the next collider.
                    if (closestRank < rank)
                        continue;
                    
                    // Update the closest rank and closest constructable object to the current collider.
                    closestRank = rank;
                    closestConstructable = constructable;
                }
            }

            // Return the closest constructable object found among the colliders.
            return closestConstructable;
        }

        private IEnumerator C_DestroyConstructable(IConstructable constructable)
        {
            float endTime = Time.time + _cancelPreviewDuration;
            while (Time.time < endTime)
            {
                CancelPreviewProgress = 1 - (endTime - Time.time) / _cancelPreviewDuration;
                yield return null;
            }

            CancelPreviewProgress = 0f;
            if (constructable != null)
            {
                var parentGrouyp = constructable.BuildingPiece.ParentGroup;

                if (parentGrouyp != null)
                {
                    foreach (var groupConstructable in constructable.BuildingPiece.ParentGroup.BuildingPieces)
                    {
                        if (!groupConstructable.IsConstructed)
                            Destroy(groupConstructable.gameObject);
                    }
                }
                else
                {
                    Destroy(constructable.gameObject);
                }
            }
        }
        
        private void HandleSuccessfulAddMaterial()
        {
            ShakeEvents.DoShake(transform.position, _addMaterialShake, 5f);
        }

        private void HandleFailedAddMaterial()
        {
            Character.AudioPlayer.PlaySafe(_invalidAddMaterialAudio, BodyPoint.Torso);
        }
    }
}