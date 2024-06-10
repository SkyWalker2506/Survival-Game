//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HQFPSTemplate
{
	public static class EditorProjectUtils
	{
		public static T[] GetAllAssetsOfType<T>(params string[] searchPath) where T : Object
		{
			var assetGUIDs = AssetDatabase.FindAssets("t:" + typeof(T).Name, AddPrefixToStringArray(searchPath, "Assets/"));
			T[] assets = new T[assetGUIDs.Length];

			for (int i = 0; i < assetGUIDs.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			}

			return assets;
		}

		public static T[] GetAllAssetObjectsOfType<T>(System.Type type, params string[] searchPath) where T : Object
		{
			var assetGUIDs = AssetDatabase.FindAssets("t:" + type.Name, AddPrefixToStringArray(searchPath, "Assets/"));
			T[] assets = new T[assetGUIDs.Length];

			for (int i = 0; i < assetGUIDs.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			}

			return assets;
		}

		public static GameObject[] GetAllPrefabsAtPath(params string[] searchPath)
		{
			var assetGUIDs = AssetDatabase.FindAssets("t:Prefab", AddPrefixToStringArray(searchPath, "Assets/"));
			GameObject[] assets = new GameObject[assetGUIDs.Length];

			for (int i = 0; i < assetGUIDs.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
			}

			return assets;
		}

		public static T[] GetAllPrefabsWithComponent<T>(params string[] searchPath) where T : Component
		{
			string[] assetGUIDs = AssetDatabase.FindAssets("t:Prefab", AddPrefixToStringArray(searchPath, "Assets/"));
			List<T> prefabsWithTComponent = new List<T>();

			for (int i = 0; i < assetGUIDs.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
				GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

				if (obj.TryGetComponent<T>(out T component))
					prefabsWithTComponent.Add(component);
			}

			return prefabsWithTComponent.ToArray();
		}

		public static T GetPrefabWithComponent<T>(params string[] searchPath) where T : Component
		{
			string[] assetGUIDs = AssetDatabase.FindAssets("t:Prefab", AddPrefixToStringArray(searchPath, "Assets/"));

			for (int i = 0; i < assetGUIDs.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
				GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

				if (obj.TryGetComponent<T>(out T component))
					return component;
			}

			return null;
		}

		private static string[] AddPrefixToStringArray(string[] st, string prefix) 
		{
			for (int i = 0; i < st.Length; i++)
				st[i] = prefix + st[i];

			return st;
		}

		/// <summary>
		/// Bool: Replaced Successfully
		/// </summary>
		/// <param name="Prefab"></param>
		/// <param name="New Prefab"></param>
		/// <returns></returns>
		public static bool ReplacePrefab(GameObject prefab, GameObject newPrefab)
		{
			if (PrefabUtility.IsPartOfPrefabAsset(prefab.gameObject) && PrefabUtility.IsPartOfPrefabAsset(newPrefab.gameObject))
			{
				var instancePrefab = PrefabUtility.LoadPrefabContents(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(newPrefab.gameObject));
				PrefabUtility.SaveAsPrefabAsset(instancePrefab, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab.gameObject));

				return true;
			}

			return false;
		}

		public static void ReplaceSerializedObject<T>(T assetToReplace, T newAsset) where T : Object
		{
			string prevName = assetToReplace.name;
			EditorUtility.CopySerialized(newAsset, assetToReplace);
			assetToReplace.name = prevName;
		}
	}
}