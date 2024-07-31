using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Toolbox.Editor.Drawers
{
    [UsedImplicitly]
    public sealed class IgnoreDisableAttributeDrawer : ToolboxConditionDrawer<IgnoreDisableAttribute>
    {
        protected override PropertyCondition OnGuiValidateSafe(SerializedProperty property, IgnoreDisableAttribute attribute)
        {
            GUI.enabled = true;
            return PropertyCondition.Valid;
        }
    }
}