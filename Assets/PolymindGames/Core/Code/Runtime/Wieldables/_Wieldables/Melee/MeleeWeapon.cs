using UnityEngine;
using System;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(IMeleeAttackHandler))]
    [AddComponentMenu("Polymind Games/Wieldables/Melee/Melee Weapon")]
    public sealed class MeleeWeapon : Wieldable, IUseInputHandler
    {
        [SerializeField]
        [Tooltip("Determines if the weapon can attack continuously while the use input is held down.")]
        private bool _attackContinuously = true;

        [SerializeField, Range(0f, 60f)]
        [Tooltip("The delay after attacking before the weapon is automatically hidden.")]
        private float _hideDelayAfterAttack;

        [SerializeField, Range(0f, 1f), SpaceArea]
        [Tooltip("The amount of accuracy kick when attacking.")]
        private float _accuracyKick = 0.3f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("The rate at which accuracy recovers after attacking.")]
        private float _accuracyRecover = 0.5f;
        
        private IAccuracyHandlerCC _characterAccuracy;
        private IMeleeAttackHandler _meleeHandler;
        private float _attackInaccuracy;
        private float _hideTimer = float.MaxValue;
        private bool _isUsing = false;


        public bool IsUsing => _isUsing;
        public ActionBlockHandler UseBlocker { get; } = new();
        
        public bool Use(WieldableInputPhase inputPhase)
        {
            if (inputPhase is WieldableInputPhase.Start || inputPhase is WieldableInputPhase.Hold && _attackContinuously)
            {
                if (_hideDelayAfterAttack > 0.01f)
                {
                    _hideTimer = Time.fixedTime + _hideDelayAfterAttack;
                    Animation.SetBool(WieldableAnimationConstants.IS_VISIBLE, true);
                }

                if (IsCrosshairActive())
                {
                    _meleeHandler.QuickAttack(Accuracy);
                    _attackInaccuracy += _accuracyKick;
                    _isUsing = true;
                    return true;
                }
            }

            _isUsing = false;
            return false;
        }

        public override bool IsCrosshairActive() => !UseBlocker.IsBlocked && _meleeHandler.CanAttack();

        protected override void OnCharacterChanged(ICharacter character)
        {
            _meleeHandler = GetComponent<IMeleeAttackHandler>();
            if (character == null || !character.TryGetCC(out _characterAccuracy))
                _characterAccuracy = new NullAccuracyHandler();
        }

        private void OnEnable()
        {
            if (_hideDelayAfterAttack > 0.01f)
                _hideTimer = 0f;
        }

        private void FixedUpdate()
        {
            if (_hideTimer < Time.fixedTime)
            {
                _hideTimer = float.MaxValue;
                Animation.SetBool(WieldableAnimationConstants.IS_VISIBLE, false);
            }

            float targetAccuracy = Mathf.Clamp01(_characterAccuracy.GetAccuracyMod() - _attackInaccuracy);
            _attackInaccuracy = Mathf.Clamp01(_attackInaccuracy - _accuracyRecover * Time.fixedDeltaTime);

            Accuracy = targetAccuracy;
        }

#if UNITY_EDITOR
        protected override void DrawDebugGUI()
        {
            GUILayout.Label($"Is Use Input Blocked: {UseBlocker.IsBlocked}");
            GUILayout.Label($"Current Speed Multiplier: {Math.Round(SpeedModifier.EvaluateValue(), 2)}");
        }
#endif
    }
}