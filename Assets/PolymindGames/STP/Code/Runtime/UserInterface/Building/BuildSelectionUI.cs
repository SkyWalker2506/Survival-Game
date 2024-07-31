using PolymindGames.ProceduralMotion;
using PolymindGames.BuildingSystem;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace PolymindGames.UserInterface
{
    public sealed class BuildSelectionUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup("Canvas")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Range(0f, 5f), EndGroup]
        private float _alphaLerpDuration = 0.35f;

        [SerializeField, BeginGroup("Free Building Pieces"), EndGroup]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        private GameObject[] _freeBuildingPieceObjects;

        [SerializeField, BeginGroup("Group Building Pieces")]
        private Image _previousImg;

        [SerializeField]
        private Image _currentImg;

        [SerializeField]
        private Image _nextImg;

        [SerializeField, EndGroup]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        private GameObject[] _groupBuildingPieceObjects;

        private IBuildControllerCC _buildController;


        protected override void Awake()
        {
            base.Awake();
            _canvasGroup.alpha = 0f;
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            _buildController = character.GetCC<IBuildControllerCC>();
            _buildController.BuildingPieceChanged += CurrentBuildingPieceChanged;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            _buildController.BuildingPieceChanged -= CurrentBuildingPieceChanged;
        }

        private void CurrentBuildingPieceChanged(BuildingPiece buildingPiece)
        {
            if (buildingPiece == null)
            {
                _canvasGroup.TweenCanvasGroupAlpha(0f, _alphaLerpDuration)
                    .SetEase(EaseType.SineInOut)
                    .PlayAndRelease(this);
            }
            else
            {
                _canvasGroup.TweenCanvasGroupAlpha(1f, _alphaLerpDuration)
                    .SetEase(EaseType.SineInOut)
                    .PlayAndRelease(this);

                if (buildingPiece is GroupBuildingPiece)
                {
                    for (int i = 0; i < _groupBuildingPieceObjects.Length; i++)
                        _groupBuildingPieceObjects[i].SetActive(true);

                    for (int i = 0; i < _freeBuildingPieceObjects.Length; i++)
                        _freeBuildingPieceObjects[i].SetActive(false);

                    var socketBuildingPieces = BuildingPieceDefinition.GroupBuildingPiecesDefinitions;

                    int currentIdx = Array.IndexOf(socketBuildingPieces, buildingPiece.Definition);
                    int previousIdx = (int)Mathf.Repeat(currentIdx - 1, socketBuildingPieces.Length);
                    int nextIdx = (int)Mathf.Repeat(currentIdx + 1, socketBuildingPieces.Length);

                    if (currentIdx != -1)
                    {
                        _currentImg.sprite = socketBuildingPieces[currentIdx].ParentGroup.Icon;
                        _previousImg.sprite = socketBuildingPieces[previousIdx].ParentGroup.Icon;
                        _nextImg.sprite = socketBuildingPieces[nextIdx].ParentGroup.Icon;
                    }
                }
                else
                {
                    for (int i = 0; i < _groupBuildingPieceObjects.Length; i++)
                        _groupBuildingPieceObjects[i].SetActive(false);

                    for (int i = 0; i < _freeBuildingPieceObjects.Length; i++)
                        _freeBuildingPieceObjects[i].SetActive(true);
                }
            }
        }
    }
}