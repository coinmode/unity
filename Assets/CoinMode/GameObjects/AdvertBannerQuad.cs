using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode
{
    [AddComponentMenu("CoinMode/CM AdvertBannerQuad")]
    class AdvertBannerQuad : AdvertBannerBase
    {
        [System.Serializable]
        public struct Materials
        {
            public Material builtinRenderer;
            public Material urpLit;
            public Material urpSimpleLit;
            public Material hdrpLit;
        }

        public enum MaterialType
        {
            BuiltinRenderer,
            UrpLit,
            UrpSimpleLit,
            HdrpLit,
        }

        [SerializeField] protected Texture2D loadingTexture = null;
        [SerializeField] protected MeshRenderer targetRenderer = null;

        [SerializeField] private MaterialType materialType = MaterialType.BuiltinRenderer;
        [SerializeField] private Materials materials = new Materials();

        private Vector2 sourceWorldSize = Vector2.one;

        protected override void OnValidate()
        {
            SetDefaultScale();
            SetUpMaterials();
            base.OnValidate();
        }

        protected override void Start()
        {
            SetDefaultScale();
            SetUpMaterials();
            base.Start();
        }              

        protected override void SetToLoading()
        {
            SetRendererMaterialTexture(loadingTexture, hideWhenEmpty);   
        }        

        protected override void OnTextureDownloaded(Texture2D texture)
        {
            if(texture != null)
            {
                SetRendererMaterialTexture(texture, false);
            }            
        }

        private void SetUpMaterials()
        {
            Material chosenMaterial = null;
            switch (materialType)
            {
                default:
                case MaterialType.BuiltinRenderer:
                    chosenMaterial = materials.builtinRenderer;
                    break;
                case MaterialType.UrpLit:
                    chosenMaterial = materials.urpLit;
                    break;
                case MaterialType.UrpSimpleLit:
                    chosenMaterial = materials.urpSimpleLit;
                    break;
                case MaterialType.HdrpLit:
                    chosenMaterial = materials.hdrpLit;
                    break;
            }
            if (chosenMaterial != null)
            {
                if (targetRenderer != null)
                {
                    targetRenderer.material = chosenMaterial;
                }
            }
            else
            {
                if (Application.isPlaying)
                {
                    CoinModeLogging.LogWarning("AdvertTextureBanner", "SetUpMaterials", "Could not assign material for {0}, component will be disabled!");
                    bannerEnabled = false;
                }
            }
        }        

        private void SetScale(MeshRenderer renderer, Texture2D texture)
        {
            Vector2 worldSize = Vector2.zero;
            if (texture != null)
            {
                float textureAspect = (float)texture.height / texture.width;
                float sourceAspect = sourceWorldSize.y / sourceWorldSize.x;
                float s = textureAspect / sourceAspect;
                worldSize = new Vector2(sourceWorldSize.x, sourceWorldSize.x * textureAspect);
                float scale = GetDesiredScale(worldSize);
                Vector3 vScale = new Vector3(scale, scale * s, scale);
                renderer.transform.localScale = vScale;
            }
            else
            {
                worldSize = sourceWorldSize;
                float scale = GetDesiredScale(worldSize);
                Vector3 vScale = new Vector3(scale, scale, scale);
                renderer.transform.localScale = vScale;
            }                                    
        }

        private void SetDefaultScale()
        {
            if (targetRenderer != null)
            {
                Texture2D texture = GetMaterialTexture(targetRenderer.sharedMaterial);
                SetScale(targetRenderer, texture);
            }
        }

        private void SetRendererMaterialTexture(Texture2D texture, bool hide)
        {
            if (targetRenderer != null)
            {
                targetRenderer.enabled = !hide;
                SetMaterialTexture(targetRenderer.material, texture);
                SetScale(targetRenderer, texture);
            }
        }

        private void SetMaterialTexture(Material material, Texture2D texture)
        {
            switch (materialType)
            {
                case MaterialType.BuiltinRenderer:
                    material.SetTexture("_MainTex", texture);
                    break;
                case MaterialType.HdrpLit:
                    material.SetTexture("_BaseColorMap", texture);
                    break;
                case MaterialType.UrpLit:
                case MaterialType.UrpSimpleLit:
                    material.SetTexture("_BaseMap", texture);
                    break;
            }
        }

        private Texture2D GetMaterialTexture(Material material)
        {
            switch (materialType)
            {
                case MaterialType.BuiltinRenderer:
                    return material.GetTexture("_MainTex") as Texture2D;
                case MaterialType.HdrpLit:
                    return material.GetTexture("_BaseColorMap") as Texture2D;
                case MaterialType.UrpLit:
                case MaterialType.UrpSimpleLit:
                    return material.GetTexture("_BaseMap") as Texture2D;
            }
            return null;
        }

        private void OnDrawGizmos()
        {
            Transform origin = targetRenderer != null ? targetRenderer.transform : transform;
            DrawFrameGizmo(origin, origin.up, origin.right);
        }
    }
}
