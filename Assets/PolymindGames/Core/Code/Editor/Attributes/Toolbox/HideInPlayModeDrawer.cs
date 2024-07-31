using UnityEditor;
using UnityEngine;

namespace Toolbox.Editor.Drawers
{
    public sealed class HideInPlayModeDrawer : ToolboxConditionDrawer<HideInPlayModeAttribute>
    {
        protected override PropertyCondition OnGuiValidateSafe(SerializedProperty property, HideInPlayModeAttribute attribute)
        {
            return EditorApplication.isPlayingOrWillChangePlaymode ? PropertyCondition.NonValid : PropertyCondition.Valid;
        }
    }
}