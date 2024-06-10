//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using System;
using UnityEngine;
using HQFPSTemplate.Items;
using HQFPSTemplate.Equipment;
using HQFPSTemplate.Examples;

namespace HQFPSTemplate
{
    [Serializable]
    public class AudioClipList : ReorderableArray<AudioClip> { }

    [Serializable]
    public class GameObjectList : ReorderableArray<GameObject> { }

    [Serializable]
    public class ItemPropertyDefinitionList : ReorderableArray<ItemPropertyDefinition> { }

    [Serializable]
    public class ItemPropertyInfoList : ReorderableArray<ItemPropertyInfo> { }

    [Serializable]
    public class ItemGeneratorList : ReorderableArray<ItemGenerator> { }

    [Serializable]
    public class ContainerGeneratorList : ReorderableArray<ContainerGenerator> { }

    [Serializable]
    public class FPArmsInfoList : ReorderableArray<FPArmsInfo> { }

    [Serializable]
    public class FPItemSkinsList : ReorderableArray<EquipmentSkin> { }

    [Serializable]
    public class ShaderSetupsList : ReorderableArray<HQFPS_HDRPConverterInfo.ShaderSetup> { }

    [Serializable]
    public class ScriptableObjectSetupsList : ReorderableArray<HQFPS_HDRPConverterInfo.ScriptableObjectSetup> { }
}