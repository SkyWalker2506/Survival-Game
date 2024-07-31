using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class MapSelectionMenuUI : MonoBehaviour
    {
        [SerializeField, NotNull]
        private SelectableGroupBaseUI _slotsGroup;
        

        public void LoadSelectedMap()
        {
            if (_slotsGroup.Selected == null || LevelManager.Instance.IsLoading)
                return;
            
            var selectedSlot = _slotsGroup.Selected.GetComponent<MapSlotUI>();
            LevelManager.Instance.TryLoadScene(selectedSlot.Scene.SceneName);
        }
    }
}