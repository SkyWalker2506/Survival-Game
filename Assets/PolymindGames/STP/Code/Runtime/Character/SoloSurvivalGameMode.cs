using PolymindGames.UserInterface;
using PolymindGames.WorldManagement;
using UnityEngine;

namespace PolymindGames
{
    [DefaultExecutionOrder(ExecutionOrderConstants.SCENE_SINGLETON)]
    public sealed class SoloSurvivalGameMode : GameMode, ISaveableComponent
    {
        private bool _firstSpawn = true;


        protected override void OnPlayerInitialized(Player player, PlayerUI playerUI)
        {
            if (_firstSpawn)
                SetPlayerPosition(player);

            player.HealthManager.Respawn += OnPlayerRespawn;

            _firstSpawn = false;
        }

        private void SetPlayerPosition(Player player)
        {
            // Set the spawn position to the sleeping place.
            if (player.TryGetCC(out ISleepControllerCC sleep) && sleep.LastSleepPosition != Vector3.zero)
                SetPlayerPosition(sleep.LastSleepPosition, sleep.LastSleepRotation);
            else
            {
                var (position, rotation) = GetRandomSpawnPoint();
                SetPlayerPosition(position, rotation);
            }
        }

        private void OnPlayerRespawn() => SetPlayerPosition(Player);

        #region Save & Load
        void ISaveableComponent.LoadMembers(object members) => _firstSpawn = (bool)members;
        object ISaveableComponent.SaveMembers() => _firstSpawn;
        #endregion
    }
}