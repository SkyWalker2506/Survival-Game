using PolymindGames.WorldManagement;
using PolymindGames.SaveSystem;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    public sealed class SaveSlotUI : MonoBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private GameObject _noDataObject;
        
        [SerializeField, BeginGroup]
        private ButtonUI _button;

        [SerializeField, EndGroup]
        private RawImage _screenshot;

        [SerializeField, BeginGroup]
        private TextMeshProUGUI _saveRealTime;

        [SerializeField]
        private TextMeshProUGUI _saveGameTime;

        [SerializeField]
        private TextMeshProUGUI _mapName;
        
        [SerializeField, EndGroup]
        private TextMeshProUGUI _difficulty;
        
        
        public ButtonUI Button => _button;

        public void SetSave(GameSaveInfo saveInfo)
        {
            if (saveInfo != null)
                ShowSave(saveInfo);
            else
                ShowNoSave();
        }

        private void ShowSave(GameSaveInfo saveInfo)
        {
            _screenshot.texture = saveInfo.Screenshot;
            _screenshot.enabled = true;
            _noDataObject.SetActive(false);
            _saveRealTime.text = $"{saveInfo.DateTime.ToShortDateString()} - {saveInfo.DateTime.ToShortTimeString()}";
            _mapName.text = saveInfo.Scene;
            _difficulty.text = $"Difficulty:    {saveInfo.Difficulty.ToString()}";
            
#if SURVIVAL_TEMPLATE_PRO
            _saveGameTime.text = saveInfo.GameTime.FormatGameTimeWithPrefixes();
#endif
        }

        private void ShowNoSave()
        {
            _screenshot.enabled = false;
            _noDataObject.SetActive(true);
            _saveRealTime.text = _saveGameTime.text = _difficulty.text = _mapName.text = string.Empty;
        }
    }
}