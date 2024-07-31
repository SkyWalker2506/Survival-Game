using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_interaction")]
    public sealed class ItemPickupInteractionUI : CharacterUIBehaviour, IInteractableInfoDisplayer
    {
        [SerializeField, BeginGroup("Settings")]
        [Tooltip("The main rect transform that will move the interactable's center (should be parent of everything else).")]
        private RectTransform _rectTransform;

        [SerializeField]
        [Tooltip("The canvas group used to fade the item pickup displayer in & out.")]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        [Tooltip("An image that used in showing the time the current interactable has been interacted with.")]
        private Image _interactProgressImg;

        [SerializeField, EndGroup]
        [Tooltip("An offset that will be applied to the position of the 'rect transform'")]
        private Vector3 _customItemOffset;

        [SerializeField, IgnoreParent, BeginGroup("Item Info")]
        private ItemNameInfo _nameInfo;

        [SerializeField, IgnoreParent]
        private ItemDescriptionInfo _descriptionInfo;

        [SerializeField, IgnoreParent]
        private ItemIconInfo _iconInfo;

        [SerializeField, IgnoreParent]
        private ItemStackInfo _stackInfo;

        [SerializeField, IgnoreParent]
        private ItemWeightInfo _weightInfo;

        [SerializeField, EndGroup, SpaceArea]
        private UnityEvent _onItemChanged;

        private Bounds _itemPickupBounds;
        private bool _isVisible;


        public bool TrySetHoverable(IHoverable hoverable)
        {
            if (hoverable != null && hoverable.gameObject.TryGetComponent(out ItemPickup itemPickup))
            {
                if (itemPickup is not ItemPickupBundle)
                {
                    SetItemPickup(itemPickup);
                    _isVisible = true;
                    enabled = true;
                    return true;
                }
            }

            _isVisible = false;
            return false;
        }

        private void SetItemPickup(ItemPickup pickup)
        {
            var item = pickup.AttachedItem;
            _nameInfo.UpdateInfo(item);
            _descriptionInfo.UpdateInfo(item);
            _iconInfo.UpdateInfo(item);
            _stackInfo.UpdateInfo(item);
            _weightInfo.UpdateInfo(item);
            _onItemChanged.Invoke();
            _itemPickupBounds = pickup.GetComponent<Collider>().bounds;
        }

        public void SetInteractionProgress(float progress) => _interactProgressImg.fillAmount = progress;

        protected override void Awake()
        {
            base.Awake();
            enabled = false;
            _interactProgressImg.fillAmount = 0f;
            _canvasGroup.alpha = 0f;
        }

        private void Update()
        {
            float boundsMedian = GetMedian(_itemPickupBounds.extents.x, _itemPickupBounds.extents.y, _itemPickupBounds.extents.z);
            Vector3 screenPosition = UnityUtils.CachedMainCamera.WorldToScreenPoint(_itemPickupBounds.center + Character.transform.right * boundsMedian);
            PositionAtScreenPoint(_rectTransform, screenPosition + _customItemOffset);

            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, _isVisible ? 1f : 0f, Time.unscaledDeltaTime * 10f);

            if (!_isVisible && _canvasGroup.alpha < 0.01f)
            {
                _canvasGroup.alpha = 0f;
                enabled = false;
            }
        }

        private static float GetMedian(params float[] values)
        {
            float sum = 0f;

            for (int i = 0; i < values.Length; i++)
                sum += values[i];

            return sum / values.Length;
        }

        private static void PositionAtScreenPoint(RectTransform rectTransform, Vector2 screenPosition)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, screenPosition, null, out Vector2 position))
                rectTransform.anchoredPosition = position;
        }
    }
}