using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM StringParameterInput")]
    public class StringParameterInput : ParameterInput
    {
        [SerializeField] private CoinModeInputField inputField = null;

        private StringParam parameter = null;

        protected override void Awake()
        {
            base.Awake();
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        }

        public override bool SetParameter(InputParameter roundParam)
        {
            parameter = roundParam as StringParam;
            if(parameter != null)
            {
                inputField.SetPlaceholderText(parameter.defaultValue);
                inputField.SetInputText(parameter.value);
                base.SetParameter(roundParam);
            }
            return false;
        }

        private void OnInputFieldValueChanged(string value)
        {
            parameter.value = value;
        }
    }
}