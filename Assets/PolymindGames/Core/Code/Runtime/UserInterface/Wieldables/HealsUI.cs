using TMPro;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class HealsUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private TextMeshProUGUI _healsCountText;


        protected override void OnCharacterAttached(ICharacter character)
        {
            var healing = Character.GetCC<IWieldableHealingHandlerCC>();
            healing.HealsCountChanged += OnHealsCountChanged;
            OnHealsCountChanged(healing.HealsCount);
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            var healing = Character.GetCC<IWieldableHealingHandlerCC>();
            healing.HealsCountChanged -= OnHealsCountChanged;
        }

        private void OnHealsCountChanged(int count) => _healsCountText.text = count.ToString();
    }
}