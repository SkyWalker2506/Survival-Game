using PolymindGames;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGamesEditor
{
    public sealed class GroupDefinitionPageContent<Group, Member> : IEditorDrawable
        where Group : GroupDefinition<Group, Member>
        where Member : GroupMemberDefinition<Member, Group>
    {
        public readonly DataDefinitionToolbar<Group> GroupsToolbar;
        public readonly DataDefinitionToolbar<Member> MembersToolbar;
        
        private readonly CachedObjectEditor _groupInspector;
        private readonly CachedObjectEditor _memberInspector;
        private readonly DataDefinitionUnlinker<Group, Member> _unlinker;
        private bool _isInspectorVisible;


        #region Logic
        public GroupDefinitionPageContent(string groupsToolbarName, string membersToolbarName)
        {
            // Create the member unlinker.
            _unlinker = new DataDefinitionUnlinker<Group, Member>(GUILayout.Height(258f));
            _unlinker.DefinitionAdded += OnMemberLinked;

            // Create the groups toolbar and inspector.
            var mergeAction = new DataDefinitionAction(MergeGroup, CanMerge, DataDefinitionEditorStyles.MergeContent, KeyCode.None, GuiStyles.BlueColor);

            GroupsToolbar = new DataDefinitionToolbar<Group>(groupsToolbarName, mergeAction)
            {
                ButtonHeight = 35f,
                IconSize = 0.7f
            };

            GroupsToolbar.DefinitionSelected += OnGroupSelected;
            GroupsToolbar.DefinitionDeleted += OnGroupDeleted;

            _groupInspector = new CachedObjectEditor(GroupsToolbar.Selected, GUILayout.Height(260f));

            // Create the items toolbar and inspector.
            var unlinkAction = new DataDefinitionAction(UnlinkMember, CanUnlink, DataDefinitionEditorStyles.UnlinkContent, KeyCode.U, GuiStyles.YellowColor);
            MembersToolbar = new DataDefinitionToolbar<Member>(membersToolbarName, unlinkAction)
            {
                ButtonHeight = 45f, // Default Value
                IconSize = 0.9f
            };

            _memberInspector = new CachedObjectEditor(MembersToolbar.Selected);

            MembersToolbar.Refreshed += _unlinker.Refresh;
            MembersToolbar.DefinitionSelected += _memberInspector.SetObject;
            MembersToolbar.DefinitionCreated += OnMemberCreated;
            MembersToolbar.DefinitionDeleted += OnMemberDeleted;
            MembersToolbar.GetDefinitions = GetMembers;

            GroupsToolbar.RefreshDefinitions();
            MembersToolbar.RefreshDefinitions();


            #region Local Methods
            void OnGroupSelected(Group group)
            {
                MembersToolbar.RefreshDefinitions();
                _groupInspector.SetObject(group);
                _memberInspector.SetObject(MembersToolbar.Selected);
            }

            void OnGroupDeleted(Group group)
            {
                if (group == null || group.Members.Count == 0)
                    return;

                if (EditorUtility.DisplayDialog("Delete all child members", "Remove all of the child members from this group?", "Ok", "Unlink"))
                {
                    foreach (var item in group.Members)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(item);
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }
            }

            void MergeGroup()
            {
                var groups = DataDefinitionEditorUtility.GetAllGUIContents<Group>(true, true, true);
                GroupMergeWindow.OpenWindow(groups, Merge, GroupsToolbar.SelectedIndex);

                void Merge(int index)
                {
                    var group1 = GroupsToolbar.Selected;
                    var group2 = DataDefinition<Group>.GetWithIndex(index);
                    group1.MergeWith(group2);

                    GroupsToolbar.RefreshDefinitions();
                    GroupsToolbar.SelectDefinition(group1);
                }
            }

            bool CanMerge()
            {
                return GroupsToolbar.Selected != null && GroupsToolbar.Count > 1;
            }

            void OnMemberCreated(Member member)
            {
                GroupsToolbar.Selected.AddMember(member);
                GroupsToolbar.Selected.AddDefaultDataToDefinition(member);
            }

            void OnMemberDeleted(Member member)
            {
                GroupsToolbar.Selected.FixIssues();
            }

            void UnlinkMember()
            {
                GroupsToolbar.Selected.RemoveMember(MembersToolbar.Selected);
                MembersToolbar.RefreshDefinitions();
            }

            bool CanUnlink()
            {
                return MembersToolbar.Selected != null;
            }

            void OnMemberLinked(Member member)
            {
                OnMemberCreated(member);
                MembersToolbar.SelectDefinition(member);
                MembersToolbar.RefreshDefinitions();
            }

            Member[] GetMembers()
            {
                if (GroupsToolbar.Selected == null)
                {
                    MembersToolbar.IsInteractable = false;
                    return null;
                }

                MembersToolbar.IsInteractable = true;
                return (Member[])GroupsToolbar.Selected.Members;
            }
            #endregion
        }
        #endregion

        #region Drawing
        public void Draw(Rect rect, EditorDrawableLayoutType layoutType)
        {
            using (new GUILayout.HorizontalScope())
            {
                float groupsWidth = Mathf.Clamp(rect.width * 0.2f, 250f, rect.width);
                DrawGroupsList(groupsWidth);
                DrawMembersList(groupsWidth);
                DrawMemberInspector();
            }
        }
        
        private void DrawGroupsList(float width)
        {
            GroupsToolbar.DoLayoutWithHeader(string.Empty, DrawGroupInspector, GUILayout.Width(width));
        }

        private void DrawGroupInspector(GUILayoutOption[] width)
        {
            _groupInspector.DrawGUIWithToggle(ref _isInspectorVisible, GroupsToolbar.SelectedName, GroupsToolbar.Selected.Color, width);
        }

        private void DrawMembersList(float width)
        {
            MembersToolbar.DoLayoutWithHeader(GroupsToolbar.SelectedName, DrawMemberUnlinker, GUILayout.Width(width));
        }

        private void DrawMemberUnlinker(GUILayoutOption[] width)
        {
            if (GroupsToolbar.Selected != null && _unlinker.HasUnlinkedDefinitions)
                _unlinker.DoLayoutWithToggle($"Unlinked {MembersToolbar.ListName}", width);
            else
                _unlinker.IsExpanded = false;
        }

        private void DrawMemberInspector()
        {
            Color bgColor = MembersToolbar.Selected != null ? MembersToolbar.Selected.Color : Color.white;
            _memberInspector.DrawGUI(MembersToolbar.SelectedName, bgColor, GUILayout.ExpandWidth(true));
        }
        #endregion
    }

    public sealed class GroupMergeWindow : EditorWindow
    {
        private static EditorWindow s_LastFocusedWindow;
        private static GroupMergeWindow s_Window;
        
        private Action<int> _combineAction;
        private GUIContent[] _groups;
        private int _selectedGroup;
        private int _targetGroup;
        

        public static void OpenWindow(GUIContent[] groups, Action<int> mergeAction, int selectedGroup)
        {
            if (s_Window != null)
                s_Window.Close();

            s_LastFocusedWindow = focusedWindow;

            s_Window = GetWindow<GroupMergeWindow>(utility: true, title: "Merge Groups", focus: true);

            s_Window.minSize = new Vector2(256, 32 * groups.Length + 64);
            s_Window.maxSize = s_Window.minSize;

            s_Window._groups = groups;
            s_Window._combineAction = mergeAction;
            s_Window._selectedGroup = selectedGroup;
        }

        private void OnGUI()
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                CloseWindow();
                return;
            }

            _targetGroup = GUILayout.SelectionGrid(_targetGroup, _groups, 1, GUILayout.Height(32 * _groups.Length));

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(_targetGroup == _selectedGroup))
            {
                bool combine = Event.current.keyCode == KeyCode.Return ||
                               GUILayout.Button($"Merge ''{_groups[_targetGroup]}'' into ''{_groups[_selectedGroup]}''", GuiStyles.LargeButton);

                if (combine)
                {
                    _combineAction?.Invoke(_targetGroup);
                    CloseWindow();
                }
            }
        }

        private static void CloseWindow()
        {
            s_Window.Close();

            if (s_LastFocusedWindow != null)
                s_LastFocusedWindow.Focus();
        }
    }
}