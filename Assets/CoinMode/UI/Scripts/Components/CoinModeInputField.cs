using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM InputField")]
    public class CoinModeInputField : InputField
    {
        [SerializeField]
        private Image frame = null;

        [SerializeField]
        private Image background = null;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            InitFromStyle();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            onEndEdit.AddListener(OnEndEdit);
            onValueChanged.AddListener(OnValueUpdated);
            InitFromStyle();
        }

        public void InitFromStyle()
        {
            if (frame != null) frame.sprite = CoinModeMenuStyle.inputFieldFrameSprite;
            if (background != null)
            {
                background.sprite = CoinModeMenuStyle.inputFieldBackgroundSprite;
                background.color = CoinModeMenuStyle.inputFieldBackgroundColor;
            }

            textComponent.color = CoinModeMenuStyle.inputFieldTextColor;
            placeholder.color = CoinModeMenuStyle.inputFieldPlaceholderColor;

            colors = CoinModeMenuStyle.inputFieldColors;
        }

        public virtual void SetInputText(string text)
        {
#if UNITY_2019_1_OR_NEWER
            SetTextWithoutNotify(text);
#else
            this.text = text;
#endif
        }

        public bool SetPlaceholderText(string text)
        {
            Text placeholderText = placeholder as Text;
            if(placeholderText != null)
            {
                placeholderText.text = text;
                return true;
            }
            return false;
        }

        public void SetInputTextColor(Color color)
        {
            textComponent.color = color;
        }

        public void SetPlaceholderColor(Color color)
        {
            placeholder.color = color;
        }

        protected virtual void OnEndEdit(string value)
        {

        }

        protected virtual void OnValueUpdated(string value)
        {

        }

        public void Decrement(float value)
        {
            switch (contentType)
            {
                case InputField.ContentType.DecimalNumber:
                    {
                        float currentValue = 0.0F;
                        float.TryParse(text, out currentValue);
                        currentValue -= value;
                        string newValue = currentValue.ToString();
                        text = newValue;
                    }
                    break;
                case InputField.ContentType.IntegerNumber:
                    {
                        int currentValue = 0;
                        int.TryParse(text, out currentValue);
                        currentValue -= Mathf.FloorToInt(value);
                        string newValue = currentValue.ToString();
                        text = newValue;
                    }
                    break;
                default:
                    CoinModeLogging.LogWarning("CoinModeInputField", "Decrement", "Cannot decrement input field whose type is: {0}", contentType.ToString());
                    break;
            }
            onEndEdit?.Invoke(text);
        }

        public void Increment(float value)
        {
            switch (contentType)
            {
                case InputField.ContentType.DecimalNumber:
                    {
                        float currentValue = 0.0F;
                        float.TryParse(text, out currentValue);
                        currentValue += value;
                        string newValue = currentValue.ToString();
                        text = newValue;
                    }
                    break;
                case InputField.ContentType.IntegerNumber:
                    {
                        int currentValue = 0;
                        int.TryParse(text, out currentValue);
                        currentValue += Mathf.FloorToInt(value);
                        string newValue = currentValue.ToString();
                        text = newValue;
                    }
                    break;
                default:
                    CoinModeLogging.LogWarning("CoinModeInputField", "Increment", "Cannot increment input field whose type is: {0}", contentType.ToString());
                    break;
            }
            onEndEdit?.Invoke(text);
        }
    }
}