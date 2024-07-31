using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Selectables/Selectable Group Reference")]
    public sealed class SelectableGroupReferenceUI : SelectableGroupBaseUI
    {
        [SerializeField, NotNull, BeginGroup, EndGroup]
        private SelectableGroupUI _referencedGroup;
        
        
        public override SelectableUI Selected => _referencedGroup.Selected;
        public override SelectableUI Highlighted => _referencedGroup.Highlighted;
        public override IReadOnlyList<SelectableUI> RegisteredSelectables => _referencedGroup.RegisteredSelectables;

        public override event UnityAction<SelectableUI> SelectedChanged
        {
            add => _referencedGroup.SelectedChanged += value;
            remove => _referencedGroup.SelectedChanged -= value;
        }

        public override event UnityAction<SelectableUI> HighlightedChanged
        {
            add => _referencedGroup.HighlightedChanged += value;
            remove => _referencedGroup.HighlightedChanged -= value;
        }

        internal override void RegisterSelectable(SelectableUI selectable) => _referencedGroup.RegisterSelectable(selectable);
        internal override void UnregisterSelectable(SelectableUI selectable) => _referencedGroup.UnregisterSelectable(selectable);
        public override void SelectSelectable(SelectableUI selectable) => _referencedGroup.SelectSelectable(selectable);
        public override void HighlightSelectable(SelectableUI selectable) => _referencedGroup.HighlightSelectable(selectable);
        public override SelectableUI GetDefaultSelectable() => _referencedGroup.GetDefaultSelectable();
    }
}