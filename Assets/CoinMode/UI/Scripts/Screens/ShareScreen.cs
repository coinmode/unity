using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM ShareScreen")]
    public class ShareScreen : CoinModeMenuScreen
    {
        public struct ScreenData
        {
            public string screenTitle { get; private set; }
            public string shareSheetTitle { get; private set; }
            public string shareContent { get; private set; }

            public ScreenData(string screenTitle, string shareSheetTitle, string shareContent)
            {
                this.screenTitle = screenTitle;
                this.shareSheetTitle = shareSheetTitle;
                this.shareContent = shareContent;
            }
        }

        [SerializeField]
        private CoinModeText titleText = null;

        [SerializeField]
        private CoinModeButton closeButton = null;

        [SerializeField]
        private CoinModeButton shareButton = null;

        [SerializeField]
        private Image qrImage = null;

        private Texture2D qrTexture = null;
        private Sprite qrSprite = null;

        public override bool requiresData { get; } = true;
        private ScreenData screenData = new ScreenData();

        protected override void OnDestroy()
        {
            UnityEngine.Object.Destroy(qrSprite);
            UnityEngine.Object.Destroy(qrImage);
        }

        protected override void Awake()
        {
            base.Awake();

            CoinModeQR.CreateQrResources(256, 256, out qrTexture, out qrSprite);

            if (closeButton != null && closeButton.onClick != null)
            {
                closeButton.onClick.AddListener(CloseAction);
            }

            if (shareButton != null && shareButton.onClick != null)
            {
                shareButton.onClick.AddListener(ShareAction);
            }
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);
            titleText.text = screenData.screenTitle;
            CoinModeQR.GenerateQr(qrTexture, screenData.shareContent);
            qrImage.sprite = qrSprite;

            CoinModeLogging.LogMessage("ShareScreen", "OnOpen", "Sharing content: {0}", screenData.shareContent);
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        private void CloseAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void ShareAction()
        {
            CoinModeManager.OpenPlatformShare(screenData.shareSheetTitle, screenData.shareContent);
        }
    }
}