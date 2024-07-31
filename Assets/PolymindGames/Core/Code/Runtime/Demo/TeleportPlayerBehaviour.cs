using UnityEngine;

namespace PolymindGames.Demo
{
    public sealed class TeleportPlayerBehaviour : MonoBehaviour
    {
        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private Transform[] _teleportPoints;


        public void TeleportPlayer(ICharacter character)
        {
            if (character.TryGetCC(out IMotorCC motor))
            {
                Transform teleportPoint = _teleportPoints.SelectRandom();
                
                if (teleportPoint != null)
                    motor.Teleport(teleportPoint.position, teleportPoint.rotation, true);
            }
        }
    }
}