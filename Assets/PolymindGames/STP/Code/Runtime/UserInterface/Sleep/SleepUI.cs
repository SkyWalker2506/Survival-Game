using System.Collections;
using System.Text;
using PolymindGames.WorldManagement;
using TMPro;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(Animator), typeof(CanvasGroup))]
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_interaction")]
    public sealed class SleepUI : CharacterUIBehaviour
    {
        [SerializeField, Range(0f, 1f), BeginGroup("Canvas")]
        [Tooltip("The max alpha that will be reached when fading in.")]
        private float _maxCanvasAlpha = 1f;

        [SerializeField, Range(0.1f, 10f), EndGroup]
        [Tooltip("The speed at which the sleep UI will be faded in & out.")]
        private float _canvasLerpSpeed = 3f;

        [SerializeField, BeginGroup]
        [Tooltip("A UI text component that's used for displaying the current time while sleeping.")]
        private TextMeshProUGUI _currentTimeText;

        [SerializeField, EndGroup]
        [Tooltip("The background rect of this UI piece.")]
        private RectTransform _backgroundRect;

        [SerializeField, BeginGroup]
        [AnimatorParameter(AnimatorControllerParameterType.Trigger)]
        [Tooltip("The 'show' animator trigger.")]
        private string _showTrigger = "Show";

        [SerializeField, EndGroup]
        [AnimatorParameter(AnimatorControllerParameterType.Trigger)]
        [Tooltip("The 'hide' animator trigger.")]
        private string _hideTrigger = "Hide";

        [SerializeField, Range(1, 10), BeginGroup]
        [Help("Number of text templates each side")]
        [Tooltip("How many time templates should be spawned each side.")]
        private int _numberOfTemplates;

        [SerializeField, PrefabObjectOnly]
        [Tooltip("A prefab with a text component on it that will be instantiated.")]
        private TextMeshProUGUI _timeTextTemplate;

        [SerializeField]
        [Tooltip("How will the text scale over the duration of its lifetime.")]
        private AnimationCurve _textSizeOverDistance;

        [SerializeField]
        [Tooltip("How will the text y position change over the duration of its lifetime.")]
        private AnimationCurve _textYPositionOverDistance;

        [SerializeField, EndGroup]
        [Tooltip("How will the color of the text change over the duration of its lifetime.")]
        private Gradient _textColorOverDistance;

        private float _backgroundRectHalfSize;
        private TextMeshProUGUI[] _timeTexts;
        private CanvasGroup _canvasGroup;
        private Animator _animator;


        protected override void Awake()
        {
            base.Awake();

            _animator = GetComponent<Animator>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _backgroundRectHalfSize = _backgroundRect.rect.size.x / 2f;
            _canvasGroup.alpha = 0f;

            SetupTemplates();
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            character.GetCC<ISleepControllerCC>().SleepStart += OnSleepStart;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            character.GetCC<ISleepControllerCC>().SleepStart -= OnSleepStart;
        }

        private void SetupTemplates()
        {
            // Instantiate templates
            _timeTexts = new TextMeshProUGUI[_numberOfTemplates * 2];
            for (int i = 0; i < _timeTexts.Length; i++)
                _timeTexts[i] = Instantiate(_timeTextTemplate, _backgroundRect);
        }

        private void OnSleepStart(int hoursToSleep)
        {
            var sleepHandler = Character.GetCC<ISleepControllerCC>();
            StartCoroutine(C_UpdateSleepUI(sleepHandler));
        }

        private IEnumerator C_UpdateSleepUI(ISleepControllerCC sleepController)
        {
            StringBuilder strBuilder = new StringBuilder(8);
            var timeManager = World.Instance.Time;
            
            // Update Current Time HUD
            _currentTimeText.text = timeManager.FormatDayTime(true, true, false, strBuilder);

            ResetSideTexts(timeManager.Hour);
            UpdateSideTexts(timeManager.Hour, timeManager.DayTimeIncrementPerSecond);

            // Trigger the show animation
            _animator.SetTrigger(_showTrigger);

            // Show canvas
            while (_canvasGroup.alpha < _maxCanvasAlpha - 0.01f)
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, _maxCanvasAlpha, _canvasLerpSpeed * Time.deltaTime);
                yield return null;
            }

            _canvasGroup.alpha = _maxCanvasAlpha;

            // Update time texts
            while (sleepController.SleepActive)
            {
                // Get the currentTime
                var gameTime = World.Instance.Time.GetGameTime();

                // Update Current Time HUD
                _currentTimeText.text = timeManager.FormatDayTime(true, true, false, strBuilder);
                UpdateSideTexts(timeManager.Hour, timeManager.DayTimeIncrementPerSecond);

                yield return null;
            }

            // Trigger the hide animation
            _animator.SetTrigger(_hideTrigger);

            // Hide Canvas
            while (_canvasGroup.alpha > 0.001f)
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 0f, _canvasLerpSpeed * Time.deltaTime);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
        }

        private void UpdateSideTexts(int hour, float increment)
        {
            // Update side time texts
            for (int i = 0; i < _timeTexts.Length; i++)
            {
                var textRect = _timeTexts[i].rectTransform;

                // Update position
                textRect.anchoredPosition -= new Vector2(increment * 24f * (_backgroundRectHalfSize / _numberOfTemplates) * Time.deltaTime, 0f);

                // Reset Text
                if (textRect.anchoredPosition.x < -_backgroundRectHalfSize)
                {
                    _timeTexts[i].text = ((int)Mathf.Repeat(hour + _numberOfTemplates, 23f)).ToString("00") + ":00";
                    textRect.anchoredPosition = new Vector2(_backgroundRectHalfSize, 0f);
                }

                // Update graphics
                float distanceFromCenter = GetDistanceFromCenter(textRect);
                UpdateTextGraphics(_timeTexts[i], textRect, distanceFromCenter);
            }
        }

        private float GetDistanceFromCenter(RectTransform textRect)
        {
            return Mathf.Abs(_backgroundRect.anchoredPosition.x - textRect.anchoredPosition.x) / _backgroundRectHalfSize;
        }

        // Update the text templates based on their distance from the center of the background rect
        private void UpdateTextGraphics(TextMeshProUGUI text, RectTransform textRect, float distanceFromCenter)
        {
            text.color = _textColorOverDistance.Evaluate(distanceFromCenter);
            textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, _textYPositionOverDistance.Evaluate(distanceFromCenter));
            textRect.localScale = Vector3.one * _textSizeOverDistance.Evaluate(distanceFromCenter);
        }

        private void ResetSideTexts(int currentHour)
        {
            float distanceBetweenTemplates = _backgroundRectHalfSize / _numberOfTemplates;
            float currentXPosition = 0f;
            currentHour++;

            for (int i = 0; i < _timeTexts.Length; i++)
            {
                currentXPosition += distanceBetweenTemplates;
                _timeTexts[i].text = ((int)Mathf.Repeat(currentHour + i, 23f)).ToString("00") + ":00";
                _timeTexts[i].rectTransform.anchoredPosition = new Vector2(currentXPosition, 0f);
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _animator = GetComponent<Animator>();
        }
#endif
    }
}