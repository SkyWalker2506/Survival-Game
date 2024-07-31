using System.Collections;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.AFTER_DEFAULT_2)]
    public sealed class PlayerNameUI : MonoBehaviour
    {
        [SerializeField, BeginGroup]
        private PanelUI _panel;

        [SerializeField]
        private TextMeshProUGUI _nameText;

        [SerializeField, EndGroup]
        private TMP_InputField _nameInputField;

        private const string PLAYER_NAME_PREF = "POLYMIND_PLAYER_NAME";
        private const string UNNAMED_PLAYER_NAME = "Unnamed";


        // TODO: This should be somewhere else
        public static string GetPlayerName()
        {
            string name = PlayerPrefs.GetString(PLAYER_NAME_PREF);

            if (string.IsNullOrEmpty(name))
                name = UNNAMED_PLAYER_NAME;

            return name;
        }

        public void SavePlayerNameFromField()
        {
            if (string.IsNullOrEmpty(_nameInputField.text))
                return;

            SavePlayerName(_nameInputField.text);
        }

        public void SavePlayerName(string playerName)
        {
            PlayerPrefs.SetString(PLAYER_NAME_PREF, playerName);
            _nameText.text = playerName;
        }

        public void ResetPlayerNameField()
        {
            _nameInputField.text = PlayerPrefs.GetString(PLAYER_NAME_PREF);
        }

        private IEnumerator Start()
        {
            yield return null;
            
            if (!PlayerPrefs.HasKey(PLAYER_NAME_PREF) || IsDefaultName(PlayerPrefs.GetString(PLAYER_NAME_PREF)))
            {
                SavePlayerName(UNNAMED_PLAYER_NAME);
                _panel.Show();
                _nameInputField.Select();
            }
            else
                ResetUI();
        }

        private static bool IsDefaultName(string playerName) =>
            string.IsNullOrEmpty(playerName) || playerName == UNNAMED_PLAYER_NAME;

        private void ResetUI()
        {
            string text = PlayerPrefs.GetString(PLAYER_NAME_PREF);
            _nameInputField.text = text;
            _nameText.text = text;
        }
    }
}