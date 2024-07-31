using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace PolymindGames
{
    public abstract class DataDefinition<T> : DataDefinition where T : DataDefinition<T>
    {
        private static T[] s_Definitions = Array.Empty<T>();
        private static Dictionary<int, T> s_DefinitionsById;
        

        public static T[] Definitions
        {
            get
            {
                if (s_Definitions == Array.Empty<T>())
                    LoadDefinitions();

                return s_Definitions;
            }
        }

        private static Dictionary<int, T> DefinitionsById
        {
            get
            {
                if (s_DefinitionsById == null)
                    CreateIdDefinitionsDict();

                return s_DefinitionsById;
            }
        }

        #region Accessing Methods
        /// <summary>
        /// Tries to return a definition with the given id.
        /// </summary>
        public static bool TryGetWithId(int id, out T def)
        {
            return DefinitionsById.TryGetValue(id, out def);
        }

        /// <summary>
        /// Returns a definition with the given id.
        /// </summary>
        public static T GetWithId(int id)
        {
            return DefinitionsById.TryGetValue(id, out T def) ? def : null;
        }

        /// <summary>
        /// Returns a definition with the given id.
        /// </summary>
        public static T GetWithIndex(int index)
        {
            if (index >= 0 && index < Definitions.Length)
                return Definitions[index];

            return null;
        }

        /// <summary>
        /// <para>Tries to return a definition with the given name.</para>
        /// Note: For much better performance use the "GetWithId" methods instead.
        /// </summary>
        public static bool TryGetWithName(string defName, out T def)
        {
            if (string.IsNullOrEmpty(defName))
            {
                def = null;
                return false;
            }

            int nameHash = defName.GetHashCode();
            int definitionsCount = Definitions.Length;

            for (int i = 0; i < definitionsCount; i++)
            {
                if (s_Definitions[i].Name.GetHashCode() == nameHash)
                {
                    def = s_Definitions[i];
                    return true;
                }
            }

            def = null;
            return false;
        }

        /// <summary>
        /// <para>Returns a definition with the given name.</para>
        /// Note: For much better performance use the "GetWithId" methods instead.
        /// </summary>
        public static T GetWithName(string defName)
        {
            if (string.IsNullOrEmpty(defName))
                return null;

            int nameHash = defName.GetHashCode();
            int definitionsCount = Definitions.Length;

            for (int i = 0; i < definitionsCount; i++)
            {
                if (s_Definitions[i].Name.GetHashCode() == nameHash)
                    return s_Definitions[i];
            }

            return null;
        }
        #endregion

        #region Definition Loading
        private static void LoadDefinitions()
        {
            string path = typeof(T).Name.Replace("Definition", "");

            s_Definitions = Resources.LoadAll<T>(path + "s");
            if (s_Definitions != null && s_Definitions.Length > 0)
                return;

            path = path.Remove(path.Length - 1, 1) + "ies";

            s_Definitions = Resources.LoadAll<T>(path);
            if (s_Definitions != null && s_Definitions.Length > 0)
                return;

            s_Definitions = Resources.LoadAll<T>(path);
            if (s_Definitions != null && s_Definitions.Length > 0)
                return;

            s_Definitions = Array.Empty<T>();
        }

        private static void CreateIdDefinitionsDict()
        {
            if (s_DefinitionsById != null)
                s_DefinitionsById.Clear();
            else
                s_DefinitionsById = new Dictionary<int, T>(s_Definitions.Length);

            var definitions = Definitions;
            for (int i = 0; i < definitions.Length; i++)
            {
                T def = definitions[i];

#if UNITY_EDITOR
                if (def.Id == -1 || s_DefinitionsById.ContainsKey(def.Id))
                    def.AssignID();
#endif

                try
                {
                    s_DefinitionsById.Add(def.Id, def);
                }
                catch
                {
                    Debug.LogError($"Multiple '{nameof(T)}' of the same id are found. Restarting Unity should fix this problem.");
                }
            }
        }
        #endregion

        #region Editor Methods
#if UNITY_EDITOR
        [Conditional("UNITY_EDITOR")]
        public static void ReloadDefinitions()
        {
            LoadDefinitions();
            CreateIdDefinitionsDict();
        }

        public override void Reset()
        {
            base.Reset();
            AssignID();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (Id == -1)
            {
                AssignID();
                return;
            }

            if (Event.current is { type: EventType.Used, commandName: "Duplicate" or "Paste" })
                AssignID();
        }

        /// <summary>
        /// Generates and assigns a unique id to this definition.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        private void AssignID()
        {
            const int MAX_ASSIGNMENT_TRIES = 10;

            int assignmentTries = 0;
            while (assignmentTries < MAX_ASSIGNMENT_TRIES)
            {
                int assignedId = Random.Range(int.MinValue, int.MaxValue);
                assignmentTries++;

                // If no other definition uses this id assign it.
                if (!FindWithId(assignedId))
                {
                    SetId(assignedId);
                    return;
                }
            }

            Debug.LogError($"Couldn't generate an unique id for definition: {Name}");
            return;

            static bool FindWithId(int id)
            {
                foreach (var definition in Definitions)
                {
                    if (definition.Id == id)
                        return true;
                }

                return false;
            }
        }
#endif
        #endregion
    }
}