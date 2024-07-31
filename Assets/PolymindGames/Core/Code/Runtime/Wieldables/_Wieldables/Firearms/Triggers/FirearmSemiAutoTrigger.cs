using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Triggers/Semi-Auto Trigger")]
    public class FirearmSemiAutoTrigger : FirearmTriggerBehaviour
    {
        [SerializeField, Range(0f, 10f), BeginGroup("Settings"), EndGroup]
        [Tooltip("The minimum time that can pass between consecutive shots.")]
        private float _pressCooldown;

        private float _shootTimer;


        protected override void TapTrigger()
        {
            if (Time.time < _shootTimer)
                return;

            RaiseShootEvent(1f);
            _shootTimer = Time.time + _pressCooldown;
        }
    }
}