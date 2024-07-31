using System;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public abstract class WorkstationInspectorBaseUI<T> : CharacterUIBehaviour, IWorkstationInspector where T : class, IWorkstation
    {
        [SerializeField, NotNull, BeginGroup, EndGroup]
        private SelectableTabUI _tab;
        
        
        public Type WorkstationType => typeof(T);
        protected T Workstation { get; private set; }

        public void Inspect(IWorkstation workstation)
        {
            Workstation = (T)workstation;

            _tab.TabName = workstation.Name;
            _tab.gameObject.SetActive(true);
            _tab.Select();
            
            OnInspectionStarted(Workstation);
        }

        public void EndInspection()
        {
            if (Workstation != null)
                OnInspectionEnded(Workstation);

            Workstation = null;
            _tab.gameObject.SetActive(false);
        }

        protected abstract void OnInspectionStarted(T workstation);
        protected abstract void OnInspectionEnded(T workstation);

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            _tab = GetComponent<SelectableTabUI>();
        }
#endif
    }
}