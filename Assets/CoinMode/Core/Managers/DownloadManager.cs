using CoinMode.NetApi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CoinMode
{
    /// <summary>
    /// Delegate signature used for all texture events when a texture has been downloaded successfully.
    /// </summary>
    /// <param name="texture">Reference to the downloaded Texture2D object.</param>
    public delegate void TextureEvent(Texture2D texture);

    public class DownloadManager
    {
        private struct TextureDownloadRequest
        {
            public string url { get; private set; }
            public TextureEvent textureDownloadSuccess { get; set; }

            public CoinModeFailure textureDownloadFailure { get; set; }

            public TextureDownloadRequest(string url)
            {
                this.url = url;
                textureDownloadSuccess = null;
                textureDownloadFailure = null;
            }
        }

        private MonoBehaviour owner = null;

        private Dictionary<string, TextureDownloadRequest> textureDownloadRequests = new Dictionary<string, TextureDownloadRequest>();
        private Dictionary<string, Texture2D> textureDownloadCache = new Dictionary<string, Texture2D>();

        public DownloadManager(MonoBehaviour owner)
        {
            this.owner = owner;
        }

        public bool DownloadTexture(string url, TextureEvent onSuccess, CoinModeFailure onFailure)
        {
            if (owner == null)
            {
                CoinModeLogging.LogWarning("DownloadManager", "DownloadTexture", "Unable to send request as the owner of this manager is null!");
                return false;
            }

            Texture2D existingTexture = null;
            textureDownloadCache.TryGetValue(url, out existingTexture);

            if (existingTexture != null)
            {
                onSuccess.Invoke(existingTexture);
            }
            else
            {
                bool sendNewRequest = false;
                TextureDownloadRequest downloadRequest;
                if (!textureDownloadRequests.TryGetValue(url, out downloadRequest))
                {
                    sendNewRequest = true;
                    downloadRequest = new TextureDownloadRequest(url);
                }
                downloadRequest.textureDownloadSuccess -= onSuccess;
                downloadRequest.textureDownloadSuccess += onSuccess;

                downloadRequest.textureDownloadFailure -= onFailure;
                downloadRequest.textureDownloadFailure += onFailure;

                textureDownloadRequests[url] = downloadRequest;

                if (sendNewRequest)
                {
                    owner.StartCoroutine(DownloadTexture(url));
                }
            }
            return true;
        }

        private IEnumerator DownloadTexture(string url)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
            yield return webRequest.SendWebRequest();

            TextureDownloadRequest downloadRequest;
            if (textureDownloadRequests.TryGetValue(url, out downloadRequest))
            {
#if UNITY_2020_1_OR_NEWER
                bool error = webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError;
#else
                bool error = webRequest.isHttpError || webRequest.isNetworkError;
#endif
                if (error)
                {
#if UNITY_2020_1_OR_NEWER
                    CoinModeErrorBase.ErrorType errorType = webRequest.result == UnityWebRequest.Result.ConnectionError ? CoinModeErrorBase.ErrorType.Network : CoinModeErrorBase.ErrorType.Http;
                    string errorCode = webRequest.result == UnityWebRequest.Result.ProtocolError ? webRequest.responseCode.ToString() : "NETWORK_ERROR";
#else
                    CoinModeErrorBase.ErrorType errorType = webRequest.isNetworkError ? CoinModeErrorBase.ErrorType.Network : CoinModeErrorBase.ErrorType.Http;
                    string errorCode = webRequest.isHttpError ? webRequest.responseCode.ToString() : "NETWORK_ERROR";
#endif                    
                    downloadRequest.textureDownloadFailure?.Invoke(new CoinModeError(errorType, errorCode, webRequest.error));
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                    textureDownloadCache.Add(url, texture);
                    downloadRequest.textureDownloadSuccess?.Invoke(texture);
                }
                textureDownloadRequests.Remove(url);
            }
        }
    }
}