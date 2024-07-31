using UnityEngine;

namespace PolymindGamesEditor
{
    public static class DataDefinitionEditorStyles
    {
        private const string PATH = "Icons/";
        
        public static readonly GUIContent CreateEmptyContent = LoadImg("Editor_Add", "Create Empty");
        public static readonly GUIContent DuplicateSelectedContent = LoadImg("Editor_Duplicate", "Duplicate Selected");
        public static readonly GUIContent DeleteSelectedContent = LoadImg("Editor_Delete", "Delete Selected");
        public static readonly GUIContent DeleteAllContent = LoadImg("Editor_DeleteAll", "Delete All");
        public static readonly GUIContent LinkContent = LoadImg("Editor_Link", "Link Selected");
        public static readonly GUIContent UnlinkContent = LoadImg("Editor_Unlink", "Unlink Selected");
        public static readonly GUIContent MergeContent = LoadImg("Editor_Merge", "Merge Selected");
        public static readonly GUILayoutOption[] ButtonLayoutOptions =  { GUILayout.Height(26f), GUILayout.Width(40f) };
 
        private static GUIContent LoadImg(string name, string tooltip)
        {
            return new GUIContent(Resources.Load<Texture2D>(PATH + name), tooltip);
        }
    }
}