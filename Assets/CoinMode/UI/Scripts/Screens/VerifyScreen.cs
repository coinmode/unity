using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public abstract class VerifyScreen : CoinModeMenuScreen
    {
        [SerializeField]
        private RectTransform contentContainer = null;

        [SerializeField]
        private VerificationInputField verificationFieldTemplate = null;

        [SerializeField]
        private ScrollRect scrollBox = null;

        [SerializeField]
        private float defaultScrollBoxHeight = 240.0F;

        public CoinModeButton verifyButton = null;
        public CoinModeButton rejectButton = null;

        public override bool requiresData { get; } = true;
        protected VerificationComponent verificationComponent = null;

        private List<GameObject> verifyChildren = new List<GameObject>();
        private Dictionary<string, VerificationInputField> verificationFields = new Dictionary<string, VerificationInputField>();

        protected override void Start()
        {
            base.Start();

            if (verifyButton != null)
            {
                verifyButton.onClick.AddListener(VerifyAction);
            }

            if (rejectButton != null)
            {
                rejectButton.onClick.AddListener(RejectAction);
            }
        }

        protected override void OnOpen(object data)
        {
            verificationComponent = ValidateObject<VerificationComponent>(data);
            for (int i = 0; i < verifyChildren.Count; i++)
            {
                Destroy(verifyChildren[i]);
            }
            verifyChildren.Clear();
            verificationFields.Clear();

            OnPrePopulateVerificationFields(contentContainer, verifyChildren);

            if (verificationComponent.requiredCount > 0)
            {
                foreach(KeyValuePair<string, VerificationMethod> pair in verificationComponent.requiredMethods)
                {
                    VerificationInputField verifyField = Instantiate(verificationFieldTemplate, contentContainer);
                    verifyField.gameObject.SetActive(true);
                    verifyField.SetVerificationDetails(pair.Key, pair.Value.properties.userText, pair.Value.properties.field, pair.Value.properties.type, pair.Value.properties.maxLength.Value);
                    verifyField.onVerificationKeyEntered += EnterVerificationKey;
                    verifyChildren.Add(verifyField.gameObject);
                    verificationFields.Add(verifyField.verificationId, verifyField);
                }
            }

            RebuildLayout(contentContainer);

            RectTransform scrollBoxTransform = scrollBox.transform as RectTransform;
            if (scrollBoxTransform != null)
            {
                Vector2 size = scrollBoxTransform.sizeDelta;
                if (contentContainer.rect.height >= defaultScrollBoxHeight)
                {
                    size.y = defaultScrollBoxHeight;
                }
                else
                {
                    size.y = contentContainer.sizeDelta.y;
                }                                
                scrollBoxTransform.sizeDelta = size;
            }            
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<VerificationComponent>(data);
        }

        private void EnterVerificationKey(VerificationInputField field, string fieldKey)
        {
            verificationComponent.SetKey(field.verificationId, fieldKey);
        }

        protected virtual void OnPrePopulateVerificationFields(RectTransform contentContainer, List<GameObject> verifyChildren) { }

        protected virtual void VerifyAction()
        {
            verifyButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            controller.Disable();
        }

        protected void OnVerifySuccess()
        {
            verifyButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
        }

        protected void OnVerifyUpdate()
        {
            verifyButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();

            foreach(KeyValuePair<string, VerificationMethod> method in verificationComponent.requiredMethods)
            {
                VerificationInputField field;
                if(method.Value.status == VerificationStatus.Approved && verificationFields.TryGetValue(method.Key, out field))
                {
                    field.interactable = false;
                    field.SetInputTextColor(Color.green);
                }
            }

            foreach (KeyValuePair<string,string> error in verificationComponent.errors)
            {
                VerificationInputField field;
                if (verificationFields.TryGetValue(error.Key, out field))
                {
                    field.SetInputText("");
                    field.SetPlaceholderText(error.Value);
                    field.SetPlaceholderColor(Color.red);
                }
            }

            RebuildLayout(contentContainer);
        }

        protected void OnVerifyFailure()
        {
            verifyButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
        }

        protected virtual void RejectAction()
        {
            controller.ReturnToPreviousScreen();
        }
    }
}