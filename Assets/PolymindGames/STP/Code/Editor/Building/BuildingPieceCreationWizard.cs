using PolymindGames.BuildingSystem;
using PolymindGames.SaveSystem;
using PolymindGames;
using UnityEngine;

namespace PolymindGamesEditor.BuildingSystem
{
    public sealed class BuildingPieceCreationWizard : DataDefinitionPrefabCreationWizard<BuildableDataContainer, BuildingPieceDefinition, BuildingPiece>
    {
        public BuildingPieceCreationWizard(BuildingPieceDefinition definition) : base(definition)
        { }

        protected override void DrawHeader()
        {
            GUILayout.Label("Create Building Piece");
        }

        protected override void HandleComponents(GameObject gameObject, BuildableDataContainer data, BuildingPieceDefinition def)
        {
            gameObject.name = $"BuildingPiece_{def.Name.Replace(" ", "")}";

            if (data.ColliderType.Type != null) gameObject.AddComponent(data.ColliderType.Type);
            if (data.IsSaveable) gameObject.GetOrAddComponent<SaveableObject>();
            if (data.AddMaterialEffect) gameObject.GetOrAddComponent<MaterialEffect>();

            // Add the buildable component..
            var buildable = gameObject.GetAddOrSwapComponent<BuildingPiece>(data.BuildableType.Type);
            buildable.SetFieldValue("_definition", def);
        }
    }

    public sealed class BuildableDataContainer : PrefabCreationWizardData
    {
        [TypeConstraint(typeof(BuildingPiece), TypeGrouping = TypeGrouping.ByFlatName)]
        public SerializedType BuildableType = new(typeof(BuildingPiece));

        [TypeConstraint(typeof(Collider), TypeGrouping = TypeGrouping.ByFlatName)]
        public SerializedType ColliderType = new(typeof(BoxCollider));

        public bool IsSaveable = true;
        public bool AddMaterialEffect = true;
    }
}