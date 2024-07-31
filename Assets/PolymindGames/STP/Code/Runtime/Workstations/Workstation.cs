using System;
using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames
{
    [RequireComponent(typeof(IHoverableInteractable))]
    public abstract class Workstation : MonoBehaviour, IWorkstation
    {
        [SerializeField, BeginGroup, EndGroup]
        private InspectionSettings _openStationSettings;
        
        [SerializeField, BeginGroup, EndGroup]
        private InspectionSettings _closeStationSettings;
        
        private IHoverableInteractable _interactable;


        public virtual IItemContainer[] GetContainers() => Array.Empty<IItemContainer>();
        public virtual string Name => _interactable.Title;

        void IWorkstation.BeginInspection()
        {
            PlayAudio(_openStationSettings.Audio);
            _openStationSettings.Event.Invoke();
        }

        void IWorkstation.EndInspection()
        {
            PlayAudio(_closeStationSettings.Audio);
            _closeStationSettings.Event.Invoke();
        }

        protected virtual void Start()
        {
            _interactable = GetComponent<IHoverableInteractable>();
            _interactable.Interacted += StartInspection;
        }
        
        private void StartInspection(IInteractable interactable, ICharacter character)
        {
            if (character.TryGetCC(out IInventoryInspectManagerCC inspection))
                inspection.StartInspection(this);
        }

        protected void PlayAudio(AudioDataSO audioData)
        {
            if (audioData != null && audioData.IsPlayable && UnityUtils.IsPlayMode)
                AudioManager.Instance.PlayClipAtPoint(audioData.Clip, transform.position, audioData.Volume, audioData.Pitch);
        }
        
#if UNITY_EDITOR
        private void Reset()
        {
            if (!gameObject.HasComponent<IInteractable>())
                gameObject.AddComponent<SimpleInteractable>();
        }
#endif

        #region Internal
        [Serializable]
        private struct InspectionSettings
        {
            [InLineEditor]
            public AudioDataSO Audio;
            
            [SpaceArea(3f)]
            public UnityEvent Event; 
        }
        #endregion
    }
}