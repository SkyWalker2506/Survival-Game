using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public abstract class CarriableAction : ScriptableObject
    {
        public abstract bool CanDoAction(ICharacter character);
        public abstract bool TryDoAction(ICharacter character);
    }
}