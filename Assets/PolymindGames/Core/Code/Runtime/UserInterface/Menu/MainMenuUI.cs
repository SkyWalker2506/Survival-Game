using PolymindGames.InputSystem;
using UnityEditor;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.SCENE_SINGLETON)]
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField]
        private InputContext _menuContext;
        
        
        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        public void RedirectToMultiplayerAddon() => Application.OpenURL("https://assetstore.unity.com/packages/add-ons/multiplayer-stp-survival-template-pro-259841");

        private void OnEnable() => InputManager.Instance.PushContext(_menuContext);
        private void OnDisable() => InputManager.Instance.PopContext(_menuContext);
    }
}