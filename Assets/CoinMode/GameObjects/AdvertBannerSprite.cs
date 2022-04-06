using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode
{
    [AddComponentMenu("CoinMode/CM AdvertBannerSprite")]
    class AdvertBannerSprite : AdvertBannerBase
    {
        private static Vector2 pivot = new Vector2(0.5F, 0.5F);

        [SerializeField] private Sprite loadingSprite = null;
        [SerializeField] private SpriteRenderer targetRenderer = null;

        [SerializeField] private float pixelsPerUnit = 100.0F;

        private Sprite bannerSprite = null;

        protected override void OnValidate()
        {
            base.OnValidate();
            SetScale();
        }

        protected override void Start()
        {
            base.Start();
            SetScale();
        }            

        protected override void OnTextureDownloaded(Texture2D texture)
        {
            bannerSprite = Sprite.Create(texture, new Rect(0.0F, 0.0F, texture.width, texture.height), pivot, 
                pixelsPerUnit, 0, SpriteMeshType.Tight, Vector4.zero);
            if (targetRenderer != null)
            {
                targetRenderer.enabled = true;
                targetRenderer.sprite = bannerSprite;
            }
            SetScale();
        }

        protected override void SetToLoading()
        {
            if (targetRenderer != null)
            {
                targetRenderer.enabled = !hideWhenEmpty;
                targetRenderer.sprite = loadingSprite;                
            }
            SetScale();
        }

        private Vector2 GetWorldSize(float spriteWidth, float spriteHeight, float pixelsPerUnit)
        {
            return new Vector2(spriteWidth / pixelsPerUnit, spriteHeight / pixelsPerUnit);
        }

        private void SetScale()
        {
            if (targetRenderer != null)
            {
                Vector2 worldSize = GetWorldSize(targetRenderer.sprite.texture.width, targetRenderer.sprite.texture.height, targetRenderer.sprite.pixelsPerUnit);
                float scale = GetDesiredScale(worldSize);
                targetRenderer.transform.localScale = new Vector2(scale, scale);
            }
        }

        private void OnDrawGizmos()
        {
            Transform origin = targetRenderer != null ? targetRenderer.transform : transform;
            DrawFrameGizmo(origin, origin.up, origin.right);
        }    
    }
}
