using System;
using System.Collections;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Casing Ejectors/Cartridge-Swap Ejector")]
    public sealed class FirearmCartridgeSwapEjector : FirearmCasingEjectorBehaviour
    {
        [SerializeField, Range(0, 100), BeginGroup("Settings")]
        private int _cartridgesCount;

        [SerializeField, Range(0f, 10f)]
        private float _reloadUpdateDelay = 0.5f;

        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false), SpaceArea]
        private GameObject[] _fullCartridges = Array.Empty<GameObject>();

        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false)]
        private GameObject[] _emptyCartridges = Array.Empty<GameObject>();

        [SerializeField, EndGroup]
        private DelayedAudioData _swapCartridgeAudio = DelayedAudioData.Default;

        private IFirearmMagazine _magazine;


        public override void Eject()
        {
            Wieldable.AudioPlayer.PlayDelayed(_swapCartridgeAudio);
            CoroutineUtils.InvokeDelayed(this, OnEject, EjectDuration);
        }

        private void OnEject()
        {
            int magSize = _magazine.MagazineSize;
            int currentInMag = _magazine.AmmoInMagazine;

            int index = magSize - (magSize - currentInMag);
            if (index < _cartridgesCount)
                EnableCartridge(index, false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _magazine = Firearm.Magazine;
            _magazine.ReloadStarted += OnReloadStart;

            Firearm.AddChangedListener(AttachmentType.Reloader, OnReloaderChanged);
        }

        private void OnDisable()
        {
            Firearm.RemoveChangedListener(AttachmentType.Reloader, OnReloaderChanged);

            if (_magazine != null)
            {
                _magazine.ReloadStarted -= OnReloadStart;
                _magazine = null;
            }
        }

        private void OnReloaderChanged()
        {
            _magazine.ReloadStarted -= OnReloadStart;
            _magazine = Firearm.Magazine;
            _magazine.ReloadStarted += OnReloadStart;
        }

        private void OnReloadStart(int loadCount)
        {
            StartCoroutine(C_ReloadUpdateCartridges(_magazine.MagazineSize, loadCount));
        }

        private IEnumerator C_ReloadUpdateCartridges(int currentInMag, int reloadingAmount)
        {
            for (float waitTimer = Time.time + _reloadUpdateDelay; waitTimer > Time.time;)
                yield return null;

            if (!_magazine.IsReloading)
                yield break;

            int numberOfCartridgesToEnable = Mathf.Clamp(currentInMag + reloadingAmount, 0, _cartridgesCount);

            for (int i = 0; i < numberOfCartridgesToEnable; i++)
                EnableCartridge(i, true);
        }

        private void EnableCartridge(int index, bool enable)
        {
            var fullCartridge = _fullCartridges[index];
            if (fullCartridge != null)
                fullCartridge.SetActive(enable);

            var emptyCartridge = _emptyCartridges[index];
            if (emptyCartridge != null)
                emptyCartridge.SetActive(!enable);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            IFirearmMagazine magazine = null;
            foreach (var rld in transform.root.GetComponentsInChildren<FirearmMagazineBehaviour>())
            {
                if (rld.IsAttached)
                {
                    magazine = rld;
                    break;
                }
            }

            _cartridgesCount = magazine?.MagazineSize ?? 0;
            _fullCartridges = new GameObject[_cartridgesCount];
            _emptyCartridges = new GameObject[_cartridgesCount];
        }
#endif
    }
}