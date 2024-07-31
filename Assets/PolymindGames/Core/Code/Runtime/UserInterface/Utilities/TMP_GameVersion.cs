using TMPro;
using UnityEditor;
using UnityEngine;

namespace PolymindGames
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class TMP_GameVersion : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField, BeginGroup, EndGroup]
        private string _prefix = "v";


        private void OnValidate()
        {
            var text = GetComponent<TextMeshProUGUI>();
            text.text = $"{_prefix}{PlayerSettings.bundleVersion}";
        }
#endif
    }
}