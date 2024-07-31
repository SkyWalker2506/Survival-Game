using UnityEngine;

namespace PolymindGames.InputSystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Input/Input Context", fileName = "InputContext_")]
    public sealed class InputContext : ScriptableObject
    {
        [field: SerializeField, ReorderableList(ListStyle.Boxed)]
        [field: ClassImplements(typeof(IInputBehaviour), AllowAbstract = false, TypeGrouping = TypeGrouping.ByAddComponentMenu)]
        public SerializedType[] BehaviourTypes { get; private set; }
    }
}
