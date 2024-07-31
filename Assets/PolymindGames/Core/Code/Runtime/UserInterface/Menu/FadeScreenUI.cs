using PolymindGames.ProceduralMotion;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    /// <summary>
    /// Concrete implementation of LoadScreen representing a fading UI screen for loading.
    /// </summary>
    public sealed class FadeScreenUI : LoadScreen
    {
        [SerializeField, NotNull, BeginGroup("References")]
        [Tooltip("References to objects used in the UI.")]
        private GameObject _saveObject;

        [SerializeField, NotNull, EndGroup]
        [Tooltip("Canvas group controlling the visibility and opacity of the UI.")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Range(0f, 5f), BeginGroup("Settings")]
        [Tooltip("Settings related to fading animation timings.")]
        private float _fadeInDelay = 0.25f;

        [SerializeField, Range(0f, 5f)]
        [Tooltip("Duration of the fade-in animation.")]
        private float _fadeInDuration = 0.5f;

        [SerializeField, Range(0f, 5f)]
        [Tooltip("Delay before starting the fade-out animation.")]
        private float _fadeOutDelay = 0.25f;
    
        [SerializeField, Range(0f, 5f)]
        [Tooltip("Duration of the fade-out animation.")]
        private float _fadeOutDuration = 0.5f;
    
        [SerializeField, Range(0f, 5f), EndGroup]
        [Tooltip("Delay before disabling the save icon after fade-out.")]
        private float _saveIconDisableDelay = 0.15f;

        [SerializeField, BeginGroup("Audio")]
        [Tooltip("Snapshot controlling the audio during fading.")]
        private AudioMixerSnapshot _fadeSnapshot;

        [SerializeField, EndGroup]
        [Tooltip("Default audio snapshot.")]
        private AudioMixerSnapshot _defaultSnapshot;

        private ValueTween<float> _canvasTween;
        

        /// <inheritdoc/>
        public override void ShowLoadIcon()
        {
            _saveObject.SetActive(true);
        }

        /// <inheritdoc/>
        public override void HideSaveIcon()
        {
            CoroutineUtils.InvokeDelayed(this, Hide, _saveIconDisableDelay);
            void Hide() => _saveObject.SetActive(false);
        }

        /// <inheritdoc/>
        public override IEnumerator FadeIn()
        {
            StartCoroutine(C_FadeAudio(true, 0f, _fadeOutDuration));
            yield return C_FadeCanvas(true, _fadeOutDelay, _fadeOutDuration);
        }

        /// <inheritdoc/>
        public override void FadeOut()
        {
            StartCoroutine(C_FadeCanvas(false, _fadeInDelay, _fadeInDuration));
            StartCoroutine(C_FadeAudio(false, _fadeInDelay, _fadeInDuration * 2f));
        }
        
        protected override void Start()
        {
            _saveObject.SetActive(false);
            FadeOut();
        }

        private IEnumerator C_FadeAudio(bool fadeIn, float fadeDelay = 0f, float duration = 1f)
        {
            for (float timer = Time.time + fadeDelay; timer > Time.time;)
                yield return null;

            // Fade audio.
            var snapshot = fadeIn ? _fadeSnapshot : _defaultSnapshot;
            snapshot.TransitionTo(duration * 0.5f);
        }

        private IEnumerator C_FadeCanvas(bool fadeIn, float fadeDelay = 0f, float duration = 1f)
        {
            for (float timer = Time.time + fadeDelay; timer > Time.time;)
                yield return null;
            
            // Fade canvas.
            var easeType = fadeIn ? EaseType.SineOut : EaseType.SineIn; 
            
            float target = fadeIn ? 1f : 0f;
            yield return _canvasGroup.TweenCanvasGroupAlpha(target, duration * Mathf.Abs(target - _canvasGroup.alpha))
                .SetEase(easeType)
                .PlayAndYield(this, 0f, true);
        }
    }
}