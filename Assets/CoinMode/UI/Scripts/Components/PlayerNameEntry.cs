using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM PlayerNameEntry")]
    public class PlayerNameEntry : CoinModeUIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color highlightColor = Color.white;

        [SerializeField]
        private Image backgroundImage = null;

        [SerializeField]
        private CoinModeText nameText = null;

        private PlayerComponent player = null;
        private string userPublicId = "";

        private void Initialize()
        {
            backgroundImage.color = normalColor * CoinModeMenuStyle.accentColor;
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if(userPublicId == player.publicId)
            {
                CoinModeMenu.OpenPlayerMenu();
            }
            else
            {
                CoinModeMenu.OpenUserInfo(userPublicId);
            }            
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            backgroundImage.color = highlightColor * CoinModeMenuStyle.accentColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            backgroundImage.color = normalColor * CoinModeMenuStyle.accentColor;
        }

        public void SetInfo(string displayName, string publicId, PlayerComponent player)
        {
            userPublicId = publicId;
            this.player = player;
            if (nameText != null)
            {
                nameText.SetText(displayName);
            }
        }
    }
}