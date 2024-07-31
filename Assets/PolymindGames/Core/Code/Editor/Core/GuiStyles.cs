using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public static class GuiStyles
    {
        #region Colors
        public static readonly Color BlueColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.92f, 1.065f, 0.78f) : new Color(0.9f, 0.97f, 1.065f, 0.75f);
        public static readonly Color GreenColor = new(0.5f, 1f, 0.5f, 0.75f);
        public static readonly Color RedColor = new(1f, 0.5f, 0.5f, 0.75f);
        public static readonly Color LightRedColor = new(1f, 0.65f, 0.65f, 0.75f);
        public static readonly Color YellowColor = new(1f, 1f, 0.8f, 0.75f);
        public static readonly Color SplitterColor = EditorGUIUtility.isProSkin ? new Color(0.175f, 0.175f, 0.175f) : new Color(0.35f, 0.35f, 0.35f);
        private static readonly Color s_DefaultTextColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.7f) : new Color(0.1f, 0.1f, 0.1f, 0.85f);
        #endregion

        #region Styles
        private static GUIStyle s_Box;
        public static GUIStyle Box
        {
            get
            {
                s_Box ??= new GUIStyle("box");
                return s_Box;
            }
        }

        private static GUIStyle s_Title;
        public static GUIStyle Title
        {
            get
            {
                if (s_Title == null)
                {
                    s_Title = new GUIStyle(EditorStyles.toolbar);
                    s_Title.fontSize = 12;
                    s_Title.alignment = TextAnchor.MiddleCenter;
                    s_Title.normal.textColor *= 1.1f;
                }
                return s_Title;
            }
        }

        private static GUIStyle s_LargeTitleLabelCentered;
        public static GUIStyle LargeTitleLabelCentered
        {
            get
            {
                if (s_LargeTitleLabelCentered == null)
                {
                    s_LargeTitleLabelCentered = new GUIStyle(LargeTitleLabel)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }
                return s_LargeTitleLabelCentered;
            }
        }
        
        private static GUIStyle s_LargeTitleLabel;
        public static GUIStyle LargeTitleLabel
        {
            get
            {
                if (s_LargeTitleLabel == null)
                {
                    s_LargeTitleLabel = new GUIStyle(EditorStyles.boldLabel);
                    s_LargeTitleLabel.fontSize = 14;
                    s_LargeTitleLabel.normal.textColor = new Color(1, 1, 1, 0.65f);
                }
                return s_LargeTitleLabel;
            }
        }

        private static GUIStyle s_BoldMiniGreyLabel;
        public static GUIStyle BoldMiniGreyLabel
        {
            get
            {
                if (s_BoldMiniGreyLabel == null)
                {
                    s_BoldMiniGreyLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                    s_BoldMiniGreyLabel.alignment = TextAnchor.MiddleLeft;
                    s_BoldMiniGreyLabel.fontSize = 11;
                    s_BoldMiniGreyLabel.fontStyle = FontStyle.Bold;
                }
                return s_BoldMiniGreyLabel;
            }
        }

        private static GUIStyle s_StandardFoldout;
        public static GUIStyle StandardFoldout
        {
            get
            {
                if (s_StandardFoldout == null)
                {
                    s_StandardFoldout = new GUIStyle(EditorStyles.foldout);
                    s_StandardFoldout.normal.textColor = s_DefaultTextColor;
                    s_StandardFoldout.padding.left += 2;
                    s_StandardFoldout.fontSize = 11;
                    s_StandardFoldout.richText = true;
                }
                return s_StandardFoldout;
            }
        }

        private static GUIStyle s_RadioToggle;
        public static GUIStyle RadioToggle
        {
            get
            {
                if (s_RadioToggle == null)
                {
                    s_RadioToggle = new GUIStyle(EditorStyles.radioButton);
                    s_RadioToggle.padding.left = 16;
                    s_RadioToggle.padding.top -= 2;
                }
                return s_RadioToggle;
            }
        }

        private static GUIStyle s_StandardButton;
        public static GUIStyle StandardButton
        {
            get
            {
                if (s_StandardButton == null)
                {
                    s_StandardButton = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).button;
                    s_StandardButton.fontStyle = FontStyle.Normal;
                    s_StandardButton.alignment = TextAnchor.MiddleCenter;
                    s_StandardButton.padding = new RectOffset(5, 0, 2, 2);
                    s_StandardButton.fontSize = 12;
                    s_StandardButton.normal.textColor = new Color(1f, 1f, 1f, 0.85f);
                }
                return s_StandardButton;
            }
        }

        private static GUIStyle s_LargeButton;
        public static GUIStyle LargeButton
        {
            get
            {
                if (s_LargeButton == null)
                {
                    s_LargeButton = new GUIStyle(StandardButton);
                    s_LargeButton.padding.top = 6;
                    s_LargeButton.padding.bottom = 6;
                }
                return s_LargeButton;
            }
        }

        private static GUIStyle s_ColoredButton;
        public static GUIStyle ColoredButton
        {
            get
            {
                if (s_ColoredButton == null)
                {
                    s_ColoredButton = new GUIStyle(StandardButton);
                    s_ColoredButton.onNormal.textColor = YellowColor;
                    s_ColoredButton.onHover.textColor = YellowColor;
                }
                return s_ColoredButton;
            }
        }

        private static GUIStyle s_Splitter;
        public static GUIStyle Splitter
        {
            get
            {
                if (s_Splitter == null)
                {
                    s_Splitter = new GUIStyle();
                    s_Splitter.normal.background = EditorGUIUtility.whiteTexture;
                    s_Splitter.stretchWidth = true;
                    s_Splitter.margin = new RectOffset(0, 0, 3, 3);
                }
                return s_Splitter;
            }
        }
        #endregion
    }
}