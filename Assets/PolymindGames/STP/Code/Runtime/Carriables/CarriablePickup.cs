using PolymindGames.SaveSystem;
using UnityEngine.Rendering;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(IHoverableInteractable), typeof(Rigidbody))]
    public sealed class CarriablePickup : MonoBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        [DataReferenceDetails(HasNullElement = false, HasAssetReference = true)]
        [Tooltip("The corresponding carriable definition.")]
        private DataIdReference<CarriableDefinition> _definition;

        private MeshRenderer _renderer;
        private Rigidbody _rigidbody;
        private Collider _collider;


        public CarriableDefinition Definition => _definition.Def;
        public Rigidbody Rigidbody => _rigidbody;
        
        public bool TryUse(ICharacter character)
        {
            var useActions = _definition.Def.UseActions;
            foreach (var action in useActions)
            {
                if (action.TryDoAction(character))
                {
                    Destroy(gameObject);
                    return true;
                }
            }

            return false;
        }
        
        public void OnPickUp(ICharacter character)
        {
            if (TryGetComponent(out MaterialEffect materialEffect))
                materialEffect.DisableEffect();
            
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            
            if (TryGetComponent(out SaveableObject saveableObject))
                saveableObject.IsSaveable = false;

            transform.localScale = Vector3.one;
        }

        public void OnDrop(ICharacter character)
        {
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
            _renderer.shadowCastingMode = ShadowCastingMode.On;

            if (TryGetComponent(out SaveableObject saveableObject))
                saveableObject.IsSaveable = true;
            
            transform.localScale = Vector3.one;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _renderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            var interactable = GetComponent<IHoverableInteractable>();
            interactable.Interacted += PickUpCarriable;
            
            if (string.IsNullOrEmpty(interactable.Title))
                interactable.Title = "Carry";
            
            if (string.IsNullOrEmpty(interactable.Description))
                interactable.Description = _definition.Name;
        }

        private void PickUpCarriable(IInteractable interactable, ICharacter character)
        {
            if (character.TryGetCC(out ICarriableControllerCC objectCarry))
                objectCarry.TryCarryObject(this);
        }

        private void Reset()
        {
            if (!gameObject.HasComponent<IInteractable>())
                gameObject.AddComponent<SimpleInteractable>();
        }
    }
}