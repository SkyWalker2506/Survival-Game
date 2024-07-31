using UnityEngine.EventSystems;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Selectables/Button")]
    public sealed class ButtonUI : SelectableUI
    {
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !IsSelectable)
                return;

            _isPointerDown = false;
            DoStateTransition(SelectionState.Selected, false);
            RaiseSelectedEvent();
        }

        public override void OnSelect(BaseEventData eventData) { }
    }
}