using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine;
using System;
using TMPro;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Selectables/Selectable Tab")]
    public sealed class SelectableTabUI : SelectableUI
    {
        [Flags]
        private enum SelectMode
        {
            None = 0,
            EnablePanel = 1,
            EnableObject = 2,
            Everything = EnablePanel | EnableObject
        }

        [SerializeField, BeginGroup, EndGroup]
        private UnityEvent<SelectableUI> _onDeselected;

        [SerializeField, BeginGroup]
        private TextMeshProUGUI _nameText;

        [SerializeField]
        private SelectMode _selectMode;

        [SerializeField]
        [ShowIf(nameof(_selectMode), SelectMode.EnablePanel, Comparison = UnityComparisonMethod.Mask)]
        private PanelUI _panelToEnable;

        [SerializeField, EndGroup]
        [ShowIf(nameof(_selectMode), SelectMode.EnableObject, Comparison = UnityComparisonMethod.Mask)]
        private GameObject _objectToEnable;
        

        public string TabName
        {
            get => _nameText == null ? string.Empty : _nameText.text;
            set
            {
                if (_nameText != null)
                    _nameText.text = value;
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (IsSelected)
            {
                if ((_selectMode & SelectMode.EnablePanel) == SelectMode.EnablePanel)
                    _panelToEnable.Show();
                
                if ((_selectMode & SelectMode.EnableObject) == SelectMode.EnableObject)
                    _objectToEnable.SetActive(true);
            }
        }

        public override void Deselect()
        {
            if (!IsSelected)
                return;

            base.Deselect();

            if ((_selectMode & SelectMode.EnablePanel) == SelectMode.EnablePanel)
                _panelToEnable.Hide();

            if ((_selectMode & SelectMode.EnableObject) == SelectMode.EnableObject)
                _objectToEnable.SetActive(false);

            _onDeselected.Invoke(this);
        }

        #region Editor
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (_nameText == null)
                _nameText = GetComponentInChildren<TextMeshProUGUI>();
        }
#endif
        #endregion
    }
}