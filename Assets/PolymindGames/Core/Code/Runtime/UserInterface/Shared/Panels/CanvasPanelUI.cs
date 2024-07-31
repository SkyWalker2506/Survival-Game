using System;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("Polymind Games/User Interface/Panels/Panel")]
    public class CanvasPanelUI : PanelUI
    {
        [SerializeField, BeginGroup("Canvas"), EndGroup]
        private CanvasShowMode _canvasShowMode = CanvasShowMode.Everything;

        private CanvasGroup _canvasGroup;


        protected override void OnShowPanel(bool show)
        {
            if ((_canvasShowMode & CanvasShowMode.EnableInteractable) == CanvasShowMode.EnableInteractable)
                _canvasGroup.interactable = show;

            if ((_canvasShowMode & CanvasShowMode.BlockRaycasts) == CanvasShowMode.BlockRaycasts)
                _canvasGroup.blocksRaycasts = show;

            if ((_canvasShowMode & CanvasShowMode.AlphaToOne) == CanvasShowMode.AlphaToOne)
                _canvasGroup.alpha = show ? 1f : 0f;
        }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }
#endif
        
        #region Internal
        [Flags]
        private enum CanvasShowMode
        {
            None = 0,
            EnableInteractable = 1,
            BlockRaycasts = 2,
            AlphaToOne = 4,
            Everything = EnableInteractable | BlockRaycasts | AlphaToOne
        }
        #endregion
    }
}