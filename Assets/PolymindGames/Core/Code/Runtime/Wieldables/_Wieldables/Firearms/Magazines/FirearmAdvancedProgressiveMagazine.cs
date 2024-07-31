using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Reloaders/Advanced-Progressive-Reload Magazine")]
    public class FirearmAdvancedProgressiveMagazine : FirearmProgressiveMagazine
    {
        [SerializeField, BeginGroup("Magazine Ejection")]
        private Rigidbody _ejectedMagazinePrefab;

        [SerializeField, Range(0f, 20f), BeginIndent]
        [ShowIf(nameof(_ejectedMagazinePrefab), true)]
        private float _magazineEjectDelay = 0.5f;

        [SerializeField, NotNull]
        [ShowIf(nameof(_ejectedMagazinePrefab), true)]
        private Transform _magazineEjectRoot;

        [SerializeField]
        [ShowIf(nameof(_ejectedMagazinePrefab), true)]
        private Vector3 _magazineEjectionForce;

        [SerializeField, EndGroup, EndIndent]
        [ShowIf(nameof(_ejectedMagazinePrefab), true)]
        private Vector3 _magazineEjectionTorque;

        [SerializeField, BeginGroup("Moving Parts"), EndGroup]
        private MovingPartsHandler _movingParts;

        private const float MAGAZINE_DESTROY_DELAY = 30f;


        public override bool TryUseAmmo(int amount)
        {
            if (base.TryUseAmmo(amount))
            {
                if (IsMagazineEmpty)
                    _movingParts.StartMoving();

                return true;
            }

            return false;
        }


        protected override void OnEmptyReloadStart(IFirearmStorageAmmo ammo)
        {
            base.OnEmptyReloadStart(ammo);

            if (_ejectedMagazinePrefab != null)
                CoroutineUtils.InvokeDelayed(this, EjectMagazine, _magazineEjectDelay);

            _movingParts.StopMoving();
        }

        protected override void OnTacticalReloadStart(IFirearmStorageAmmo ammo)
        {
            base.OnTacticalReloadStart(ammo);

            _movingParts.StopMoving();
        }

        // If this weapon's magazine is empty, update the moving parts
        private void LateUpdate() => _movingParts.UpdateMoving();

        private void EjectMagazine()
        {
            var magazine = Instantiate(_ejectedMagazinePrefab, _magazineEjectRoot.position, _magazineEjectRoot.rotation);

            Vector3 force = Wieldable.Character.transform.TransformVector(_magazineEjectionForce);
            Vector3 torque = Wieldable.Character.transform.TransformVector(_magazineEjectionTorque);

            magazine.linearVelocity = force;
            magazine.angularVelocity = torque;

            CoroutineUtils.InvokeDelayedGlobal(DestroyMagazine, _magazineEjectDelay);

            return;

            void DestroyMagazine()
            {
                if (magazine != null)
                    Destroy(magazine.gameObject, MAGAZINE_DESTROY_DELAY);
            }
        }
    }
}