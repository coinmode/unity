#if UNITY_EDITOR
using System.Collections.Generic;
using CoinMode.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoinMode.Editor
{
    public class CoinModeSettingsEditorWindow : CoinModeEditorWindow
    {
        [SerializeField]
        private List<bool> gameFoldouts = new List<bool>();
        private string[] gameNames = new string[0];

#if UNITY_2019_3_OR_NEWER
        private List<string> challengeGameNames = new List<string>();
        private List<int> challengeGameIndexes = new List<int>();

        private List<string> deepLinkUrlSchemes = new List<string>();
#endif

        private GUIContent gameLocalAliasLabel = new GUIContent("Local Alias", "A unique local alias used throughout the plugin to identify your game and associate CoinMode game id.");
        private GUIContent productionGameIdLabel = new GUIContent("Production", "The unique identifier for this game as assigned on the CoinMode website.");
        private GUIContent stagingGameIdLabel = new GUIContent("Staging", "The unique identifier for this game as assigned on the CoinMode staging website.");

#if UNITY_2019_3_OR_NEWER
        private GUIContent supportsInvitesLabel = new GUIContent("Supports Invites", "Whether or not this game supports inviting players to existing rounds.");
        private GUIContent supportsChallengeLabel = new GUIContent("Supports PvP Challenge", "Whether or not this game supports creating a PvP challenge.");
        private GUIContent useDifferentGameChallengeLabel = new GUIContent("Use Alternative Game", "Whether or not challenges should use this game or a separate game.");
        private GUIContent challengeAliasLabel = new GUIContent("PvP Challenge Game", "The challenge game linked to this game.");
        private GUIContent deepLinkLabel = new GUIContent("Deeplink Url Scheme", "The deeplink url scheme used when sharing the challenge or round.");

        [SerializeField]
        private string tempDeeplinkScheme = "";
        [SerializeField]
        private bool deeplinkUrlSchemesFoldout = false;
#endif

        [SerializeField]
        private bool analyticsSuccessFoldout = false;
        [SerializeField]
        private bool analyticsFailureFoldout = false;

        private Vector2 scrollPos;

        [MenuItem("CoinMode/Settings")]
        static void Open()
        {
            CoinModeSettingsEditorWindow window = (CoinModeSettingsEditorWindow)EditorWindow.GetWindow(typeof(CoinModeSettingsEditorWindow));
            window.Show();
        }      

        protected override void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5.0F);
            GUILayout.Label("Title Environment", headingLabel);
            GUILayout.Space(5.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            GUILayout.Label("Title ID");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30.0F);
            CoinModeSettings.instance.EditorSetProductionTitleID(EditorGUILayout.TextField("Production", CoinModeSettings.instance.EditorGetProductionTitleID()));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30.0F);
            CoinModeSettings.instance.EditorSetStagingTitleID(EditorGUILayout.TextField("Staging", CoinModeSettings.instance.EditorGetStagingTitleID()));
            GUILayout.EndHorizontal();

            GUILayout.Space(2.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            GUILayout.Label("Games");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Add Game"))
            {
                CoinModeSettings.instance.EditorAddGameSetup();
                gameFoldouts.Add(true);
                gameNames = new string[CoinModeSettings.games.Count];
                for (int i = 0; i < CoinModeSettings.games.Count; i++)
                {
                    gameNames[i] = GetGameSetupLabel(CoinModeSettings.games[i], i);
                }
            }
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Remove Game"))
            {
                if(CoinModeSettings.games.Count > 0)
                {
                    CoinModeSettings.instance.EditorRemoveGameSetup();
                    gameFoldouts.RemoveAt(gameFoldouts.Count - 1);

                    gameNames = new string[CoinModeSettings.games.Count];
                    for (int i = 0; i < CoinModeSettings.games.Count; i++)
                    {
                        gameNames[i] = GetGameSetupLabel(CoinModeSettings.games[i], i);
                    }
                }                
            }
            GUILayout.EndHorizontal();

            CoinModeSettings.GameSetup setup;
            for(int i = 0; i < CoinModeSettings.games.Count; i++)
            {
                setup = CoinModeSettings.games[i];
                GUILayout.BeginHorizontal();
                GUILayout.Space(15.0F);
                gameFoldouts[i] = EditorGUILayout.Foldout(gameFoldouts[i], GetGameSetupLabel(CoinModeSettings.games[i], i));
                GUILayout.EndHorizontal();
                if (gameFoldouts[i])
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30.0F);
                    setup.localGameAlias = EditorGUILayout.TextField(gameLocalAliasLabel, CoinModeSettings.games[i].localGameAlias);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30.0F);
                    GUILayout.Label("Game ID");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(45.0F);
                    setup.productionCoinModeGameId = EditorGUILayout.TextField(productionGameIdLabel, CoinModeSettings.games[i].productionCoinModeGameId);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(45.0F);
                    setup.stagingCoinModeGameId = EditorGUILayout.TextField(stagingGameIdLabel, CoinModeSettings.games[i].stagingCoinModeGameId);
                    GUILayout.EndHorizontal();

