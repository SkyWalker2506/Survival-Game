using PolymindGames.ProceduralMotion;
using PolymindGames.WieldableSystem;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

namespace PolymindGames.UserInterface
{
    public sealed class FirearmFiremodeUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup]
        [Tooltip("A UI text component that's used for displaying the currently selected fire mode.")]
        private TextMeshProUGUI _firemodeNameText;

        [SerializeField]
        private Image _firemodeIconImage;

        [SerializeField, EndGroup]
        [ReorderableList(ListStyle.Lined), LabelFromChild("Name")]
        private FiremodeInfo[] _firemodes;

        [SerializeField, BeginGroup("Animation")]
        [Tooltip("The animation sequence for showing the fire mode UI.")]
        private TweenSequence _showAnimation;

        [SerializeField]
        [Tooltip("The animation sequence for updating the fire mode UI.")]
        private TweenSequence _updateAnimation;

        [SerializeField, EndGroup]
        [Tooltip("The animation sequence for hiding the fire mode UI.")]
        private TweenSequence _hideAnimation;

        private IWieldableControllerCC _wieldableController;
        private IFirearmIndexModeHandler _indexAttachments;
        private bool _isAnimationActive;

        
        public static FirearmFiremodeUI Instance { get; private set; }

        public FiremodeInfo GetFiremodeInfoFromAttachment(FirearmAttachmentBehaviour attachment)
        {
            var targetType = attachment.GetType();
            for (int i = 0; i < _firemodes.Length; i++)
            {
                if (_firemodes[i].Type.Type == targetType)
                    return _firemodes[i];
            }

            return null;
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            Instance = this;
            _wieldableController = character.GetCC<IWieldableControllerCC>();
            _wieldableController.EquippingStopped += OnWieldableEquipped;

            _hideAnimation.SetTime(1f);
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnWieldableEquipped(IWieldable wieldable)
        {
            // Unsubscribe from previous index-based attachments handler
            if (_indexAttachments != null)
            {
                _indexAttachments.ModeChanged -= OnAttachmentIndexChanged;
                _indexAttachments = null;
            }

            // Subscribe to current index-based attachments handler
            if (wieldable.gameObject != null && wieldable.gameObject.TryGetComponent(out _indexAttachments))
            {
                _indexAttachments.ModeChanged += OnAttachmentIndexChanged;
                SetDisplayedFiremode(_indexAttachments.CurrentMode);

                if (!_isAnimationActive)
                {
                    _showAnimation.Play();
                    _isAnimationActive = true;
                }
            }
            else
            {
                _indexAttachments = null;

                if (_isAnimationActive)
                {
                    _hideAnimation.Play();
                    _isAnimationActive = false;
                }
            }

            return;

            void OnAttachmentIndexChanged(FirearmAttachmentBehaviour attachment)
            {
                SetDisplayedFiremode(attachment);
                _updateAnimation.Play();
            }
        }

        private void SetDisplayedFiremode(FirearmAttachmentBehaviour attachment)
        {
            var info = GetFiremodeInfoFromAttachment(attachment);
            if (info == null)
                return;

            if (_firemodeNameText != null)
                _firemodeNameText.text = info.Name;

            if (_firemodeIconImage != null)
                _firemodeIconImage.sprite = info.Icon;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _showAnimation?.Validate(gameObject);
            _updateAnimation?.Validate(gameObject);
            _hideAnimation?.Validate(gameObject);
        }
#endif
        
        [Serializable]
        public sealed class FiremodeInfo
        {
            [TypeConstraint(typeof(FirearmAttachmentBehaviour), AllowAbstract = false, TypeGrouping = TypeGrouping.ByFlatName)]
            public SerializedType Type;
            public string Name;
            public Sprite Icon;
        }
    }
}