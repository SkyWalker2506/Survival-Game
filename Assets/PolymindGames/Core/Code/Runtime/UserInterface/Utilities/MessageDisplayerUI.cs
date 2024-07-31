using PolymindGames.ProceduralMotion;
using PolymindGames.WorldManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    public sealed class MessageDisplayerUI : MonoBehaviour, IMessageListener
    {
        [SerializeField, BeginGroup("Template")]
        private GameObject _messageTemplate;

        [SerializeField, Range(1, 30), EndGroup]
        private int _templatesCount = 8;

        [SerializeField, Range(0f, 10f), BeginGroup("Fading")]
        private float _fadeDelay = 2f;

        [SerializeField, Range(0f, 10f), EndGroup]
        private float _fadeDuration = 1f;

        [SerializeField, BeginGroup("Colors")]
        private Color _infoColor;

        [SerializeField]
        private Color _warningColor;

        [SerializeField, EndGroup]
        private Color _errorColor;

        private MessageTemplateData[] _messageTemplates;
        private int _currentIndex = -1;


        void IMessageListener.OnMessageReceived(ICharacter character, in MessageArgs args)
        {
            Color color = GetColorForMessageType(args.Type);
            PushMessage(args.Message, color, args.Sprite);
        }

        private void Awake()
        {
            _messageTemplates = new MessageTemplateData[_templatesCount];
            for (int i = 0; i < _templatesCount; i++)
                _messageTemplates[i] = new MessageTemplateData(_messageTemplate, transform);
        }

        private void OnEnable() => World.Instance.Message.AddListener(this);
        private void OnDisable() => World.Instance.Message.RemoveListener(this);

        private void PushMessage(string message, Color color, Sprite sprite)
        {
            var template = GetMessageTemplate();

            template.Root.SetActive(true);
            template.Root.transform.SetAsLastSibling();

            template.Text.text = message.ToUpper();
            template.Text.color = new Color(color.r, color.g, color.b, 1f);

            template.IconImg.gameObject.SetActive(sprite != null);
            template.IconImg.sprite = sprite;

            template.CanvasGroup.alpha = color.a;
            template.CanvasGroup.TweenCanvasGroupAlpha(0f, _fadeDuration)
                .PlayAndRelease(this, _fadeDelay);
        }

        private MessageTemplateData GetMessageTemplate() =>
            _messageTemplates[(int)Mathf.Repeat(++_currentIndex, _templatesCount)];
        
        private Color GetColorForMessageType(MessageType type) => type switch
        {
            MessageType.Info => _infoColor,
            MessageType.Warning => _warningColor,
            MessageType.Error => _errorColor,
            _ => Color.black
        };

		#region Internal
        private sealed class MessageTemplateData
        {
            public readonly CanvasGroup CanvasGroup;
            public readonly Image IconImg;
            public readonly GameObject Root;
            public readonly TextMeshProUGUI Text;

            public MessageTemplateData(GameObject objectTemplate, Transform spawnRoot)
            {
                GameObject instance = Instantiate(objectTemplate, spawnRoot);
                Root = instance;
                Text = instance.GetComponentInChildren<TextMeshProUGUI>();
                IconImg = instance.transform.Find("Icon").GetComponent<Image>();
                CanvasGroup = instance.GetComponentInChildren<CanvasGroup>();
                CanvasGroup.alpha = 0f;
            }
        }
        #endregion
    }
}