using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public abstract class SelectableGroupBaseUI : MonoBehaviour
    {
        public abstract SelectableUI Selected { get; }
        public abstract SelectableUI Highlighted { get; }
        public abstract IReadOnlyList<SelectableUI> RegisteredSelectables { get; }

        public abstract event UnityAction<SelectableUI> SelectedChanged;
        public abstract event UnityAction<SelectableUI> HighlightedChanged;

        internal abstract void RegisterSelectable(SelectableUI selectable);
        internal abstract void UnregisterSelectable(SelectableUI selectable);
        public abstract void SelectSelectable(SelectableUI selectable);
        public abstract void HighlightSelectable(SelectableUI selectable);
        public abstract SelectableUI GetDefaultSelectable();

        public void EnableAllSelectables()
        {
            var selectables = RegisteredSelectables;
            for (int i = 0; i < selectables.Count; i++)
                EnableSelectable(selectables[i]);
        }

        public void DisableAllSelectables()
        {
            var selectables = RegisteredSelectables;
            for (int i = 0; i < selectables.Count; i++)
                DisableSelectable(selectables[i]);
        }

        public void RefreshSelected()
        {
            if (Selected != null && EventSystem.current.gameObject != Selected.gameObject)
                EventSystem.current.SetSelectedGameObject(Selected.gameObject);
        }

        public void DeselectSelected()
        {
            if (Selected != null)
                Selected.Deselect();
        }
        
        public void SelectDefault()
        {
            GetDefaultSelectable().OnSelect(null);
        }

        public void SelectDefaultIfNoSelected()
        {
            if (Selected == null || !Selected.isActiveAndEnabled)
                GetDefaultSelectable().OnSelect(null);
        }

        public void EnableAndSelectDefaultIfNoSelected()
        {
            var defaultSelectable = GetDefaultSelectable();
            EnableSelectable(defaultSelectable);
            if (Selected == null || !Selected.isActiveAndEnabled)
                defaultSelectable.OnSelect(null);
        }

        public void EnableSelectable(SelectableUI selectable) => selectable.gameObject.SetActive(true);
        public void DisableSelectable(SelectableUI selectable) => selectable.gameObject.SetActive(false);
    }
}