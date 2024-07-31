using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PolymindGames
{
    public abstract class GroupMemberDefinition<TMember, TGroup> : DataDefinition<TMember>
        where TMember : GroupMemberDefinition<TMember, TGroup>
        where TGroup : GroupDefinition<TGroup, TMember>
    {
        [SerializeField, Disable, BeginGroup, EndGroup]
        private TGroup _parentGroup;

        private const string UNASSIGNED_GROUP = "No Group";
        
        
        public TGroup ParentGroup => _parentGroup;
        public bool HasParentGroup => _parentGroup != null;

        public override string FullName
        {
            get
            {
                string categoryName = ParentGroup != null ? ParentGroup.Name : UNASSIGNED_GROUP;
                return $"( {categoryName} ) / {Name}";
            }
        }

        #region Editor
#if UNITY_EDITOR
        /// <summary>
        /// Sets the category of this item (internal).
        /// </summary> 
        [Conditional("UNITY_EDITOR")]
        public void SetGroup(GroupDefinition<TGroup, TMember> group)
        {
            _parentGroup = group as TGroup;
            EditorUtility.SetDirty(this);
        }

        public override void FixIssues()
        {
            if (_parentGroup != null && !_parentGroup.Members.Contains(this))
                _parentGroup.AddMember(this as TMember);
        }
#endif
        #endregion
    }
}