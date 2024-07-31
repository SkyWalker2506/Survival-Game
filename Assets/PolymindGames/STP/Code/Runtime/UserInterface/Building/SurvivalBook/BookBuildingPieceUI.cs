using PolymindGames.BuildingSystem;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    public sealed class BookBuildingPieceUI : CharacterUIBehaviour
    {
        [BeginGroup("Building Pieces"), EndGroup]
        [SerializeField, DataReferenceDetails(HasNullElement = false)]
        private DataIdReference<BuildingPieceCategoryDefinition> _buildingPieceCategory;

        [SerializeField, BeginGroup("Template")]
        private BuildingPieceSlotUI _template;

        [SerializeField, EndGroup]
        private RectTransform _templateSpawnRect;

        [SerializeField, NotNull, BeginGroup("Display")]
        private TextMeshProUGUI _nameTxt;

        [SerializeField, NotNull, EndGroup]
        private Image _categoryImg;
        
        private BuildingPieceSlotUI[] _slots;


        protected override void Awake()
        {
            base.Awake();
            CreateSlots();
        } 

        private void StartBuilding(SelectableUI selectable)
        {
            if (Character == null)
            {
                Debug.LogWarning("This behaviour is not attached to a character.", gameObject);
                return;
            }

            Character.GetCC<IWieldableControllerCC>().TryEquipWieldable(null);
            var buildingPieceDef = selectable.GetComponent<BuildingPieceSlotUI>().Definition;
            var buildingPiece = Instantiate(buildingPieceDef.Prefab);
            Character.GetCC<IBuildControllerCC>().SetBuildingPiece(buildingPiece);
        }

        private void CreateSlots()
        {
            bool wasTemplateActive = _templateSpawnRect.gameObject.activeInHierarchy;
            _templateSpawnRect.gameObject.SetActive(true);

            var buildingPieceDefs = _buildingPieceCategory.Def.Members;
            _slots = new BuildingPieceSlotUI[buildingPieceDefs.Count];
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = Instantiate(_template, _templateSpawnRect);
                _slots[i].SetBuildingPiece(buildingPieceDefs[i]);
                _slots[i].Selectable.OnSelectableSelected += StartBuilding;
            }

            if (!wasTemplateActive)
                _templateSpawnRect.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityUtils.SafeOnValidate(this, () =>
            {
                if (_categoryImg != null)
                    _categoryImg.sprite = _buildingPieceCategory.Icon;

                if (_nameTxt != null)
                    _nameTxt.text = _buildingPieceCategory.Name;
            });
        }
#endif
    }
}