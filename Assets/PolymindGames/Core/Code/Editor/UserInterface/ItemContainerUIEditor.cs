using PolymindGames;
using PolymindGames.UserInterface;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.UISystem
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ItemContainerUI), true)]
    public sealed class ItemContainerUIEditor : ObjectEditor
    {
        private SerializedProperty _slotsParent;
        private SerializedProperty _slotTemplate;
        private int _slotCount;


        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            serializedObject.Update();

            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                GuiLayout.Separator();
                using (new GUILayout.HorizontalScope())
                {
                    if (!serializedObject.isEditingMultipleObjects && GuiLayout.ColoredButton("Spawn Default Slots", GuiStyles.GreenColor))
                        ((ItemContainerUI)target).GenerateSlots(_slotCount);

                    _slotCount = EditorGUILayout.IntField(_slotCount);
                    _slotCount = Mathf.Clamp(_slotCount, 0, 100);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _slotCount = ((Component)target).gameObject.GetComponentsInFirstChildren<ItemSlotUI>().Count;
        }
    }
}