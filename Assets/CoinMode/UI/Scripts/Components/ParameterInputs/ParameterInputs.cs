using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public abstract class ParameterInput : CoinModeUIBehaviour
    {
        [SerializeField] private CoinModeText nameLabel = null;

        public virtual bool SetParameter(InputParameter roundParam)
        {
            nameLabel.text = roundParam.paramName;
            return true;
        }
    }

    public abstract class NumberParameterInput : ParameterInput
    {
        [SerializeField] private Button incrementButton = null;
        [SerializeField] private Button decrementButton = null;

        [SerializeField] private Slider slider = null;

        [SerializeField] private CoinModeText valueLabel = null;

        [SerializeField] protected float incrementValue = 1.0F;

        protected override void Awake()
        {
            base.Awake();
            slider.onValueChanged.AddListener(SliderValueChanged);
            incrementButton.onClick.AddListener(IncrementValue);
            decrementButton.onClick.AddListener(DecrementValue);
        }

        public override bool SetParameter(InputParameter roundParam)
        {
            base.SetParameter(roundParam);
            slider.minValue = GetMinSliderValue();
            slider.maxValue = GetMaxSliderValue();
            slider.SetValueWithoutNotify(GetSliderDefaultValue());
            return true;
        }

        protected abstract float GetMinSliderValue();
        protected abstract float GetMaxSliderValue();
        protected abstract float GetSliderDefaultValue();

        private void IncrementValue()
        {
            float value = Mathf.Clamp(slider.value + incrementValue, GetMinSliderValue(), GetMaxSliderValue());
            slider.value = value;
        }

        private void DecrementValue()
        {
            float value = Mathf.Clamp(slider.value - incrementValue, GetMinSliderValue(), GetMaxSliderValue());
            slider.value = value;
        }

        private void SliderValueChanged(float value)
        {
            OnSliderValueChanged(value);
            valueLabel.text = GetValueLabelString();
        }

        protected abstract void OnSliderValueChanged(float value);

        protected abstract string GetValueLabelString();
    }
}