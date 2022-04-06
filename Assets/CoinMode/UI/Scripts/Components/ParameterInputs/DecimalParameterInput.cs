using UnityEngine;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM DecimalParameterInput")]
    public class DecimalParameterInput : NumberParameterInput
    {
        private DecimalParam parameter = null;

        public override bool SetParameter(InputParameter roundParam)
        {
            parameter = roundParam as DecimalParam;
            if(parameter != null)
            {
                base.SetParameter(roundParam);
                return true;
            }            
            return false;
        }

        protected override float GetMinSliderValue()
        {
            return parameter.minValue;
        }

        protected override float GetMaxSliderValue()
        {
            return parameter.maxValue;
        }

        protected override float GetSliderDefaultValue()
        {
            return parameter.value;
        }

        protected override void OnSliderValueChanged(float value)
        {
            value = (float)System.Math.Round(value, 1);
            parameter.value = value;
        }

        protected override string GetValueLabelString()
        {
            return parameter.value.ToString("0.0");
        }
    }
}