using System;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class FillBarUI
    {
        [SerializeField]
        private GameObject _background;

        [SerializeField]
        private Image _fillBar;

        [SerializeField]
        private Gradient _colorGradient;

        private bool _active = true;


        public void SetActive(bool active)
        {
            if (_active == active)
                return;

            if (_background != null)
                _background.SetActive(active);

            if (_fillBar != null)
                _fillBar.enabled = active;

            _active = active;
        }

        public void SetFillAmount(float fillAmount)
        {
            if (_fillBar != null)
            {
                _fillBar.color = _colorGradient.Evaluate(fillAmount);
                _fillBar.fillAmount = fillAmount;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (_fillBar != null)
                _fillBar.color = new Color(_fillBar.color.r, _fillBar.color.g, _fillBar.color.b, alpha);
        }
    }
}