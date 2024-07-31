using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/User Interface/Selectables/Selectable Group")]
    public class SelectableGroupUI : SelectableGroupBaseUI
    {
        private enum SelectableRegisterMode
        {
            None = 0,
            Disable = 2
        }

        [SerializeField, BeginGroup]
        private SelectableRegisterMode _selectableRegisterMode = SelectableRegisterMode.None;

        [SerializeField, EndGroup]
        private SelectableUI _defaultSelectable;

        private readonly List<SelectableUI> _selectables = new();
        
        private SelectableUI _highlighted;
        private SelectableUI _selected;
        

        public override IReadOnlyList<SelectableUI> RegisteredSelectables => _selectables;
        public override SelectableUI Selected => _selected;
        public override SelectableUI Highlighted => _highlighted;

        public override event UnityAction<SelectableUI> SelectedChanged;
        public override event UnityAction<SelectableUI> HighlightedChanged;

        internal override void RegisterSelectable(SelectableUI selectable)
        {
            if (selectable == null)
                return;

            _selectables.Add(selectable);
            if (_selectableRegisterMode == SelectableRegisterMode.Disable)
                DisableSelectable(selectable);
        }

        internal override void UnregisterSelectable(SelectableUI selectable)
        {
            if (selectable == null)
                return;

            _selectables.Remove(selectable);
        }

        public override void SelectSelectable(SelectableUI selectable)
        {
            if (selectable == _selected)
                return;

            var prevSelectable = _selected;
            _selected = selectable;

            if (prevSelectable != null)
                prevSelectable.Deselect();

            if (selectable != null)
                selectable.Select();

            OnSelectedChanged(selectable);
            SelectedChanged?.Invoke(selectable);
        }

        public override void HighlightSelectable(SelectableUI selectable)
        {
            _highlighted = selectable;
            HighlightedChanged?.Invoke(selectable);
        }

        public override SelectableUI GetDefaultSelectable()
        {
            return _defaultSelectable != null ? _defaultSelectable : _selectables[0];
        }

        protected virtual void OnSelectedChanged(SelectableUI selectable) { }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_defaultSelectable == null)
                _defaultSelectable = GetComponentsInChildren<SelectableUI>().FirstOrDefault();
        }
#endif
    }
}