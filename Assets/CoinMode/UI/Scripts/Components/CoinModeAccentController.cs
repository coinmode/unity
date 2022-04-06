using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public abstract class CoinModeAccentController : CoinModeUIBehaviour
    {
        public bool initialised { get; private set; } = false;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            RefreshAccent();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            RefreshAccent();
            initialised = true;
        }

        public void RefreshAccent()
        {
            SetAccent(CoinModeMenuStyle.accentColor);
        }


        public virtual void SetAccent(Color accentColor)
        {

        }
    }
}
