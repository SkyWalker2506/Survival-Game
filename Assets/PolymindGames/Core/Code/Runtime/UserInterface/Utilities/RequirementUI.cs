using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    public sealed class RequirementUI : MonoBehaviour
    {
        [SerializeField]
        private Image _icon;

        [SerializeField]
        private TextMeshProUGUI _amount;


        public void Display(Sprite icon, string amount, Color textColor)
        {
            _icon.sprite = icon;
            _amount.text = amount;
            _amount.color = textColor;
        }

        public void Display(Sprite icon, string amount)
        {
            _icon.sprite = icon;
            _amount.text = amount;
        }

        public void Enable(bool value) => gameObject.SetActive(value);
    }
}