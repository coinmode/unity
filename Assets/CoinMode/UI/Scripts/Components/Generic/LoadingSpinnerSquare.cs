using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM LoadingSpinnerSquare")]
    public class LoadingSpinnerSquare : CoinModeUIBehaviour
    {
        public Image topImage = null;
        public Image bottomImage = null;
        public Image leftImage = null;
        public Image rightImage = null;

        public float cellFadeTimer = 0.25F;
        public float spinnerDelay = 0.75F;

        private int fadedCellCount = 0;

        private float startAlpha = 0.0F;
        private float targetAlpha = 0.0F;
        private float cellFadeTime = 0.0F;
        private float spinnerDelayTime = 0.0F;

        protected override void Start()
        {
            base.Start();
            startAlpha = topImage.color.a;
            targetAlpha = 0.0F;
        }

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if(fadedCellCount < 8)
            {
                cellFadeTime += deltaTime;
                float normalTime = cellFadeTime / cellFadeTimer;
                float newAlpha = 0.0F;
                if(normalTime < 1.0F)
                {
                    newAlpha = Mathf.Lerp(startAlpha, targetAlpha, normalTime);
                }
                else
                {
                    newAlpha = targetAlpha;
                    cellFadeTime = 0.0F;
                }
                Color newColor = new Color();
                int img = fadedCellCount % 4;
                switch (img)
                {
                    default:
                    case 0:
                        newColor = topImage.color;
                        newColor.a = newAlpha;
                        topImage.color = newColor;
                        break;
                    case 1:
                        newColor = rightImage.color;
                        newColor.a = newAlpha;
                        rightImage.color = newColor;
                        break;
                    case 2:
                        newColor = bottomImage.color;
                        newColor.a = newAlpha;
                        bottomImage.color = newColor;
                        break;
                    case 3:
                        newColor = leftImage.color;
                        newColor.a = newAlpha;
                        leftImage.color = newColor;
                        break;
                }
                if (normalTime >= 1.0F)
                {
                    fadedCellCount++;
                    img = fadedCellCount % 4;
                    switch (img)
                    {
                        default:
                        case 0:
                            startAlpha = topImage.color.a;                            
                            break;
                        case 1:
                            startAlpha = rightImage.color.a;
                            break;
                        case 2:
                            startAlpha = bottomImage.color.a;
                            break;
                        case 3:
                            startAlpha = leftImage.color.a;
                            break;
                    }
                    if (startAlpha > 0.0F)
                    {
                        targetAlpha = 0.0F;
                    }
                    else
                    {
                        targetAlpha = 1.0F;
                    }
                }
            }
            else
            {
                spinnerDelayTime += deltaTime;
                if(spinnerDelayTime >= spinnerDelay)
                {
                    spinnerDelayTime = 0.0F;
                    fadedCellCount = 0;
                }
            }
        }

        public void StartFromEmpty()
        {
            Color newColor = topImage.color;
            newColor.a = 0.0F;
            topImage.color = newColor;

            newColor = rightImage.color;
            newColor.a = 0.0F;
            rightImage.color = newColor;

            newColor = bottomImage.color;
            newColor.a = 0.0F;
            bottomImage.color = newColor;

            newColor = leftImage.color;
            newColor.a = 0.0F;
            leftImage.color = newColor;

            startAlpha = 0.0F;
            targetAlpha = 1.0F;
            fadedCellCount = 0;
            cellFadeTime = 0.0F;
            spinnerDelayTime = 0.0F;
        }
    }
}