using PolymindGames.InventorySystem;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    public interface ICharacter : IDamageSource
    {
        string Name { get; }
        IAudioPlayer AudioPlayer { get; }
        IHealthManager HealthManager { get; }
        IInventory Inventory { get; }

        event UnityAction<ICharacter> Destroyed;

        Transform GetTransformOfBodyPoint(BodyPoint point);
        bool TryGetCC<T>(out T component) where T : class, ICharacterComponent;
        T GetCC<T>() where T : class, ICharacterComponent;
    }
}