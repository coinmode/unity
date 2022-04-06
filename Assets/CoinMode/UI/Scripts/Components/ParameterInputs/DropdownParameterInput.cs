using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM DropdownParameterInput")]
    public class DropdownParameterInput : ParameterInput
    {
        [SerializeField] private Dropdown dropdown = null;

        private DropdownParam parameter = null;

        protected override void Awake()
        {
            base.Awake();
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        public override bool SetParameter(InputParameter roundParam)
        {
            parameter = roundParam as DropdownParam;
            if(parameter != null)
            {
                dropdown.options = parameter.options;
                dropdown.SetValueWithoutNotify(parameter.value);
                base.SetParameter(roundParam);
            }
            return false;
        }

        private void OnDropdownValueChanged(int value)
        {
            parameter.value = value;
        }
    }
}