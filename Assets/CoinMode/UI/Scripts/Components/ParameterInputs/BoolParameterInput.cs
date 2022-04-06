using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM BoolParameterInput")]
    public class BoolParameterInput : ParameterInput
    {
        [SerializeField] private CoinModeToggle toggle = null;
        [SerializeField] private CoinModeText valueLabel = null;

        private BoolParam parameter = null;

        protected override void Awake()
        {
            base.Awake();
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }        

        public override bool SetParameter(InputParameter roundParam)
        {
            parameter = roundParam as BoolParam;
            if(parameter != null)
            {
                toggle.SetValue(parameter.value);
                base.SetParameter(roundParam);
            }
            return false;
        }

        private void OnToggleValueChanged(bool value)
        {
            parameter.value = value;
            valueLabel.text = GetValueLabelString();
        }

        private string GetValueLabelString()
        {
            return parameter.value ? "Enabled" : "Disabled";
        }
    }
}