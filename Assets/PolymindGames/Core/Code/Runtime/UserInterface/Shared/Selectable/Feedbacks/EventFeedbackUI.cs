using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.UserInterface
{
    [NestedObjectPath(MenuName = "Event Feedback")]
    public sealed class EventFeedbackUI : SelectableFeedbackUI
    {
        [SerializeField]
        private UnityEvent _onHighlighted;

        [SerializeField]
        private UnityEvent _onPressed;


        public override void OnNormal(bool instant) { }
        public override void OnHighlighted(bool instant) => _onHighlighted.Invoke();
        public override void OnSelected(bool instant) { }
        public override void OnPressed(bool instant) => _onPressed.Invoke();
        public override void OnDisabled(bool instant) { }
    }
}