#if UNITY_2019_3_OR_NEWER
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30.0F);
                    GUILayout.Label("Sharing");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(45.0F);
                    setup.supportsInvites = EditorGUILayout.Toggle(supportsInvitesLabel, CoinModeSettings.games[i].supportsInvites);
                    GUILayout.EndHorizontal();

                    if (CoinModeSettings.games[i].linkedGameIndex == -1)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30.0F);
                        GUILayout.Label("Challenge Setup");
                        GUILayout.EndHorizontal();                        

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(45.0F);
                        setup.supportsPvpChallenge = EditorGUILayout.Toggle(supportsChallengeLabel, CoinModeSettings.games[i].supportsPvpChallenge);
                        GUILayout.EndHorizontal();

                        if(setup.supportsPvpChallenge)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(45.0F);
                            setup.useDifferentGame = EditorGUILayout.Toggle(useDifferentGameChallengeLabel, CoinModeSettings.games[i].useDifferentGame);
                            GUILayout.EndHorizontal();

                            if(setup.useDifferentGame)
                            {
                                BuildChallengeGamesList(i);
                                if(challengeGameIndexes.Count > 0)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(45.0F);
                                    int currentIndex = CoinModeSettings.games[i].pvpChallengeGameIndex;
                                    int indexListId = challengeGameIndexes.IndexOf(currentIndex);
                                    if (indexListId == -1) indexListId = 0;
                                    int newIndex = EditorGUILayout.Popup(challengeAliasLabel, indexListId, challengeGameNames.ToArray());
                                    setup.pvpChallengeGameIndex = challengeGameIndexes[newIndex];
                                    GUILayout.EndHorizontal();
                                }                                
                            }                            
                            else
                            {
                                setup.pvpChallengeGameIndex = -1;
                            }                           
                        }
                        else
                        {
                            setup.useDifferentGame = false;                            
                        }
                    }

                    if(setup.supportsPvpChallenge || setup.supportsInvites)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30.0F);
                        string[] deepLinkPopUp = BuildDeepLinkUrlSchemes();
                        int currentIndex = setup.deeplinkSchemeIndex >= 0 && setup.deeplinkSchemeIndex < deepLinkPopUp.Length - 1 ? setup.deeplinkSchemeIndex + 1 : 0;
                        int newIndex = EditorGUILayout.Popup(deepLinkLabel, currentIndex, BuildDeepLinkUrlSchemes());
                        setup.deeplinkSchemeIndex = newIndex - 1;
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        setup.deeplinkSchemeIndex = -1;
                    }
#endif

                    CoinModeSettings.instance.EditorSetGameSetup(setup, i);
                    gameNames[i] = GetGameSetupLabel(CoinModeSettings.games[i], i);
                }
            }

#if UNITY_2019_3_OR_NEWER
            CoinModeSettings.instance.EditorClearUnlinkedPvpChallengeGames();
