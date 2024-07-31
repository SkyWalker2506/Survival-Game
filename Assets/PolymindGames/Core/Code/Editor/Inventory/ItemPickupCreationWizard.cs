using PolymindGames.InventorySystem;
using PolymindGames.SurfaceSystem;
using PolymindGames.SaveSystem;
using PolymindGames;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.InventorySystem
{
    public sealed class ItemPickupCreationWizard : DataDefinitionPrefabCreationWizard<ItemPickupDataContainer, ItemDefinition, ItemPickup>
    {
        public ItemPickupCreationWizard(ItemDefinition definition) : base(definition)
        { }

        protected override void DrawHeader()
        {
            GUILayout.Label("Create Item Pickup");
        }

        protected override void HandleComponents(GameObject gameObject, ItemPickupDataContainer data, ItemDefinition def)
        {
            gameObject.name = $"Pickup_{def.Name.Replace(" ", "")}";
            gameObject.SetLayersInChildren(LayerConstants.INTERACTABLE);

            // Add collider & rigidbody..
            if (data.ColliderType.Type != null)
            {
                var collider = gameObject.AddComponent(data.ColliderType.Type) as Collider;
                if (collider != null)
                {
                    collider.sharedMaterial = data.Surface.Def.Materials.FirstOrDefault();
                    gameObject.GetOrAddComponent<SurfaceImpactHandler>();

                    if (collider is MeshCollider meshCollider)
                        meshCollider.convex = true;

                    if (data.AddRigidbody)
                        gameObject.GetOrAddComponent<Rigidbody>().mass = def.Weight;
                }
            }

            if (data.MaterialEffect != null)
                gameObject.GetOrAddComponent<MaterialEffect>().SetFieldValue("_defaultEffect", data.MaterialEffect);

            if (data.IsSaveable)
                gameObject.GetOrAddComponent<SaveableObject>();

            // Add the item pickup component..
            var pickup = gameObject.GetAddOrSwapComponent<ItemPickup>(data.PickupType.Type);
            pickup.SetFieldValue("_item", new DataIdReference<ItemDefinition>(def.Id));
            pickup.SetFieldValue("_minCount", data.Count);
            pickup.SetFieldValue("_maxCount", data.Count);
            pickup.SetFieldValue("_addAudio", data.Sound);
            EditorUtility.SetDirty(pickup);
            EditorUtility.SetDirty(gameObject);
        }
    }

    public sealed class ItemPickupDataContainer : PrefabCreationWizardData
    {
        [TypeConstraint(typeof(ItemPickup))]
        public SerializedType PickupType = new(typeof(ItemPickup));

        [Range(1, 24)]
        public int Count = 1;

        public AudioDataSO Sound;

        [SpaceArea]
        [TypeConstraint(typeof(Collider))]
        public SerializedType ColliderType = new(typeof(BoxCollider));

        public bool AddRigidbody = true;

        [SpaceArea]
        public bool IsSaveable = true;

        public MaterialEffectSO MaterialEffect;
        public DataIdReference<SurfaceDefinition> Surface;

        private static ItemPickup s_DefaultPickup;


        private void Awake()
        {
            if (TryGetDefaultPickup(out var pickup))
            {
                Sound = pickup.GetPrivateFieldValue<AudioDataSO>("_addAudio");
                MaterialEffect = pickup.TryGetComponent<MaterialEffect>(out var materialEffect)
                    ? materialEffect.GetPrivateFieldValue<MaterialEffectSO>("_defaultEffect")
                    : null;

                Surface = SurfaceManager.LoadEditorInstance()
                    .GetSurfaceFromCollider(pickup.GetComponent<Collider>());
            }
        }

        private static bool TryGetDefaultPickup(out ItemPickup pickup)
        {
            if (s_DefaultPickup != null)
            {
                pickup = s_DefaultPickup;
                return true;
            }

            foreach (var item in ItemDefinition.Definitions)
            {
                if (item.Pickup == null)
                    continue;

                pickup = item.GetPrivateFieldValue<ItemPickup>("_pickup");
                if (pickup != null)
                {
                    s_DefaultPickup = pickup;
                    return true;
                }
            }

            pickup = null;
            return false;
        }
    }
}