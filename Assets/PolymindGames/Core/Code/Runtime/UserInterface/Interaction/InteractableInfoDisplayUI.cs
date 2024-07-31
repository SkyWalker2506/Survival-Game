using System.Linq;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public interface IInteractableInfoDisplayer
    {
        bool TrySetHoverable(IHoverable hoverable);
        void SetInteractionProgress(float interactProgress);
    }

    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_interaction#interactable-info-display-ui")]
    public sealed class InteractableInfoDisplayUI : CharacterUIBehaviour
    {
        private IInteractableInfoDisplayer[] _objectDisplayers;
        private IInteractableInfoDisplayer _activeDisplayer;
        private IInteractableInfoDisplayer _defaultDisplayer;


        protected override void OnCharacterAttached(ICharacter character)
        {
            InitializeDisplayers();

            var interaction = character.GetCC<IInteractionHandlerCC>();
            interaction.HoverableInViewChanged += OnHoverableInViewChanged;
            interaction.InteractProgressChanged += SetInteractionProgress;
            interaction.InteractionEnabledChanged += OnEnabledStateChanged;

            if (interaction.Hoverable != null && interaction.InteractionEnabled)
                OnHoverableInViewChanged(interaction.Hoverable);
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            var interaction = character.GetCC<IInteractionHandlerCC>();
            interaction.HoverableInViewChanged -= OnHoverableInViewChanged;
            interaction.InteractProgressChanged -= SetInteractionProgress;
            interaction.InteractionEnabledChanged -= OnEnabledStateChanged;
        }

        private void InitializeDisplayers()
        {
            var objectDisplayers = gameObject.GetComponentsInFirstChildren<IInteractableInfoDisplayer>();
            
            foreach (var displayer in objectDisplayers)
                displayer.SetInteractionProgress(0f);

            _defaultDisplayer = objectDisplayers.FirstOrDefault();
            _activeDisplayer = _defaultDisplayer;

            objectDisplayers.Remove(_defaultDisplayer);
            _objectDisplayers = objectDisplayers.ToArray();
        }

        private void OnEnabledStateChanged(bool enable)
        {
            if (enable)
            {
                var hoverable = Character.GetCC<IInteractionHandlerCC>().Hoverable;
                OnHoverableInViewChanged(hoverable);
            }
            else
                OnHoverableInViewChanged(null);
        }

        private void SetInteractionProgress(float progress) => _activeDisplayer?.SetInteractionProgress(progress);

        private void OnHoverableInViewChanged(IHoverable hoverable)
        {
            _activeDisplayer?.TrySetHoverable(null);
            if (hoverable != null)
            {
                var newDisplayer = GetDisplayerForHoverable(hoverable);
                newDisplayer.SetInteractionProgress(0f);
                _activeDisplayer = newDisplayer;
            }
            else
                _activeDisplayer = null;
        }

        private IInteractableInfoDisplayer GetDisplayerForHoverable(IHoverable hoverable)
        {
            foreach (var displayer in _objectDisplayers)
            {
                if (displayer.TrySetHoverable(hoverable))
                    return displayer;
            }

            _defaultDisplayer.TrySetHoverable(hoverable);
            return _defaultDisplayer;
        }
    }
}