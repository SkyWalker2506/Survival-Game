using System;
using System.Diagnostics;

namespace UnityEngine
{
    /// <summary>
    /// Draws an associated built-in Editor.
    /// <para>Supported types: any <see cref="Object"/>.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class DisabledInLineEditorAttribute : ToolboxSelfPropertyAttribute
    {
        public DisabledInLineEditorAttribute(bool drawPreview = true, bool drawSettings = false)
        {
            DrawPreview = drawPreview;
            DrawSettings = drawSettings;
        }

        public bool DrawPreview { get; private set; }

        public bool ForceEnable { get; set; }
        
        public bool DrawSettings { get; private set; }

        /// <summary>
        /// Indicates if the "m_Script" property should be hidden.
        /// Will work only for Toolbox-based Editors.
        /// </summary>
        public bool HideScript { get; set; } = true;

        /// <summary>
        /// Indicates if the inlined Editor should be disabled.
        /// </summary>
        public bool DisableEditor { get; set; }

        public float PreviewHeight { get; set; } = 90.0f;
    }
}