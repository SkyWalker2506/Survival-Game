using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolymindGames
{
    public class GroupDefinition<TGroup, TMember> : DataDefinition<TGroup>
        where TGroup : GroupDefinition<TGroup, TMember>
        where TMember : GroupMemberDefinition<TMember, TGroup>
    {
        [SerializeField, NewLabel("Name"), BeginGroup]
        private string _groupName;

        [SerializeField, SpritePreview, EndGroup]
        private Sprite _icon;

        [SerializeField, Disable, SpaceArea, BeginGroup, EndGroup]
        [ReorderableList(ListStyle.Lined, fixedSize: true, HasLabels = false)]
        private TMember[] _members = Array.Empty<TMember>();

        
        public IReadOnlyList<TMember> Members
        {
            get => _members;
            protected set => _members = value as TMember[] ?? Array.Empty<TMember>();
        }

        public bool HasMembers => _members.Length > 0;

        public override string Name
        {
            get => _groupName;
            protected set => _groupName = value;
        }

        public override Sprite Icon => _icon;
        
		#region Editor
#if UNITY_EDITOR
        /// <summary>
        /// Warning: This is an editor method, don't call it at runtime.
        /// </summary>
        public void MergeWith(TGroup group)
        {
            if (group == null)
                return;

            ArrayUtility.AddRange(ref _members, group._members);
            EditorUtility.SetDirty(this);

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(group));

            ReloadDefinitions();
            FixMembers();
        }

        /// <summary>
        /// Warning: This is an editor method, don't call it at runtime.
        /// </summary>
        public virtual void AddDefaultDataToDefinition(TMember def) { }

        public override void FixIssues()
        {
            if (this == null)
            {
                Debug.LogError("Destroy");
                ReloadDefinitions();
                return;
            }

            EditorUtility.SetDirty(this);
            FixMembers();
        }

        /// <summary>
        /// Warning: This is an editor method, don't call it at runtime.
        /// </summary>
        public void AddMember(TMember def)
        {
            if (def == null)
                return;

            if (!ArrayUtility.Contains(_members, def))
            {
                def.SetGroup(this);
                ArrayUtility.Add(ref _members, def);

                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(def);
            }
        }

        /// <summary>
        /// Warning: This is an editor method, don't call it at runtime.
        /// </summary>
        public void RemoveMember(TMember def)
        {
            if (def == null)
                return;

            if (ArrayUtility.Contains(_members, def))
            {
                ArrayUtility.Remove(ref _members, def);
                def.SetGroup(null);

                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(def);
            }
        }

        /// <summary>
        /// Warning: This is an editor method, don't call it at runtime.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            _members = new TMember[] { };
            FixMembers();
        }

        private void FixMembers()
        {
            CollectionExtensions.RemoveAllNull(ref _members);
            foreach (var member in _members)
                member.SetGroup(this);
        }
#endif
		#endregion
    }
}