using UnityEditor.PackageManager;
using UnityEditor.Build;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace PolymindGamesEditor
{
    /// <summary>
    /// Utility class for modifying project dependencies and settings.
    /// </summary>
    public static class ProjectModificationUtility
    {
        /// <summary>
        /// Modifies project dependencies by adding and removing specified dependencies.
        /// </summary>
        /// <param name="dependenciesToAdd">Array of dependencies to add.</param>
        /// <param name="dependenciesToRemove">Array of dependencies to remove.</param>
        public static void ModifyDependencies(string[] dependenciesToAdd, string[] dependenciesToRemove)
        {
            var addAndRemoveRequest = Client.AddAndRemove(dependenciesToAdd, dependenciesToRemove);

            // Wait for the add and remove request to complete
            while (!addAndRemoveRequest.IsCompleted)
            { }

            if (addAndRemoveRequest.IsCompleted)
            {
                // Check if the request was successful
                if (addAndRemoveRequest.Status != StatusCode.Success)
                    Debug.LogError("Failed to add or remove dependencies. Error: " + addAndRemoveRequest.Error.message);
            }
            else
            {
                Debug.LogError("Add and remove request timed out.");
            }
        }

        /// <summary>
        /// Modifies scripting define symbols for a specific build target by adding and removing specified defines.
        /// </summary>
        /// <param name="definesToAdd">Array of defines to add.</param>
        /// <param name="definesToRemove">Array of defines to remove.</param>
        /// <param name="namedBuildTarget">The named build target for which to modify defines.</param>
        public static void ModifyDefines(string[] definesToAdd, string[] definesToRemove, NamedBuildTarget namedBuildTarget)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var defineList = defines.Split(';').ToList();

            foreach (var define in definesToAdd)
            {
                if (!defineList.Contains(define))
                    defineList.Add(define);
            }

            foreach (var define in definesToRemove)
            {
                if (defineList.Contains(define))
                    defineList.Remove(define);
            }

            defines = string.Join(";", defineList);
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        }
    }
}