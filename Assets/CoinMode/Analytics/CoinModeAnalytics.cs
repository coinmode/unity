using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_ANALYTICS
using Unity.Services.Analytics;
#endif

namespace CoinMode
{
    public static class Analytics
    {
        [Flags]
        public enum EventType
        {
            None = 0,
            UiClick = 1,
            Quit = 2,
            ClientError = 4,
            ClientWarning = 8,
            UserLogin = 16,
            RequestSuccess = 32,
            RequestFailure = 64,
        }

        public enum RequestType
        {
            Standard,
            Location,
            Oauth
        }

        [Serializable]
        public class Settings
        {
            [Serializable]
            public class SupportedEndpoints
            {
                public static SupportedEndpoints defaultSuccessEndpoints
                {
                    get
                    {
                        return new SupportedEndpoints(
                            new bool[]
                            {
                                false, //0
                                true, //1
                                true, //2
                                true, //3
                                false, //4
                                false, //5
                                false, //6
                                false, //7
                                true, //8
                                true, //9
                                true, //10
                                true, //11
                                false, //12
                                false, //13
                                false, //14
                                false, //15
                                true, //16
                                false, //17
                                true, //18
                                true, //19
                                false, //20
                                false, //21
                                false, //22
                                false, //23
                                true, //24
                                true, //25
                                false, //26
                                false, //27
                            },
                            new bool[Analytics.locationApiEndpoints.Length],
                            new bool[Analytics.oauthApiEndpoints.Length]
                        );
                    }
                }

                public static SupportedEndpoints defaultFailureEndpoints
                {
                    get
                    {
                        return new SupportedEndpoints(
                            new bool[]
                            {
                                true, //0
                                true, //1
                                true, //2
                                true, //3
                                true, //4
                                true, //5
                                true, //6
                                true, //7
                                true, //8
                                true, //9
                                true, //10
                                true, //11
                                true, //12
                                true, //13
                                true, //14
                                true, //15
                                true, //16
                                true, //17
                                true, //18
                                true, //19
                                true, //20
                                true, //21
                                true, //22
                                true, //23
                                true, //24
                                true, //25
                                true, //26
                                true, //27
                            },
                            new bool[]
                            {
                                true, //0
                                true, //1
                                true, //2
                            },
                            new bool[]
                            {
                                true, //0
                                true, //1
                            }
                        );
                    }
                }

                internal SupportedEndpoints(bool[] apiEndpoints, bool[] locationApiEndpoints, bool[] oauthApiEndpoints)
                {
                    this.apiEndpoints = apiEndpoints;
                    this.locationApiEndpoints = locationApiEndpoints;
                    this.oauthApiEndpoints = oauthApiEndpoints;
                }

                [SerializeField]
                private bool[] apiEndpoints = new bool[Analytics.apiEndpoints.Length];
                [SerializeField]
                private bool[] locationApiEndpoints = new bool[Analytics.locationApiEndpoints.Length];
                [SerializeField]
                private bool[] oauthApiEndpoints = new bool[Analytics.oauthApiEndpoints.Length];

                public bool SupportsEndpoint(RequestType type, int endpointId)
                {
                    if (endpointId >= 0)
                    {
                        switch (type)
                        {
                            case RequestType.Standard:
                                if (endpointId < apiEndpoints.Length) return apiEndpoints[endpointId];
                                break;
                            case RequestType.Location:
                                if (endpointId < locationApiEndpoints.Length) return locationApiEndpoints[endpointId];
                                break;
                            case RequestType.Oauth:
                                if (endpointId < oauthApiEndpoints.Length) return oauthApiEndpoints[endpointId];
                                break;
                        }
                    }
                    return false;
                }

#if UNITY_EDITOR
                internal void EditorUpdate(RequestType type, int endpointId, bool value)
                {
                    if (endpointId >= 0)
                    {
                        switch (type)
                        {
                            case RequestType.Standard:
                                if (endpointId < apiEndpoints.Length) apiEndpoints[endpointId] = value;
                                break;
                            case RequestType.Location:
                                if (endpointId < locationApiEndpoints.Length) locationApiEndpoints[endpointId] = value;
                                break;
                            case RequestType.Oauth:
                                if (endpointId < oauthApiEndpoints.Length) oauthApiEndpoints[endpointId] = value;
                                break;
                        }
                    }
                }
#endif
            }

