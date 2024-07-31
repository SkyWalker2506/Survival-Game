using PolymindGames;
using PolymindGames.WieldableSystem;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.WieldableSystem
{
    [CustomEditor(typeof(MeleeWeapon))]
    public sealed class MeleeWeaponEditor : WieldableEditor
    {
        protected override void DrawChildInspector()
        {
            base.DrawChildInspector();

            EditorGUILayout.Space();

            if (((MonoBehaviour)target).gameObject.HasComponent<IMeleeAttackHandler>())
                EditorGUILayout.HelpBox("Melee Attack Handler: found", UnityEditor.MessageType.Info);
            else
                EditorGUILayout.HelpBox("Melee Attack Handler: missing!", UnityEditor.MessageType.Error);
        }
    }
}