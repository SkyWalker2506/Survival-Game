using PolymindGames.SurfaceSystem;
using JetBrains.Annotations;
using UnityEngine;
using System.Linq;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = UnityEngine.Object;
    
    [UsedImplicitly]
    public sealed class SurfacesEditorToolPage : DataDefinitionPage
    {
        public override string DisplayName => "Surfaces";
        public override bool DisableInPlaymode => true;
        public override int Order => 10;

        private readonly Lazy<DataDefinitionsContent> _content = new(() => new DataDefinitionsContent(
                new DataDefinitionsContent.Element<SurfaceDefinition>("Surfaces", EditorDrawableLayoutType.Horizontal)));

        public override void DrawPage(Rect rect)
        {
            _content.Value.Draw(rect, EditorDrawableLayoutType.Horizontal);
        }

        public override bool IsCompatibleWithObject(UnityObject unityObject)
        {
            if (unityObject is SurfaceDefinition surface)
            {
                var element = _content.Value.Elements.Cast<DataDefinitionsContent.Element<SurfaceDefinition>>().FirstOrDefault();
                element?.Toolbar.SelectDefinition(surface);

                return true;
            }

            return false;
        }
    }
}