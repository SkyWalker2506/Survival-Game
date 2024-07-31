using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SelectableUI))]
    public abstract class SlotUI : MonoBehaviour
    {
        private SelectableUI _selectable;

        
        public SelectableUI Selectable
        {
            get
            {
                if (_selectable == null)
                    _selectable = GetComponent<SelectableUI>();

                return _selectable;
            }
        }
    }
}