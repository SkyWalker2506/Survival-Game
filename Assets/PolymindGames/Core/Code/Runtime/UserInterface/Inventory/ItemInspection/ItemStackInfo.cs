using System;
using PolymindGames.InventorySystem;
using TMPro;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class ItemStackInfo : DataInfo
    {
        [SerializeField]
        private GameObject _stackObject;

        [SerializeField]
        private TextMeshProUGUI _stackTxt;


        public void UpdateInfo(IItem data)
        {
            if (data != null)
            {
                _stackObject.SetActive(data.StackCount > 1);
                _stackTxt.text = data.StackCount > 1 ? "x" + data.StackCount : string.Empty;
            }
            else
            {
                _stackObject.SetActive(false);
                _stackTxt.text = string.Empty;
            }
        }
    }
}