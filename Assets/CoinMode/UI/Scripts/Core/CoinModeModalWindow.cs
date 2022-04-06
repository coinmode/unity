using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM ModalWindow")]
    public class CoinModeModalWindow : CoinModeWindow
    {
        public struct WindowData
        {
            public string message { get; private set; } 
            public List<ModalWindowAction> actions { get; private set; } 
            public Color? backgroundColor { get; private set; }

            public WindowData(string message, List<ModalWindowAction> actions, Color? backgroundColor)
            {
                this.message = message;
                this.actions = actions;
                this.backgroundColor = backgroundColor;
            }

            public WindowData(string message, List<ModalWindowAction> actions)
            {
                this.message = message;
                this.actions = actions;
                this.backgroundColor = null;
            }
        }

        public struct ModalWindowAction
        {
            public string buttonLabel { get; private set; }
            public UnityAction buttonEvent { get; private set; }

            public ModalWindowAction(string buttonLabel, UnityAction buttonEvent)
            {
                this.buttonLabel = buttonLabel;
                this.buttonEvent = buttonEvent;
            }
        }

        [SerializeField]
        private CoinModeText messageText = null;

        [SerializeField]
        private RectTransform actionButtonContainer = null;

        [SerializeField]
        private CoinModeButton actionButtonTemplate = null;

        [SerializeField]
        private Button closeButton = null;

        [SerializeField]
        private Image windowBackground = null;

        public override bool requiresData { get; } = true;
        private WindowData windowData = new WindowData();

        private CanvasGroup fadeCanvas = null;
        private List<CoinModeButton> modalActionButtons = new List<CoinModeButton>();

        private void Inititialize()
        {
            if(windowBackground != null)
            {
                if(CoinModeMenuStyle.messageBackgroundSprite != null)
                {
                    windowBackground.sprite = CoinModeMenuStyle.messageBackgroundSprite;
                }
                windowBackground.color = CoinModeMenuStyle.messageBackgroundColor;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Inititialize();
        }
#endif

        protected override void Start()
        {
            base.Start();

            GameObject fadeGo = new GameObject("ModalBackgroundFade");
            fadeGo.transform.parent = transform.parent;
            fadeGo.transform.SetSiblingIndex(transform.GetSiblingIndex());
            fadeGo.transform.localScale = Vector3.one;
            Image image = fadeGo.AddComponent<Image>();
            image.color = new Color(0.0F, 0.0F, 0.0F, 0.5F);
            fadeCanvas = fadeGo.AddComponent<CanvasGroup>();
            fadeCanvas.alpha = canvasGroup.alpha;
            fadeCanvas.blocksRaycasts = false;
            fadeCanvas.interactable = false;
            RectTransform fadeTransform = fadeGo.transform as RectTransform;
            if (fadeTransform != null)
            {
                fadeTransform.anchorMin = new Vector2(0.0F, 0.0F);
                fadeTransform.anchorMax = new Vector2(1.0F, 1.0F);
                fadeTransform.offsetMin = new Vector2(0.0F, 0.0F);
                fadeTransform.offsetMax = new Vector2(0.0F, 0.0F);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseModalWindow);
            }

            Inititialize();
        }

        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                UpdateUI(Time.unscaledDeltaTime);
            }
        }

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if (fadeCanvas != null && fadeCanvas.alpha != canvasGroup.alpha)
            {
                fadeCanvas.alpha = canvasGroup.alpha;
                fadeCanvas.blocksRaycasts = canvasGroup.blocksRaycasts;
            }
        }

        protected override void OnOpen(object data)
        {
            windowData = ValidateObject<WindowData>(data);
            SetUp();
        }

        protected override bool OnUpdateData(object data) 
        {
            windowData = ValidateObject<WindowData>(data);
            SetUp();
            return true; 
        }
        public override bool IsValidData(object data)
        {
            return IsValidObject<WindowData>(data);
        }

        private void SetUp()
        {
            for (int i = 0; i < modalActionButtons.Count; i++)
            {
                Destroy(modalActionButtons[i].gameObject);
            }
            modalActionButtons.Clear();

            if (messageText != null)
            {
                messageText.SetText(windowData.message);
            }

            if (actionButtonContainer != null)
            {
                actionButtonContainer.gameObject.SetActive(windowData.actions != null && windowData.actions.Count > 0);
            }

            if (actionButtonTemplate != null)
            {
                if (windowData.actions != null)
                {
                    for (int i = 0; i < windowData.actions.Count; i++)
                    {
                        CoinModeButton button = Instantiate(actionButtonTemplate, actionButtonContainer);
                        modalActionButtons.Add(button);
                        button.gameObject.SetActive(true);
                        button.SetMainButtonText(windowData.actions[i].buttonLabel);
                        button.onClick.AddListener(CloseModalWindow);
                        if (windowData.actions[i].buttonEvent != null)
                        {
                            button.onClick.AddListener(windowData.actions[i].buttonEvent);
                        }
                    }
                }
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(windowData.actions == null || windowData.actions.Count == 0);
            }

            if (windowBackground != null)
            {
                Color bgColor = windowData.backgroundColor != null ? windowData.backgroundColor.Value : CoinModeMenuStyle.messageBackgroundColor;
                windowBackground.color = bgColor;

                if (messageText != null)
                {
                    messageText.SetTextColor(CoinModeMenuStyle.GetContastingColor(bgColor));
                }
            }
        }

        private void CloseModalWindow()
        {
            Close();
        }
    }
}
