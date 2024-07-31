using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.UserInterface
{
    public sealed class ItemTooltipUI : CharacterUIBehaviour
    {
        [SerializeField, NotNull, BeginGroup]
        [Tooltip("The CanvasGroup component used for controlling the visibility of the tooltip.")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Range(1f, 100f), EndGroup]
        [Tooltip("The speed at which the tooltip is shown or hidden.")]
        private float _showSpeed = 10f;

        [SerializeField, IgnoreParent, BeginGroup("Item Info")]
        [Tooltip("The component responsible for displaying the item name.")]
        private ItemNameInfo _nameInfo;

        [SerializeField, IgnoreParent]
        [Tooltip("The component responsible for displaying the item description.")]
        private ItemDescriptionInfo _descriptionInfo;

        [SerializeField, IgnoreParent]
        [Tooltip("The component responsible for displaying the item icon.")]
        private ItemIconInfo _iconInfo;

        [SerializeField, IgnoreParent]
        [Tooltip("The component responsible for displaying the item stack information.")]
        private ItemStackInfo _stackInfo;

        [SerializeField, IgnoreParent, EndGroup]
        [Tooltip("The component responsible for displaying the item weight.")]
        private ItemWeightInfo _weightInfo;

        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("Event invoked when the displayed item in the tooltip changes.")]
        private UnityEvent _onItemChanged;
        
        private RectTransform _cachedRect;
        private RectTransform _cachedRectParent;
        private bool _isActive;
        private bool _wasDragging;


        protected override void Awake()
        {
            base.Awake();

            enabled = false;
            _canvasGroup.alpha = 0f;
            _cachedRect = (RectTransform)transform;
            _cachedRectParent = (RectTransform)_cachedRect.parent;
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            var inspection = character.GetCC<IInventoryInspectManagerCC>();
            inspection.InspectionStarted += OnInventoryInspectionStarted;
            inspection.InspectionEnded += OnInventoryInspectionEnded;
            ItemSelectorUI.Instance.HighlightedSlotChanged += UpdateTooltipInfo;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            var inspection = character.GetCC<IInventoryInspectManagerCC>();
            inspection.InspectionStarted -= OnInventoryInspectionStarted;
            inspection.InspectionEnded -= OnInventoryInspectionEnded;
            ItemSelectorUI.Instance.HighlightedSlotChanged -= UpdateTooltipInfo;
        }

        private void OnInventoryInspectionStarted() => enabled = true;

        private void OnInventoryInspectionEnded()
        {
            _canvasGroup.alpha = 0f;
            _isActive = false;
            enabled = false;
        }

        private void UpdateTooltipInfo(ItemSlotUI slot)
        {
            var item = slot != null ? slot.Item : null;
            _isActive = item != null;

            _nameInfo.UpdateInfo(item);
            _descriptionInfo.UpdateInfo(item);
            _iconInfo.UpdateInfo(item);
            _stackInfo.UpdateInfo(item);
            _weightInfo.UpdateInfo(item);
            _onItemChanged.Invoke();
        }

        private void LateUpdate()
        {
            bool isDragging = ItemDragger.Instance.IsDragging;

            if (_wasDragging && !isDragging)
                UpdateTooltipInfo(ItemSelectorUI.Instance.HighlightedSlot);

            UpdatePosition(RaycastManagerUI.Instance.GetCursorPosition());

            bool isActive = _isActive && !isDragging;
            float targetAlpha = isActive ? 1f : 0f;
            float lerpSpeed = isActive ? _showSpeed : _showSpeed * 1.5f;

            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, Time.deltaTime * lerpSpeed);
            _wasDragging = isDragging;
        }

        private void UpdatePosition(Vector2 pointerPosition)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_cachedRectParent, pointerPosition, null, out Vector2 position))
                _cachedRect.anchoredPosition = position;
        }
    }
}