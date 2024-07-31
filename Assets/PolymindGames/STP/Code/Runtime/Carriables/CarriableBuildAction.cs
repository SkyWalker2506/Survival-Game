using PolymindGames.BuildingSystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public sealed class CarriableBuildAction : CarriableAction
    {
        [SerializeField, DataReferenceDetails(HasNullElement = false, HasAssetReference = true)]
        private DataIdReference<BuildMaterialDefinition> _buildMaterial;

        [SerializeField, Range(1, 10)]
        private int _amount = 1;


        public override bool CanDoAction(ICharacter character)
        {
            return character.TryGetCC(out IConstructableBuilderCC builder) && builder.ConstructableInView != null;
        }

        public override bool TryDoAction(ICharacter character)
        {
            var constructableBuilder = character.GetCC<IConstructableBuilderCC>();
            var constructable = constructableBuilder?.ConstructableInView;
            if (constructable == null)
                return false;

            bool added = false;
            for (int i = 0; i < _amount; i++)
            {
                if (constructableBuilder.TryAddMaterial(_buildMaterial))
                    added = true;
            }

            return added;
        }
    }
}