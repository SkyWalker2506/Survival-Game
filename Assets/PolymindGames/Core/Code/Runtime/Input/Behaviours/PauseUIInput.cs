using PolymindGames.UserInterface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Pause Input")]
    [RequireComponent(typeof(PauseMenuUI))]
    public class PauseUIInput : PlayerUIInputBehaviour
    {
        [SerializeField, BeginGroup("Actions"), EndGroup]
        private InputActionReference _pauseInput;

        private PauseMenuUI _pauseMenu;


        #region Initialization
        private void Start() => _pauseMenu = GetComponent<PauseMenuUI>();
        private void OnEnable() => _pauseInput.action.performed += OnPauseInput;
        private void OnDisable() => _pauseInput.action.performed -= OnPauseInput;
        #endregion

        #region Input handling
        private void OnPauseInput(InputAction.CallbackContext ctx) => _pauseMenu.TryPause();
        #endregion
    }
}
