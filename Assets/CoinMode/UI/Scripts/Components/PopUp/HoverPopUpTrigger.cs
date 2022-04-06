using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public delegate void HoverPopUpEvent(CoinModeWindow popUpWindow);

    [AddComponentMenu("CoinMode/UI/CM HoverPopUpTrigger")]
    public class HoverPopUpTrigger : CoinModeUIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private CoinModeWindow windowTemplate = null;

        [SerializeField]
        float hoverTime = 2.0F;

        private object popUpData = null;

        private CoinModeWindow activeWindow = null;

        private float currentHoverTime = 0.0F;

        private bool hovered = false;
        private bool held = false;

        private Canvas rootCanvas
        {
            get 
            {
                if(_rootCanvas == null)
                {
                    _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
                }
                return _rootCanvas;
            } 
        }
        private Canvas _rootCanvas = null;

        public HoverPopUpEvent onPopUpOpened = null;

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if((hovered || held) && windowTemplate != null)
            {
                if(currentHoverTime < hoverTime)
                {
                    currentHoverTime += deltaTime;
                    if(currentHoverTime >= hoverTime)
                    {
                        OpenPopUp();
                    }
                }
            }
        }        
        
        public void SetPopUpData(object data)
        {
            popUpData = data;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            if(!held)
            {
                ClosePopUp();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            held = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            held = false;
            if(!hovered)
            {
                ClosePopUp();
            }
        }

        private void OpenPopUp()
        {
            if(activeWindow == null && windowTemplate != null)
            {
                activeWindow = Instantiate(windowTemplate, transform);
            }

            if (activeWindow != null && !activeWindow.isOpen &&
                    ( (activeWindow.requiresData && activeWindow.IsValidData(popUpData) ) || !activeWindow.requiresData) )
            {
                Vector2 anchor = Vector2.zero;
                Vector2 pivot = Vector2.zero;
                if (rootCanvas.transform.position.x > transform.position.x)
                {
                    anchor.x = 1.0F;
                    anchor.y = 0.5F;
                    pivot.x = 0.0F;
                    pivot.y = 0.5F;
                }
                else
                {
                    anchor.x = 0.0F;
                    anchor.y = 0.5F;
                    pivot.x = 1.0F;
                    pivot.y = 0.5F;
                }

                RectTransform windowRectTransform = activeWindow.transform as RectTransform;
                if (windowRectTransform != null)
                {
                    windowRectTransform.anchorMin = anchor;
                    windowRectTransform.anchorMax = anchor;
                    windowRectTransform.pivot = pivot;
                    windowRectTransform.anchoredPosition = Vector2.zero;
                }
                activeWindow.Open(popUpData);
                onPopUpOpened?.Invoke(activeWindow);
            }
        }

        private void ClosePopUp()
        {
            if(activeWindow != null)
            {
                activeWindow.Close();
            }
            currentHoverTime = 0.0F;
        }
    }
}