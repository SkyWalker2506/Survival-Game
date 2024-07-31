using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Melee/Polearm")]
    public sealed class Polearm : Wieldable, IAimInputHandler, IUseInputHandler
    {
        [SerializeField]
        [Tooltip("Determines if the weapon can attack continuously while the use input is held down.")]
        private bool _attackContinuously = true;

        [SerializeField, Range(0f, 5f)]
        [Tooltip("For how long should this melee weapon be unable to be used after aiming.")]
        private float _aimAttackCooldown = 0.3f;
        
        [SerializeField, Range(0f, 1f), SpaceArea]
        [Tooltip("The amount of accuracy kick when attacking.")]
        private float _accuracyKick = 0.3f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("The rate at which accuracy recovers after attacking.")]
        private float _accuracyRecover = 0.5f;
        
        private IAccuracyHandlerCC _characterAccuracy;
        private IMeleeAttackHandler _meleeHandler;
        private float _attackInaccuracy;
        private ISight _meleeSight;
        private float _useTimer;
        private bool _isUsing;
        
        
        public bool IsAiming => _meleeSight?.IsAiming ?? false;
        public bool IsUsing => _isUsing;
        public ActionBlockHandler AimBlocker { get; } = new();
        public ActionBlockHandler UseBlocker { get; } = new();

        public bool Use(WieldableInputPhase inputPhase)
        {
            if (inputPhase is WieldableInputPhase.Start || inputPhase is WieldableInputPhase.Hold && _attackContinuously)
            {
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
        
        public bool Aim(WieldableInputPhase inputPhase) => inputPhase switch
        {
            WieldableInputPhase.Start => StartAiming(),
            WieldableInputPhase.End => EndAiming(),
            _ => false
        };

        public override bool IsCrosshairActive() => _useTimer < Time.time && !UseBlocker.IsBlocked && _meleeHandler.CanAttack();
        
        protected override void OnCharacterChanged(ICharacter character)
        {
            _meleeSight = GetComponent<ISight>();
            _meleeHandler = GetComponent<IMeleeAttackHandler>();

            AimBlocker.OnBlocked += ForceEndAiming;

            if (character == null || !character.TryGetCC(out _characterAccuracy))
                _characterAccuracy = new NullAccuracyHandler();

            return;

            void ForceEndAiming() => EndAiming();
        }

        private bool StartAiming()
        {
            if (_meleeSight == null || AimBlocker.IsBlocked)
                return false;

            if (_meleeSight.StartAim())
            {
                _useTimer = Time.time + _aimAttackCooldown;
                return true;
            }

            return false;
        }

        private bool EndAiming()
        {
            if (_meleeSight == null)
                return false;

            if (_meleeSight.EndAim())
            {
                _useTimer = Time.time + _aimAttackCooldown;
                return true;
            }

            return false;
        }

        private void FixedUpdate()
        {
            float targetAccuracy = Mathf.Clamp01(_characterAccuracy.GetAccuracyMod() - _attackInaccuracy);
            _attackInaccuracy = Mathf.Clamp01(_attackInaccuracy - _accuracyRecover * Time.fixedDeltaTime);

            Accuracy = targetAccuracy;
        }
    }
}