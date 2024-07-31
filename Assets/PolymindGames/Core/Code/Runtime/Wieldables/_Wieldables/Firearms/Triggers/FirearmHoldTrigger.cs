using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Triggers/Hold Trigger")]
    public class FirearmHoldTrigger : FirearmTriggerBehaviour
    {
        [SerializeField, Range(0f, 3f), BeginGroup, EndGroup]
        private float _holdTime = 0.2f;

        [SerializeField, BeginGroup("Effects")]
        private SimpleAudioData _holdAudio;

        [SerializeField, EndGroup]
        private MovingPartsHandler _movingParts;

        private float _pressTimer;


        public override void HoldTrigger()
        {
            base.HoldTrigger();

            if (_pressTimer < 0f)
                return;

            if (Time.time > _pressTimer)
            {
                RaiseShootEvent(1f);
                _pressTimer = -100f;
                _movingParts.StopMoving();
            }
        }

        public override void ReleaseTrigger()
        {
            base.ReleaseTrigger();
            _movingParts.StopMoving();
        }

        protected override void TapTrigger()
        {
            _movingParts.StartMoving();
            _pressTimer = _holdTime + Time.time;
            Wieldable.AudioPlayer.Play(_holdAudio);
        }

        private void LateUpdate() => _movingParts.UpdateMoving();
    }
}