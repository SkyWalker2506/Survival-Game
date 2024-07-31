using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGamesEditor
{
    public abstract class AssetCreationWizardData : ScriptableObject
    { }

    [CustomEditor(typeof(AssetCreationWizardData), true)]
    public sealed class AssetCreationPopupDataEditor : ObjectEditor
    {
        protected override bool IgnoreScriptProperty => true;
    }

    public abstract class AssetCreationWizard : PopupWindowContent
    {
        public UnityAction<Object> ObjectCreatedCallback;
    }
}