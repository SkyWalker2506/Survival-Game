using UnityEngine;

namespace PolymindGames.UserInterface
{
    public abstract class SelectableFeedbackUI : MonoBehaviour
    {
        public virtual void OnNormal(bool instant) { }
        public virtual void OnHighlighted(bool instant) { }
        public virtual void OnSelected(bool instant) { }
        public virtual void OnPressed(bool instant) { }
        public virtual void OnDisabled(bool instant) { }
    }
}