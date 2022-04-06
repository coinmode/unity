using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM InputFieldControl")]
    public class CoinModeInputFieldControl : CoinModeUIBehaviour
    {
        [SerializeField]
        private CoinModeInputField targetInputField = null;

        [SerializeField]
        private Button incrementButton = null;

        [SerializeField]
        private Button decrementButton = null;

        [SerializeField]
        private float increment = 1.0F;

        protected override void Awake()
        {
            base.Awake();
            if (incrementButton != null) incrementButton.onClick.AddListener(Increment);
            if (decrementButton != null) decrementButton.onClick.AddListener(Decrement);
        }

        public void Decrement()
        {
            targetInputField.Decrement(increment);
        }

        public void Increment()
        {
            targetInputField.Increment(increment);
        }
    }
}