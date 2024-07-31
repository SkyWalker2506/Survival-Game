using UnityEngine;

namespace PolymindGames.SaveSystem
{
    [DefaultExecutionOrder(ExecutionOrderConstants.AFTER_DEFAULT_1)]
    public sealed class SaveableObjectWithPriority : SaveableObject
    {
        private void Start() => RegisterSaveable();
    }
}