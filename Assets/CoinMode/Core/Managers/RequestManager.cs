using CoinMode.NetApi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CoinMode
{
    public class RequestManager
    {
        private class RequestRetry
        {
            private const float retryInterval = 5.0F;

            public CoinModeRequestBase request { get; private set; } = null;
            public float timeElapsed { get; private set; } = 0.0F;
            public bool canRetry { get { return request != null && request.canRetry; } }
            public bool retryReady { get { return canRetry && timeElapsed >= retryInterval; } }

            public void AddElapsedTime(float timeElapsed)
            {
                this.timeElapsed += timeElapsed;
            }

            public void Reset()
            {
                request = null;
                timeElapsed = 0.0F;
            }

            public bool AssignRequest(CoinModeRequestBase request)
            {
                if(this.request == null)
                {
                    this.request = request;
                }
                return false;
            }

            public RequestRetry(CoinModeRequestBase request)
            {
                this.request = request;
            }
        }

        private const string productionAPIURL = "https://api.coinmode.com/v1/";
        private const string testAPIURL = "https://api.coinmode-staging.com/v1/";

        private const string productionLocationApiUrl = "https://location.coinmode.com/api/v1/";
        private const string testLocationApiUrl = "https://location.coinmode-staging.com/api/v1/";

        private const string productionOauthApiUrl = "https://oauth.coinmode.com/v1/";
        private const string testOauthApiUrl = "https://oauth.coinmode-staging.com/v1/";

        private List<RequestRetry> requestsToRetry = new List<RequestRetry>();
        private Stack<RequestRetry> requestRetryPool = new Stack<RequestRetry>();

        private MonoBehaviour owner = null;

        private string apiUrl
        {
            get
            {
                switch (CoinModeSettings.environment)
                {
                    case CoinModeEnvironment.Production:
                        {
                            return productionAPIURL;
                        }
                    case CoinModeEnvironment.Staging:
                        {
                            return testAPIURL;
                        }
                    default:
                        {
                            return productionAPIURL;
                        }
                }
            }
        }

        private string locationApiUrl
        {
            get
            {
                switch (CoinModeSettings.environment)
                {
                    case CoinModeEnvironment.Production:
                        {
                            return productionLocationApiUrl;
                        }
                    case CoinModeEnvironment.Staging:
                        {
                            return testLocationApiUrl;
                        }
                    default:
                        {
                            return productionLocationApiUrl;
                        }
                }
            }
        }

        private string oathApiUrl
        {
            get
            {
                switch (CoinModeSettings.environment)
                {
                    case CoinModeEnvironment.Production:
                        {
                            return productionOauthApiUrl;
                        }
                    case CoinModeEnvironment.Staging:
                        {
                            return testOauthApiUrl;
                        }
                    default:
                        {
                            return productionOauthApiUrl;
                        }
                }
            }
        }

        public RequestManager(MonoBehaviour owner)
        {
            this.owner = owner;
        }

        public bool SendRequest<T>(CoinModeRequest<T> request) where T : CoinModeResponse
        {
            if(owner != null)
            {
                StartRequestCoroutine(request, apiUrl);
                return true;
            }
            else
            {
                CoinModeLogging.LogWarning("RequestManager", "SendRequest", "Unable to send request as the owner of this manager is null!");
                return false;
            }
        }

        public bool SendLocationRequest<T>(CoinModeLocationRequest<T> request) where T : CoinModeResponse
        {            
            if (owner != null)
            {
                StartRequestCoroutine(request, locationApiUrl);
                return true;
            }
            else
            {
                CoinModeLogging.LogWarning("RequestManager", "SendLocationRequest", "Unable to send request as the owner of this manager is null!");
                return false;
            }
        }

        public bool SendOauthRequest<T>(CoinModeOauthRequest<T> request) where T : CoinModeResponse
        {
            if (owner != null)
            {
                StartRequestCoroutine(request, oathApiUrl);
                return true;
            }
            else
            {
                CoinModeLogging.LogWarning("RequestManager", "SendOauthRequest", "Unable to send request as the owner of this manager is null!");
                return false;
            }
        }

        private void StartRequestCoroutine(CoinModeRequestBase request, string apiUrl)
        {
            request.AddRetryHandler(HandleRetry);
            owner.StartCoroutine(request.SendRequest(apiUrl));
        }

        public void TickRetries(float timeElapsed)
        {
            if (owner != null)
            {
                for(int i = requestsToRetry.Count - 1; i >= 0; i--)
                {
                    RequestRetry retry = requestsToRetry[i];
                    retry.AddElapsedTime(timeElapsed);
                    if(retry.retryReady)
                    {
                        owner.StartCoroutine(retry.request.AttemptRetry());
                        retry.Reset();
                        requestRetryPool.Push(retry);
                        requestsToRetry.RemoveAt(i);
                    }
                }
            }
        }

        private void HandleRetry(CoinModeRequestBase request)
        {
            if(request.canRetry)
            {
                requestsToRetry.Add(GetRequestRetry(request));
                CoinModeLogging.LogMessage("RequestManager", "HandleRetry", "Queuing Request {0} for retry. {1} attempts remaining.", 
                    request.apiUrl + request.apiDir, request.maxRetries - request.currentRetries);
            }            
        }

        private RequestRetry GetRequestRetry(CoinModeRequestBase request)
        {
            if(requestRetryPool.Count > 0)
            {
                RequestRetry retry = requestRetryPool.Pop();
                retry.AssignRequest(request);
                return retry;
            }
            else
            {
                return new RequestRetry(request);
            }
        }
    }
}