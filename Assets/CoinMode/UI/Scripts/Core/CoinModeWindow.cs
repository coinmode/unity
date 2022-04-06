using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [RequireComponent(typeof(CanvasGroup)), DisallowMultipleComponent]
    public abstract class CoinModeWindow : CoinModeUIBehaviour
    {
        public enum WindowState
        {
            Open,
            Opening,
            Closed,
            Closing
        }

        public RectTransform rectTransform
        {
            get
            {
                if(_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }
        private RectTransform _rectTransform = null;

        public CanvasGroup canvasGroup
        { 
            get 
            { 
                if(_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                    _canvasGroup.alpha = 0.0F;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
                return _canvasGroup;
            } 
        }
        private CanvasGroup _canvasGroup = null;

        private float fadeTimer = 0.2F;
        private bool fade = false;
        private float fadeTime = 0.0F;
        private float startAlpha = 0.0F;
        private float targetAlpha = 0.0F;

        public WindowState state { get { return _state; } private set { _state = value; } }
        private WindowState _state = WindowState.Closed;

        public bool isOpen { get { return state != WindowState.Closed; } }

        private bool windowEnabled = false;

        public abstract bool requiresData { get; }
        public object data { get; private set; } = null;

        protected override void Awake()
        {
            base.Awake();                        
        }

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if (fade)
            {
                CanvasGroup c = canvasGroup;
                fadeTime += deltaTime;
                float normalTime = fadeTime / fadeTimer;
                if (normalTime < 1.0F)
                {
                    c.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalTime);
                }
                else
                {
                    c.alpha = targetAlpha;
                    fade = false;
                    fadeTime = 0.0F;
                    if (state == WindowState.Opening)
                    {
                        state = WindowState.Open;
                        c.interactable = true;
                        c.blocksRaycasts = true;
                        OnOpened();
                    }
                    else if(state == WindowState.Closing)
                    {
                        state = WindowState.Closed;
                        OnClosed();
                    }
                    else
                    {
                        CoinModeLogging.LogError("CoinModeWindow", "UpdateUI", "Invalid window state when finalizing animation!");
                    }
                }
            }
        }

        internal bool Enable()
        {
            if (!windowEnabled)
            {
                windowEnabled = true;
                if (isOpen)
                {
                    canvasGroup.interactable = true;
                }
            }
            return false;
        }

        internal bool Disable()
        {
            if (windowEnabled)
            {
                windowEnabled = false;
                if (isOpen)
                {
                    canvasGroup.interactable = false;
                }
            }
            return false;
        }

        //Need to subtract current time from fade time

        internal bool Open(object data)
        {
            return Open(false, data);
        }

        internal bool OpenImmediately(object data)
        {
            return Open(true, data);
        }

        private bool Open(bool force, object data)
        {
            if (state == WindowState.Closed || state == WindowState.Closing)
            {
                this.data = data;
                gameObject.SetActive(true);
                CanvasGroup c = canvasGroup;
                if (force)
                {
                    state = WindowState.Open;
                    c.alpha = 1.0F;
                    c.interactable = true;
                }
                else
                {
                    state = WindowState.Opening;
                    fade = true;
                    startAlpha = canvasGroup.alpha;
                    targetAlpha = 1.0F;
                    fadeTime = 0.0F;
                }
                c.blocksRaycasts = true;
                OnOpen(this.data);
                if(force)
                {
                    OnOpened();
                }
                return true;
            }
            return false;
        }

        protected abstract void OnOpen(object data);

        protected virtual void OnOpened()
        {
            return;
        }

        internal bool UpdateData(object data)
        {
            if(requiresData)
            {
                if (OnUpdateData(data))
                {
                    this.data = data;
                    return true;
                }
                CoinModeLogging.LogWarning(GetType().ToString(), "UpdateData", "data can only bet set via CoinModeWindow.Open(object data)");
            }
            CoinModeLogging.LogWarning(GetType().ToString(), "UpdateData", "Window does not require any data");
            return false;
        }

        protected abstract bool OnUpdateData(object data);

        public abstract bool IsValidData(object data);

        internal bool Close()
        {
            return Close(false);
        }

        internal bool CloseImmediately()
        {
            return Close(true);
        }

        private bool Close(bool force)
        {
            if (state == WindowState.Open || state == WindowState.Opening)
            {
                if (force)
                {
                    state = WindowState.Closed;
                    canvasGroup.alpha = 0.0F;
                }
                else
                {
                    state = WindowState.Closing;
                    fade = true;
                    startAlpha = canvasGroup.alpha;
                    targetAlpha = 0.0F;
                    fadeTime = 0.0F;
                }
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                OnClose();
                if(force)
                {
                    OnClosed();
                }
                return true;
            }
            return false;
        }

        protected virtual void OnClose()
        {
            return;
        }

        protected virtual void OnClosed()
        {
            gameObject.SetActive(false);
        }

        protected void RebuildLayout(RectTransform target = null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(target != null ? target : rectTransform);
        }

        protected bool IsValidObject<U>(object inputObject, bool allowNull = false)
        {
            System.Type expectedType = typeof(U);
            if (inputObject != null)
            {
                System.Type inType = inputObject.GetType();
                if (inType == expectedType || inType.IsSubclassOf(expectedType))
                {
                    return true;
                }
            }
            else if (allowNull && expectedType.IsSubclassOf(typeof(object)))
            {
                return true;
            }
            return false;
        }

        protected U ValidateObject<U>(object inputObject, bool allowNull = false)
        {
            U output;
            if (!ValidateObjectType(inputObject, allowNull, out output))
            {
                string type = inputObject == null ? "null" : inputObject.GetType().ToString();
                CoinModeLogging.LogError(this.GetType().ToString(), "ValidateObject", string.Format(
                    "Unexpected type, object is: {0}, expected object: {1}", type, typeof(U)
                    ));
            }
            return output;
        }

        private bool ValidateObjectType<U>(object inObject, bool allowNull, out U outObject) 
        {
            outObject = default;
            System.Type expectedType = typeof(U);            
            if(inObject != null)
            {
                System.Type inType = inObject.GetType();
                if (inType == expectedType || inType.IsSubclassOf(expectedType))
                {
                    outObject = (U)inObject;
                    return true;
                }
            }
            else if(allowNull && expectedType.IsSubclassOf(typeof(object)))
            {
                return true;
            }
            return false;
        }
    }
}
