using System;
using System.Diagnostics;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class NestedBehaviourListInLineAttribute : ToolboxArchetypeAttribute
    {
        public override ToolboxAttribute[] Process()
        {
            return new ToolboxAttribute[]
            {
                new DisabledInLineEditorAttribute(false, true)
                {
                    ForceEnable = true,
                    HideScript = true
                },
                new NestedBehaviourListAttribute(ListStyle)
                {
                    Foldable = Foldable,
                    HasLabels = HasLabels,
                    HasHeader = HasHeader,
                    HideBehaviours = HideBehaviours,
                    ParentType = ParentType
                }
            };
        }

        public Type ParentType { get; set; }
        
        public bool HideBehaviours { get; set; } = true;

        public ListStyle ListStyle { get; set; } = ListStyle.Lined;

        /// <summary>
        /// Indicates whether list should be allowed to fold in and out.
        /// </summary>
        public bool Foldable { get; set; } = false;
        
        /// <summary>
        /// Indicates whether list should have a label above elements.
        /// </summary>
        public bool HasHeader { get; set; } = true;
        
        /// <summary>
        /// Indicates whether each element should have an additional label.
        /// </summary>
        public bool HasLabels { get; set; } = false;
    }
}
