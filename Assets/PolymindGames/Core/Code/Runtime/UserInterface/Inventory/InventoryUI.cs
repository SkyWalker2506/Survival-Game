using PolymindGames.PostProcessing;
using PolymindGames.UserInterface;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    public sealed class InventoryUI : CharacterUIBehaviour
    {
        [SerializeField, Range(0f, 0.3f), BeginGroup, EndGroup]
        private float _detachContainersDelay = 0.25f;
        
        [SerializeField, BeginGroup]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        private PlayerItemContainerUI[] _containers;

        [SerializeField, EndGroup]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        private SelectableGroupBaseUI[] _selectableGroups;
        
        [SerializeField, BeginGroup, EndGroup]
        private VolumeAnimationProfile _volumeAnimation;
        
        [SerializeField, BeginGroup]
        private UnityEvent _inspectionStarted;

        [SerializeField, EndGroup]
        private UnityEvent _inspectionStopped;


        protected override void OnCharacterAttached(ICharacter character)
        {
            var inspection = character.GetCC<IInventoryInspectManagerCC>();
            inspection.InspectionStarted += OnInspectionStarted;
            inspection.InspectionEnded += OnInspectionEnded;

            if (inspection.IsInspecting)
                _inspectionStarted.Invoke();
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            var inspection = character.GetCC<IInventoryInspectManagerCC>();
            inspection.InspectionStarted -= OnInspectionStarted;
            inspection.InspectionEnded -= OnInspectionEnded;
        }

        private void OnInspectionStarted()
        {
            foreach (var group in _selectableGroups)
            {
                group.EnableAllSelectables();
                if (group.Selected == null)
                    group.GetDefaultSelectable().Select();
            }

            AttachContainers();
            PostProcessingManager.Instance.TryPlayAnimation(this, _volumeAnimation);

            _inspectionStarted.Invoke();
        }

        private void OnInspectionEnded()
        {
            foreach (var group in _selectableGroups)
                group.DisableAllSelectables();
            
            CoroutineUtils.InvokeDelayed(this, DetachContainers, _detachContainersDelay);
            PostProcessingManager.Instance.CancelAnimation(this, _volumeAnimation);
            
            _inspectionStopped.Invoke();
        }

        private void AttachContainers()
        {
            foreach (var container in _containers)
                container.AttachToCharacterContainer();
        }

        private void DetachContainers()
        {
            foreach (var container in _containers)
                container.DetachFromCharacterContainer();
        }
    }
}
