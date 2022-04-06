using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CoinMode.UI
{
    public abstract class CoinModeListSelectable : CoinModeUIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private float highlightMultiplier = 1.5F;

        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color hoveredColor = Color.white;

        [SerializeField]
        private Image backgroundImage = null;

        public bool selected { get; private set; } = false;

        private void Initialize()
        {
            Color c = normalColor * CoinModeMenuStyle.accentColor;
            if(selected) c.a *= highlightMultiplier;
            SetBackgroundColor(c);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Initialize();
        }
#endif 

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Color c = hoveredColor * CoinModeMenuStyle.accentColor;
            if (selected) c.a *= highlightMultiplier;
            SetBackgroundColor(c);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Color c = normalColor * CoinModeMenuStyle.accentColor;
            if (selected) c.a *= highlightMultiplier;
            SetBackgroundColor(c);
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
            if (this.selected)
            {
                Color c = normalColor * CoinModeMenuStyle.accentColor;
                c.a *= highlightMultiplier;
                SetBackgroundColor(c);
            }
            OnSelectedSet(selected);
        }

        private void SetBackgroundColor(Color color)
        {
            if(backgroundImage != null)
            {
                backgroundImage.color = color;
            }            
            OnBackgroundColorSet(color);
        }

        protected virtual void OnBackgroundColorSet(Color color) { }

        protected virtual void OnSelectedSet(bool selected) { }
    }
}