using PolymindGames.WieldableSystem;
using PolymindGames.BuildingSystem;
using JetBrains.Annotations;
using UnityEngine;
using System.Linq;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = UnityEngine.Object;
    
    [UsedImplicitly]
    public sealed class CarriablesAndBuildMaterialsPage : DataDefinitionPage
    {
        public override string DisplayName => "Carriables & Materials";
        public override bool DisableInPlaymode => true;
        public override int Order => 5;

        private readonly Lazy<DataDefinitionsContent> _content = new(() => new DataDefinitionsContent(
                new DataDefinitionsContent.Element<CarriableDefinition>("Carriable"),
                new DataDefinitionsContent.Element<BuildMaterialDefinition>("Material (Building)")));

        public override void DrawPage(Rect rect)
        {
            _content.Value.Draw(rect, EditorDrawableLayoutType.Horizontal);
        }

        public override bool IsCompatibleWithObject(UnityObject unityObject)
        {
            if (unityObject is CarriableDefinition or BuildMaterialDefinition)
            {
                switch (unityObject)
                {
                    case CarriableDefinition carriable:
                        {
                            var element = _content.Value.Elements.Where(item => item is DataDefinitionsContent.Element<CarriableDefinition>)
                                .Cast<DataDefinitionsContent.Element<CarriableDefinition>>().FirstOrDefault();

                            element?.Toolbar.SelectDefinition(carriable);
                            break;
                        }
                    case BuildMaterialDefinition buildMaterial:
                        {
                            var element = _content.Value.Elements.Where(item => item is DataDefinitionsContent.Element<BuildMaterialDefinition>)
                                .Cast<DataDefinitionsContent.Element<BuildMaterialDefinition>>().FirstOrDefault();

                            element?.Toolbar.SelectDefinition(buildMaterial);
                            break;
                        }
                }

                return true;
            }

            return false;
        }
    }
}