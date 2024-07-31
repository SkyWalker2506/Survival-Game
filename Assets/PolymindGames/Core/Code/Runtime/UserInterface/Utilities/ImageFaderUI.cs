using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class ImageFaderUI
    {
        [SerializeField, NotNull]
        private Image _image;

        [SerializeField, Range(0f, 1f)]
        private float _minAlpha = 0.4f;

        [SerializeField, Range(0f, 100f)]
        private float _fadeInSpeed = 25f;

        [SerializeField, Range(0f, 100f)]
        private float _fadeOutSpeed = 0.3f;

        [SerializeField, Range(0f, 10f)]
        private float _fadeOutPause = 0.5f;

        private Coroutine _fadeRoutine;
        
        
        public bool Fading { get; private set; }
        public Image Image { get => _image; set => _image = value; }

        public void DoFadeCycle(MonoBehaviour parent, float targetAlpha)
        {
#if DEBUG
            if (_image == null)
            {
                Debug.LogError("[ImageFader] - The image to fade is not assigned!");
                return;
            }
#endif

            targetAlpha = Mathf.Clamp01(Mathf.Max(Mathf.Abs(targetAlpha), _minAlpha));
            CoroutineUtils.StartAndReplaceCoroutine(parent, C_DoFadeCycle(targetAlpha), ref _fadeRoutine);
        }

        private IEnumerator C_DoFadeCycle(float targetAlpha)
        {
            Fading = true;

            while (Mathf.Abs(_image.color.a - targetAlpha) > 0.01f)
            {
                _image.color = Color.Lerp(_image.color, new Color(_image.color.r, _image.color.g, _image.color.b, targetAlpha), _fadeInSpeed * Time.deltaTime);
                yield return null;
            }

            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, targetAlpha);

            for (float waitTimer = Time.time + _fadeOutPause; waitTimer > Time.time;)
                yield return null;

            while (_image.color.a > 0.01f)
            {
                _image.color = Color.Lerp(_image.color, new Color(_image.color.r, _image.color.g, _image.color.b, 0f), _fadeOutSpeed * Time.deltaTime);
                yield return null;
            }

            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0f);

            Fading = false;
        }
    }
}