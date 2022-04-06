using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM Button")]
    public class CoinModeButton : Button
    {
        public enum ButtonState
        {
            Interatable,
            Disabled,
            Waiting,
        }

        public int displayTemplate
        {
            get { return _displayTemplate; }
        }
        [SerializeField]
        private int _displayTemplate = -1;

        public ButtonState buttonState
        {
            get { return _buttonState; }
        }
        [SerializeField]
        private ButtonState _buttonState = ButtonState.Interatable;

        [SerializeField]
        private LoadingSpinnerCircle loadingSpinner = null;

        [SerializeField]
        private Text mainButtonText = null;

        [SerializeField]
        private bool ignoreMenuStyle = false;

        public bool initialised { get; private set; } = false;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Initialise();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            Initialise();
            initialised = true;
        }

        private void Initialise()
        {
            SetButtonTemplate(_displayTemplate);
            SetButtonState(_buttonState);
        }

        public void RefreshStylye()
        {
            SetButtonTemplate(_displayTemplate);
        }

        public void SetButtonTemplate(int templateIndex)
        {
            if(!ignoreMenuStyle)
            {
                if (templateIndex < -2 || templateIndex >= CoinModeMenuStyle.customButtonTemplateCount)
                {
                    _displayTemplate = -1;
                }
                else
                {
                    _displayTemplate = templateIndex;
                }

                Image targetImage = targetGraphic as Image;
                if (targetImage != null)
                {
                    targetImage.sprite = CoinModeMenuStyle.GetButtonSprite(_displayTemplate);
                }

                colors = CoinModeMenuStyle.GetButtonColors(_displayTemplate);

                if (mainButtonText != null)
                {
                    mainButtonText.font = CoinModeMenuStyle.GetButtonFont(_displayTemplate);
                    mainButtonText.fontStyle = CoinModeMenuStyle.GetButtonFontStyle(_displayTemplate);
                }

                if (loadingSpinner != null)
                {
                    loadingSpinner.SetColorMode(CoinModeMenuStyle.GetButtonLoadingColorMode(_displayTemplate));
                }
            }
        }

        public void SetButtonState(ButtonState state)
        {
            _buttonState = state;
            switch(_buttonState)
            {
                case ButtonState.Interatable:
                    interactable = true;
                    if(image != null) image.raycastTarget = true;
                    SetSpinnerVisibility(false);
                    SetTextVisibility(true);
                    if(!ignoreMenuStyle) SetTextColor(CoinModeMenuStyle.GetButtonTextColors(_displayTemplate).normalColor);
                    break;
                case ButtonState.Disabled:
                    interactable = false;
                    if (image != null) image.raycastTarget = true;
                    SetSpinnerVisibility(false);
                    SetTextVisibility(true);
                    if (!ignoreMenuStyle) SetTextColor(CoinModeMenuStyle.GetButtonTextColors(_displayTemplate).disabledColor);
                    break;
                case ButtonState.Waiting:
                    UnityEngine.EventSystems.BaseEventData data = new UnityEngine.EventSystems.BaseEventData(UnityEngine.EventSystems.EventSystem.current);
                    OnDeselect(data);
                    interactable = true;
                    if (image != null) image.raycastTarget = false;                    
                    SetSpinnerVisibility(true);
                    SetTextVisibility(false);
                    break;
            }
        }

        public virtual bool SetMainButtonText(string text)
        {
            if(mainButtonText != null)
            {
                mainButtonText.text = text;
                return true;
            }
            return false;
        }

        protected virtual void SetSpinnerVisibility(bool visible)
        {
            if(loadingSpinner != null)
            {
                loadingSpinner.gameObject.SetActive(visible);
            }            
        }

        protected virtual void SetTextVisibility(bool visible)
        {
            if (mainButtonText != null)
            {
                mainButtonText.gameObject.SetActive(visible);
            }
        }

        protected virtual void SetTextColor(Color color)
        {
            if (mainButtonText != null)
            {
                mainButtonText.color = color;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            Analytics.RecordUIClick(transform);
        }
    }
}
