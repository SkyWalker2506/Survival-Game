using System;
using System.Collections.Generic;
using PolymindGames;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public abstract class DataDefinitionListBase
    {
        private static DataDefinitionListBase s_FocusedList;


        protected static void FocusList(DataDefinitionListBase list) => s_FocusedList = list;
        
        protected bool HasFocus() => s_FocusedList == this;
        protected void RemoveFocus()
        {
            if (s_FocusedList == this)
                s_FocusedList = null;
        }
    }

    public abstract class DataDefinitionList<T> : DataDefinitionListBase where T : DataDefinition<T>
    {
        protected readonly List<T> Definitions;
        
        private readonly DataDefinitionAction[] _customActions;
        private int _selectedIndex;
        private bool _isDirty;

        private static DataDefinition s_Copy;


        protected DataDefinitionList(string listName, params DataDefinitionAction[] customActions)
        {
            ListName = listName;
            Definitions = new List<T>();
            _customActions = customActions;
        }

        public Func<T[]> GetDefinitions { get; set; }
        public bool IsInteractable { get; set; } = true;
        public string ListName { get; }

        public int Count => Definitions?.Count ?? 0;
        public string SelectedName => Selected != null ? Selected.Name : string.Empty;

        public T Selected => Definitions.Count > 0 ? Definitions[SelectedIndex] : null;

        public int SelectedIndex
        {
            get => Mathf.Clamp(_selectedIndex, 0, Definitions.Count != 0 ? Definitions.Count - 1 : 0);
            private set => _selectedIndex = Mathf.Clamp(value, 0, Definitions.Count - 1);
        }

        public event Action Refreshed;
        public event Action<T> DefinitionSelected;
        public event Action<T> DefinitionCreated;
        public event Action<T> DefinitionDeleted;

        public void DoLayoutWithHeader(string selectedName, Action<GUILayoutOption[]> drawAction, params GUILayoutOption[] options)
        {
            using (new GUILayout.VerticalScope(GuiStyles.Box, options))
            {
                selectedName = selectedName != string.Empty ? $"({selectedName}) " : string.Empty;
                GUILayout.Label($"{ListName} {selectedName}- {Count}", GuiStyles.Title, options);
                DoLayout();

                drawAction?.Invoke(options);
            }
        }

        public void DoLayoutWithHeader(string selectedName, params GUILayoutOption[] options)
        {
            using (new GUILayout.VerticalScope(GuiStyles.Box, options))
            {
                selectedName = selectedName != string.Empty ? $"({selectedName}) " : string.Empty;
                GUILayout.Label($"{ListName} {selectedName}- {Count}", GuiStyles.Title, options);
                DoLayout();
            }
        }

        protected virtual void DoLayout()
        {
            if (Event.current.keyCode == KeyCode.F5)
            {
                RefreshDefinitions();
                return;
            }

            if (_isDirty)
            {
                RefreshDefinitions();
                _isDirty = false;
            }
            else if (Selected != null && Selected.IsDirty())
            {
                _isDirty = true;
                RemoveFocus();
                Selected.ClearDirty();
            }
        }

        public void RefreshDefinitions()
        {
            DataDefinition<T>.ReloadDefinitions();
            SetDefinitions(GetDefinitions != null ? GetDefinitions() : DataDefinition<T>.Definitions);

            Refreshed?.Invoke();
        }

        public void SelectIndex(int index)
        {
            SelectedIndex = index;
            DefinitionSelected?.Invoke(Selected);
            FocusList(this);
        }

        public void SelectDefinition(T def)
        {
            int index = GetIndexOfDefinition(def);

            if (index != -1)
                SelectIndex(index);
        }

        private void CreateDefinition(string defName)
        {
            if (DataDefinitionEditorUtility.TryGetAssetCreationPath<T>(defName, out string path))
            {
                T newDef = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(newDef, path);

                FocusList(this);
                EditorUtility.SetDirty(newDef);

                newDef.Reset();

                DefinitionCreated?.Invoke(newDef);
                Undo.RegisterCreatedObjectUndo(newDef, "New Definition");

                RefreshDefinitions();
                SelectIndex(GetIndexOfDefinition(newDef));
            }
        }

        private void DuplicateDefinition(T def, string defName)
        {
            if (def != null && DataDefinitionEditorUtility.TryGetAssetCreationPath<T>(defName, out string defPath))
            {
                string assetPath = AssetDatabase.GetAssetPath(def);

                if (AssetDatabase.CopyAsset(assetPath, defPath))
                {
                    T newDef = AssetDatabase.LoadAssetAtPath<T>(defPath);

                    FocusList(this);
                    EditorUtility.SetDirty(newDef);

                    newDef.Reset();

                    DefinitionCreated?.Invoke(newDef);
                    Undo.RegisterCreatedObjectUndo(newDef, "New Definition");

                    RefreshDefinitions();
                    SelectIndex(GetIndexOfDefinition(newDef));
                }
            }
        }

        private void DeleteDefinition(T def)
        {
            if (def == null)
                return;

            int lastIndex = GetIndexOfDefinition(Selected);

            string assetPath = AssetDatabase.GetAssetPath(def);
            if (AssetDatabase.DeleteAsset(assetPath))
            {
                FocusList(this);

                DefinitionDeleted?.Invoke(def);
                RefreshDefinitions();
                SelectIndex(lastIndex - 1);
            }
        }

        private void DeleteAllDefinitions()
        {
            for (int i = Count - 1; i > -1; i--)
                DeleteDefinition(Definitions[i]);
        }

        private int GetIndexOfDefinition(T def)
        {
            for (int i = 0; i < Definitions.Count; i++)
            {
                if (def == Definitions[i])
                    return i;
            }

            return -1;
        }

        protected virtual void SetDefinitions(T[] dataDefs)
        {
            int prevScriptableCount = Count;

            Definitions.Clear();
            for (int i = 0; i < dataDefs.Length; i++)
            {
                if (dataDefs[i] != null)
                    Definitions.Add(Definitions[i]);
            }

            if (prevScriptableCount != Count)
                SelectDefinition(Selected);
        }

        protected void DrawListEditingGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (HandleListEditing())
                {
                    Event.current.Use();
                    FocusList(this);
                }
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(3f);
        }

        private bool HandleListEditing()
        {
            var current = Event.current;
            bool doAction = HasFocus() && current.type == EventType.KeyDown;
            bool control = current.control;
            KeyCode keyCode = current.keyCode;

            if (doAction)
            {
                switch (keyCode)
                {
                    case KeyCode.DownArrow:
                        SelectIndex(_selectedIndex + 1);
                        return true;
                    case KeyCode.UpArrow:
                        SelectIndex(_selectedIndex - 1);
                        return true;
                }
            }

            using (new EditorGUI.DisabledScope(!IsInteractable))
            {
                if (GuiLayout.ColoredButton(DataDefinitionEditorStyles.CreateEmptyContent, GuiStyles.GreenColor, DataDefinitionEditorStyles.ButtonLayoutOptions) ||
                    doAction && control && keyCode == KeyCode.Space)
                {
                    DataDefinitionActionWindow.OpenWindow(CreateDefinition, typeof(T).Name, "Create");
                    return true;
                }
            }

            using (new EditorGUI.DisabledScope(Selected == null))
            {
                if (GuiLayout.ColoredButton(DataDefinitionEditorStyles.DuplicateSelectedContent,
                        GuiStyles.BlueColor, DataDefinitionEditorStyles.ButtonLayoutOptions) ||
                    doAction && control && keyCode == KeyCode.D)
                {
                    DataDefinitionActionWindow.OpenWindow(name => DuplicateDefinition(Selected, name), typeof(T).Name,
                        "Duplicate");
                    return true;
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(Selected == null))
            {
                if (GuiLayout.ColoredButton(DataDefinitionEditorStyles.DeleteSelectedContent,
                        GuiStyles.RedColor, DataDefinitionEditorStyles.ButtonLayoutOptions) ||
                    doAction && !control && keyCode == KeyCode.Delete)
                {
                    if (EditorUtility.DisplayDialog("Delete element?", $"Delete ''{Selected.Name}''?", "Ok", "Cancel"))
                    {
                        DeleteDefinition(Selected);
                        return true;
                    }
                }
            }

            using (new EditorGUI.DisabledScope(Selected == null || Count == 0))
            {
                if (GuiLayout.ColoredButton(DataDefinitionEditorStyles.DeleteAllContent, GuiStyles.RedColor, DataDefinitionEditorStyles.ButtonLayoutOptions) ||
                    doAction && control && keyCode == KeyCode.Delete)
                {
                    if (EditorUtility.DisplayDialog("Delete all elements", "Remove all of the elements from this list?", "Ok", "Cancel"))
                    {
                        DeleteAllDefinitions();
                        return true;
                    }
                }
            }

            if (doAction && control && keyCode == KeyCode.C)
            {
                s_Copy = Selected;
                EditorGUIUtility.systemCopyBuffer = string.Empty;
                return true;
            }

            if (EditorGUIUtility.systemCopyBuffer == string.Empty && s_Copy != null)
            {
                if (doAction && control && keyCode == KeyCode.V)
                {
                    DuplicateDefinition((T)s_Copy, (T)s_Copy != null ? s_Copy.Name : string.Empty);
                    s_Copy = null;
                    return true;
                }
            }

            EditorGUILayout.Space();

            foreach (var action in _customActions)
            {
                action.DrawGUI(DataDefinitionEditorStyles.ButtonLayoutOptions);

                if (doAction)
                {
                    action.HandleEvent(current);
                    return true;
                }
            }

            return false;
        }
    }
}