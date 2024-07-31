using System;
using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class ItemIconInfo : DataInfo
    {
        [SerializeField, NotNull]
        private Image _iconImg;

        [SerializeField]
        private Image _bgIconImg;
        
        
        public Image IconImage => _iconImg;
        public Image BgIconImage => _bgIconImg;

        public void UpdateInfo(IItem data)
        {
            if (data != null)
            {
                if (_bgIconImg != null)
                    _bgIconImg.enabled = false;

                _iconImg.enabled = true;
                _iconImg.sprite = data.Definition.Icon;
            }
            else
            {
                if (_bgIconImg != null)
                    _bgIconImg.enabled = true;

                _iconImg.enabled = false;
            }
        }
    }
}