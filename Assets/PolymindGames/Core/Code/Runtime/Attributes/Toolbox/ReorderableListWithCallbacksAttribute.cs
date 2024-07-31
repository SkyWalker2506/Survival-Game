using System;
using System.Diagnostics;

namespace UnityEngine
{
    /// <inheritdoc cref="ReorderableListAttribute"/>
    /// <remarks>Works in the same way like <see cref="ReorderableListAttribute"/> but additionally allows to override some internal callbacks.</remarks>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class ReorderableListWithCallbacksAttribute : ReorderableListAttribute
    {
        public ReorderableListWithCallbacksAttribute(ListStyle style = ListStyle.Round, string elementLabel = null,
            bool fixedSize = false, bool draggable = true) :
            base(style, elementLabel, fixedSize, draggable)
        { }

        /// <summary>
        /// Name of the method that should be called every time new element is added to the list.
        /// </summary>
        public string OverrideNewElementMethodName { get; set; }

        /// <summary>
        /// Name of the method that should be called every time an element is removed from the list.
        /// </summary>
        public string OverrideRemoveElementMethodName { get; set; }
    }
}
