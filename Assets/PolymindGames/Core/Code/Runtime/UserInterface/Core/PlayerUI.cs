using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_2)]
    public class PlayerUI : CharacterUI
    {
        public static PlayerUI LocalUI { get; private set; }


        private void Awake()
        {
            if (LocalUI != null)
                Destroy(this);
            else
                LocalUI = this;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (LocalUI == null)
                LocalUI = null;
        }
    }
}