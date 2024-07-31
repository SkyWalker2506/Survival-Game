using PolymindGames.UserInterface;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    [RequireComponent(typeof(PanelUI))]
    public sealed class PanelEventsUI : MonoBehaviour
    {
        [SerializeField, BeginGroup]
        private UnityEvent _showEvent;
        
        [SerializeField, EndGroup]
        private UnityEvent _hideEvent;
        

        private void Start()
        {
            var panel = GetComponent<IPanel>();
            panel.PanelToggled += OnPanelToggled;
        }

        private void OnPanelToggled(bool enable)
        {
            if (enable)
                _showEvent.Invoke();
            else
                _hideEvent.Invoke();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            gameObject.GetOrAddComponent<PanelUI>();
        }
#endif
    }
}
