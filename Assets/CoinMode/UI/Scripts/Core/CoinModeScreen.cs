using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public abstract class CoinModeScreen : CoinModeWindow
    {
        protected CoinModeScreenController controller { get { return _controller; } }
        [NonSerialized]
        private CoinModeScreenController _controller = null;

        public virtual bool supportsHistory { get; } = true;

        private List<Selectable> screenSelectables = new List<Selectable>();

        internal virtual void InitScreen(CoinModeScreenController controller)
        {
            if (controller != null)
            {
                _controller = controller;
            }
        }

        protected override void Start()
        {
            base.Start();
        }     

        protected override void OnOpened()
        {
            UnityEngine.EventSystems.BaseEventData data = new UnityEngine.EventSystems.BaseEventData(UnityEngine.EventSystems.EventSystem.current);
            screenSelectables.Clear();
            GetAllChildSelectables(ref screenSelectables);
            foreach(Selectable s in screenSelectables)
            {
                if(s != null) s.OnDeselect(data);
            }
        }

        public static CoinModeScreen FindParent(Transform child)
        {
            return child.GetComponentInParent<CoinModeScreen>();
        }
    }
}
