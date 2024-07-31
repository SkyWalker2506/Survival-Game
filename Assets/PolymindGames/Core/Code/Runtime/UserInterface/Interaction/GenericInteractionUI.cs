using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_interaction")]
    public sealed class GenericInteractionUI : MonoBehaviour, IInteractableInfoDisplayer
    {
        [SerializeField, BeginGroup("References")]
        [Tooltip("The UI panel used in showing / hiding the underlying images.")]
        private PanelUI _panel;

        [SerializeField]
        [Tooltip("A UI text component that's used for displaying the current interactable's name.")]
        private TextMeshProUGUI _nameText;

        [SerializeField]
        [Tooltip("A UI text component that's used for displaying the current interactable's description.")]
        private TextMeshProUGUI _descriptionText;

        [SerializeField]
        [Tooltip("An image that used in showing the time the current interactable has been interacted with.")]
        private Image _interactProgress;

        [SerializeField]
        [Tooltip("An image that separate the name text from the description text (optional). " +
                 "It gets disabled when the current interactable doesn't have a description.")]
        private Image _separator;

        [SerializeField, NotNull, EndGroup]
        private GameObject _inputRoot;

        private IHoverable _hoverable;


        public bool TrySetHoverable(IHoverable hoverable)
        {
            if (_hoverable != null)
                _hoverable.DescriptionChanged -= OnDescriptionChanged;

            _hoverable = hoverable;
            if (hoverable != null && !string.IsNullOrEmpty(hoverable.Title))
            {
                hoverable.DescriptionChanged += OnDescriptionChanged;
                OnDescriptionChanged();

                _panel.Show();
                
                _inputRoot.gameObject.SetActive(hoverable is IInteractable { InteractionEnabled: true });
            }
            else
                _panel.Hide();

            return true;
        }

        public void SetInteractionProgress(float progress) => _interactProgress.fillAmount = progress;

        private void OnDescriptionChanged()
        {
            _nameText.text = _hoverable.Title;
            _descriptionText.text = _hoverable.Description;

            if (_separator != null)
                _separator.enabled = !string.IsNullOrEmpty(_descriptionText.text);
        }
    }
}