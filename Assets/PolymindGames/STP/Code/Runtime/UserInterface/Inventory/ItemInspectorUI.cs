using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class ItemInspectorUI : MonoBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The panel UI component used for displaying item information.")]
        private PanelUI _panel;

        [SerializeField, IgnoreParent, BeginGroup("Item Info")]
        [Tooltip("The component responsible for displaying the item name.")]
        private ItemNameInfo _nameInfo;

        [SerializeField, IgnoreParent]
        [Tooltip("The component responsible for displaying the item description.")]
        private ItemDescriptionInfo _descriptionInfo;

        [SerializeField, IgnoreParent]
        [Tooltip("The component responsible for displaying the item icon.")]
        private ItemIconInfo _iconInfo;

        [SerializeField, IgnoreParent, EndGroup]
        [Tooltip("The component responsible for displaying the item weight.")]
        private ItemWeightInfo _weightInfo;


        private void UpdateShownSlot(ItemSlotUI slot)
        {
            if (slot == null || !slot.HasItem)
            {
                _panel.Hide();
                return;
            }

            var item = slot.Item;

            _nameInfo.UpdateInfo(item);
            _descriptionInfo.UpdateInfo(item);
            _iconInfo.UpdateInfo(item);
            _weightInfo.UpdateInfo(item);
            _panel.Show();
        }

        private void OnEnable()
        {
            ItemSelectorUI.Instance.SelectedSlotChanged += UpdateShownSlot;
            if (ItemSelectorUI.Instance.SelectedSlot != null)
                UpdateShownSlot(ItemSelectorUI.Instance.SelectedSlot);
        }

        private void OnDisable() => ItemSelectorUI.Instance.SelectedSlotChanged -= UpdateShownSlot;
    }
}