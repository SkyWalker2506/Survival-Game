using System;
using PolymindGames;
using UnityEngine;

namespace PolymindGamesEditor
{
    public abstract class DataDefinitionPrefabCreationWizard<T, D, C> : PrefabCreationWizard<T, C>
        where T : PrefabCreationWizardData
        where D : DataDefinition<D>
        where C : Component
    {
        private readonly D _definition;


        protected DataDefinitionPrefabCreationWizard(D definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition), "Cannot create a Data Definition Prefab Creation Wizard from a null definition!");

            _definition = definition;
        }

        protected sealed override void HandleComponents(GameObject gameObject, T data) => HandleComponents(gameObject, data, _definition);
        protected abstract void HandleComponents(GameObject gameObject, T data, D def);
    }
}