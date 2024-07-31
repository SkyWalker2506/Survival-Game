using System;
using PolymindGames.InventorySystem;
using PolymindGames.WieldableSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class ItemFiremodeInfo : DataInfo
    {
        [SerializeField]
        private TextMeshProUGUI _firemodeText;

        [SerializeField]
        private Image _firemodeImg;
        
        private IFirearmIndexModeHandler _firemodeHandler;
        private IWieldableInventory _selection;


        public void UpdateInfo(IItem data)
        {
            if (IsItemValid(data))
            {
                var info = FirearmFiremodeUI.Instance.GetFiremodeInfoFromAttachment(_firemodeHandler.CurrentMode);
                if (info == null)
                    return;

                if (_firemodeText != null)
                    _firemodeText.text = info.Name;

                if (_firemodeImg != null)
                {
                    _firemodeImg.enabled = _firemodeImg.sprite != null;
                    _firemodeImg.sprite = info.Icon;
                }
            }
            else
            {
                if (_firemodeText != null) _firemodeText.text = string.Empty;
                if (_firemodeImg != null) _firemodeImg.enabled = false;
            }
        }

        private bool IsItemValid(IItem item)
        {
            if (item == null)
                return false;

            _selection ??= Player.LocalPlayer.GetCC<IWieldableInventory>();
            var wieldable = _selection.GetWieldableWithId(item.Id);
            return wieldable != null && wieldable.gameObject.TryGetComponent(out _firemodeHandler);
        }
    }
}