using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM LightningDepositScreen")]
    public class LightningDepositScreen : CoinModeMenuScreen
    {
        [SerializeField]
        private CoinModeButton backButton = null;

        [SerializeField]
        private Image qrImage = null;

        [SerializeField]
        private CoinModeText linkText = null;

        public override bool requiresData { get; } = false;

        private Texture2D qrTexture = null;
        private Sprite qrSprite = null;

        private string address = "";

        protected override void OnDestroy()
        {
            UnityEngine.Object.Destroy(qrSprite);
            UnityEngine.Object.Destroy(qrImage);
        }

        protected override void Start()
        {
            base.Start();

            CoinModeQR.CreateQrResources(256, 256, out qrTexture, out qrSprite);

            if (backButton != null && backButton.onClick != null)
            {
                backButton.onClick.AddListener(BackAction);
            }
        }

        protected override void OnOpen(object data)
        {
            CoinModeQR.GenerateQr(qrTexture, address);
            qrImage.sprite = qrSprite;
            linkText.SetText(address);
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return false;
        }

        private void BackAction()
        {
            controller.ReturnToPreviousScreen();
        }
    }
}