using System;
using Toolbox.Editor;
using UnityEditor;

namespace PolymindGamesEditor
{
    public class ObjectEditor : ToolboxEditor
    {
        private IToolboxEditorDrawer _drawer;

        public sealed override IToolboxEditorDrawer Drawer
        {
            get
            {
                if (_drawer == null)
                {
                    _drawer = new ToolboxEditorDrawer(GetDrawingAction());
                    if (IgnoreScriptProperty)
                        _drawer.IgnoreProperty("m_Script");
                }

                return _drawer;
            }
        }

        protected virtual bool IgnoreScriptProperty => false;


        protected virtual Action<SerializedProperty> GetDrawingAction()
        {
            return ToolboxEditorGui.DrawToolboxProperty;
        }

        protected void DrawCustomProperty(string propertyPath)
        {
            if (Drawer.IsPropertyIgnored(propertyPath))
                return;

            var property = serializedObject.FindProperty(propertyPath);
            ToolboxEditorGui.DrawToolboxProperty(property);
        }

        protected void DrawCustomPropertySkipIgnore(string propertyPath)
        {
            var property = serializedObject.FindProperty(propertyPath);
            ToolboxEditorGui.DrawToolboxProperty(property);
        }
    }
}