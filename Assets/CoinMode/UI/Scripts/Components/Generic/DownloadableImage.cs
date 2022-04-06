using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM DownloadableImage")]
    public class DownloadableImage : CoinModeUIBehaviour
    {
        public enum ImageConstraint
        {
            Height,
            Width
        }

        [SerializeField]
        private LoadingSpinnerCircle loadingSpinner = null;

        [SerializeField]
        private ImageConstraint constraint = ImageConstraint.Height;

        [SerializeField][Tooltip("The maximum size of the downloaded image, a value of zero will not constrain the size of the image")]
        private Vector2 maxImageSize = Vector2.zero;

        [SerializeField]
        private Image image = null;

        [SerializeField]
        private LayoutElement layoutElement = null;

        private Vector2 initialPreferredSize = Vector2.zero;

        private Sprite currentSprite = null;

        protected override void Awake()
        {
            SetInitialSize();
            base.Awake();            
        }

        protected override void OnValidate()
        {
            base.OnValidate();
        }

        public void SetImageFromUrl(string url)
        {
            if (layoutElement != null && (initialPreferredSize.x != layoutElement.preferredHeight || initialPreferredSize.y != layoutElement.preferredWidth))
            {
                SetInitialSize();
            }
            SetLoading();
            CoinModeManager.DownloadTexture(url, DownloadSuccess, DownloadFailure);
        }

        private void SetLoading()
        {
            image.sprite = null;
            image.color = CoinModeMenuStyle.accentColor;
            image.gameObject.SetActive(false);

            SetSize(initialPreferredSize);

            loadingSpinner.gameObject.SetActive(true);
        }

        private void DownloadSuccess(Texture2D texture)
        {
            currentSprite = Sprite.Create(texture, new Rect(0.0F, 0.0F, texture.width, texture.height), new Vector2(0.5F, 0.5F), 100.0F, 0, SpriteMeshType.Tight, new Vector4(0.0F, 0.0F, 0.0F, 0.0F));            
            image.sprite = currentSprite;
            image.color = Color.white;
            image.gameObject.SetActive(true);

            SetSize(GetDimensions(texture.width, texture.height));

            loadingSpinner.gameObject.SetActive(false);
        }        

        private void DownloadFailure(CoinModeError error)
        {
            image.color = Color.white;
            loadingSpinner.gameObject.SetActive(false);
        }

        private Vector2 GetDimensions(float width, float height)
        {
            Vector2 d = Vector2.zero;
            float aspect = 1.0F;
            RectTransform parentRect = transform.parent as RectTransform;
            Vector2 parentSize = parentRect != null ? parentRect.rect.size : Vector2.zero;
            Vector2 max = new Vector2(
                constraint == ImageConstraint.Height ? (maxImageSize.x > 0.0F && maxImageSize.x > parentSize.x ? maxImageSize.x : parentSize.x) : initialPreferredSize.x,
                constraint == ImageConstraint.Width ? (maxImageSize.y > 0.0F && maxImageSize.y > parentSize.y ? maxImageSize.y : parentSize.y) : initialPreferredSize.y
                );

            if (max.x > width) max.x = width;
            if (max.y > height) max.y = height;

            switch (constraint)
            {
                case ImageConstraint.Height:
                    aspect = width / height;
                    float newWidth = initialPreferredSize.x * aspect;                    
                    if(newWidth > max.x)
                    {
                        d.x = max.x;
                        d.y = max.x / aspect;
                    }
                    else
                    {
                        d.x = newWidth;
                        d.y = initialPreferredSize.y;
                    }
                    break;
                case ImageConstraint.Width:
                    aspect = height / width;
                    float newHeight = initialPreferredSize.y * aspect;
                    if (newHeight > max.y)
                    {
                        d.x = max.y / aspect;
                        d.y = max.y;
                    }
                    else
                    {
                        d.x = initialPreferredSize.x;
                        d.y = newHeight;
                    }
                    break;
            }
            return d;
        }

        private void SetSize(Vector2 size)
        {
            RectTransform rect = transform as RectTransform;
            if(rect != null)
            {
                rect.sizeDelta = size;
            }

            if (layoutElement != null)
            {
                layoutElement.preferredWidth = size.x;
                layoutElement.preferredHeight = size.y;
            }
        }

        private void SetInitialSize()
        {
            if (layoutElement != null)
            {
                switch (constraint)
                {
                    case ImageConstraint.Height:
                        initialPreferredSize = new Vector2(layoutElement.preferredHeight, layoutElement.preferredHeight);
                        break;
                    case ImageConstraint.Width:
                        initialPreferredSize = new Vector2(layoutElement.preferredWidth, layoutElement.preferredWidth);
                        break;
                }
            }
        }
    }
}
