using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode.UI
{
    public delegate void VerificationFieldEvent(VerificationInputField field, string fieldText);

    [AddComponentMenu("CoinMode/UI/CM VerificationInputField")]
    public class VerificationInputField : CoinModeInputField
    {
        public string verificationId { get; private set; } = "invalid";
        public string description { get; private set; } = "";
        public string type { get; private set; } = "none";
        public int maxCharacters { get; private set; } = 0;

        public VerificationFieldEvent onVerificationKeyEntered = null;

        public void SetVerificationDetails(string id, string description, string field, string type, int maxCharacters)
        {
            verificationId = id;
            this.description = description;
            this.type = type;
            this.maxCharacters = maxCharacters;
            characterLimit = maxCharacters;
            SetPlaceholderText(description);
            if (field == "email" || field == "sms" || field == "gauth")
            {
                contentType = ContentType.IntegerNumber;
                keyboardType = TouchScreenKeyboardType.PhonePad;
            }
            else if (field == "password")
            {
                contentType = ContentType.Password;
                keyboardType = TouchScreenKeyboardType.Default;
            }
            else
            {
                contentType = ContentType.Standard;
                keyboardType = TouchScreenKeyboardType.Default;
            }
        }

        protected override void OnEndEdit(string value)
        {
            onVerificationKeyEntered?.Invoke(this, value);
        }
    }
}