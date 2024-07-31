using UnityEditor;
using UnityEngine;

namespace Toolbox.Editor.Drawers
{
    public class BeginGroupAttributeDrawer : ToolboxDecoratorDrawer<BeginGroupAttribute>
    {
        protected override void OnGuiBeginSafe(BeginGroupAttribute attribute)
        {
            ToolboxLayoutHandler.BeginVertical(Style.groupBackgroundStyle);
            if (attribute.HasLabel)
            {
                GUILayout.Label(attribute.Label, EditorStyles.boldLabel);
            }
        }

        private static class Style
        {
            internal static readonly GUIStyle groupBackgroundStyle;

            static Style()
            {
                groupBackgroundStyle = new GUIStyle("box")
                {
                    padding = new RectOffset(13, 12, 5, 5)
                };
            }
        }
    }
}