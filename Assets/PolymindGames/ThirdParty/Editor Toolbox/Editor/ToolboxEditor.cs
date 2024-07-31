using UnityEditor;

namespace Toolbox.Editor
{
    using Editor = UnityEditor.Editor;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Base Editor class for all Toolbox-related features.
    /// </summary>
    [CustomEditor(typeof(Object), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class ToolboxEditor : Editor, IToolboxEditor
    {
        /// <summary>
        /// Inspector GUI re-draw call.
        /// </summary>
        public sealed override void OnInspectorGUI()
        {
            ToolboxEditorHandler.HandleToolboxEditor(this);
        }

        /// <inheritdoc />
        public virtual void DrawCustomInspector()
        {
            Drawer.DrawEditor(serializedObject);
        }

        /// <inheritdoc />
        public void IgnoreProperty(SerializedProperty property)
        {
            Drawer.IgnoreProperty(property);
        }

        /// <inheritdoc />
        public void IgnoreProperty(string propertyPath)
        {
            Drawer.IgnoreProperty(propertyPath);
        }

        Editor IToolboxEditor.ContextEditor => this;
        
        /// <inheritdoc />
        public virtual IToolboxEditorDrawer Drawer { get; } = new ToolboxEditorDrawer();
    }
}