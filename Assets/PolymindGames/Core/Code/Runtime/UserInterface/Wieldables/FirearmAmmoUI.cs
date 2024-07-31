using PolymindGames.ProceduralMotion;
using PolymindGames.WieldableSystem;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_wieldables#ammo")]
    public sealed class FirearmAmmoUI : CharacterUIBehaviour
    {
        [SerializeField, NotNull, BeginGroup("Magazine")]
        [Tooltip("A UI text component that's used for displaying the current ammo in the magazine.")]
        private TextMeshProUGUI _magazineText;

        [SerializeField]
        [Tooltip("The color gradient for visualizing the current ammo in the magazine.")]
        private Gradient _magazineColor;

        [SerializeField]
        [Tooltip("An image component that represents infinite storage of ammo.")]
        private Image _infiniteStorageImage;

        [SerializeField, NotNull, BeginGroup("Storage")]
        [Tooltip("A UI text component that's used for displaying the current ammo in the storage.")]
        private TextMeshProUGUI _storageText;

        [SerializeField]
        [Tooltip("The animation sequence for showing the ammo UI.")]
        private TweenSequence _showAnimation;

        [SerializeField]
        [Tooltip("The animation sequence for updating the ammo UI.")]
        private TweenSequence _updateAnimation;

        [SerializeField, EndGroup]
        [Tooltip("The animation sequence for hiding the ammo UI.")]
        private TweenSequence _hideAnimation;

        private IFirearmStorageAmmo _storageAmmo;
        private IFirearmMagazine _magazine;
        private IFirearm _firearm;
        private bool _isAnimationActive;


        protected override void OnCharacterAttached(ICharacter character)
        {
            _infiniteStorageImage.enabled = false;
            character.GetCC<IWieldableControllerCC>().EquippingStopped += OnWieldableEquipped;
            _hideAnimation.SetTime(1f);
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
                _firearm.RemoveChangedListener(AttachmentType.Ammo, OnAmmoChanged);
                _firearm.RemoveChangedListener(AttachmentType.Reloader, OnReloaderChanged);
                _firearm = null;

                OnReloaderChanged();
                OnAmmoChanged();
            }

            if (wieldable is IFirearm firearm)
            {
                // Subscribe to current firearm
                _firearm = firearm;
                _firearm.AddChangedListener(AttachmentType.Ammo, OnAmmoChanged);
                _firearm.AddChangedListener(AttachmentType.Reloader, OnReloaderChanged);

                OnReloaderChanged();
                OnAmmoChanged();
                
                if (!_isAnimationActive)
                {
                    _showAnimation.Play();
                    _isAnimationActive = true;
                }
                
            }
            else if (_isAnimationActive)
            {
                _hideAnimation.Play();
                _isAnimationActive = false;
            }
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

        private void OnAmmoChanged()
        {
            // Prev ammo
            if (_storageAmmo != null)
                _storageAmmo.AmmoCountChanged -= UpdateStorageText;

            _storageAmmo = _firearm?.StorageAmmo;

            // Current ammo
            if (_storageAmmo != null)
            {
                _storageAmmo.AmmoCountChanged += UpdateStorageText;
                UpdateStorageText(_storageAmmo.GetAmmoCount());
            }
        }

        private void UpdateMagazineText(int prevAmmo, int currentAmmo)
        {
            _magazineText.text = currentAmmo.ToString();
            _magazineText.color = _magazineColor.Evaluate(currentAmmo / (float)_magazine.MagazineSize);

            if (prevAmmo > currentAmmo)
                _updateAnimation.Play();
        }

        private void UpdateStorageText(int currentAmmo)
        {
            if (GameplayOptions.Instance.InfiniteStorageAmmo || currentAmmo > 100000)
            {
                _storageText.text = string.Empty;
                _infiniteStorageImage.enabled = true;
            }
            else
            {
                _storageText.text = currentAmmo.ToString();
                _infiniteStorageImage.enabled = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _showAnimation?.Validate(gameObject);
            _updateAnimation?.Validate(gameObject);
            _hideAnimation?.Validate(gameObject);
        }
#endif
    }
}