using UnityEngine;
using System.Linq;
using System;

namespace PolymindGames.BuildingSystem
{
    /// <summary>
    /// Definition for a building piece used in construction.
    /// </summary>
    [CreateAssetMenu(menuName = "Polymind Games/Building/Building Piece Definition", fileName = "BuildingPiece_")]
    public sealed class BuildingPieceDefinition : GroupMemberDefinition<BuildingPieceDefinition, BuildingPieceCategoryDefinition>
    {
        [SerializeField, NewLabel("Name"), BeginGroup]
        [Tooltip("The name of the building piece.")]
        private string _buildingPieceName;

        [SerializeField, SpritePreview, SpaceArea]
        [Tooltip("The icon representing the building piece.")]
        private Sprite _icon;

        [SerializeField, Tooltip("The prefab of the building piece.")]
        private BuildingPiece _prefab;

        [SerializeField, Multiline, EndGroup]
        [Tooltip("A description of the building piece.")]
        private string _description;
        
        [SerializeField, NotNull, BeginGroup("Effects")]
        [Tooltip("Effects played when placing the building piece.")]
        private EffectPairSO _placeEffects;
        
        [SerializeField, NotNull, EndGroup]
        [Tooltip("Effects played when constructing the building piece.")]
        private EffectPairSO _constructEffects;

        private static BuildingPieceDefinition[] s_GroupPiecesDefinitions;
        

        /// <summary>
        /// Gets or sets the name of the building piece.
        /// </summary>
        public override string Name
        {
            get => _buildingPieceName;
            protected set => _buildingPieceName = value;
        }

        /// <summary>
        /// Gets the icon representing the building piece.
        /// </summary>
        public override Sprite Icon => _icon;

        /// <summary>
        /// Gets the description of the building piece.
        /// </summary>
        public override string Description => _description;

        /// <summary>
        /// Gets the prefab of the building piece.
        /// </summary>
        public BuildingPiece Prefab => _prefab;

        /// <summary>
        /// Gets the effects played when placing the building piece.
        /// </summary>
        public EffectPairSO PlaceEffects => _placeEffects;

        /// <summary>
        /// Gets the effects played when constructing the building piece.
        /// </summary>
        public EffectPairSO ConstructEffects => _constructEffects;

        /// <summary>
        /// Gets an array of all group building piece definitions.
        /// </summary>
        public static BuildingPieceDefinition[] GroupBuildingPiecesDefinitions
        {
            get
            {
                return s_GroupPiecesDefinitions ??= GetGroupBuildingPieces();

                static BuildingPieceDefinition[] GetGroupBuildingPieces()
                {
                    return Definitions.Where(def => def.Prefab is GroupBuildingPiece).ToArray(); 
                }
            }
        }

        /// <summary>
        /// Gets the next group building piece definition relative to the specified definition.
        /// </summary>
        /// <param name="definition">The current building piece definition.</param>
        /// <param name="next">True to get the next building piece definition; false to get the previous one.</param>
        /// <returns>The next group building piece definition.</returns>
        public static BuildingPieceDefinition GetNextGroupBuildingPiece(BuildingPieceDefinition definition, bool next)
        {
            var buildingPieces = GroupBuildingPiecesDefinitions; 
            
            int index = Mathf.Max(Array.IndexOf(buildingPieces, definition), 0);
            index = (int)Mathf.Repeat(index + (next ? 1 : -1), buildingPieces.Length);

            return buildingPieces[index];
        }

#if UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            _prefab = null;
        }
#endif
    }
}