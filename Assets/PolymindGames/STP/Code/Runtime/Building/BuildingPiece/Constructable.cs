using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames.BuildingSystem
{
    /// <summary>
    /// Simple constructable object in the game world.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BuildingPiece))]
    public class Constructable : MonoBehaviour, ISaveableComponent, IConstructable
    {
        [Tooltip("The build requirements for constructing this object.")]
        [SerializeField, IgnoreParent, DisableInPlayMode]
        private BuildRequirements _requirements;

        private BuildingPiece _buildingPiece;

        
        /// <summary>
        /// Gets a value indicating whether this constructable is fully constructed.
        /// </summary>
        public bool IsConstructed => _requirements.IsComplete();

        /// <summary>
        /// Gets the associated building piece of this constructable.
        /// </summary>
        public BuildingPiece BuildingPiece => _buildingPiece;

        /// <summary>
        /// Event triggered when this constructable is fully constructed.
        /// </summary>
        public event UnityAction Constructed;

        /// <summary>
        /// Retrieves the build requirements for constructing this object.
        /// </summary>
        /// <returns>An array of build requirements.</returns>
        public ReadOnlySpan<BuildRequirement> GetBuildRequirements() => _requirements.Requirements;

        /// <summary>
        /// Attempts to add the specified material to this constructable.
        /// </summary>
        /// <param name="material">The build material to add.</param>
        /// <returns>True if the material was successfully added, false otherwise.</returns>
        public bool TryAddMaterial(BuildMaterialDefinition material)
        {
            if (IsConstructed)
                return false;

            if (!BuildingPiece.IsCollidingWithCharacters() && _requirements.TryAddMaterial(material))
            {
                OnMaterialAdded(material);
                if (_requirements.IsComplete())
                    OnConstructed();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Invoked when a material is successfully added to this constructable.
        /// </summary>
        /// <param name="material">The material that was added.</param>
        protected virtual void OnMaterialAdded(BuildMaterialDefinition material)
        {
            var addMaterialAudio = material.UseAudio;
            AudioManager.Instance.PlayClipAtPoint(addMaterialAudio.Clip, transform.position, addMaterialAudio.Volume, addMaterialAudio.Pitch);
        }

        /// <summary>
        /// Invoked when this constructable is fully constructed.
        /// </summary>
        protected virtual void OnConstructed() => Constructed?.Invoke();
        
        /// <summary>
        /// Initializes the constructable component by retrieving the associated building piece.
        /// </summary>
        protected virtual void Awake()
        {
            _buildingPiece = GetComponent<BuildingPiece>();
        }

    #if UNITY_EDITOR
        /// <summary>
        /// Resets the constructable component by adding a FreeBuildingPiece if no BuildingPiece is found.
        /// </summary>
        protected virtual void Reset()
        {
            if (!gameObject.HasComponent<BuildingPiece>())
                gameObject.AddComponent<FreeBuildingPiece>();
        }
    #endif

        #region Save & Load
        void ISaveableComponent.LoadMembers(object data) => _requirements = (BuildRequirements)data;
        object ISaveableComponent.SaveMembers() => _requirements;
        #endregion

        #region Internal
        [Serializable]
        private sealed class BuildRequirements
        {
            [SerializeField, ReorderableList(ListStyle = ListStyle.Boxed), IgnoreParent]
            private BuildRequirement[] _requirements;


            public ReadOnlySpan<BuildRequirement> Requirements => _requirements;

            public bool TryAddMaterial(DataIdReference<BuildMaterialDefinition> material)
            {
                if (material.IsNull)
                    return false;

                for (int i = 0; i < _requirements.Length; i++)
                {
                    var requirement = _requirements[i];

                    if (!requirement.IsCompleted() && requirement.BuildMaterial == material)
                    {
                        int newCount = Mathf.Min(requirement.CurrentAmount + 1, requirement.RequiredAmount);
                        _requirements[i] = new BuildRequirement(requirement.BuildMaterial, requirement.RequiredAmount, newCount);

                        CheckForCompletion();
                        return true;
                    }
                }

                return false;
            }

            public int TryAddMaterial(DataIdReference<BuildMaterialDefinition> material, int count)
            {
                if (material.IsNull)
                    return 0;

                for (int i = 0; i < _requirements.Length; i++)
                {
                    var requirement = _requirements[i];

                    if (!requirement.IsCompleted() && requirement.BuildMaterial == material)
                    {
                        int newCount = Mathf.Min(requirement.CurrentAmount + Mathf.Abs(count), requirement.RequiredAmount);
                        _requirements[i] = new BuildRequirement(requirement.BuildMaterial, requirement.RequiredAmount, newCount);
                        CheckForCompletion();
                        return newCount + requirement.CurrentAmount;
                    }
                }

                return 0;
            }

            private void CheckForCompletion()
            {
                for (int i = 0; i < _requirements.Length; i++)
                {
                    if (!_requirements[i].IsCompleted())
                        return;
                }

                OnComplete();
                
                void OnComplete()
                {
                    Array.Clear(_requirements, 0, _requirements.Length);
                    _requirements = Array.Empty<BuildRequirement>();
                }
            }

            public bool IsComplete() => _requirements.Length == 0;
        }
        #endregion
    }
}