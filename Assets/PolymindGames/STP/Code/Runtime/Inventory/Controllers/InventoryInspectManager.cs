using PolymindGames.InputSystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames
{
    /// <summary>
    /// Handles any type of inventory inspection (e.g. Backpack, external containers etc.)
    /// </summary>
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/player/modules-and-behaviours/inventory#inventory-inspect-manager-module")]
    public sealed class InventoryInspectManager : CharacterBehaviour, IInventoryInspectManagerCC
    {
        [SerializeField, BeginGroup]
        private InputContext _inventoryContext;

        [SerializeField, Range(0f, 10f), EndGroup]
        [Tooltip("How often can the inventory inspection be toggled (e.g. open/close backpack).")]
        private float _toggleThreshold = 0.35f;

        private float _nextAllowedToggleTime;
        
        
        public bool IsInspecting { get; private set; }
        public IWorkstation Workstation { get; private set; }

        public event UnityAction InspectionStarted;
        public event UnityAction AfterInspectionStarted;
        public event UnityAction InspectionEnded;

        public void StartInspection(IWorkstation workstation)
        {
            bool isSameWorkstation = workstation != null && Workstation == workstation;

            if (IsInspecting || Time.time < _nextAllowedToggleTime || isSameWorkstation)
                return;

            Workstation = workstation;

            IsInspecting = true;
            _nextAllowedToggleTime = Time.time + _toggleThreshold;

            UnityUtils.UnlockCursor();
            InputManager.Instance.PushEscapeCallback(StopInspection);
            InputManager.Instance.PushContext(_inventoryContext);

            Workstation?.BeginInspection();
            InspectionStarted?.Invoke();
            AfterInspectionStarted?.Invoke();
        }

        public void StopInspection()
        {
            if (!IsInspecting)
                return;

            Workstation?.EndInspection();

            IsInspecting = false;
            _nextAllowedToggleTime = Time.time + _toggleThreshold;

            UnityUtils.LockCursor();
            InputManager.Instance.PopEscapeCallback(StopInspection);
            InputManager.Instance.PopContext(_inventoryContext);

            InspectionEnded?.Invoke();
            Workstation = null;
        }

        protected override void OnBehaviourStart(ICharacter character) => character.HealthManager.Death += OnDeath;
        protected override void OnBehaviourDestroy(ICharacter character) => character.HealthManager.Death -= OnDeath;
        private void OnDeath(in DamageArgs args) => StopInspection();
    }
}