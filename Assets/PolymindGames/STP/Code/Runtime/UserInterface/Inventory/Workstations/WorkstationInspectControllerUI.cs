using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public interface IWorkstationInspector
    {
        Type WorkstationType { get; }

        void Inspect(IWorkstation workstation);
        void EndInspection();
    }

    [DefaultExecutionOrder(ExecutionOrderConstants.AFTER_DEFAULT_1)]
    public sealed class WorkstationInspectControllerUI : CharacterUIBehaviour
    {
        [SerializeField, NotNull]
        private SelectableUI _defaultTab;
        
        private readonly Dictionary<Type, IWorkstationInspector> _workstationInspectors = new();
        private IInventoryInspectManagerCC _inventoryInspector;
        private IWorkstationInspector _activeInspector;


        protected override void Awake()
        {
            base.Awake();
            InitializeWorkstations();
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            _inventoryInspector = character.GetCC<IInventoryInspectManagerCC>();
            _inventoryInspector.InspectionStarted += OnInspectionStarted;
            _inventoryInspector.InspectionEnded += OnInspectionEnded;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            _inventoryInspector.InspectionStarted -= OnInspectionStarted;
            _inventoryInspector.InspectionEnded -= OnInspectionEnded;
        }

        private void InitializeWorkstations()
        {
            foreach (IWorkstationInspector inspector in gameObject.GetComponentsInFirstChildren<IWorkstationInspector>())
            {
                if (!_workstationInspectors.ContainsKey(inspector.WorkstationType))
                {
                    _workstationInspectors.Add(inspector.WorkstationType, inspector);
                    inspector.EndInspection();
                }
            }
        }

        private void OnInspectionStarted()
        {
            var workstation = _inventoryInspector.Workstation;
            if (workstation != null && _workstationInspectors.TryGetValue(workstation.GetType(), out IWorkstationInspector objInspector))
            {
                objInspector.Inspect(workstation);
                _activeInspector = objInspector;
                _defaultTab.gameObject.SetActive(true);
            }
            else
            {
                _defaultTab.gameObject.SetActive(true);
                _defaultTab.OnSelect(null);
            }
        }

        private void OnInspectionEnded()
        {
            _activeInspector?.EndInspection();
            _activeInspector = null;
            _defaultTab.gameObject.SetActive(false);
        }
    }
}