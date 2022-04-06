using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode
{
    public abstract class AdvertBannerBase : MonoBehaviour
    {
        [SerializeField] private string advertAssetKey = "";
        [SerializeField] protected bool hideWhenEmpty = false;

        [SerializeField] protected Vector2 desiredWorldSize = Vector2.one;

        protected bool bannerEnabled = true;

        protected virtual void OnValidate() { }

        protected virtual void Start()
        {
            if(bannerEnabled)
            {
                CoinModeManager.advertisementComponent.onAdvertDataUpdated += OnAdvertDataUpdated;
                TrySetupBanner(CoinModeManager.advertisementComponent);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnAdvertDataUpdated(AdvertisementComponent advertComponent)
        {
            TrySetupBanner(CoinModeManager.advertisementComponent);
        }

        private void TrySetupBanner(AdvertisementComponent advertComponent)
        {
            SetToLoading();
            AdvertAsset asset = null;
            if (advertComponent.TryGetFirstAdvertAsset(advertAssetKey, out asset))
            {
                // TODO create enum to list types instead of rely directly on string comparisons
                if (asset.type == "image")
                {
                    CoinModeManager.DownloadTexture(asset.value, TextureDownloaded, null);
                }
            }
        }

        private void TextureDownloaded(Texture2D texture)
        {
            OnTextureDownloaded(texture);
        }

        protected float GetDesiredScale(Vector2 worldSize)
        {
            return Mathf.Min(desiredWorldSize.x / worldSize.x, desiredWorldSize.y / worldSize.y);
        }

        protected abstract void SetToLoading();
        protected abstract void OnTextureDownloaded(Texture2D texture);

        protected void DrawFrameGizmo(Transform origin, Vector3 up, Vector3 right)
        {
            Vector3 t = up * (desiredWorldSize.y / 2.0F);
            Vector3 r = right * (desiredWorldSize.x / 2.0F);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin.position + t + -r, origin.position + t + r);
            Gizmos.DrawLine(origin.position + t + r, origin.position + -t + r);
            Gizmos.DrawLine(origin.position + -t + r, origin.position + -t + -r);
            Gizmos.DrawLine(origin.position + -t + -r, origin.position + t + -r);
        }
    }
}