            [SerializeField]
            internal EventType eventsToRecord =
                EventType.UiClick | EventType.Quit | EventType.ClientError | EventType.ClientWarning | EventType.UserLogin | EventType.RequestSuccess | EventType.RequestFailure;

            [SerializeField]
            internal SupportedEndpoints requestSuccessesToRecord = SupportedEndpoints.defaultSuccessEndpoints;
            [SerializeField]
            internal SupportedEndpoints requestFailuresToRecord = SupportedEndpoints.defaultFailureEndpoints;

            [SerializeField]
            internal bool useCustomProvider = false;

            [SerializeField]
            internal bool enabled = true;

            internal bool ShouldCaptureEvent(EventType e)
            {
                return enabled && eventsToRecord.HasFlag(e);
            }

#if UNITY_EDITOR
            internal void EditorSetEventsToRecord(EventType eventsToRecord)
            {
                this.eventsToRecord = eventsToRecord;
            }

            internal void EditorSetUseCustomProvider(bool useCustoms)
            {
                useCustomProvider = useCustoms;
            }

            internal void EditorSetEnabled(bool enabled)
            {
                this.enabled = enabled;
            }
#endif
        }

        internal static readonly string[] apiEndpoints =
        {
            "titles/games/get_info",                        //0
            "titles/games/round/create",                    //1
            "titles/games/round/lock",                      //2
            "titles/games/round/end",                       //3
            "titles/games/round/list",                      //4
            "titles/games/round/get_info",                  //5
            "titles/games/round/get_highscores",            //6
            "titles/games/round/connection/get_info",       //7
            "titles/games/round/connection/set_info",       //8
            "titles/games/round/session/request",           //9
            "titles/games/round/session/start",             //10
            "titles/games/round/session/stop",              //11
            "titles/games/round/session/list",              //12
            "info/get_display_currencies",                  //13
            "info/get_exchange_rates",                      //14
            "licenses/get",                                 //15
            "players/create",                               //16
            "players/get_properties",                       //17
            "players/playtokens/request",                   //18
            "players/playtokens/verify",                    //19
            "players/playtokens/cancel",                    //20
            "players/playtokens/get_token_info",            //21
            "players/wallet/get_deposit_address",           //22
            "players/wallet/get_balances",                  //23
            "players/wallet/transfer_funds/verify",         //24
            "players/wallet/transfer_funds/request",        //25
            "players/wallet/transfer_funds/get_fees",       //26
            "titles/get_info",                              //27
        };

        internal static readonly string[] locationApiEndpoints =
        {
            "list",         //0
            "add",          //1
            "remove",       //2
        };

        internal static readonly string[] oauthApiEndpoints =
        {
            "auth/{service}/user",      //0
            "auth/{service}/url",       //1
        };

        public delegate bool CaptureEventRecorded(EventType type, Dictionary<string, object> eventParams);

        public static CaptureEventRecorded onEventRecorded = null;

