using System;
using PolymindGames.InventorySystem;
using PolymindGames.UserInterface;
using UnityEngine;

namespace PolymindGames
{
    public sealed class BodyClothing : CharacterUIBehaviour
    {
        [SerializeField, NotNull, BeginGroup]
        private SkinnedMeshRenderer _bodyRenderer;

        [SerializeField, Disable, EndGroup]
        [ReorderableList(ListStyle.Lined, fixedSize: true), LabelFromChild("Type")]
        private ClothingClassData[] _clothing;

        private readonly ClothingItemData[] _activeClothes = new ClothingItemData[BodyPointUtils.BodyPointsCount];


        public void SetClothing(BodyPoint bodyPoint, DataIdReference<ItemDefinition> itemId)
        {
            var clothingItem = GetClothingData(bodyPoint, itemId);
            SetClothing(bodyPoint, clothingItem);
        }

        public void HideAllClothing()
        {
            foreach (var bodyPoint in BodyPointUtils.BodyPoints)
                SetClothing(bodyPoint, clothingItem: null);
        }

        private ClothingItemData GetClothingData(BodyPoint bodyPoint, DataIdReference<ItemDefinition> itemId)
        {
            var clothingItems = _clothing[bodyPoint.GetIndex()].Items;
            foreach (var clothingItem in clothingItems)
            {
                if (clothingItem.Item == itemId)
                    return clothingItem;
            }

            return null;
        }

        private void SetClothing(BodyPoint bodyPoint, ClothingItemData clothingItem)
        {
            int index = bodyPoint.GetIndex();

            var prevActiveItem = _activeClothes[index];
            prevActiveItem?.Renderer.gameObject.SetActive(false);

            _activeClothes[index] = clothingItem;
            clothingItem?.Renderer.gameObject.SetActive(true);

            UpdateOpacityMasksInShader(bodyPoint, clothingItem);
        }

        private void UpdateOpacityMasksInShader(BodyPoint bodyPoint, ClothingItemData clothingItem)
        {
            string shaderProperty = $"_OpacityMask_{bodyPoint}";
            Texture2D opacityMask = clothingItem?.OpacityMask;
            _bodyRenderer.material.SetTexture(shaderProperty, opacityMask);
        }

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_clothing == null || _clothing.Length != BodyPointUtils.BodyPointsCount)
            {
                _clothing = new ClothingClassData[BodyPointUtils.BodyPointsCount];
                for (int i = 0; i < _clothing.Length; i++)
                    _clothing[i] = new ClothingClassData(BodyPointUtils.BodyPoints[i]);
            }

            for (int i = 0; i < _clothing.Length; i++)
                _clothing[i].Type = BodyPointUtils.BodyPoints[i];
        }
#endif
        #endregion
        
        #region Internal
        [Serializable]
        private struct ClothingClassData
        {
            public BodyPoint Type;

            [SpaceArea]
            [ReorderableList(ListStyle.Lined), LabelFromChild("Renderer"), IgnoreDisable]
            public ClothingItemData[] Items;


            public ClothingClassData(BodyPoint type)
            {
                Type = type;
                Items = Array.Empty<ClothingItemData>();
            }
        }

        [Serializable]
        private class ClothingItemData
        {
            public DataIdReference<ItemDefinition> Item;

            [NotNull]
            public SkinnedMeshRenderer Renderer;

            public Texture2D OpacityMask;
        }
        #endregion
    }
}