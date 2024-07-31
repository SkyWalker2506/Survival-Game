using PolymindGames.MovementSystem;
using System.Collections;
using System.Diagnostics;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    using Debug = UnityEngine.Debug;
    
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_2)]
    public abstract class Wieldable : MonoBehaviour, IWieldable, ICrosshairHandler, IMovementSpeedHandler
    {
        protected enum WieldableStateType
        {
            Hidden = 0,
            Equipping = 1,
            Equipped = 2,
            Holstering = 3
        }
        
        [OnValueChanged(nameof(Editor_CrosshairChanged))]
        [SerializeField, Range(-1, 100), NewLabel("Default Crosshair")]
        [Tooltip("Index of the default crosshair for this wieldable. Use -1 or lower for no crosshair.")]
        private int _baseCrosshair;

        [SerializeField, Range(0f, 5f), Title("Equipping")]
        [Tooltip("Duration of the equipping animation.")]
        private float _equipDuration = 0.5f;

        [SerializeField, SpaceArea]
        [ReorderableList(ListStyle.Lined, elementLabel: "Audio Clip")]
        [Tooltip("Audio clips played during equipping.")]
        private DelayedSimpleAudioData[] _equipAudio;

        [SerializeField, Range(0f, 5f), Title("Holstering")]
        [Tooltip("Duration of the holstering animation.")]
        private float _holsterDuration = 0.5f;

        [SerializeField, SpaceArea]
        [ReorderableList(ListStyle.Lined, elementLabel: "Audio Clip")]
        [Tooltip("Audio clips played during holstering.")]
        private DelayedSimpleAudioData[] _holsterAudio;

        private WieldableStateType _state;
        private bool _isGeometryVisible = true;
        private Renderer[] _renderers;


        public ICharacter Character { get; private set; }
        public IAnimator Animation { get; private set; }
        public IWieldableMotion Motion { get; private set; }
        public IAudioPlayer AudioPlayer { get; private set; }
        
        public bool IsGeometryVisible
        {
            get => _isGeometryVisible;
            set
            {
                if (_isGeometryVisible == value)
                    return;
                
                _renderers ??= GetComponentsInChildren<Renderer>();
                if (_renderers.Length == 0)
                {
                    Debug.LogError("This wieldable has no renderers.");
                    return;
                }

                foreach (var rend in _renderers)
                    rend.enabled = value;

                _isGeometryVisible = value;
            }
        }
        
        private WieldableStateType State
        {
            get => _state;
            set
            {
                _state = value;
                OnStateChanged(value);
            }
        }

        void IWieldable.SetCharacter(ICharacter character)
        {
            if (Character == null || character == Character)
            {
                Character = character;
                AudioPlayer ??= Character.AudioPlayer;
                
                OnCharacterChanged(Character);
            }
            else
                Debug.LogError("The parent character has been changed, this is not supported in the current version.", gameObject);
        }

        IEnumerator IWieldable.Equip()
        {
            if (State is WieldableStateType.Equipping or WieldableStateType.Equipped)
                yield break;

            gameObject.SetActive(true);

            State = WieldableStateType.Equipping;
            Animation.SetTrigger(WieldableAnimationConstants.EQUIP);
            AudioPlayer.PlayDelayed(_equipAudio);

            for (float timer = Time.time + _equipDuration; timer > Time.time;)
                yield return null;

            if (Character.TryGetCC(out IMovementControllerCC movement))
                movement.SpeedModifier.AddModifier(SpeedModifier.EvaluateValue);

            State = WieldableStateType.Equipped;
        }

        IEnumerator IWieldable.Holster(float holsterSpeed)
        {
            if (State is WieldableStateType.Holstering or WieldableStateType.Hidden)
                yield break;

            State = WieldableStateType.Holstering;

            AudioPlayer.PlayDelayed(_holsterAudio, holsterSpeed);

            Animation.SetTrigger(WieldableAnimationConstants.HOLSTER);
            Animation.SetFloat(WieldableAnimationConstants.HOLSTER_SPEED, holsterSpeed);

            OnStateChanged(WieldableStateType.Holstering);

            for (float timer = Time.time + _holsterDuration / holsterSpeed; timer > Time.time;)
                yield return null;

            if (Character.TryGetCC(out IMovementControllerCC movement))
                movement.SpeedModifier.RemoveModifier(SpeedModifier.EvaluateValue);

            gameObject.SetActive(false);
            State = WieldableStateType.Hidden;
        }

        protected virtual void OnStateChanged(WieldableStateType state) { }
        protected virtual void OnCharacterChanged(ICharacter character) { }

        private void Awake()
        {
            State = WieldableStateType.Hidden;

            _currentCrosshairIndex = _baseCrosshair;
            AudioPlayer = GetComponent<IAudioPlayer>();

            Motion = gameObject.GetComponentInFirstChildren<IWieldableMotion>();

            if (Motion == null)
                Debug.LogError("No motion handler found under this wieldable!", gameObject);

            // Initialize animators
            var animators = gameObject.GetComponentsInChildren<IAnimator>(false);

            switch (animators.Length)
            {
                case 0:
                    Debug.LogError("No animator found found under this wieldable!", gameObject);
                    return;
                case 1:
                    Animation = animators[0];
                    break;
                default:
                    {
                        var animatorProxy = new MultiAnimator(animators);
                        Animation = animatorProxy;
                        break;
                    }
            }

            gameObject.SetActive(false);
        }
        
        #region Movement Speed
        public MovementModifierGroup SpeedModifier { get; } = new();
        #endregion

        #region Accuracy
        public float Accuracy { get; protected set; }

        private int _currentCrosshairIndex;

        public int CrosshairIndex
        {
            get => _currentCrosshairIndex;
            set
            {
                if (_currentCrosshairIndex == value)
                    return;

                _currentCrosshairIndex = value;
                CrosshairChanged?.Invoke(value);
            }
        }

        public event UnityAction<int> CrosshairChanged;
        public void ResetCrosshair() => CrosshairIndex = _baseCrosshair;
        public virtual bool IsCrosshairActive() => true;

        [Conditional("UNITY_EDITOR")]
        protected void Editor_CrosshairChanged()
        {
            if (Application.isPlaying)
                CrosshairIndex = _baseCrosshair;
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        private static bool? s_IsDebugMode;

        public static bool IsDebugMode
        {
            get
            {
                s_IsDebugMode ??= UnityEditor.EditorPrefs.GetBool("WieldableDebug", false);
                return s_IsDebugMode.Value;
            }
        }

        public static void EnableDebugMode(bool enable)
        {
            s_IsDebugMode = enable;
            UnityEditor.EditorPrefs.SetBool("WieldableDebug", enable);
        }

        private void OnGUI()
        {
            if (IsDebugMode)
                DrawDebugGUI();
        }

        protected virtual void DrawDebugGUI() { }
#endif
        #endregion
    }
}