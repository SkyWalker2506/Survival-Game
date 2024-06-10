//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using System.Collections.Generic;
using UnityEngine;

namespace HQFPSTemplate
{
    public static class ObjectUtils
    {
		/// <summary>
		/// <para>Key: Default Prefab </para> 
		/// Value: Suffixed Prefab
		/// </summary>
		/// <param name="prefabs"></param>
		public static Dictionary<T, T> GetSuffixedObjectsDictionary<T>(T[] objectsList, string suffix) where T : Object
		{
            Dictionary<T, T> suffixedObjectsDictionary = new Dictionary<T, T>();

            for (int i = 0; i < objectsList.Length; i++)
            {
                if (!objectsList[i].name.Contains(suffix))
                {
                    for (int j = 0; j < objectsList.Length; j++)
                    {
                        if (objectsList[i].GetType() == objectsList[j].GetType())
                        {
                            if (objectsList[j].name.Contains(suffix))
                            {
                                if (objectsList[i].name + suffix == objectsList[j].name)
                                {
                                    suffixedObjectsDictionary.Add(objectsList[i], objectsList[j]);
                                }
                            }
                        }
                    }
                }
            }

            return suffixedObjectsDictionary;
        }

        public static T TryGetSuffixedObject<T>(string objectName, T[] objectsToSearch, string suffix) where T : Object
        {
            foreach (var objectToSearch in objectsToSearch)
            {
                if(objectToSearch.name == objectName + suffix)
                    return objectToSearch;
            }

            return null;
        }
	}
}
