using PolymindGames.ProceduralMotion;
using PolymindGames.PostProcessing;
using PolymindGames.PoolingSystem;
using System.Collections.Generic;
using JetBrains.Annotations;
using PolymindGames;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = UnityEngine.Object;
    
    [UsedImplicitly]
    public sealed class ManagersPage : RootPage
    {
        private static readonly HashSet<Type> s_IgnoredTypes = new(3)
        {
            typeof(Tweens),
            typeof(ScenePools),
            typeof(PostProcessingManager)
        };
        
        public override string DisplayName => "Managers";
        public override int Order => 2;
        public override bool DisableInPlaymode => false;
        public override void DrawPage(Rect rect) { }

        public override IEnumerable<IEditorToolPage> GetSubPages()
        {
            var types = GetAllSubTypes(typeof(Manager));
            types.RemoveAll(type => s_IgnoredTypes.Contains(type));
            var subPages = new SimpleToolPage[types.Count];

            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                string pageName = ObjectNames.NicifyVariableName(type.Name);
                subPages[i] = new SimpleToolPage(pageName, type, 0, GetContent);

                IEditorDrawable GetContent()
                {
                    var unityObject = Resources.LoadAll(string.Empty, type).FirstOrDefault();
                    return unityObject == null ? null : new EditorDrawableInspector(unityObject);
                }
            }

            return subPages;
        }
        
        public override bool IsCompatibleWithObject(UnityObject unityObject) => false;
    }
}