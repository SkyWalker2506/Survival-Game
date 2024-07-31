using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public sealed class DataDefinitionTools : ScriptableObject
    { }

    [CustomEditor(typeof(DataDefinitionTools), true)]
    public sealed class DataDefinitionToolsEditor : ObjectEditor
    { }
}