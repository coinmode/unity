using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM Toggle")]
    public class CoinModeToggle : Toggle
    {
        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnValueChanged);
        }

        public void SetValue(bool value)
        {
            isOn = value;
        }

        protected virtual void OnValueChanged(bool value)
        {

        }
    }
}