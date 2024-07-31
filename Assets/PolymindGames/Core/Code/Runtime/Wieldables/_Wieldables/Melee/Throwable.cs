using System;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Melee/Throwable")]
    public sealed class Throwable : Wieldable, IUseInputHandler, IAimInputHandler
    {
        [SerializeField]
        [Tooltip("A display image that represents this throwable.")]
        private Sprite _displayIcon;

        private IMeleeAttackHandler _meleeHandler;
        private bool _isThrowing;


        public ActionBlockHandler UseBlocker { get; } = new();
        public ActionBlockHandler AimBlocker { get; } = new();
        public Sprite DisplayIcon => _displayIcon;
        public bool IsAiming => false;
        public bool IsUsing => false;

        public bool Use(WieldableInputPhase inputPhase)
        {
            if (inputPhase != WieldableInputPhase.Start || _isThrowing)
                return false;
            
            _meleeHandler.QuickAttack();
            _isThrowing = true;
            return true;
        }

        public bool Aim(WieldableInputPhase inputPhase) => inputPhase switch
        {
            WieldableInputPhase.Start => StartAiming(),
            WieldableInputPhase.End => EndAiming(),
            _ => false
        };

        public override bool IsCrosshairActive() => !_isThrowing && _meleeHandler.CanAttack();

        protected override void OnCharacterChanged(ICharacter character)
        {
            _meleeHandler = GetComponent<IMeleeAttackHandler>();
        }

        private bool StartAiming()
        {
            if (!_isThrowing)
            {
                _meleeHandler.QuickAttack(1f, true);
                _isThrowing = true;
            }

            return false;
        }

        private bool EndAiming()
        {
            return true;
        }

        private void OnEnable()
        {
            _isThrowing = false;
        }

#if UNITY_EDITOR
        protected override void DrawDebugGUI()
        {
            GUILayout.Label($"Is Use Input Blocked: {UseBlocker.IsBlocked}");
            GUILayout.Label($"Is Throwing: {_isThrowing}");
            GUILayout.Label($"Current Speed Multiplier: {Math.Round(SpeedModifier.EvaluateValue(), 2)}");
        }
#endif
    }
}