using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM LoadingScreen")]
    public class LoadingScreen : CoinModeScreen
    {
        public struct ScreenData
        {
            public LoadingScreenState state { get; private set; }
            public UnityAction retryEvent { get; private set; }
            public string message { get; private set; }

            public ScreenData(LoadingScreenState state, UnityAction retryEvent)
            {
                this.state = state;
                this.retryEvent = retryEvent;
                this.message = "";
            }

            public ScreenData(LoadingScreenState state, UnityAction retryEvent, string message)
            {
                this.state = state;
                this.retryEvent = retryEvent;
                this.message = message;
            }

            public ScreenData(bool operationSuccessful, UnityAction retryEvent, string successMessage, string errorMessage)
            {
                this.state = operationSuccessful ? LoadingScreenState.Loading : LoadingScreenState.Failed;
                this.retryEvent = retryEvent;
                this.message = operationSuccessful ? successMessage : errorMessage;
            }
        }

        public enum LoadingScreenState
        {
            Loading,
            Failed,
        }

        [SerializeField]
        private Text messageText = null;

        [SerializeField]
        private LoadingSpinnerCircle loadingSpinner = null;

        [SerializeField]
        private RectTransform buttonContainer = null;

        [SerializeField]
        private CoinModeButton closeButton = null;

        [SerializeField]
        private CoinModeButton retryButton = null;

        public override bool supportsHistory { get; } = false;
        public override bool requiresData { get; } = true;
        private ScreenData loadingScreenData = new ScreenData();

        private LoadingScreenState loadingState = LoadingScreenState.Loading;

        protected override void Start()
        {
            base.Start();

            if(closeButton != null)
            {
                closeButton.onClick.AddListener(CloseAction);
            }

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RetryAction);
            }
        }

        protected override void OnOpen(object data)
        {
            loadingScreenData = ValidateObject<ScreenData>(data);
            SetScreenState(loadingScreenData.state, loadingScreenData.message);
        }

        protected override bool OnUpdateData(object data) 
        {
            loadingScreenData = ValidateObject<ScreenData>(data);
            SetScreenState(loadingScreenData.state, loadingScreenData.message);            
            return true; 
        }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        public void SetScreenState(bool operationSuccessful, string successMessage, string errorMessage)
        {
            SetScreenState(operationSuccessful ? LoadingScreenState.Loading : LoadingScreenState.Failed,
                operationSuccessful ? successMessage : errorMessage);
        }

        public void SetScreenState(LoadingScreenState state, string message = "")
        {
            this.loadingState = state;
            if (loadingSpinner != null)
            {
                loadingSpinner.gameObject.SetActive(this.loadingState == LoadingScreenState.Loading);
            }
            if (buttonContainer != null)
            {
                buttonContainer.gameObject.SetActive(this.loadingState == LoadingScreenState.Failed);
            }
            if(messageText != null)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    messageText.gameObject.SetActive(false);
                }
                else
                {
                    messageText.gameObject.SetActive(true);
                    messageText.text = message;
                }
            }
        }

        private void CloseAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void RetryAction()
        {
            loadingScreenData.retryEvent?.Invoke();
        }
    }
}