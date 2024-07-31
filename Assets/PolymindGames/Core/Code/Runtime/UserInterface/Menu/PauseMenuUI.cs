using PolymindGames.ProceduralMotion;
using PolymindGames.PostProcessing;
using PolymindGames.InputSystem;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class PauseMenuUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private InputContext _context;

        [SerializeField, BeginGroup]
        private PanelUI _panel;

        [SerializeField]
        private CanvasRenderer _background;

        [SerializeField]
        private VolumeAnimationProfile _volumeAnimation;
        
        [SerializeField, Range(0f, 1f), EndGroup]
        private float _timeScale = 0.1f;

        [SerializeField, BeginGroup]
        private UnityEvent _onPause;

        [SerializeField, EndGroup]
        private UnityEvent _onResume;

        private ValueTween<float> _canvasTween;
        private float _pauseTimer;


        public void TryPause()
        {
            if (Time.time > _pauseTimer && !InputManager.Instance.HasEscapeCallbacks)
            {
                _pauseTimer = Time.time + 0.3f;
                _panel.Show();
            }
        }

        public void QuitToMenu() =>
            LevelManager.Instance.TryLoadScene(LevelManager.Instance.MainMenuScene);

        public void QuitToDesktop()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            character.HealthManager.Death += OnDeath;
            _panel.PanelToggled += OnPanelToggled;
            
            _background.SetAlpha(0f);
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            character.HealthManager.Death -= OnDeath;
            _panel.PanelToggled -= OnPanelToggled;
            Resume();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _canvasTween?.Stop();
        }

        private void OnDeath(in DamageArgs args)
        {
            _panel.Hide();
            foreach (var panel in gameObject.GetComponentsInFirstChildren<PanelUI>())
                panel.Hide();
        }

        private void OnPanelToggled(bool show)
        {
            if (show) Pause();
            else Resume();
        }

        private void Pause()
        {
            InputManager.Instance.PushContext(_context);

            Time.timeScale = _timeScale;
            _onPause.Invoke();
            
            PostProcessingManager.Instance.PlayAnimation(this, _volumeAnimation, 0f, true);

            if (UnityUtils.IsPlayMode && !LevelManager.Instance.IsLoading)
            {
                _canvasTween = _background.TweenCanvasRendererAlpha(1f, 0.5f);
                _canvasTween.PlayAndRelease(this);
            }
        }

        private void Resume()
        {
            InputManager.Instance.PopContext(_context);

            Time.timeScale = 1f;
            _onResume.Invoke();
            
            PostProcessingManager.Instance.CancelAnimation(this, _volumeAnimation);

            if (UnityUtils.IsPlayMode && !LevelManager.Instance.IsLoading)
            {
                _canvasTween = _background.TweenCanvasRendererAlpha(0f, 0.5f);
                _canvasTween.PlayAndRelease(this);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_background != null)
                _background.SetAlpha(0f);
        }
#endif
    }
}