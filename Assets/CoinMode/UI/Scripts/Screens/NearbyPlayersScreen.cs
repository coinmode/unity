using CoinMode.NetApi;
using System.Collections.Generic;
using UnityEngine;
using static CoinMode.NetApi.Location;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM NearbyPlayersScreen")]
    public class NearbyPlayersScreen : CoinModeMenuScreen
    {
        public struct ScreenData
        {
            public PlayerComponent player { get; private set; }
            public LocationType type { get; private set; }
            public string lobby { get; private set; }
            public double? longitude { get; private set; }
            public double? latitude { get; private set; }
            public Vector3? xyz { get; private set; }
            public Vector2? xy { get; private set; }
            public double? range { get; private set; }

            public ScreenData(PlayerComponent player, LocationType type, string lobby, double? longitude, double? latitude, Vector3? xyz, Vector2? xy)
            {
                this.player = player;
                this.type = type;
                this.lobby = lobby;
                this.longitude = longitude;
                this.latitude = latitude;
                this.xyz = xyz;
                this.xy = xy;
                this.range = null;
            }

            public ScreenData(PlayerComponent player, LocationType type, string lobby, double? longitude, double? latitude, Vector3? xyz, Vector2? xy, double? range)
            {
                this.player = player;
                this.type = type;
                this.lobby = lobby;
                this.longitude = longitude;
                this.latitude = latitude;
                this.xyz = xyz;
                this.xy = xy;
                this.range = range;
            }
        }

        [SerializeField]
        private PlayerNameEntry playerNameTemplate = null;

        [SerializeField]
        private RectTransform playerContainer = null;

        [SerializeField]
        private CoinModeText noneText = null;

        [SerializeField]
        private CoinModeButton closeButton = null;

        public override bool requiresData { get; } = true;
        private ScreenData screenData = new ScreenData();
        private List<PlayerNameEntry> playerNameEntries = new List<PlayerNameEntry>();        

        protected override void Start()
        {
            base.Start();
            if (playerNameTemplate != null)
            {
                playerNameTemplate.gameObject.SetActive(false);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseAction);
            }
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);
            ClearNames();
            controller.ShowLoading();
            if (noneText != null)
            {
                noneText.gameObject.SetActive(false);
            }
            switch (screenData.type)
            {
                case LocationType.Geolocation:
                    CoinModeManager.SendLocationRequest(ListNearbyPlayers(screenData.longitude, screenData.latitude, screenData.range, OnGetNearbyPlayersSuccess, OnGetNearbyPlayersFailure));
                    break;
                case LocationType.GameLobby:
                    CoinModeManager.SendLocationRequest(ListNearbyPlayers(screenData.lobby, OnGetNearbyPlayersSuccess, OnGetNearbyPlayersFailure));
                    break;
                case LocationType.ThreeDimensional:
                    CoinModeManager.SendLocationRequest(ListNearbyPlayers(screenData.xyz, screenData.range, OnGetNearbyPlayersSuccess, OnGetNearbyPlayersFailure));
                    break;
                case LocationType.TwoDimensional:
                    CoinModeManager.SendLocationRequest(ListNearbyPlayers(screenData.xy, screenData.range, OnGetNearbyPlayersSuccess, OnGetNearbyPlayersFailure));
                    break;
            }         
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        private void OnGetNearbyPlayersSuccess(ListNearbyPlayersResponse response)
        {
            controller.HideLoading();

            foreach (ListNearbyPlayersResponse.PlayerLocation player in response.players)
            {
                PlayerNameEntry display = Instantiate(playerNameTemplate, playerContainer);
                display.gameObject.SetActive(true);
                display.SetInfo(player.displayName, player.playerId, screenData.player);
                playerNameEntries.Add(display);
            }

            if (playerNameEntries.Count == 0)
            {
                if (noneText != null)
                {
                    noneText.gameObject.SetActive(true);
                }
            }
        }

        private void OnGetNearbyPlayersFailure (CoinModeErrorResponse error)
        {
            controller.HideLoading();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        private void ClearNames()
        {
            for (int i = 0; i < playerNameEntries.Count; i++)
            {
                playerNameEntries[i].transform.SetParent(null);
                Destroy(playerNameEntries[i].gameObject);
            }
            playerNameEntries.Clear();
        }

        private void CloseAction()
        {
            controller.ReturnToPreviousScreen();
        }
    }
}