                // TODO [Luke] The Oauth path need to be optimized
        private static int EndPointDeclared(RequestType type, string endpoint)
        {
            int index = -1;
            switch (type)
            {
                case RequestType.Standard:
                    index = Array.IndexOf(apiEndpoints, endpoint);
                    break;
                case RequestType.Location:
                    index = Array.IndexOf(locationApiEndpoints, endpoint);
                    break;
                case RequestType.Oauth:
                    {
                        string[] path = endpoint.Split('/');
                        string genericEndpoint = "";
                        for (int i = 0; i < path.Length; i++)
                        {
                            if (i == 1)
                            {
                                genericEndpoint += "{service}";
                            }
                            else
                            {
                                genericEndpoint += path[i];
                            }

                            if (i < path.Length - 1)
                            {
                                genericEndpoint += '/';
                            }
                        }
                        index = Array.IndexOf(oauthApiEndpoints, genericEndpoint);
                    }
                    break;
            }
            if (index == -1)
            {
                CoinModeLogging.LogWarning("Analytics", "EndPointDeclared", "Failed to find endpoint {0} for request type: {1}", endpoint, type.ToString());
            }
            return index;
        }

        private static bool ShouldCaptureEndpoint(Settings.SupportedEndpoints endpoints, RequestType type, string endpoint)
        {
            int index = EndPointDeclared(type, endpoint);
            if (index >= 0)
            {
                return endpoints.SupportsEndpoint(type, index);
            }
            return false;
        }

        internal static void RecordUIClick(Transform target)
        {

            if (CoinModeSettings.analyticsSettings.ShouldCaptureEvent(EventType.UiClick))
            {
#if !UNITY_EDITOR
                CoinMode.UI.CoinModeScreen parentScreen = CoinMode.UI.CoinModeScreen.FindParent(target);
                string screenName = parentScreen != null ? parentScreen.GetType().ToString() : "null";
                Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "cm_ui_click_id", string.Format("Screen: {0}, Object {1}", screenName, target.gameObject.name) }
                };

#if UNITY_ANALYTICS
                Events.CustomData("cm_ui_click", parameters);
#else
                if (!CoinModeSettings.analyticsSettings.useCustomProvider)
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordUIClick", "Expected to record event but custom analytics are disabled");
                }

                if (onEventRecorded == null || !onEventRecorded.Invoke(EventType.UiClick, parameters))
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordUIClick", "Expected to record event but capture returned unsuccessfully");
                }
#endif
#endif
            }
        }

        internal static void RecordQuit(string game_state)
        {
            if (CoinModeSettings.analyticsSettings.ShouldCaptureEvent(EventType.Quit))
            {
#if !UNITY_EDITOR
                Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "cm_quit_state", game_state}
                };
#if UNITY_ANALYTICS
                Events.CustomData("cm_quit", parameters);
#else
                if (!CoinModeSettings.analyticsSettings.useCustomProvider)
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordQuit", "Expected to record event but custom analytics are disabled");
                }

                if (onEventRecorded == null || !onEventRecorded.Invoke(EventType.Quit, parameters))
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordQuit", "Expected to record event but capture returned unsuccessfully");
                }
#endif
#endif
            }
        }

        internal static void RecordUserLogin(PlayerAuthMode mode)
        {
            if (CoinModeSettings.analyticsSettings.ShouldCaptureEvent(EventType.UserLogin))
            {
#if !UNITY_EDITOR
                Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "cm_login_type", mode.ToString()}
                };
#if UNITY_ANALYTICS
                Events.CustomData("cm_login", parameters);
#else
                if (!CoinModeSettings.analyticsSettings.useCustomProvider)
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordUserLogin", "Expected to record event but custom analytics are disabled");
                }

                if (onEventRecorded == null || !onEventRecorded.Invoke(EventType.UserLogin, parameters))
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordUserLogin", "Expected to record event but capture returned unsuccessfully");
                }
