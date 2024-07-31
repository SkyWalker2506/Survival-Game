using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PolymindGames;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public sealed class DataReferenceDrawer
    {
        private static readonly Dictionary<Type, CachedDataMethods> s_DataMethods = new();
        
        private readonly CachedDataMethods _cachedMethods;
        private readonly GUIContent[] _contents;
        private readonly DataReferenceDetailsAttribute _details;


        public DataReferenceDrawer(Type dataType, DataReferenceDetailsAttribute details)
        {
            _cachedMethods = GetDataMethods(dataType);
            _details = details;
            _contents = GetContents(_cachedMethods, details);
        }

        public DataReferenceDrawer(SerializedProperty property, FieldInfo fieldInfo)
        {
            var fieldType = fieldInfo != null ? fieldInfo.FieldType : property.GetFieldInfo().FieldType;
            var dataType = fieldType.IsArray
                ? fieldType.GetElementType()?.GenericTypeArguments.FirstOrDefault()
                : fieldType.GenericTypeArguments.FirstOrDefault();

            _cachedMethods = GetDataMethods(dataType);
            _details = PropertyUtility.GetAttribute<DataReferenceDetailsAttribute>(property);
            _contents = GetContents(_cachedMethods, _details);
        }

        public float GetHeight()
        {
            float extraHeight = HasAssetReference() ? EditorGUIUtility.singleLineHeight + 3f : 0f;
            return EditorGUIUtility.singleLineHeight + extraHeight;
        }

        public void OnGUI(SerializedProperty property, Rect position, GUIContent label)
        {
            Rect popupRect = new(position)
            {
                height = EditorGUIUtility.singleLineHeight
            };

            label ??= GUIContent.none;
            label.text = HasLabel() ? label.text : string.Empty;

            int selectedIndex = 0;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    selectedIndex = IndexOfElement(property.intValue);
                    selectedIndex = EditorGUI.Popup(popupRect, label, selectedIndex, _contents);
                    property.intValue = IdOfElement(selectedIndex);
                    break;
                case SerializedPropertyType.Vector2:
                    selectedIndex = IndexOfElement(Mathf.RoundToInt(property.vector2Value.x));
                    selectedIndex = EditorGUI.Popup(popupRect, label, selectedIndex, _contents);
                    property.vector2Value = new Vector2(IdOfElement(selectedIndex), 0f);
                    break;
                case SerializedPropertyType.String:
                    selectedIndex = IndexOfContent(_contents, property.stringValue);
                    selectedIndex = EditorGUI.Popup(position, label, selectedIndex, _contents);
                    property.stringValue = ContentAtIndex(_contents, selectedIndex).text;
                    break;
            }

            if (HasAssetReference())
                DrawAssetReference(position, selectedIndex);
        }

        private static CachedDataMethods GetDataMethods(Type dataType)
        {
            if (dataType == null)
                throw new ArgumentNullException();

            if (!s_DataMethods.TryGetValue(dataType, out var methods))
            {
                methods = new CachedDataMethods(dataType);
                s_DataMethods.Add(dataType, methods);
            }

            return methods;
        }

        private GUIContent[] GetContents(CachedDataMethods methods, DataReferenceDetailsAttribute details)
        {
            return methods.GetAllGUIContents(true, true, HasIcon(), HasNullElement()
                ? details is null ? new GUIContent("Empty") : new GUIContent(details.NullElementName)
                : null);
        }

        private bool HasLabel()
        {
            bool drawLabel = _details == null || _details.HasLabel;
            return drawLabel;
        }

        private bool HasNullElement()
        {
            bool hasNullElement = _details == null || _details.HasNullElement;
            return hasNullElement;
        }

        private bool HasIcon()
        {
            bool hasIcon = _details == null || _details.HasIcon;
            return hasIcon;
        }

        private bool HasAssetReference()
        {
            bool drawLabel = _details != null && _details.HasAssetReference;
            return drawLabel;
        }

        private void DrawAssetReference(Rect position, int selectedIndex)
        {
            if (HasNullElement())
                selectedIndex -= 1;

            using (new EditorGUI.DisabledScope(true))
            {
                float lineHeight = EditorGUIUtility.singleLineHeight;
                position = new Rect(position.x + lineHeight / 2, position.y + lineHeight, position.width - lineHeight,
                    lineHeight);

                var scriptable = _cachedMethods.GetDataAtIndex.Invoke(selectedIndex);
                EditorGUI.ObjectField(position, "Asset", scriptable, typeof(ScriptableObject), false);
            }
        }

        private int IdOfElement(int index)
        {
            if (HasNullElement() && index == 0)
                return 0;

            return _cachedMethods.GetIdAtIndex(index - (HasNullElement() ? 1 : 0));
        }

        private int IndexOfElement(int id)
        {
            if (HasNullElement() && id == 0)
                return 0;

            return _cachedMethods.GetIndexOfId(id) + (HasNullElement() ? 1 : 0);
        }

        private static int IndexOfContent(GUIContent[] allStrings, string str)
        {
            for (int i = 0; i < allStrings.Length; i++)
            {
                if (allStrings[i].text == str)
                    return i;
            }

            return 0;
        }

        private static GUIContent ContentAtIndex(GUIContent[] allStrings, int i)
        {
            return allStrings.Length > i ? allStrings[i] : GUIContent.none;
        }
        
        private sealed class CachedDataMethods
        {
            public delegate GUIContent[] GetAllGUIContentsDelegate(bool name, bool tooltip, bool icon, GUIContent including = null);
            public delegate DataDefinition GetDataAtIndexDelegate(int index);
            public delegate int GetIdAtIndexDelegate(int index);
            public delegate int GetIndexOfIdDelegate(int id);
            
            public readonly GetAllGUIContentsDelegate GetAllGUIContents;
            public readonly GetDataAtIndexDelegate GetDataAtIndex;
            public readonly GetIdAtIndexDelegate GetIdAtIndex;
            public readonly GetIndexOfIdDelegate GetIndexOfId;


            public CachedDataMethods(Type dataType)
            {
                GetDataAtIndex = typeof(DataDefinition<>).MakeGenericType(dataType)
                    .GetMethod("GetWithIndex")!
                    .CreateDelegate(typeof(GetDataAtIndexDelegate)) as GetDataAtIndexDelegate;

                GetIdAtIndex = typeof(DataDefinitionEditorUtility)
                    .GetMethod(nameof(DataDefinitionEditorUtility.GetIdAtIndex))!
                    .MakeGenericMethod(dataType).CreateDelegate(typeof(GetIdAtIndexDelegate)) as GetIdAtIndexDelegate;

                GetIndexOfId = typeof(DataDefinitionEditorUtility)
                    .GetMethod(nameof(DataDefinitionEditorUtility.GetIndexOfId))!
                    .MakeGenericMethod(dataType).CreateDelegate(typeof(GetIndexOfIdDelegate)) as GetIndexOfIdDelegate;

                GetAllGUIContents = typeof(DataDefinitionEditorUtility)
                    .GetMethod(nameof(DataDefinitionEditorUtility.GetAllGUIContents))!
                    .MakeGenericMethod(dataType).CreateDelegate(typeof(GetAllGUIContentsDelegate)) as GetAllGUIContentsDelegate;
            }
        }
    }
}