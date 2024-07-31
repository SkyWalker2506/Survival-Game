using System.Collections;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Triggers/Burst-Fire Trigger")]
    public sealed class FirearmBurstFireTrigger : FirearmTriggerBehaviour
    {
        [SerializeField, Range(0, 100), BeginGroup("Settings")]
        [Tooltip("How many times in succession will the trigger be pressed.")]
        private int _burstLength = 3;

        [SerializeField, Range(0f, 100f)]
        [Tooltip("How much time it takes to complete the burst.")]
        private float _burstDuration = 0.3f;

        [SerializeField, Range(0f, 100f), EndGroup]
        [Tooltip("The minimum time that can pass between consecutive bursts.")]
        private float _burstPause = 0.35f;

        private float _shootTimer;


        protected override void TapTrigger()
        {
            if (Time.time < _shootTimer)
                return;

            if (Firearm.Magazine.IsReloading || Firearm.Magazine.IsMagazineEmpty())
                RaiseShootEvent(1f);
            else
                StartCoroutine(C_DoBurst());

            _shootTimer = Time.time + _burstDuration + _burstPause;
        }

        private IEnumerator C_DoBurst()
        {
            for (int i = 0; i < _burstLength; i++)
            {
                RaiseShootEvent(1f);

                for (float waitTimer = Time.time + _burstDuration / _burstLength; waitTimer > Time.time;)
                    yield return null;
            }
        }
    }
}