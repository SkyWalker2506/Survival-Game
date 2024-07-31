using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class MapSlotUI : MonoBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private SerializedScene _scene;


        public SerializedScene Scene => _scene;
    }
}