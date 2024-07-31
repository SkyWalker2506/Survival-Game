using PolymindGames.WieldableSystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class ObjectCarryUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup]
        private CanvasGroup _canvasGroup;

        [SerializeField, Range(0.1f, 20f), EndGroup]
        private float _alphaLerpSpeed = 7f;

        private float _targetAlpha;


        protected override void Awake()
        {
            base.Awake();
            enabled = false;
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            var objCarry = character.GetCC<ICarriableControllerCC>();
            objCarry.ObjectCarryStarted += OnObjectCarryStart;
            objCarry.ObjectCarryStopped += OnObjectCarryEnd;

            enabled = objCarry.CarryCount > 0;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            var objCarry = character.GetCC<ICarriableControllerCC>();
            objCarry.ObjectCarryStarted -= OnObjectCarryStart;
            objCarry.ObjectCarryStopped -= OnObjectCarryEnd;
        }

        private void OnObjectCarryStart(CarriablePickup carriable)
        {
            _targetAlpha = 1f;
            enabled = true;
        }

        private void OnObjectCarryEnd() => _targetAlpha = 0f;

        private void Update()
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * _alphaLerpSpeed);

            if (_canvasGroup.alpha < 0.001f)
                enabled = false;
        }
    }
}