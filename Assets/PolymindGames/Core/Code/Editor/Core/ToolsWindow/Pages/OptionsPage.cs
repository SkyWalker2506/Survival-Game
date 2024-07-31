using System.Collections.Generic;
using JetBrains.Annotations;
using PolymindGames;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = Object;
    
    [UsedImplicitly]
    public sealed class OptionsPage : RootPage
    {
        public override string DisplayName => "Options";
        public override int Order => 3;
        public override bool DisableInPlaymode => false;
        public override void DrawPage(Rect rect) { }
        
        public override IEnumerable<IEditorToolPage> GetSubPages()
        {
            var settingsTypes = GetAllSubTypes(typeof(UserOptions));
            var subPages = new SimpleToolPage[settingsTypes.Count];

            for (var i = 0; i < settingsTypes.Count; i++)
            {
                var settingsType = settingsTypes[i];
                string pageName = ObjectNames.NicifyVariableName(settingsType.Name);
                int index = i;
                subPages[i] = new SimpleToolPage(pageName, settingsType, 0, GetContent);

                IEditorDrawable GetContent()
                {
                    var unityObject = Resources.LoadAll(string.Empty, settingsTypes[index]).FirstOrDefault();
                    return unityObject == null ? null : new EditorDrawableInspector(unityObject);
                }
            }

            return subPages;
        }
        
        public override bool IsCompatibleWithObject(UnityObject unityObject) => false;
    }
}