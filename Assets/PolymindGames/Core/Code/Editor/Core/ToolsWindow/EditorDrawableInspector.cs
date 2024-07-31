using UnityEngine;
using UnityEditor;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = UnityEngine.Object;
    
    public sealed class EditorDrawableInspector : IEditorDrawable
    {
        private readonly CachedObjectEditor _inspectorDrawer;
        private readonly string _inspectorName;
 

        public EditorDrawableInspector(UnityObject unityObject)
        {
            if (unityObject == null)
                throw new ArgumentNullException();

            _inspectorDrawer = new CachedObjectEditor(unityObject);
            _inspectorName = ObjectNames.NicifyVariableName(unityObject.GetType().Name);
        }

        public void Draw(Rect rect, EditorDrawableLayoutType layoutType)
        {
            var expand = GUILayout.ExpandWidth(true);
            using (new GUILayout.VerticalScope(GuiStyles.Box, expand))
            {
                GUILayout.Label(_inspectorName, GuiStyles.Title, expand);
                _inspectorDrawer.DrawGUI(); 
            }
        }
    }
}