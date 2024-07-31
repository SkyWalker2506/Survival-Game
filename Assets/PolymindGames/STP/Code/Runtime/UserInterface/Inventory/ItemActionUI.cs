using PolymindGames.InventorySystem;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(ButtonUI))]
    public sealed class ItemActionUI : MonoBehaviour
    {
        [SerializeField, NotNull]
        private ButtonUI _button;

        [SerializeField, NotNull]
        private Image _iconImage;

        [SerializeField, NotNull]
        private TextMeshProUGUI _nameText;

        private Coroutine _actionCoroutine;
        private ItemAction _itemAction;
        private ICharacter _character;
        private ItemSlot _itemSlot;


        public void SetAction(ItemAction itemAction, ItemSlot itemSlot, ICharacter character)
        {
            // If nothing changed, return..
            if (_itemSlot == itemSlot)
                return;
            
            _itemSlot = itemSlot;
            if (itemSlot != null)
            {
                _itemAction = itemAction;
                _character = character;
                _iconImage.sprite = _itemAction.DisplayIcon;
                _nameText.text = _itemAction.DisplayName;
                gameObject.SetActive(CanBeEnabled());
            }
            else
                gameObject.SetActive(false);
        }

        private void Start()
        {
            gameObject.SetActive(false);
            _button.OnSelected += StartAction;
        }

        private void StartAction()
        {
            if (_itemSlot == null || _itemAction == null)
                return;

            if (_character == null)
            {
                Debug.LogWarning("This behaviour is not attached to a character.", gameObject);
                return;
            }

            float duration = _itemAction.GetDuration(_itemSlot, _character);
            _actionCoroutine = _itemAction.Perform(_itemSlot, _character);

            if (duration > 0.01f)
            {
                string actionVerb = _itemAction.DisplayVerb;

                var aParams = new CustomActionArgs(actionVerb + "...", duration, true, null, CancelAction);
                CustomActionManagerUI.Instance.StartAction(aParams);
            }
            
            return;

            void CancelAction()
            {
                if (_actionCoroutine != null && _itemSlot != null)
                    _itemAction.CancelAction(ref _actionCoroutine, _character);
            }
        }

        private bool CanBeEnabled() =>
               _itemSlot != null
            && _itemSlot.HasItem
            && _itemAction != null
            && _itemAction.IsPerformable(_itemSlot, _character);

#if UNITY_EDITOR
        private void Reset()
        {
            _button = GetComponent<ButtonUI>();
            _iconImage = GetComponentInChildren<Image>();
            _nameText = GetComponentInChildren<TextMeshProUGUI>();
        }
#endif
    }
}