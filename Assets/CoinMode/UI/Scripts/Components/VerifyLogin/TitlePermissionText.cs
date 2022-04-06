using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM TitlePermissionText")]
    public class TitlePermissionText : CoinModeUIBehaviour
    {
        [SerializeField]
        private CoinModeText titleText = null;

        [SerializeField]
        private CoinModeText descriptionText = null;

        public string permissionId { get; private set; } = "";

        public void SetPermissionDetails(string id, string title, string description)
        {
            permissionId = id;
            if (titleText != null) titleText.SetText(title);
            if (descriptionText != null) descriptionText.SetText(description);
        }
    }
}