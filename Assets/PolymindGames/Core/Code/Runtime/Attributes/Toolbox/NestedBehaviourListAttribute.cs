using System;
using System.Diagnostics;

namespace UnityEngine
{
    /// <summary>
    /// Draws collection in form of the reorderable list and handles hidden mono behaviour creation.
    /// <para>Supported types: any <see cref="System.Collections.IList"/>.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class NestedBehaviourListAttribute : ReorderableListAttribute
    {
        public NestedBehaviourListAttribute (ListStyle style = ListStyle.Round, string childLabel = null,
            bool fixedSize = false, bool draggable = true) : base(style, childLabel, fixedSize, draggable) { }

        public Type ParentType { get; set; }
        public bool HideBehaviours { get; set; }
    }
}
