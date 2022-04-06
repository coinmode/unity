using UnityEngine;
using ZXing;
using ZXing.Common;

namespace CoinMode
{
    public static class CoinModeQR
    {        
        public static void CreateQrResources(int width, int height, out Texture2D texture, out Sprite sprite)
        {
            texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;
            texture.anisoLevel = 0;

            sprite = Sprite.Create(texture, new Rect(0.0F, 0.0F, texture.width, texture.height), new Vector2(0.5F, 0.5F), 100.0F, 0, SpriteMeshType.Tight, new Vector4(0.0F, 0.0F, 0.0F, 0.0F));
        }

        public static void GenerateQr(Texture2D qrTexture, string textToEncode)
        {
            var color32 = EncodeForQr(textToEncode, qrTexture.width, qrTexture.height);
            qrTexture.SetPixels32(color32);
            qrTexture.Apply();
        }

        public static void ClearQr(Texture2D qrTexture)
        {
            qrTexture.SetPixels32(new Color32[qrTexture.width * qrTexture.height]);
            qrTexture.Apply();
        }

        private static Color32[] EncodeForQr(string textForEncoding, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = 1
                }
            };

            return writer.Write(textForEncoding);
        }
    }
}