#endif
#endif
            }
        }

        internal static void RecordRequestFailure(string apiDir, string error)
        {
            RecordRequestFailure(RequestType.Standard, apiDir, error);
        }

        internal static void RecordLocationRequestFailure(string apiDir, string error)
        {
            RecordRequestFailure(RequestType.Location, apiDir, error);
        }

        internal static void RecordOauthRequestFailure(string apiDir, string error)
        {
            RecordRequestFailure(RequestType.Oauth, apiDir, error);
        }

        internal static void RecordRequestFailure(RequestType type, string apiDir, string error)
        {
            if (CoinModeSettings.analyticsSettings.ShouldCaptureEvent(EventType.RequestFailure) && ShouldCaptureEndpoint(CoinModeSettings.analyticsSettings.requestFailuresToRecord, type, apiDir))
            {
#if !UNITY_EDITOR
                Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "cm_endpoint", apiDir},
                    { "cm_error", error},
                    { "cm_request_type", type.ToString()}
                };
#if UNITY_ANALYTICS
                Events.CustomData("cm_api_error", parameters);
#else
                if (!CoinModeSettings.analyticsSettings.useCustomProvider)
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordRequestFailure", "Expected to record event but custom analytics are disabled");
                }
                else if (onEventRecorded == null || !onEventRecorded.Invoke(EventType.RequestFailure, parameters))
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordRequestFailure", "Expected to record event but capture returned unsuccessfully");
                }
#endif
#endif
            }
        }

        internal static void RecordRequestSuccess(string apiDir)
        {
            RecordRequestSuccess(RequestType.Standard, apiDir);
        }

        internal static void RecordLocationRequestSuccess(string apiDir)
        {
            RecordRequestSuccess(RequestType.Location, apiDir);
        }

        internal static void RecordOauthRequestSuccess(string apiDir)
        {
            RecordRequestSuccess(RequestType.Oauth, apiDir);
        }

        internal static void RecordRequestSuccess(RequestType type, string apiDir)
        {
            if (CoinModeSettings.analyticsSettings.ShouldCaptureEvent(EventType.RequestSuccess) && ShouldCaptureEndpoint(CoinModeSettings.analyticsSettings.requestSuccessesToRecord, type, apiDir))
            {
#if !UNITY_EDITOR
                Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "cm_endpoint", apiDir},
                    { "cm_request_type", type.ToString()}
                };
#if UNITY_ANALYTICS
                Events.CustomData("cm_api_success", parameters);
#else
                if (!CoinModeSettings.analyticsSettings.useCustomProvider)
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordRequestSuccess", "Expected to record event but custom analytics are disabled");
                }
                else if (onEventRecorded == null || !onEventRecorded.Invoke(EventType.RequestSuccess, parameters))
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordRequestSuccess", "Expected to record event but capture returned unsuccessfully");
                }
#endif
#endif
            }
        }

        internal static void RecordError(string error)
        {
            if (CoinModeSettings.analyticsSettings.ShouldCaptureEvent(EventType.ClientError))
            {
#if !UNITY_EDITOR
                Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "cm_error", error},
                };
#if UNITY_ANALYTICS
                Events.CustomData("cm_client_error", parameters);
#else
                if (!CoinModeSettings.analyticsSettings.useCustomProvider)
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordError", "Expected to record event but custom analytics are disabled");
                }
                else if (onEventRecorded == null || !onEventRecorded.Invoke(EventType.ClientError, parameters))
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordError", "Expected to record event but capture returned unsuccessfully");
                }
#endif
#endif
            }
        }

        internal static void RecordWarning(string warning)
        {
            if (CoinModeSettings.analyticsSettings.ShouldCaptureEvent(EventType.ClientWarning))
            {
#if !UNITY_EDITOR
                Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "cm_warning", warning},
                };
#if UNITY_ANALYTICS
                Events.CustomData("cm_client_warning", parameters);
#else
                if (!CoinModeSettings.analyticsSettings.useCustomProvider)
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordWarning", "Expected to record event but custom analytics are disabled");
                }
                else if (onEventRecorded == null || !onEventRecorded.Invoke(EventType.ClientWarning, parameters))
                {
                    CoinModeLogging.LogMessage("Analytics", "RecordWarning", "Expected to record event but capture returned unsuccessfully");
                }
#endif
#endif
            }
        }
    }
}
