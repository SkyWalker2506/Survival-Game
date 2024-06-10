//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace HQFPSTemplate.Examples
{
    public class HQFPS_HDRPConverterInfo : ScriptableObject
    {
        #region Internal
        [Serializable]
        public class HDRPConverterInfo
        {
            [Header("Materials Upgrade")]
            public string MaterialsPath = "TEST";
            [Reorderable] public ShaderSetupsList ShaderSetups;

            [BHeader("Objects Upgrade")]
            public string HDRPFilesPath = "HQFPSTemplate/_Integrations/HDRP Support/_HDRP Upgrade Files";
            public string ObjectMatchSuffix = "_HDRP";

            [Header("Scriptable Objects Types", order = 1)]
            [Reorderable] public ScriptableObjectSetupsList ScriptableObjectSetups;

            [Header("Prefabs", order = 1)]
            [Reorderable] public GameObjectList PrefabsToConvert;

            [Header("HQ FPS Specific", order = 1)]
            public string EquipmentPrefabsPath = "HQFPSTemplate/Core/_Prefabs&Profiles/_FirstPerson/_Equipment";
        }

        [Serializable]
        public class ScriptableObjectSetup
        {
            public ScriptableObject ScriptableObjectType;
            public string SearchPath;
        }

        [Serializable]
        public struct ShaderProperty
        {
            [FormerlySerializedAs("CurrentKeyword")]
            public string CurrentPropertyName;

            [FormerlySerializedAs("TargetKeyword")]
            public string TargetPropertyName;
        }

        [Serializable]
        public struct TargetShaderProperties
        {
            public string PropertyName;

            public ShaderPropertyType PropertyType;

            [ShowIf("PropertyType", (int)ShaderPropertyType.Color)]
            public Color Color;

            [ShowIf("PropertyType", (int)ShaderPropertyType.Float)]
            public float Float;

            [ShowIf("PropertyType", (int)ShaderPropertyType.Vector)]
            public Vector4 Vector;

            [ShowIf("PropertyType", (int)ShaderPropertyType.Texture)]
            public Texture Texture; 
        }

        [Serializable]
        public class ShaderSetup
        {
            public Shader CurrentShader;
            public Shader TargetShader;

            [Tooltip("Properties to transfer from the Current Shader to the Target one.")]
            public ShaderProperty[] ShaderProperties;

            [Tooltip("Property values that get set to the Target shader after the transfer.")]
            public TargetShaderProperties[] TargetShaderCustomProperties;
        }
        #endregion

        public HDRPConverterInfo m_HDRPConverterInfo;
    }
}
