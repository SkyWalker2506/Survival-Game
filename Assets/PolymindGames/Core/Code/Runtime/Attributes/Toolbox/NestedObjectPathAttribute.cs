using System;
using System.Diagnostics;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    [Conditional("UNITY_EDITOR")]
    public sealed class NestedObjectPathAttribute : Attribute
    {
        /// <summary>
        /// <para> The display name for this type shown in the Assets/Create menu.</para>
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        ///   <para> The default file name used by newly created instances of this type.</para>
        /// </summary>
        public string FileName { get; set; }
    }
}
