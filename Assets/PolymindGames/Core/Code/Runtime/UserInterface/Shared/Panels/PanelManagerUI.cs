using System.Collections.Generic;
using PolymindGames.InputSystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class PanelManagerUI
    {
        private const int FREE_PANEL_INDEX = -1; 
        private const int PANELS_LAYER_COUNT = 8;

        private static readonly PanelManagerUI s_Instance = new();
        private readonly List<IPanel>[] _panels = new List<IPanel>[PANELS_LAYER_COUNT];
        


        #region Initialization
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reload()
        {
            if (s_Instance != null)
            {
                foreach (var panelList in s_Instance._panels)
                    panelList?.Clear();
            }
        }
#endif
        #endregion

        public static void ShowPanel(IPanel panel)
        {
            if (panel == null)
            {
                Debug.LogError("Cannot show a null panel.");
                return;
            }
            
            if (IsPanelStackable(panel))
                s_Instance.PushPanel_Internal(panel);
            else
                panel.ChangeVisibility(true);
        }

        public static void HidePanel(IPanel panel)
        {
            if (panel == null)
            {
                Debug.LogError("Cannot hide a null panel.");
                return;
            }

            if (IsPanelStackable(panel))
                s_Instance.PopPanel_Internal(panel);
            else
                panel.ChangeVisibility(false);
        }

        private static bool IsPanelStackable(IPanel panel)
        {
            int panelLayer = panel.PanelLayer;
            return panelLayer < PANELS_LAYER_COUNT && (panelLayer != FREE_PANEL_INDEX || panel.CanEscape);
        }

        private void PushPanel_Internal(IPanel panel)
        {
            var panelList = GetPanelListWithLayer(panel.PanelLayer);
            
            var prevTopPanel = panelList.Count > 0 ? panelList[panelList.Count - 1] : null;
            if (prevTopPanel == panel)
                return;

            if (panel.PanelLayer != FREE_PANEL_INDEX && prevTopPanel != null)
                prevTopPanel.ChangeVisibility(false);

            if (panel.CanEscape)
                InputManager.Instance.PushEscapeCallback(panel.Hide);
            
            // Set the new panel as the top panel.
            panelList.Remove(panel);
            panelList.Add(panel);
            
            panel.ChangeVisibility(true);
        }

        private void PopPanel_Internal(IPanel panel)
        {
            var panelList = GetPanelListWithLayer(panel.PanelLayer);
            int index = panelList.IndexOf(panel);

            if (index != -1)
            {
                panelList.RemoveAt(index);

                if (panel.CanEscape)
                    InputManager.Instance.PopEscapeCallback(panel.Hide);

                if (panel.PanelLayer != FREE_PANEL_INDEX)
                {
                    var topPanel = panelList.Count > 0 ? panelList[panelList.Count - 1] : null;
                    topPanel?.ChangeVisibility(true);
                }
            }
            
            panel.ChangeVisibility(false);
        }

        private List<IPanel> GetPanelListWithLayer(int layer)
        {
            layer = Mathf.Clamp(layer, 0, PANELS_LAYER_COUNT - 1);

            var panelList = _panels[layer];
            if (panelList == null)
            {
                panelList = new List<IPanel>(1);
                _panels[layer] = panelList;
            }

            return panelList;
        }
    }
}