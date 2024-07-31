using PolymindGames;
using PolymindGames.InventorySystem;
using Toolbox.Editor;
using UnityEditor;

namespace PolymindGamesEditor.InventorySystem
{
    [CustomEditor(typeof(ItemPickup), true)]
    public class ItemPickupEditor : ToolboxEditor
    {
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            EditorGUILayout.Space();

            if (GuiLayout.ColoredButton("Reset Item", GuiStyles.YellowColor))
            {
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot((ItemPickup)target);

                ItemPickup itemPickup;

                if (!string.IsNullOrEmpty(path))
                    itemPickup = AssetDatabase.LoadAssetAtPath<ItemPickup>(path);
                else
                    itemPickup = (ItemPickup)target;

                if (itemPickup == null)
                    return;

                foreach (var def in ItemDefinition.Definitions)
                {
                    if (def.Pickup == itemPickup)
                    {
                        target.SetFieldValue("m_Item", new DataIdReference<ItemDefinition>(def));
                        break;
                    }
                }

                EditorUtility.SetDirty(target);
            }
        }
    }
}