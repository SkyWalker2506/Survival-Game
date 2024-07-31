using PolymindGames.WorldManagement;
using PolymindGames.SaveSystem;
using UnityEngine;
using System;
using TMPro;

namespace PolymindGames.UserInterface
{
    public sealed class SaveManagerUI : MonoBehaviour
    {
        [SerializeField, NotNull, BeginGroup]
        private PanelUI _panel;

        [SerializeField, EndGroup]
        private TextMeshProUGUI _header;

        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private SaveSlotUI[] _saveSlots;

        private const string SAVE_GAME = "Save Game";
        private const string LOAD_GAME = "Load Game";
        private Mode _currentMode;


        /// <summary>
        /// Refreshes all save slots UI.
        /// </summary>
        public void RefreshSaveSlots()
        {
            // Clear all save slots UI
            foreach (SaveSlotUI slot in _saveSlots)
                slot.SetSave(null);

            // Load save information and update UI
            var savesInfo = SaveLoadManager.Instance.LoadSavesInfo(_saveSlots.Length);
            foreach (var saveInfo in savesInfo)
                _saveSlots[saveInfo.SaveId].SetSave(saveInfo);
        }

        /// <summary>
        /// Clears all save slots and refreshes UI.
        /// </summary>
        public void ClearAllSlots()
        {
            // Delete save files and refresh UI
            for (int i = 0; i < 10; i++)
                SaveLoadManager.Instance.DeleteSaveFile(i);

            foreach (SaveSlotUI slot in _saveSlots)
                slot.SetSave(null);
        }

        /// <summary>
        /// Sets the mode of the save manager UI.
        /// </summary>
        /// <param name="mode">The mode to set (0: Closed, 1: SaveSlots, 2: LoadSlots).</param>
        public void SetMode(int mode)
        {
            var targetMode = (Mode)Mathf.Clamp(mode, 0, 2);
            SwitchMode(targetMode);
        }

        private void SwitchMode(Mode mode)
        {
            _currentMode = mode;

            switch (mode)
            {
                case Mode.Closed:
                    _panel.Hide();
                    break;
                case Mode.SaveSlots:
                    _header.text = SAVE_GAME;
                    _panel.Show();
                    break;
                case Mode.LoadSlots:
                    _header.text = LOAD_GAME;
                    _panel.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private void OnSlotClicked(SelectableUI selectable)
        {
            // If the current mode is closed, do nothing
            if (_currentMode == Mode.Closed)
                return;

            // Find the index of the clicked slot
            int slotIndex = Array.IndexOf(_saveSlots, selectable.GetComponent<SaveSlotUI>());

            // Perform actions based on the current mode
            switch (_currentMode)
            {
                case Mode.SaveSlots:
                    SaveGameToSlot(slotIndex);
                    break;
                case Mode.LoadSlots:
                    LoadGameFromSlot(slotIndex);
                    break;
                case Mode.Closed:
                default:
                    throw new ArgumentOutOfRangeException(nameof(_currentMode), _currentMode, "Invalid mode");
            }
        }

        private static void SaveGameToSlot(int slotIndex)
        {
            // Attempt to save the current game to the specified slot index
            LevelManager.Instance.TrySaveCurrentGameToIndex(slotIndex);
        }

        private static void LoadGameFromSlot(int slotIndex)
        {
            // Attempt to load the game from the specified slot index
            if (!LevelManager.Instance.TryLoadGame(slotIndex))
            {
                // Notify if loading failed
                World.Instance.Message.Dispatch(null, MessageType.Error, $"No save file selected!");
            }
        }

        private void OnEnable()
        {
            // Ensure that the mode is set to Closed when the UI is enabled
            SwitchMode(Mode.Closed);

            // Subscribe to the GameSaved event to refresh save slots when a game is saved
            LevelManager.Instance.GameSaved += RefreshSaveSlots;

            // Subscribe to the click events of all save slots
            foreach (var slot in _saveSlots)
            {
                // Subscribe to the click event of each save slot
                slot.Button.OnSelectableSelected += OnSlotClicked;
            }
            
            RefreshSaveSlots();
        }

        private void OnDisable()
        {
            // Unsubscribe from the GameSaved event
            LevelManager.Instance.GameSaved -= RefreshSaveSlots;

            // Unsubscribe from the click events of all save slots
            foreach (var slot in _saveSlots)
            {
                // Unsubscribe from the click event of each save slot
                slot.Button.OnSelectableSelected -= OnSlotClicked;
            }
        }

        #region Internal
        private enum Mode
        {
            Closed,
            SaveSlots,
            LoadSlots
        }
        #endregion
    }
}