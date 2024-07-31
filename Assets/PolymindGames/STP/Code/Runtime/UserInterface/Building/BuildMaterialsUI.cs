using PolymindGames.ProceduralMotion;
using PolymindGames.BuildingSystem;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_2)]
    public sealed class BuildMaterialsUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup("Canvas")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Range(0f, 1f), EndGroup]
        private float _alphaTweenDuration = 0.25f;

        [SerializeField, BeginGroup("Build Materials")]
        private RectTransform _buildMaterialsRoot;

        [SerializeField]
        private Image _addAllMaterialsProgressImg;

        [SerializeField, PrefabObjectOnly]
        private RequirementUI _buildMaterialTemplate;

        [SerializeField, Range(3, 30), EndGroup]
        private int _cachedBuildMaterialCount = 10;

        [SerializeField, BeginGroup("Cancelling"), EndGroup]
        private Image _cancelProgressImg;

        private IConstructableBuilderCC _constructableManager;
        private IBuildControllerCC _buildController;
        private RequirementUI[] _materialDisplayers;
        private IConstructable _constructableInView;
        private RectTransform _rectTransform;
        private Vector3 _constructableCenter;
        private float _targetAlpha;

        private readonly List<BuildRequirement> _requirements = new(4);


        protected override void Awake()
        {
            base.Awake();

            _rectTransform = transform as RectTransform;
            
            _materialDisplayers = new RequirementUI[_cachedBuildMaterialCount];
            for (int i = 0; i < _cachedBuildMaterialCount; i++)
            {
                _materialDisplayers[i] = Instantiate(_buildMaterialTemplate, _buildMaterialsRoot);
                _materialDisplayers[i].gameObject.SetActive(false);
            }

            _canvasGroup.alpha = 0f;
            _cancelProgressImg.fillAmount = 0f;
            _addAllMaterialsProgressImg.fillAmount = 0f;

            enabled = false;
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            _buildController = character.GetCC<IBuildControllerCC>();
            _buildController.ObjectPlaced += OnBuildingPiecePlaced;
            _constructableManager = character.GetCC<IConstructableBuilderCC>();
            _constructableManager.ConstructableChanged += UpdateActiveConstructable;
            _constructableManager.CancelConstructableProgressChanged += OnCancelProgressChanged;
            _constructableManager.BuildMaterialAdded += UpdateBuildRequirementsDisplay;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            _buildController.ObjectPlaced -= OnBuildingPiecePlaced;
            _constructableManager.ConstructableChanged -= UpdateActiveConstructable;
            _constructableManager.CancelConstructableProgressChanged -= OnCancelProgressChanged;
            _constructableManager.BuildMaterialAdded -= UpdateBuildRequirementsDisplay;
        }

        private void OnBuildingPiecePlaced(BuildingPiece piece) => UpdateActiveConstructable(_constructableManager.ConstructableInView);

        private void OnCancelProgressChanged(float progress)
        {
            _cancelProgressImg.fillAmount = progress;
        }

        private void UpdateActiveConstructable(IConstructable constructable)
        {
            if (constructable != null && !constructable.IsConstructed)
            {
                Tweens.CancelAllForObject(this);

                _canvasGroup.TweenCanvasGroupAlpha(1f, _alphaTweenDuration)
                    .SetEase(EaseType.SineInOut)
                    .PlayAndRelease(this);
                
                enabled = true;
                _constructableInView = constructable;
                _constructableCenter = constructable.BuildingPiece.GetCenter();
                UpdateBuildRequirementsDisplay(constructable);
            }
            else
            {
                _canvasGroup.TweenCanvasGroupAlpha(0f, _alphaTweenDuration)
                    .SetEase(EaseType.SineInOut)
                    .PlayAndRelease(this);
            }
        }

        private void LateUpdate()
        {
            UpdateBuildRequirementsPosition();

            if (_canvasGroup.alpha < Mathf.Epsilon)
            {
                enabled = false;
                _constructableInView = null;
            }
        }
        
        private void UpdateBuildRequirementsPosition()
        {
            Vector2 screenPositionOfPreview = UnityUtils.CachedMainCamera.WorldToScreenPoint(_constructableCenter);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_rectTransform.parent, screenPositionOfPreview, null, out Vector2 positionOfUI))
                _rectTransform.localPosition = positionOfUI;
        }

        /// <summary>
        /// Updates the display of build requirements information based on the constructable in view.
        /// </summary>
        private void UpdateBuildRequirementsDisplay(IConstructable constructable)
        {
            if (constructable.BuildingPiece.ParentGroup == null)
                UpdateDisplayForNoParentGroup();
            else
                UpdateDisplayForParentGroup();
        }

        /// <summary>
        /// Updates the display of build requirements information when the constructable has no parent group.
        /// </summary>
        private void UpdateDisplayForNoParentGroup()
        {
            var buildRequirements = _constructableInView.GetBuildRequirements();
            for (int i = 0; i < _materialDisplayers.Length; i++)
            {
                RequirementUI displayer = _materialDisplayers[i];

                if (i >= buildRequirements.Length)
                    displayer.gameObject.SetActive(false);
                else
                {
                    displayer.gameObject.SetActive(true);

                    BuildRequirement requirement = buildRequirements[i];

                    var buildMaterial = BuildMaterialDefinition.GetWithId(requirement.BuildMaterial);

                    if (buildMaterial != null)
                        displayer.Display(buildMaterial.Icon, requirement.CurrentAmount + "/" + requirement.RequiredAmount);
                }
            }
        }

        /// <summary>
        /// Updates the display of build requirements information when the constructable has a parent group.
        /// </summary>
        private void UpdateDisplayForParentGroup()
        {
            var parentGroup = _constructableInView.BuildingPiece.ParentGroup;
            var buildRequirements = GetAllBuildRequirements(parentGroup);

            for (int i = 0; i < _materialDisplayers.Length; i++)
            {
                RequirementUI displayer = _materialDisplayers[i];

                if (i >= buildRequirements.Count)
                    displayer.gameObject.SetActive(false);
                else
                {
                    displayer.gameObject.SetActive(true);

                    BuildRequirement requirement = buildRequirements[i];

                    var buildMaterial = BuildMaterialDefinition.GetWithId(requirement.BuildMaterial);

                    if (buildMaterial != null)
                        displayer.Display(buildMaterial.Icon, requirement.CurrentAmount + "/" + requirement.RequiredAmount);
                }
            }
        }

        private List<BuildRequirement> GetAllBuildRequirements(IBuildingPieceGroup group)
        {
            _requirements.Clear();

            foreach (var buildingPiece in group.BuildingPieces)
            {
                var constructable = buildingPiece.Constructable;
            
                foreach (var requirement in constructable.GetBuildRequirements())
                {
                    var indexOfExisting = IndexOfExisting(_requirements, requirement.BuildMaterial);

                    if (indexOfExisting != -1)
                    {
                        _requirements[indexOfExisting] = new BuildRequirement(_requirements[indexOfExisting].BuildMaterial,
                            _requirements[indexOfExisting].RequiredAmount + requirement.RequiredAmount,
                            _requirements[indexOfExisting].CurrentAmount + requirement.CurrentAmount);
                    }
                    else
                        _requirements.Add(requirement);
                }
            }
        
            return _requirements;

            static int IndexOfExisting(List<BuildRequirement> requirements, int id)
            {
                for (int i = 0; i < requirements.Count; i++)
                {
                    var requirement = requirements[i];
                    if (requirement.BuildMaterial == id)
                        return i;
                }

                return -1;
            }
        }
    }
}