using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    public abstract class ItemAction : ScriptableObject
    {
        [SerializeField, BeginGroup("Display")]
        private string _displayName;

        [SerializeField]
        private string _displayVerb;

        [SerializeField, SpritePreview, EndGroup]
        private Sprite _displayIcon;
        
        
        public string DisplayName => _displayName;
        public string DisplayVerb => _displayVerb;
        public Sprite DisplayIcon => _displayIcon;

        public abstract float GetDuration(ItemSlot itemSlot, ICharacter character);
        public abstract float GetDuration(IItem item, ICharacter character);

        public abstract bool IsPerformable(ItemSlot itemSlot, ICharacter character);
        public abstract bool IsPerformable(IItem item, ICharacter character);

        public Coroutine Perform(ItemSlot slot, ICharacter character, float duration = 1f)
        {
            if (!IsPerformable(slot, character))
                return null;

            duration *= GetDuration(slot, character);

            if (character is MonoBehaviour characterMono)
                return characterMono.StartCoroutine(C_PerformAction(character, slot, duration));

            return CoroutineUtils.StartGlobalCoroutine(C_PerformAction(character, slot, duration));
        }

        public Coroutine Perform(IItem item, ICharacter character, float duration = 1f)
        {
            if (!IsPerformable(item, character))
                return null;

            duration *= GetDuration(item, character);

            if (character is MonoBehaviour mono)
                return mono.StartCoroutine(C_PerformAction(character, item, duration));

            return CoroutineUtils.StartGlobalCoroutine(C_PerformAction(character, item, duration));
        }

        public void CancelAction(ref Coroutine coroutine, ICharacter character)
        {
            if (character is MonoBehaviour mono)
            {
                CoroutineUtils.StopCoroutine(mono, ref coroutine);
                return;
            }

            CoroutineUtils.StopGlobalCoroutine(ref coroutine);
        }

        protected abstract IEnumerator C_PerformAction(ICharacter character, ItemSlot itemSlot, float duration);
        protected abstract IEnumerator C_PerformAction(ICharacter character, IItem item, float duration);

#if UNITY_EDITOR
        private void Reset()
        {
            _displayName = name;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_displayName))
            {
                var path = AssetDatabase.GetAssetPath(this);
                var index = path.LastIndexOf("/", StringComparison.Ordinal) + 10;

                if (path.Length < index)
                    return;

                _displayName = path.Substring(index).Replace(".asset", "");
            }
        }
#endif
    }
}