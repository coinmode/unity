using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public abstract class CoinModeScreenController : CoinModeWindow
    {
        internal struct ScreenReturnResponse
        {
            public bool success { get; private set; }
            public Type screenReturnedTo { get; private set; }
            public bool ControllerClosed() { return screenReturnedTo == null; }

            internal ScreenReturnResponse(bool success, Type screenReturnedTo) 
            {
                this.success = success;
                this.screenReturnedTo = screenReturnedTo;
            }
        }

        protected abstract Type defaultScreen { get; }

        protected abstract object defaultData { get; }

        private Dictionary<Type, CoinModeScreen> screens = new Dictionary<Type, CoinModeScreen>();

        public Type activeScreen { get; private set; } = null;

        private bool reSize = false;
        private float reSizeTimer = 0.2F;
        private float reSizeTime = 0.0F;

        private Vector2 currentSize = Vector2.zero;
        private Vector2 targetSize = Vector2.zero;

        private Stack<Type> screenHistory = new Stack<Type>();

        protected override void Awake()
        {
            StartCoroutine(EndAwake());
            base.Awake();
            CoinModeScreen[] screens = GetComponentsInChildren<CoinModeScreen>(true);

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            canvasGroup.alpha = 0.0F;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i] != this)
                {
                    Type screenType = screens[i].GetType();
                    if (!this.screens.ContainsKey(screenType))
                    {
                        screens[i].InitScreen(this);
                        this.screens.Add(screenType, screens[i]);
                        if (screenType == defaultScreen)
                        {
                            screens[i].OpenImmediately(defaultData);
                            activeScreen = defaultScreen;
                            Vector2 targetSize = GetTargetSize(screens[i]);
                            if (targetSize != rectTransform.sizeDelta)
                            {
                                rectTransform.sizeDelta = targetSize;
                            }
                        }
                        else
                        {
                            screens[i].CloseImmediately();
                        }
                    }
                }
            }
        }

        private IEnumerator EndAwake()
        {
            yield return new WaitForEndOfFrame();
            BuildSelectableTree();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);

            if(isOpen)
            {
                CoinModeScreen currentScreen = screens[activeScreen];
                if(GetTargetSize(currentScreen) != targetSize)
                {
                    StartResize(currentScreen);
                }
            }

            if (reSize)
            {
                reSizeTime += deltaTime;
                float normalTime = reSizeTime / reSizeTimer;
                if (normalTime < 1.0F)
                {
                    rectTransform.sizeDelta = Vector2.Lerp(currentSize, targetSize, normalTime);
                }
                else
                {
                    rectTransform.sizeDelta = targetSize;
                    currentSize = targetSize;
                    reSizeTime = 0.0F;
                    reSize = false;
                }
            }
        }

        internal bool SwitchScreen<T>(object data, bool excludeCurrentFromHistory = false) where T : CoinModeScreen
        {
            Type targetType = typeof(T);
            if (screens.ContainsKey(targetType) && activeScreen != targetType)
            {
                screens[activeScreen].Close();
                if (screens[activeScreen].supportsHistory && !excludeCurrentFromHistory)
                {
                    screenHistory.Push(activeScreen);
                }
                screens[targetType].Open(data);
                StartResize(screens[targetType]);
                activeScreen = targetType;                
                return true;
            }
            return false;
        }

        internal ScreenReturnResponse ReturnToPreviousScreen()
        {
            if (screens.ContainsKey(activeScreen))
            {
                if (screenHistory.Count > 0)
                {
                    Type targetScreen = screenHistory.Pop();

                    screens[activeScreen].Close();

                    screens[targetScreen].Open(screens[targetScreen].data);
                    StartResize(screens[targetScreen]);
                    activeScreen = targetScreen;
                    return new ScreenReturnResponse(true, activeScreen);
                }
                else
                {
                    Close();
                    return new ScreenReturnResponse(true, null);
                }                
            }
            return new ScreenReturnResponse(false, null);
        }

        internal T GetScreen<T>() where T : CoinModeScreen
        {
            T screen = null;
            Type targetType = typeof(T);
            if(screens.ContainsKey(targetType))
            {
                CoinModeScreen cms = screens[targetType];
                System.Type screenType = cms.GetType();
                if(targetType.IsAssignableFrom(screenType))
                {
                    screen = (T)cms;
                }
                else
                {
                    CoinModeLogging.LogWarning("CoinModeScreenController", "GetScreen", "Expected screen to be type {0}, screen is actually {1}", targetType.ToString(), screenType.ToString());
                }
            }
            return screen;
        }

        internal virtual bool OpenToScreen<T>(object screenData) where T : CoinModeScreen
        {
            Type targetType = typeof(T);
            if (Open(screenData))
            {
                targetType = targetType == null ? defaultScreen : targetType;
                if (screens.ContainsKey(targetType))
                {
                    if (targetType != activeScreen && activeScreen != null)
                    {
                        screens[activeScreen].CloseImmediately();
                    }
                    screens[targetType].OpenImmediately(screenData);                    
                    activeScreen = targetType;                    

                    Vector2 targetSize = GetTargetSize(screens[targetType]);
                    if (targetSize != rectTransform.sizeDelta)
                    {
                        rectTransform.sizeDelta = targetSize;
                    }
                }
                return true;
            }
            else if (activeScreen != null)
            {
                CoinModeLogging.LogMessage("CoinModeMenu", "OpenToScreen", "Already open at screen {0}, attempting to switch to {1}", activeScreen, targetType);
                return SwitchScreen<T>(screenData);
            }
            return false;
        }

        new public bool Close()
        {            
            screenHistory.Clear();
            return base.Close();
        }

        protected override void OnClosed()
        {
            if (activeScreen != null && screens.ContainsKey(activeScreen))
            {
                screens[activeScreen].CloseImmediately();
                activeScreen = null;                
            }
        }

        private Vector2 GetTargetSize(CoinModeScreen targetScreen)
        {
            RectTransform screenParent = targetScreen.rectTransform.parent as RectTransform;
            Vector2 offset = Vector2.zero;
            if (screenParent != null)
            {
                offset = rectTransform.rect.size - screenParent.rect.size;
            }
            return targetScreen.rectTransform.rect.size + offset;
        }

        private void StartResize(CoinModeScreen targetScreen)
        {
            Vector2 targetSize = GetTargetSize(targetScreen);
            if (targetSize != rectTransform.sizeDelta)
            {
                reSize = true;
                this.targetSize = targetSize;
                currentSize = rectTransform.sizeDelta;
                reSizeTime = 0.0F;
            }
        }

        protected void ClearScreenHistory()
        {
            screenHistory.Clear();
        }

        public void RemoveLatestFromHistory(params Type[] screens)
        {
            List<Type> history = new List<Type>(screenHistory);
            history.Reverse();
            for(int i = 0; i < screens.Length; i++)
            {
                for(int j = history.Count - 1; j >= 0; j--)
                {
                    if(history[j] == screens[i])
                    {
                        history.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}