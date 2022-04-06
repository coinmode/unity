using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM LoadingSpinnerCircle")]
    public class LoadingSpinnerCircle : CoinModeUIBehaviour
    {
        [SerializeField]
        private ColorMode colorMode = ColorMode.Light;
        [SerializeField]
        private Image spinnerImage = null;
        [SerializeField]
        private Image spinnerBackground = null;
        [SerializeField]
        private float spinnerSpeed = 100.0F;

        protected override void OnValidate()
        {
            base.OnValidate();
            SetColorMode(colorMode);
        }

        protected override void Awake()
        {
            base.Awake();
            SetColorMode(colorMode);
        }

        public void SetColorMode(ColorMode mode)
        {
            switch (mode)
            {
                case ColorMode.Light:
                    if(spinnerImage != null)
                    {
                        spinnerImage.color = new Color32(220, 220, 220, 200);
                    }
                    if(spinnerBackground != null)
                    {
                        spinnerBackground.color = new Color32(200, 200, 200, 100);
                    }
                    break;
                case ColorMode.Dark:
                    if (spinnerImage != null)
                    {
                        spinnerImage.color = new Color32(50, 50, 50, 200);
                    }
                    if (spinnerBackground != null)
                    {
                        spinnerBackground.color = new Color32(50, 50, 50, 100);
                    }
                    break;
            }
        }

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if(spinnerImage != null && spinnerImage.rectTransform != null)
            {
                spinnerImage.rectTransform.Rotate(0.0F, 0.0F, -spinnerSpeed * deltaTime);
            }
        }
    }
}