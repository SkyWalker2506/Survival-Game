using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class DataDefinitionInfo<T> : DataInfo where T : DataDefinition<T>
    {
        [SerializeField]
        private TextMeshProUGUI _nameText;

        [SerializeField]
        private TextMeshProUGUI _descriptionText;

        [SerializeField]
        private Image _iconImg;


        public void UpdateInfo(T data)
        {
            if (data != null)
            {
                if (_nameText != null)
                    _nameText.text = data.Name;

                if (_descriptionText != null)
                    _descriptionText.text = data.Description;

                if (_iconImg != null)
                {
                    _iconImg.enabled = true;
                    _iconImg.sprite = data.Icon;
                }
            }
            else
            {
                if (_nameText != null)
                    _nameText.text = string.Empty;

                if (_descriptionText != null)
                    _descriptionText.text = string.Empty;

                if (_iconImg != null)
                    _iconImg.enabled = false;
            }
        }
    }
}