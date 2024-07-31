using PolymindGames.ProceduralMotion;
using PolymindGames.WieldableSystem;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    public sealed class FirearmReloadUI : CharacterUIBehaviour
    {
        [SerializeField, NotNull, BeginGroup("Settings")]
        private TextMeshProUGUI _text;

        [SerializeField]
        private Gradient _color;

        [SerializeField, Range(0f, 1f), EndGroup]
        private float _activatePercent = 0.35f;

        [SerializeField, BeginGroup("Animations")]
        private TweenAnimation _enableTextAnimation;

        [SerializeField, EndGroup]
        private TweenAnimation _changeTextAnimation;

        private const string RELOAD = "Reload";
        private const string LOW_AMMO = "Low Ammo";
        private const string NO_AMMO = "No Ammo";

        private IFirearmMagazine _magazine;
        private IFirearm _firearm;
        private bool _isVisible;


        protected override void OnCharacterAttached(ICharacter character)
        {
            _text.gameObject.SetActive(false);
            character.GetCC<IWieldableControllerCC>().EquippingStopped += OnWieldableEquipped;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            character.GetCC<IWieldableControllerCC>().EquippingStopped -= OnWieldableEquipped;
        }

        private void OnWieldableEquipped(IWieldable wieldable)
        {
            // Unsubscribe from previous firearm
            if (_firearm != null)
            {
                _firearm.RemoveChangedListener(AttachmentType.Reloader, OnReloaderChanged);
                _firearm = null;
                OnReloaderChanged();
            }

            // Subscribe to current firearm
            if (wieldable is IFirearm firearm)
            {
                _firearm = firearm;
                _firearm.AddChangedListener(AttachmentType.Reloader, OnReloaderChanged);
                OnReloaderChanged();
            }
            else
                SetVisibility(false);
        }

        private void OnReloaderChanged()
        {
            // Prev reloader
            if (_magazine != null)
                _magazine.AmmoInMagazineChanged -= UpdateMagazineText;

            _magazine = _firearm?.Magazine;

            // Current reloader
            if (_magazine != null)
            {
                _magazine.AmmoInMagazineChanged += UpdateMagazineText;
                UpdateMagazineText(_magazine.AmmoInMagazine, _magazine.AmmoInMagazine);
            }
        }

        private void UpdateMagazineText(int prevAmmo, int currentAmmo)
        {
            float ammoPercent = (float)currentAmmo / (float)_magazine.MagazineSize;
            bool isVisbile = ammoPercent < _activatePercent;
            SetVisibility(isVisbile);
            
            // Animates and updates the text.
            if (isVisbile)
                UpdateText(ammoPercent);
        }

        private void UpdateText(float percent)
        {
            _text.color = _color.Evaluate(percent);
            _text.text = percent == 0f
                ? _firearm.StorageAmmo.HasAmmo() ? RELOAD : NO_AMMO
                : LOW_AMMO;
                
            _changeTextAnimation.PlayAnimation();
        }

        private void SetVisibility(bool value)
        {
            if (_isVisible == value)
                return;

            if (value)
                _enableTextAnimation.PlayAnimation();
            else
                _enableTextAnimation.CancelAnimation();

            _text.gameObject.SetActive(value);
            _isVisible = value;
        }
    }
}