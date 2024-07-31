using System;
using System.Reflection;
using JetBrains.Annotations;
using Toolbox.Editor.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Toolbox.Editor.Drawers
{
    [UsedImplicitly]
    public sealed class NestedBehaviourListPropertyDrawer : ToolboxListPropertyDrawer<NestedBehaviourListAttribute>
    {
        private static readonly PropertyDataStorage<ReorderableListBase, NestedBehaviourListAttribute> s_Storage = new(false, CreateList);


        protected override void OnGuiSafe(SerializedProperty property, GUIContent label, NestedBehaviourListAttribute attribute)
        {
            s_Storage.ReturnItem(property, attribute).DoList(label);
        }

        private static ReorderableListBase CreateList(SerializedProperty property, NestedBehaviourListAttribute attribute)
        {
            var list = ToolboxEditorGui.CreateList(property, attribute.ListStyle);
            list.drawHeaderCallback = DrawHeader;
            list.Draggable = attribute.Draggable;
            list.FixedSize = attribute.FixedSize;
            list.ElementLabel = attribute.ElementLabel;
            list.HasHeader = attribute.HasHeader;
            list.HasLabels = attribute.HasLabels;
            list.Foldable = attribute.Foldable;
            list.drawElementCallback = DrawElement;
            list.onRemoveCallback = OnRemove;
            list.onAppendDropdownCallback = OnAddDropdown;

            void DrawHeader(Rect rect)
            {
                var label = EditorGUI.BeginProperty(rect, list.TitleLabel, list.List);

                //display the property label using the preprocessed name
                list.DrawStandardName(rect, label, list.Foldable);
                EditorGUI.EndProperty();
            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                var element = list.List.GetArrayElementAtIndex(index);

                if (Event.current.type is not (EventType.Layout or EventType.Repaint))
                {
                    var fieldInfo = element.GetFieldInfo();
                    var behaviour = element.GetProperValue(fieldInfo) as MonoBehaviour;
                    if (behaviour == null)
                    {
                        Debug.LogWarning("Null element found and removed.");
                        list.RemoveElement(index);
                        return;
                    }

                    GameObject parent = ((MonoBehaviour)list.List.serializedObject.targetObject).gameObject;
                    if (parent != null && behaviour.gameObject != null && parent != behaviour.gameObject)
                        DuplicateMultiReferencedBehavior(element, parent, behaviour, fieldInfo);
                }

                ToolboxEditorGui.DrawToolboxProperty(element, GUIContent.none);
            }

            static void OnRemove(ReorderableListBase reorderableList)
            {
                SerializedProperty element = reorderableList.List.GetArrayElementAtIndex(reorderableList.Index);
                DeleteBehaviour(element.objectReferenceValue);
                reorderableList.List.DeleteArrayElementAtIndex(reorderableList.Index);
            }

            void OnAddDropdown(Rect _, ReorderableListBase reorderableList)
            {
                GenericMenu menu = new();

                var parentType = GetParentType(property, attribute);
                foreach (var type in parentType.GetAllChildClasses())
                {
                    var pathAttribute = type.GetCustomAttribute<NestedObjectPathAttribute>();
                    string path = pathAttribute == null ? type.Name : pathAttribute.MenuName;
                    menu.AddItem(new GUIContent(path), false, _ => AddBehaviour(list, type), type.Name);
                }

                menu.ShowAsContext();
            }

            void AddBehaviour(ReorderableListBase reorderableList, Type scriptableType)
            {
                GameObject parent = ((MonoBehaviour)property.serializedObject.targetObject).gameObject;
                var newBehaviour = CreateBehaviour(parent, scriptableType, attribute.HideBehaviours);

                reorderableList.SerializedObject.Update();

                var index = reorderableList.List.arraySize;
                reorderableList.List.arraySize++;
                reorderableList.Index = index;
                var element = list.List.GetArrayElementAtIndex(index);
                element.objectReferenceValue = newBehaviour;

                reorderableList.SerializedObject.ApplyModifiedProperties();
            }

            void DuplicateMultiReferencedBehavior(SerializedProperty element, GameObject parent, MonoBehaviour behaviour, FieldInfo fieldInfo)
            {
                var newBehaviour = DuplicateBehaviour(parent, behaviour, attribute.HideBehaviours);
                element.SetProperValue(fieldInfo, newBehaviour);
            }

            return list;
        }

        private static Type GetParentType(SerializedProperty property, NestedBehaviourListAttribute attribute)
        {
            var fieldInfo = property.GetFieldInfo(out _);
            var fieldType = property.GetProperType(fieldInfo);
            fieldType = fieldType.GetElementType();

            var candidateType = attribute.ParentType;
            if (candidateType != null)
            {
                if (fieldType != null && fieldType.IsAssignableFrom(candidateType))
                    return candidateType;

                Debug.LogError($"Provided {nameof(attribute.ParentType)} ({candidateType}) cannot be used because it's not assignable from: '{fieldType}'");
            }

            return fieldType;
        }

        private static MonoBehaviour DuplicateBehaviour(GameObject parent, MonoBehaviour copy, bool hide)
        {
            ComponentUtility.CopyComponent(copy);
            ComponentUtility.PasteComponentAsNew(parent);

            MonoBehaviour newBehaviour = (MonoBehaviour)parent.GetComponent(copy.GetType());
            newBehaviour.hideFlags = hide ? HideFlags.HideInInspector : HideFlags.None;

            EditorUtility.SetDirty(parent);
            EditorUtility.SetDirty(newBehaviour);

            return newBehaviour;
        }

        private static MonoBehaviour CreateBehaviour(GameObject parent, Type type, bool hide)
        {
            MonoBehaviour newBehaviour = (MonoBehaviour)parent.AddComponent(type);
            newBehaviour.hideFlags = hide ? HideFlags.HideInInspector : HideFlags.None;

            EditorUtility.SetDirty(parent);
            EditorUtility.SetDirty(newBehaviour);

            return newBehaviour;
        }

        private static void DeleteBehaviour(Object objectToDelete)
        {
            Object.DestroyImmediate(objectToDelete, true);
        }
    }
}