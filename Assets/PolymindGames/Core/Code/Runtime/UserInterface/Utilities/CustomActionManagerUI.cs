using PolymindGames.InputSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    public readonly struct CustomActionArgs
    {
        public readonly float EndTime;
        public readonly string Description;
        public readonly bool CanCancel;
        public readonly UnityAction CompletedCallback;
        public readonly UnityAction CancelledCallback;
        private readonly float _startTime;

        public CustomActionArgs(string description, float duration, bool canCancel, UnityAction completedCallback, UnityAction cancelledCallback)
        {
            _startTime = Time.time;
            EndTime = Time.time + duration;
            Description = description;
            CanCancel = canCancel;
            CompletedCallback = completedCallback;
            CancelledCallback = cancelledCallback;
        }

        public float GetProgress() => 1f - (EndTime - Time.time) / (EndTime - _startTime);
    }

    [DefaultExecutionOrder(ExecutionOrderConstants.SCENE_SINGLETON)]
    public sealed class CustomActionManagerUI : MonoBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private InputContext _actionContext;

        [SerializeField, BeginGroup]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private Image _fillImg;

        [SerializeField]
        private TextMeshProUGUI _loadTxt;

        [SerializeField, Range(1f, 20f), EndGroup]
        private float _alphaLerpSpeed = 10f;

        private CustomActionArgs _customAction;
        private float _actionEndTime;


        public static CustomActionManagerUI Instance { get; private set; }

        public void StartAction(in CustomActionArgs customActionArgs)
        {
            if (enabled)
                StopAction();

            InputManager.Instance.PushContext(_actionContext);
            InputManager.Instance.PushEscapeCallback(StopAction);

            _customAction = customActionArgs;
            _actionEndTime = customActionArgs.EndTime;

            _customAction = customActionArgs;
            _canvasGroup.blocksRaycasts = true;
            _loadTxt.text = _customAction.Description;

            enabled = true;
        }

        public bool CancelCurrentAction()
        {
            if (!enabled || !_customAction.CanCancel)
                return false;

            StopAction();
            return true;
        }
        
        private void Awake()
        {
            _canvasGroup.alpha = 0f;
            enabled = false;
            
            // Ensure only one instance of CustomActionManager exists
            if (Instance == null)
                Instance = this;
        }
        
        private void OnDestroy()
        {
            // Clear singleton instance when destroyed
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 1f, Time.deltaTime * _alphaLerpSpeed);
            _fillImg.fillAmount = _customAction.GetProgress();

            if (Time.time > _actionEndTime)
                StopAction();
        }

        private void StopAction()
        {
            if (!enabled)
                return;

            InputManager.Instance.PopEscapeCallback(StopAction);
            InputManager.Instance.PopContext(_actionContext);

            if (Time.time > _actionEndTime)
                _customAction.CompletedCallback?.Invoke();
            else
                _customAction.CancelledCallback?.Invoke();

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;

            enabled = false;
        }
    }
}