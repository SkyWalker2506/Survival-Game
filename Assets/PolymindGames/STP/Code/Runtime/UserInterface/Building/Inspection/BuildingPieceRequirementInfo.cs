using System;
using PolymindGames.BuildingSystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class BuildingPieceRequirementInfo : DataInfo
    {
        [SerializeField, NotNull]
        private GameObject _requirementsRoot;

        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false)]
        private RequirementUI[] _requirements;


        public void UpdateInfo(BuildingPieceDefinition data)
        {
            if (data == null || !data.Prefab.TryGetComponent(out IConstructable constructable))
            {
                _requirementsRoot.SetActive(false);
                return;
            }

            _requirementsRoot.SetActive(true);
            var buildReq = constructable.GetBuildRequirements();

            for (int i = 0; i < _requirements.Length; i++)
            {
                if (i > buildReq.Length - 1)
                {
                    _requirements[i].gameObject.SetActive(false);
                    continue;
                }

                _requirements[i].gameObject.SetActive(true);

                if (buildReq[i].BuildMaterial.IsNull)
                    continue;

                _requirements[i].Display(buildReq[i].BuildMaterial.Icon, "x" + buildReq[i].RequiredAmount);
            }
        }
    }
}