#endif

            float originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 256.0F;

            if (CoinModeSettings.games.Count > 1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15.0F);
                CoinModeSettings.instance.EditorSetDefaultGameIdIndex(EditorGUILayout.Popup("Default Game", CoinModeSettings.instance.EditorGetDefaultGameIdIndex(), gameNames));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);           
            CoinModeSettings.instance.EditorSetAllowUserCreatedRoundsForMoney(EditorGUILayout.Toggle("Allow User Created Round For Money", CoinModeSettings.allowUserCreatedRoundsForMoney));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            CoinModeSettings.instance.EditorSetEnvironment((CoinModeEnvironment)EditorGUILayout.EnumPopup("Environment", CoinModeSettings.environment));
            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = originalWidth;

            GUILayout.Space(10.0F);
            GUILayout.Label("Menu", headingLabel);
            GUILayout.Space(5.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            CoinModeSettings.instance.EditorSetReferenceResolution(EditorGUILayout.Vector2Field("Menu Resolution", CoinModeSettings.referenceResolution));
            GUILayout.EndHorizontal();

            GUILayout.Space(5.0F);
            GUILayout.Label("Scene Setup", headingLabel);
            GUILayout.Space(5.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Add Title Login Component"))
            {
                AddTitleLoginComponent();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5.0F);
            GUILayout.Label("Messaging and Logs", headingLabel);
            GUILayout.Space(5.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            CoinModeSettings.instance.EditorSetMenuMessageDisplayMode(
                (CoinModeMenu.MessageDisplayMode)EditorGUILayout.EnumPopup("Menu Message Display", CoinModeSettings.menuMessageDisplayMode));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            CoinModeSettings.instance.EditorSetLoggingMode(
                (CoinModeLogging.LoggingMode)EditorGUILayout.EnumPopup("Logging Mode", CoinModeSettings.loggingMode));
            GUILayout.EndHorizontal();

#if UNITY_2019_3_OR_NEWER
            GUILayout.Space(5.0F);
            GUILayout.Label("Deeplinks", headingLabel);
            GUILayout.Space(5.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Add Deep Link Capture to Scene"))
            {
                AddDeepLinkCapture();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            tempDeeplinkScheme = EditorGUILayout.TextField(tempDeeplinkScheme);

            GUILayout.Space(15.0F);
            if (GUILayout.Button("Add Url Scheme"))
            {
                CoinModeSettings.instance.EditorAddDeepLinkUrlScheme(tempDeeplinkScheme);
                tempDeeplinkScheme = "";
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            deeplinkUrlSchemesFoldout = EditorGUILayout.Foldout(deeplinkUrlSchemesFoldout, "Deeplink Url Schemes");
            GUILayout.EndHorizontal();

            if (deeplinkUrlSchemesFoldout)
            {
                for (int i = 0; i < CoinModeSettings.deeplinkUrlSchemes.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30.0F);
                    GUILayout.Label(CoinModeSettings.deeplinkUrlSchemes[i] + "://coinmode");

                    GUILayout.Space(15.0F);
                    if (GUILayout.Button("Remove Url Scheme"))
                    {
                        CoinModeSettings.instance.EditorRemoveDeepLinkUrlScheme(CoinModeSettings.deeplinkUrlSchemes[i]);
                    }
                    GUILayout.EndHorizontal();
                }
            }
#endif

            GUILayout.Space(5.0F);
            GUILayout.Label("Analytics", headingLabel);
            GUILayout.Space(5.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            CoinModeSettings.analyticsSettings.EditorSetEnabled(EditorGUILayout.Toggle("Enabled", CoinModeSettings.analyticsSettings.enabled));
            GUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!CoinModeSettings.analyticsSettings.enabled);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            CoinModeSettings.analyticsSettings.EditorSetUseCustomProvider(EditorGUILayout.Toggle("Use Custom Provider", CoinModeSettings.analyticsSettings.useCustomProvider));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            CoinModeSettings.analyticsSettings.EditorSetEventsToRecord(
                (Analytics.EventType)EditorGUILayout.EnumFlagsField("Events To Record", CoinModeSettings.analyticsSettings.eventsToRecord));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            analyticsSuccessFoldout = EditorGUILayout.Foldout(analyticsSuccessFoldout, "Successful Requests To Record");
            GUILayout.EndHorizontal();
            if (analyticsSuccessFoldout)
            {
                DrawAnalyticsRequestToRecord(CoinModeSettings.analyticsSettings.requestSuccessesToRecord);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            analyticsFailureFoldout = EditorGUILayout.Foldout(analyticsFailureFoldout, "Failed Requests To Record");
            GUILayout.EndHorizontal();
            if (analyticsFailureFoldout)
            {
                DrawAnalyticsRequestToRecord(CoinModeSettings.analyticsSettings.requestFailuresToRecord);
            }

            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                unsavedChanges = true;
            }

            GUILayout.Space(10.0F);
            GUILayout.Label("Other", headingLabel);
            GUILayout.Space(5.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if(GUILayout.Button("Clear local player cache"))
            {
                ClearPlayerPrefs();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(15.0F);

            EditorGUI.BeginDisabledGroup(!unsavedChanges);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Save changes"))
            {
                SaveObject(true);
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
        }

        protected override void Init()
        {
            base.Init();
            CoinModeSettings.InitSettings();
            objectInContext = CoinModeSettings.instance;

            titleContent.text = "CoinMode Settings";

            gameFoldouts.Clear();
            for(int i = 0; i < CoinModeSettings.games.Count; i++)
            {
                gameFoldouts.Add(false);
            }

            gameNames = new string[CoinModeSettings.games.Count];
            for(int i = 0; i < CoinModeSettings.games.Count; i++)
            {
                gameNames[i] = GetGameSetupLabel(CoinModeSettings.games[i], i);
            }
        }

        private string GetGameSetupLabel(CoinModeSettings.GameSetup game, int index)
        {
            return !string.IsNullOrWhiteSpace(game.localGameAlias) ?
                    game.localGameAlias : !string.IsNullOrWhiteSpace(game.productionCoinModeGameId) ?
                    game.productionCoinModeGameId : !string.IsNullOrWhiteSpace(game.stagingCoinModeGameId) ?
                    game.stagingCoinModeGameId : "Game " + index.ToString();
        }

        private void ClearPlayerPrefs()
        {
            CoinModeManager.ClearPlayerCache();
            CoinModePlayerPrefs.SaveToPlayerPrefs();
        }

        private void AddTitleLoginComponent()
        {
            TitleLoginComponent firstActiveTitleComp = null;
            Canvas firstActiveCanvas = null;
            EventSystem firstActiveEventSystem = null;

            TitleLoginComponent[] titleLoginObjects = Resources.FindObjectsOfTypeAll<TitleLoginComponent>();
            for (int i = 0; i < titleLoginObjects.Length; i++)
            {
                if (titleLoginObjects[i].gameObject.scene.IsValid())
                {
                    firstActiveTitleComp = titleLoginObjects[i];
                    break;
                }
            }

            if(firstActiveTitleComp != null)
            {
                UnityEditor.Selection.activeGameObject = firstActiveTitleComp.gameObject;
                return;
            }

            if(UnityEditor.Selection.activeGameObject != null)
            {
                Canvas selectedCanvas = UnityEditor.Selection.activeGameObject.GetComponentInChildren<Canvas>();
                if(selectedCanvas != null)
                {
                    TitleLoginComponent titleComponentPrefab = (TitleLoginComponent)AssetDatabase.LoadAssetAtPath
                        ("Assets/CoinMode/UI/Prefabs/Components/TitleLogin/TitleLoginComponent.prefab", typeof(TitleLoginComponent));
                    if (titleComponentPrefab != null)
                    {
                        TitleLoginComponent titleComp = Instantiate(titleComponentPrefab, selectedCanvas.transform);
                        if (titleComp != null)
                        {
                            titleComp.gameObject.SetActive(true);
                            return;
                        }
                    }
                }
            }

            Canvas[] canvasObjects = Resources.FindObjectsOfTypeAll<Canvas>();
            for (int i = 0; i < canvasObjects.Length; i++)
            {
                if (canvasObjects[i].gameObject.scene.IsValid())
                {
                    firstActiveCanvas = canvasObjects[i];
                    break;
                }
            }

            EventSystem[] eventSystemObjects = Resources.FindObjectsOfTypeAll<EventSystem>();
            for (int i = 0; i < eventSystemObjects.Length; i++)
            {
                if (eventSystemObjects[i].gameObject.scene.IsValid())
                {
                    firstActiveEventSystem = eventSystemObjects[i];
                    break;
                }
            }

            if (firstActiveCanvas == null)
            {
                GameObject canvasGo = new GameObject("Canvas");

                firstActiveCanvas = canvasGo.AddComponent<Canvas>();
                firstActiveCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                firstActiveCanvas.sortingOrder = 0;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();                
            }

            if(firstActiveEventSystem == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }

            {
                TitleLoginComponent titleComponentPrefab = (TitleLoginComponent)AssetDatabase.LoadAssetAtPath
                    ("Assets/CoinMode/UI/Prefabs/Components/TitleLogin/TitleLoginComponent.prefab", typeof(TitleLoginComponent));
                if (titleComponentPrefab != null)
                {
                    TitleLoginComponent titleComp = Instantiate(titleComponentPrefab, firstActiveCanvas.transform);
                    if (titleComp != null)
                    {
                        titleComp.gameObject.SetActive(true);
                    }
                }
            }
        }

#if UNITY_2019_3_OR_NEWER
        private void AddDeepLinkCapture()
        {
            DeepLinkCapture firstActiveDeepLinkCapture = null;

            DeepLinkCapture[] deepLinkCaptureObjects = Resources.FindObjectsOfTypeAll<DeepLinkCapture>();
            for (int i = 0; i < deepLinkCaptureObjects.Length; i++)
            {
                if (deepLinkCaptureObjects[i].gameObject.scene.IsValid())
                {
                    firstActiveDeepLinkCapture = deepLinkCaptureObjects[i];
                    break;
                }
            }

            if (firstActiveDeepLinkCapture != null)
            {
                UnityEditor.Selection.activeGameObject = firstActiveDeepLinkCapture.gameObject;
                return;
            }

            GameObject deepLinkGo = new GameObject("CoinMode DeepLinkCapture");
            deepLinkGo.AddComponent<DeepLinkCapture>();
        }
#endif

        private void DrawAnalyticsRequestToRecord(Analytics.Settings.SupportedEndpoints endpoints)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30.0F);
            GUILayout.Label("Standard API");
            GUILayout.EndHorizontal();

            for (int i = 0; i < Analytics.apiEndpoints.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(45.0F);
                endpoints.EditorUpdate(Analytics.RequestType.Standard, i, 
                    GUILayout.Toggle(endpoints.SupportsEndpoint(Analytics.RequestType.Standard, i), Analytics.apiEndpoints[i], GUILayout.ExpandWidth(true)));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(30.0F);
            GUILayout.Label("Location API");
            GUILayout.EndHorizontal();

            for (int i = 0; i < Analytics.locationApiEndpoints.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(45.0F);
                endpoints.EditorUpdate(Analytics.RequestType.Location, i,
                    GUILayout.Toggle(endpoints.SupportsEndpoint(Analytics.RequestType.Location, i), Analytics.locationApiEndpoints[i], GUILayout.ExpandWidth(true)));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(30.0F);
            GUILayout.Label("OAuth API");
            GUILayout.EndHorizontal();

            for (int i = 0; i < Analytics.oauthApiEndpoints.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(45.0F);
                endpoints.EditorUpdate(Analytics.RequestType.Oauth, i,
                    GUILayout.Toggle(endpoints.SupportsEndpoint(Analytics.RequestType.Oauth, i), Analytics.oauthApiEndpoints[i], GUILayout.ExpandWidth(true)));
                GUILayout.EndHorizontal();
            }
        }

#if UNITY_2019_3_OR_NEWER
        private void BuildChallengeGamesList(int gameInContextId)
        {
            challengeGameNames.Clear();
            challengeGameIndexes.Clear();
            challengeGameNames.Add("None");
            challengeGameIndexes.Add(-1);
            for (int i = 0; i < CoinModeSettings.games.Count; i++)
            {
                if(i != gameInContextId && !CoinModeSettings.games[i].supportsPvpChallenge)
                {                    
                    challengeGameNames.Add(gameNames[i]);
                    challengeGameIndexes.Add(i);
                }
            }
        }

        private string[] BuildDeepLinkUrlSchemes()
        {
            deepLinkUrlSchemes.Clear();
            deepLinkUrlSchemes.Add("None");
            for (int i = 0; i < CoinModeSettings.deeplinkUrlSchemes.Count; i++)
            {
                deepLinkUrlSchemes.Add(CoinModeSettings.deeplinkUrlSchemes[i]);
            }
            return deepLinkUrlSchemes.ToArray();
        }
#endif
    }
}
#endif