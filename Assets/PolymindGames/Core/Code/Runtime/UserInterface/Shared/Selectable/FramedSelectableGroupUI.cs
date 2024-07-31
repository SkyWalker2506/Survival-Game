using UnityEngine.UI;
using UnityEngine;
using System;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Selectables/Framed Selectable Group")]
    public class FramedSelectableGroupUI : SelectableGroupUI
    {
        [BeginGroup("Frame Settings")]
        [SerializeField, PrefabObjectOnly, NotNull]
        private RectTransform _selectionFrame;

        [SerializeField]
        private Vector2 _selectionOffset = Vector2.zero;

        [SerializeField, EndGroup]
        private FrameSelectionMatchType _selectionMatchFlags;

        private RectTransform _frame;
        private bool _frameActive = true;


        protected override void OnSelectedChanged(SelectableUI selectable)
        {
            if (_frame == null)
                CreateFrame();

            if (selectable == null || selectable.gameObject.HasComponent<RaycastMaskUI>())
            {
                EnableFrame(false);
                return;
            }

            EnableFrame(true);

            _frame.SetParent(selectable.transform);
            _frame.anchoredPosition = _selectionOffset;
            _frame.localRotation = Quaternion.identity;
            _frame.localScale = Vector3.one;
            var localPos = _frame.localPosition;
            _frame.localPosition = new Vector3(localPos.x, localPos.y, 0f);

            bool matchXSize = (_selectionMatchFlags & FrameSelectionMatchType.MatchXSize) == FrameSelectionMatchType.MatchXSize;
            bool matchYSize = (_selectionMatchFlags & FrameSelectionMatchType.MatchYSize) == FrameSelectionMatchType.MatchYSize;

            if (matchXSize || matchYSize)
            {
                var frameSize = _frame.sizeDelta;
                var selectableSize = ((RectTransform)selectable.transform).sizeDelta;

                _frame.sizeDelta = new Vector2(matchXSize ? selectableSize.x : frameSize.x, matchYSize ? selectableSize.y : frameSize.y);
            }

            bool matchColor = (_selectionMatchFlags & FrameSelectionMatchType.MatchColor) == FrameSelectionMatchType.MatchColor;

            if (matchColor && _frame.TryGetComponent<Graphic>(out var frameGraphic))
                frameGraphic.color = selectable.GetComponent<Graphic>().color;
        }

        private void CreateFrame()
        {
            _frame = Instantiate(_selectionFrame, transform);
            _frameActive = _frame.gameObject.activeSelf;
        }

        private void EnableFrame(bool enable)
        {
            if (_frameActive == enable)
                return;

            _frame.gameObject.SetActive(enable);
            _frameActive = enable;
        }

        #region Internal
        [Flags]
        private enum FrameSelectionMatchType
        {
            MatchColor = 1,
            MatchXSize = 2,
            MatchYSize = 4
        }
        #endregion
    }
}