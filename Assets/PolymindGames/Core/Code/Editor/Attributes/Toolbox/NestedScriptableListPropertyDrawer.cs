using System;
using System.Reflection;
using JetBrains.Annotations;
using Toolbox.Editor.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Toolbox.Editor.Drawers
{
    [UsedImplicitly]
    public sealed class NestedScriptableListPropertyDrawer : ToolboxListPropertyDrawer<NestedScriptableListAttribute>
    {
        private static readonly PropertyDataStorage<ReorderableListBase, NestedScriptableListAttribute> s_Storage = new(false, CreateList);


        protected override void OnGuiSafe(SerializedProperty property, GUIContent label, NestedScriptableListAttribute attribute)
        {
            s_Storage.ReturnItem(property, attribute).DoList(label);
        }

        private static ReorderableListBase CreateList(SerializedProperty property, NestedScriptableListAttribute attribute)
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
                    var subAsset = element.GetProperValue(fieldInfo) as ScriptableObject;

                    if (subAsset == null)
                    {
                        Debug.LogWarning("Null element found and removed.");
                        list.RemoveElement(index);
                        return;
                    }

                    var asset = (ScriptableObject)list.List.serializedObject.targetObject;
                    if (AssetDatabase.GetAssetPath(asset) != AssetDatabase.GetAssetPath(subAsset))
                        DuplicateMultiReferencedScriptable(element, asset, subAsset, fieldInfo);
                }

                ToolboxEditorGui.DrawToolboxProperty(element, GUIContent.none);
            }

            static void OnRemove(ReorderableListBase reorderableList)
            {
                SerializedProperty element = reorderableList.List.GetArrayElementAtIndex(reorderableList.Index);
                DeleteScriptable(element.objectReferenceValue);
                reorderableList.List.DeleteArrayElementAtIndex(reorderableList.Index);
                reorderableList.SerializedObject.ApplyModifiedProperties();
                
                AssetDatabase.SaveAssets();
            }

            void OnAddDropdown(Rect _, ReorderableListBase reorderableList)
            {
                GenericMenu menu = new();

                var parentType = GetParentType(property, attribute);
                foreach (var type in parentType.GetAllChildClasses())
                {
                    var pathAttribute = type.GetCustomAttribute<NestedObjectPathAttribute>();
                    string path = pathAttribute == null ? type.Name : pathAttribute.MenuName;
                    menu.AddItem(new GUIContent(path), false, _ => AddScriptable(list, type, pathAttribute), type.Name);
                }

                menu.ShowAsContext();
            }

            void AddScriptable(ReorderableListBase reorderableList, Type scriptableType, NestedObjectPathAttribute pathAttribute)
            {
                var parentScriptable = property.serializedObject.targetObject as ScriptableObject;
                string newScriptableName = pathAttribute == null ? scriptableType.Name : pathAttribute.FileName;
                var newScriptable = CreateScriptable(parentScriptable, scriptableType, attribute.HideAssets, newScriptableName);

                reorderableList.SerializedObject.Update();
                
                var index = reorderableList.List.arraySize;
                reorderableList.List.arraySize++;
                reorderableList.Index = index;
                var element = list.List.GetArrayElementAtIndex(index);
                element.objectReferenceValue = newScriptable;

                reorderableList.SerializedObject.ApplyModifiedProperties();
                
                AssetDatabase.SaveAssets();
            }

            void DuplicateMultiReferencedScriptable(SerializedProperty element, ScriptableObject asset, ScriptableObject subAsset, FieldInfo fieldInfo)
            {
                var sType = subAsset.GetType();
                var pathAttribute = sType.GetCustomAttribute<NestedObjectPathAttribute>();

                string newScriptableName = pathAttribute == null ? sType.Name : pathAttribute.FileName;
                var newScriptable = DuplicateScriptable(asset, subAsset, attribute.HideAssets, newScriptableName);

                element.SetProperValue(fieldInfo, newScriptable);
                
                AssetDatabase.SaveAssets();
            }

            return list;
        }

        private static Type GetParentType(SerializedProperty property, NestedScriptableListAttribute attribute)
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

        private static ScriptableObject DuplicateScriptable(ScriptableObject parentScriptable, ScriptableObject copy, bool hide, string fileName)
        {
            ScriptableObject newScriptable = Object.Instantiate(copy);

            newScriptable.name = "_" + fileName;
            newScriptable.hideFlags = hide ? HideFlags.HideInHierarchy : HideFlags.None;

            AssetDatabase.AddObjectToAsset(newScriptable, parentScriptable);
            return newScriptable;
        }

        private static ScriptableObject CreateScriptable(ScriptableObject parentScriptable, Type type, bool hide, string fileName)
        {
            ScriptableObject newScriptable = ScriptableObject.CreateInstance(type);

            newScriptable.name = "_" + fileName;
            newScriptable.hideFlags = hide ? HideFlags.HideInHierarchy : HideFlags.None;

            AssetDatabase.AddObjectToAsset(newScriptable, parentScriptable);
            return newScriptable;
        }

        private static void DeleteScriptable(Object objectToDelete)
        {
            if (objectToDelete == null)
                return;

            AssetDatabase.RemoveObjectFromAsset(objectToDelete);
            Object.DestroyImmediate(objectToDelete, true);
        }
    }
}