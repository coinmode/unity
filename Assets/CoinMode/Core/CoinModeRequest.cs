using LightJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CoinMode.NetApi
{
    public delegate void CoinModeRequestEvent(CoinModeRequestBase request);
    public delegate void CoinModeRequestFailure(CoinModeErrorResponse response);

    public abstract class CoinModeRequestBase
    {
        public enum RequestMethod
        {
            GET,
            POST
        }

        private UnityWebRequest webRequest = null;
        protected CoinModeResponse response = null;

        public virtual RequestMethod requestMethod { get; protected set; } = RequestMethod.POST;

        public string apiUrl { get; private set; } = "";
        public string apiDir { get; protected set; } = "";
        public JsonObject requestJson { get; protected set; } = null;
        public bool requestSent { get; private set; } = false;

        public bool requestComplete { get { return webRequest.isDone && requestSent; } }        

        public bool downloadComplete { get { return webRequest.downloadHandler.isDone; } }

        public bool responseError { get { return response != null && response as CoinModeErrorResponse != null; } }

        public abstract bool responseSuccess { get; }

        public bool canRetry { get { return webRequest != null && webRequest.isDone && autoRetry && currentRetries < maxRetries; } }

        public long responseCode { get { return webRequest != null ? webRequest.responseCode : 0; } }

        public virtual bool autoRetry { get; protected set; } = false;
        public virtual int maxRetries { get; protected set; } = 4;
        public virtual int currentRetries { get; private set; } = 0;

        protected CoinModeRequestFailure onRequestFailure = null;
        private CoinModeRequestEvent handleRetry = null;

        internal IEnumerator SendRequest(string apiUrl, int maxRetries = -1)
        {
            if (!requestSent)
            {
                this.apiUrl = apiUrl;
                requestSent = true;

                if (maxRetries > 0)
                {
                    autoRetry = true;
                    this.maxRetries = maxRetries;
                    currentRetries = 0;
                }
                
                yield return SendWebRequest(apiUrl);                              
            }

            while (!webRequest.isDone)
            {
                yield return null;
            }

            HandleResponse(apiUrl);
        }

        private UnityWebRequestAsyncOperation SendWebRequest(string apiUrl)
        {
            webRequest = ConstructRequest(apiUrl + apiDir);

            if (requestJson == null || requestMethod == RequestMethod.GET)
            {
                CoinModeLogging.LogMessage("CoinModeRequest", "SendRequest", "Requesting - {0}", webRequest.url);
            }
            else
            {
                CoinModeLogging.LogMessage("CoinModeRequest", "SendRequest", "Requesting - {0} - JSON: {1}", webRequest.url, requestJson);
            }
            return webRequest.SendWebRequest();
        }

        private UnityWebRequest ConstructRequest(string url)
        {
            switch (requestMethod)
            {
                case RequestMethod.POST:
                    {
                        UnityWebRequest request = UnityWebRequest.Post(url, (string)null);
                        if (requestJson != null)
                        {
                            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(requestJson.ToString()));
                            request.SetRequestHeader("Content-Type", "application/json");
                        }
                        return request;
                    }
                case RequestMethod.GET:
                    {
                        if (requestJson != null && requestJson.Count > 0)
                        {
                            StringBuilder urlBuilder = new StringBuilder(url);
                            urlBuilder.Append('?');
                            int i = 0;
                            foreach (KeyValuePair<string, JsonValue> pair in requestJson)
                            {
                                urlBuilder.Append(pair.Key);
                                urlBuilder.Append('=');
                                switch (pair.Value.Type)
                                {
                                    case JsonValueType.Boolean:
                                        urlBuilder.Append(pair.Value.AsBoolean.ToString());
                                        break;
                                    case JsonValueType.Number:
                                        urlBuilder.Append(pair.Value.AsNumber.ToString());
                                        break;
                                    case JsonValueType.String:
                                        urlBuilder.Append(pair.Value.AsString.ToString());
                                        break;
                                    case JsonValueType.Object:
                                        urlBuilder.Append(pair.Value.AsJsonObject.ToString());
                                        break;
                                    case JsonValueType.Array:
                                        urlBuilder.Append(pair.Value.AsJsonArray.ToString());
                                        break;
                                }
                                if (i < requestJson.Count - 1)
                                {
                                    urlBuilder.Append('&');
                                }
                                i++;
                            }
                            url = urlBuilder.ToString();
                        }
                        return UnityWebRequest.Get(url);
                    }
            }

            return null;
        }

        private void HandleResponse(string apiUrl)
        {
#if UNITY_2020_1_OR_NEWER
            bool error = webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError;
#else
            bool error = webRequest.isHttpError || webRequest.isNetworkError;
#endif
            string errorMessage = webRequest.error;

            JsonObject responseJson = null;

            if (!error)
            {                
                try
                {
                    responseJson = JsonValue.Parse(webRequest.downloadHandler.text).AsJsonObject;
                }
                catch
                {
                    responseJson = null;
                }

                if (responseJson == null)
                {
                    error = true;
                    errorMessage = "Could not parse response!";
                }
                else if (!responseJson.ContainsKey("status") || responseJson["status"] == "")
                {
                    error = true;
                    errorMessage = "Status code not found in response! Json: " + webRequest.downloadHandler.text;
                }
                else if (responseJson["status"] == "error")
                {
                    error = true;
                }
            }

            string requestURL = apiUrl + apiDir;
            string message = !string.IsNullOrEmpty(errorMessage) ? errorMessage : webRequest.downloadHandler.text;
            CoinModeLogging.LogMessage("CoinModeRequest", "HandleRequest", "Received - {0}/ - Response Code: {1} - Response: {2}", requestURL, webRequest.responseCode, message);

            if (!error)
            {
                HandleSuccessResponse(responseJson);
            }
            else
            {
                CoinModeErrorResponse errorResponse = new CoinModeErrorResponse();
                if(responseJson != null && (responseJson.ContainsKey("status") && responseJson["status"] == "error"))
                {
                    errorResponse.FromJson(responseJson);
                }
                else
                {
#if UNITY_2020_1_OR_NEWER
                    errorResponse.FromError(webRequest.result == UnityWebRequest.Result.ConnectionError, webRequest.result == UnityWebRequest.Result.ProtocolError, errorMessage);
#else
                    errorResponse.FromError(webRequest.isNetworkError, webRequest.isHttpError, errorMessage);
#endif                    
                }
                response = errorResponse;
                RequestFailed(errorResponse);                              
            }
        }

        protected abstract void HandleSuccessResponse(JsonObject responseJson);

        protected virtual void RequestFailed(CoinModeErrorResponse response)
        {            
            if(canRetry && handleRetry != null)
            {
                handleRetry?.Invoke(this);
            }
            else
            {
                onRequestFailure?.Invoke(response);
            }
        }

        internal bool AddRetryHandler(CoinModeRequestEvent retryHandler)
        {
            if(autoRetry && (handleRetry == null || handleRetry.GetInvocationList().Length == 0))
            {
                handleRetry = retryHandler;
                return true;
            }
            return false;
        }

        internal IEnumerator AttemptRetry()
        {
            if(webRequest != null)
            {
                if(webRequest.isDone && currentRetries < maxRetries)
                {
                    currentRetries++;

                    yield return SendWebRequest(apiUrl);

                    while (!webRequest.isDone)
                    {
                        yield return null;
                    }

                    HandleResponse(apiUrl);
                }
                else
                {
                    CoinModeLogging.LogMessage("CoinModeRequest", "AttemptRetry", "Cannot attempt retry, webRequest.isDone = {0}, currentRetries = {1} of {2}",
                        webRequest.isDone, currentRetries, maxRetries);
                }
            }
            else
            {
                CoinModeLogging.LogMessage("CoinModeRequest", "AttemptRetry", "Cannot attempt retry, initial request has not been made!");
            }
        }
    }

    public abstract class CoinModeRequestTemplate<T> : CoinModeRequestBase where T : CoinModeResponse
    {
        public override bool responseSuccess { get { return response != null && response as T != null; } }

        protected sealed override void HandleSuccessResponse(JsonObject responseJson)
        {
            T successResponse = ConstructSuccessResponse();
            successResponse.FromJson(responseJson);
            response = successResponse;
            RequestSuccess(successResponse);
        }

        protected abstract T ConstructSuccessResponse();

        protected abstract void RequestSuccess(T response);
    }

    public abstract class CoinModeRequest<T> : CoinModeRequestTemplate<T> where T : CoinModeResponse 
    {
        protected override void RequestSuccess(T Response)
        {
            Analytics.RecordRequestSuccess(apiDir);
        }

        protected override void RequestFailed(CoinModeErrorResponse response)
        {
            base.RequestFailed(response);
            Analytics.RecordRequestFailure(apiDir, response.GetErrorString());
        }
    } 

    public abstract class CoinModeLocationRequest<T> : CoinModeRequestTemplate<T> where T : CoinModeResponse 
    {
        protected override void RequestSuccess(T Response)
        {
            Analytics.RecordLocationRequestSuccess(apiDir);
        }

        protected override void RequestFailed(CoinModeErrorResponse response)
        {
            base.RequestFailed(response);
            Analytics.RecordLocationRequestFailure(apiDir, response.GetErrorString());
        }
    }

    public abstract class CoinModeOauthRequest<T> : CoinModeRequestTemplate<T> where T : CoinModeResponse 
    {
        protected override void RequestSuccess(T Response)
        {
            Analytics.RecordOauthRequestSuccess(apiDir);
        }

        protected override void RequestFailed(CoinModeErrorResponse response)
        {
            base.RequestFailed(response);
            Analytics.RecordOauthRequestFailure(apiDir, response.GetErrorString());
        }
    }
}
