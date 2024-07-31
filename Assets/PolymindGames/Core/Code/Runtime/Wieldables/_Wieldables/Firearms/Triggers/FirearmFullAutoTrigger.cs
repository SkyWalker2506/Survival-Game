using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Triggers/Full-Auto Trigger")]
    public class FirearmFullAutoTrigger : FirearmTriggerBehaviour
    {
        [SerializeField, Range(0, 10000), BeginGroup("Settings"), EndGroup]
        [Tooltip("The maximum amount of shots that can be executed in a minute.")]
        private int _roundsPerMinute = 450;

        private float _shootTimer;


        public override void HoldTrigger()
        {
            base.HoldTrigger();

            if (Time.time < _shootTimer)
                return;

            RaiseShootEvent(1f);

            _shootTimer = Time.time + 60f / _roundsPerMinute;
        }

        protected override void TapTrigger()
        {
            if (Time.time < _shootTimer)
                return;

            if (Firearm.Magazine.IsReloading || Firearm.Magazine.IsMagazineEmpty())
                RaiseShootEvent(1f);
        }
    }
}