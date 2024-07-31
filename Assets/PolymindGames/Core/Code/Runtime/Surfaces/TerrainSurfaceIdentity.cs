using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

namespace PolymindGames.SurfaceSystem
{
    [AddComponentMenu("Polymind Games/Surfaces/Terrain Surface Identity")]
    public sealed class TerrainSurfaceIdentity : SurfaceIdentity<TerrainCollider>
    {
        [SerializeField, ReorderableList(ListStyle.Lined, "Pair", true)]
        private LayerSurfacePair[] _layerSurfaceData = Array.Empty<LayerSurfacePair>();

        private TerrainLayer[] _terrainLayers;
        private TerrainData _terrainData;
        private Vector3 _terrainPosition;


        protected override SurfaceDefinition GetSurfaceFromHit(TerrainCollider col, ref RaycastHit hit)
        {
            float[] layerMix = GetTerrainTextureMix(hit.point, _terrainData, _terrainPosition);
            int layerIndex = GetTerrainLayerIndex(layerMix);
            var layer = _terrainLayers[layerIndex];
            
            return GetSurfaceFromLayer(layer);
        }
        
        protected override SurfaceDefinition GetSurfaceFromCollision(TerrainCollider col, Collision collision)
        {
            float[] layerMix = GetTerrainTextureMix(collision.GetContact(0).point, _terrainData, _terrainPosition);
            int layerIndex = GetTerrainLayerIndex(layerMix);
            var layer = _terrainLayers[layerIndex];
            
            return GetSurfaceFromLayer(layer);
        }

        private SurfaceDefinition GetSurfaceFromLayer(TerrainLayer layer)
        {
            for (int i = 0; i < _layerSurfaceData.Length; i++)
            {
                if (_layerSurfaceData[i].Layer == layer)
                    return _layerSurfaceData[i].Surface.Def;
            }

            return null;
        }

        private static float[] GetTerrainTextureMix(Vector3 worldPos, TerrainData terrainData, Vector3 terrainPos)
        {
            // Returns an array containing the relative mix of textures
            // on the terrain at this world position.

            // The number of values in the array will equal the number
            // of textures added to the terrain.

            // Calculate which splat map cell the worldPos falls within (ignoring y)
            int mapX = (int)((worldPos.x - terrainPos.x) / terrainData.size.x * terrainData.alphamapWidth);
            int mapZ = (int)((worldPos.z - terrainPos.z) / terrainData.size.z * terrainData.alphamapHeight);

            // Get the splat data for this cell as a 1x1xN 3D array (where N = number of textures)
            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            // Extract the 3D array data to a 1D array:
            float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

            for (int n = 0;n < cellMix.Length;n++)
                cellMix[n] = splatmapData[0, 0, n];

            return cellMix;
        }

        private static int GetTerrainLayerIndex(float[] textureMix)
        {
            // Returns the zero-based index of the most dominant texture
            float maxMix = 0;
            int maxIndex = 0;

            // Loop through each mix value and find the maximum.
            for (int n = 0;n < textureMix.Length;n++)
            {
                if (textureMix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = textureMix[n];
                }
            }

            return maxIndex;
        }
        
        private void Start()
        {
            var terrain = GetComponent<Terrain>();
            _terrainPosition = terrain.GetPosition();
            _terrainData = terrain.terrainData;
            _terrainLayers = _terrainData.terrainLayers;
        }

        #region Editor
#if UNITY_EDITOR
        private static readonly List<TerrainLayer> s_TempMissingLayerList = new();
        
        private void Reset() => OnValidate();
        protected override void OnValidate()
        {
            base.OnValidate();
            
            var terrain = GetComponent<Terrain>();
            if (terrain == null)
                return;
            
            var terrainLayers = terrain.terrainData.terrainLayers;
            
            // Clear the original array of any nulls or missing layers 
            for (int i = 0; i < _layerSurfaceData.Length; i++)
            {
                var layer = _layerSurfaceData[i].Layer;
                if (layer == null || !((IList)terrainLayers).Contains(layer))
                {
                    UnityEditor.ArrayUtility.RemoveAt(ref _layerSurfaceData, i);
                    i--;
                }
            }
            
            // Create a list for the missing layers
            for (int i = 0; i < terrainLayers.Length; i++)
            {
                if (GetIndexOfLayer(terrainLayers[i]) == -1)
                    s_TempMissingLayerList.Add(terrainLayers[i]);
            }
            
            // Transfer the original data to a new array if there's missing terrain layers
            // This can happen when adding new layers to the terrain
            if (s_TempMissingLayerList.Count > 0)
            {
                int baseLength = _layerSurfaceData.Length;
                var newArray = new LayerSurfacePair[baseLength + s_TempMissingLayerList.Count];
                Array.Copy(_layerSurfaceData, newArray, _layerSurfaceData.Length);

                for (int i = 0; i < s_TempMissingLayerList.Count; i++)
                    newArray[baseLength + i] = new LayerSurfacePair { Layer = s_TempMissingLayerList[i] };

                _layerSurfaceData = newArray; 
                s_TempMissingLayerList.Clear();
                
                UnityEditor.EditorUtility.SetDirty(this);
            }
            return;

            int GetIndexOfLayer(TerrainLayer layer)
            {
                for (int i = 0; i < _layerSurfaceData.Length; i++)
                {
                    if (_layerSurfaceData[i].Layer == layer)
                        return i;
                }

                return -1;
            }
        }
#endif
        #endregion
        
        #region Internal
        [Serializable]
        private struct LayerSurfacePair
        {
            [Disable]
            public TerrainLayer Layer;
            
            [DataReferenceDetails(HasNullElement = false)]
            public DataIdReference<SurfaceDefinition> Surface;
        }
        #endregion

    }
}
