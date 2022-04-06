using UnityEngine;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM IntegerParameterInput")]
    public class IntegerParameterInput : NumberParameterInput
    {
        private IntegerParam parameter = null;

        public override bool SetParameter(InputParameter roundParam)
        {
            parameter = roundParam as IntegerParam;
            if (parameter != null)
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
            parameter.value = Mathf.RoundToInt(value);
        }

        protected override string GetValueLabelString()
        {
            return parameter.value.ToString();
        }
    }
}