#if UNITY_2019_3_OR_NEWER
using UnityEngine;
using UnityEngine.Events;

namespace CoinMode
{    
    [AddComponentMenu("CoinMode/CM DeepLinkCapture")]
    public class DeepLinkCapture : MonoBehaviour
    {
        [System.Serializable] public class CoinModeDeepLinkUnityEvent : UnityEvent<DeepLinkParams> { }
        public delegate void CoinModeDeepLinkEvent(DeepLinkParams parameters);

        private static DeepLinkCapture current { get; set; } = null;

        private static CoinModeDeepLinkEvent onDeepLinkCaptured;

        public static void AssignOnDeepLinkCaptured(CoinModeDeepLinkEvent onDeepLinkCaptured)
        {
            DeepLinkCapture.onDeepLinkCaptured -= onDeepLinkCaptured;
            DeepLinkCapture.onDeepLinkCaptured += onDeepLinkCaptured;

            if (current != null && current.deepLinkParams != null)
            {
                DeepLinkCapture.onDeepLinkCaptured?.Invoke(current.deepLinkParams);
            }
        }

#if UNITY_EDITOR
        [SerializeField] private string testLink = "";
        [SerializeField] private bool test = false;
#endif

        [SerializeField] private CoinModeDeepLinkUnityEvent onDeepLinkCapturedEvent = null;

        public DeepLinkParams deepLinkParams { get; private set; } = null;

#if CM_TEST_DEEPLINK
        public bool test = false;
        public string testUrl = "";

        private void OnValidate()
        {
            if (test)
            {
                test = false;
                string scheme = null;
                if (ValidateCoinModeDeepLink(testUrl, out scheme))
                {
                    DeepLinkParams testParams = new DeepLinkParams(scheme, testUrl);

                    if(testParams.namedValues.Count > 0)
                    {
                        CoinModeLogging.LogMessage("DeepLinkCapture", "OnValidate", "Named values:");
                        foreach (KeyValuePair<string, string> pair in testParams.namedValues)
                        {
                            CoinModeLogging.LogMessage("DeepLinkCapture", "OnValidate", "Name: {0} Value: {1}", pair.Key, pair.Value);
                        }
                    }
                    else
                    {
                        CoinModeLogging.LogMessage("DeepLinkCapture", "OnValidate", "No named values");
                    }

                    if (testParams.unnamedValues.Count > 0)
                    {
                        CoinModeLogging.LogMessage("DeepLinkCapture", "OnValidate", "Unnamed values:");
                        foreach (string value in testParams.unnamedValues)
                        {
                            CoinModeLogging.LogMessage("DeepLinkCapture", "OnValidate", "Value: {0}", value);
                        }
                    }
                    else
                    {
                        CoinModeLogging.LogMessage("DeepLinkCapture", "OnValidate", "No unnamed values");
                    }
                }
                else
                {
                    CoinModeLogging.LogMessage("DeepLinkCapture", "OnValidate", "Not a valid coinmode url");
                }
            }
        }
#endif
        private void Awake()
        {
            if (current == null)
            {
                current = this;
                Application.deepLinkActivated += OnDeeplinkCaptured_Internal;
#if UNITY_EDITOR                
                if (!string.IsNullOrWhiteSpace(testLink) && test)
                {
                    OnDeeplinkCaptured_Internal(testLink);
                }
#else
                if (!string.IsNullOrWhiteSpace(Application.absoluteURL))
                {
                    OnDeeplinkCaptured_Internal(Application.absoluteURL);
                }              
#endif
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDeeplinkCaptured_Internal(string url)
        {
            string scheme = null;
            if(ValidateCoinModeDeepLink(url, out scheme))
            {
                deepLinkParams = new DeepLinkParams(scheme, url);
                onDeepLinkCaptured?.Invoke(deepLinkParams);
                onDeepLinkCapturedEvent.Invoke(deepLinkParams);
            }            
        }

        private bool ValidateCoinModeDeepLink(string url, out string scheme)
        {
            scheme = "";
            int schemeEndIndex = url.IndexOf("://");
            if (schemeEndIndex >= 1)
            {
                scheme = url.Substring(0, schemeEndIndex);
                if (url.Length >= schemeEndIndex + 11)
                {
                    string cmSubString = url.Substring(schemeEndIndex + 3, 8);
                    return cmSubString == "coinmode";
                }
            }
            return false;
        }
    }
}
#endif