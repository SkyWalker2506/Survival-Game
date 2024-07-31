using PolymindGames.InputSystem;
using PolymindGames.WieldableSystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_wieldables#crosshairs")]
    public sealed class CrosshairControllerUI : CharacterUIBehaviour
    {
        [SerializeField, NotNull, BeginGroup]
        [Tooltip("The canvas group used to fade a crosshair in & out.")]
        private CanvasGroup _crosshairsGroup;

        [SerializeField, NotNull, EndGroup]
        [Tooltip("Default Crosshair: The default (starting) crosshair.")]
        private HitmarkerBehaviourUI _hitmarker;

        [SerializeField, Range(0.1f, 1f), BeginGroup]
        [Tooltip("The max crosshair alpha.")]
        private float _crosshairAlpha = 0.7f;

        [SerializeField, Range(0.5f, 35f), EndGroup]
        [Tooltip("The speed at which the crosshairs will change their alpha.")]
        private float _alphaLerpSpeed = 5f;

        private IInteractionHandlerCC _interactionHandler;
        private CrosshairBehaviourUI _currentCrosshair;
        private ICrosshairHandler _crosshairHandler;
        private CrosshairBehaviourUI[] _crosshairs;
        private bool _isCrosshairEnabled;
        private float _currentCrosshairScale;
        private float _currentCrosshairAlpha;

        private const float UNUSABLE_ALPHA = 0.25f;


        protected override void Awake()
        {
            base.Awake();

            _crosshairs = GetComponentsInChildren<CrosshairBehaviourUI>(true);
            foreach (var crosshair in _crosshairs)
                crosshair.Hide();

            enabled = false;
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            _interactionHandler = character.GetCC<IInteractionHandlerCC>();

            var wieldableController = character.GetCC<IWieldableControllerCC>();
            wieldableController.HolsteringStarted += OnHolsterStart;
            wieldableController.EquippingStarted += OnEquipStart;
            OnEquipStart(wieldableController.ActiveWieldable);

            DamageTracker.AddListener(character, OnDamageDealt);
            
            enabled = true;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            var wieldableController = character.GetCC<IWieldableControllerCC>();
            wieldableController.HolsteringStarted -= OnHolsterStart;
            wieldableController.EquippingStarted -= OnEquipStart;

            DamageTracker.RemoveListener(character, OnDamageDealt);
            
            enabled = false;
        }

        private void OnEquipStart(IWieldable wieldable)
        {
            if (wieldable is ICrosshairHandler crosshairHandler)
            {
                _crosshairHandler = crosshairHandler;
                crosshairHandler.CrosshairChanged += SetCrosshair;
                SetCrosshair(crosshairHandler.CrosshairIndex);
            }
            else
                SetCrosshair(0);
        }

        private void OnHolsterStart(IWieldable wieldable)
        {
            if (_crosshairHandler != null)
                _crosshairHandler.CrosshairChanged -= SetCrosshair;

            _crosshairHandler = null;
            _isCrosshairEnabled = false;

            if (_currentCrosshair != null)
                _currentCrosshair.SetSize(1f, 0f);
        }

        private void SetCrosshair(int index)
        {
            CrosshairBehaviourUI prevCrosshair = _currentCrosshair;
            CrosshairBehaviourUI newCrosshair = index < 0 ? null : _crosshairs[Mathf.Clamp(index, 0, _crosshairs.Length - 1)];

            if (prevCrosshair != null && prevCrosshair != newCrosshair)
                prevCrosshair.Hide();

            _currentCrosshair = newCrosshair;

            if (newCrosshair != null)
            {
                _currentCrosshairScale = 0f;
                _currentCrosshairAlpha = 0f;
                _isCrosshairEnabled = true;

                newCrosshair.Show();
                newCrosshair.SetSize(1f, 0f);
                newCrosshair.SetColor(GameplayOptions.Instance.CrosshairColor);
            }
            else
                _isCrosshairEnabled = false;
        }

        private void OnDamageDealt(IDamageReceiver receiver, DamageResult result, float damage, in DamageArgs args)
        {
            if (receiver.Character != Character)
                _hitmarker.StartAnimation(result);
        }

        private void LateUpdate()
        {
            float accuracy = _crosshairHandler?.Accuracy ?? 1f;

            if (_isCrosshairEnabled)
            {
                float lerpSpeed = Time.deltaTime * _alphaLerpSpeed;

                bool isEnabled = _isCrosshairEnabled && _interactionHandler.Hoverable == null
                                                      && !InputManager.Instance.HasEscapeCallbacks;

                // Calculate and update the crosshair ALPHA.
                float targetAlpha = isEnabled ? 1f * _crosshairAlpha : 0f;
                targetAlpha *= _crosshairHandler != null && _crosshairHandler.IsCrosshairActive() ? 1f : UNUSABLE_ALPHA;
                _currentCrosshairAlpha = Mathf.Lerp(_currentCrosshairAlpha, targetAlpha, lerpSpeed);
                _crosshairsGroup.alpha = _currentCrosshairAlpha;

                // Calculate and update the crosshair SIZE.
                _currentCrosshairScale = Mathf.Lerp(_currentCrosshairScale, isEnabled ? 1f : 0f, lerpSpeed);
                _currentCrosshair.SetSize(accuracy, _currentCrosshairScale);
            }

            if (_hitmarker.IsActive)
                _hitmarker.UpdateAnimation(accuracy);
        }
    }
}