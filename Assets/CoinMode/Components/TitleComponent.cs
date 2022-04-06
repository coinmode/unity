using CoinMode.NetApi;
using System.Collections.Generic;

namespace CoinMode
{
    public delegate void TitleEvent(TitleComponent title);
    public delegate void TitleFailureEvent(TitleComponent title, CoinModeError error);

    [System.Serializable]
    public class TitleComponent : CoinModeComponent
    {
        public enum TitleState
        {
            Clean,
            RetrievingInfo,
            Ready,
        }

        public string titleId { get; private set; } = "";
        public string name { get; private set; } = "";
        public string description { get; private set; } = "";
        public string imageUrl { get; private set; } = "";
        public bool enabled { get; private set; } = false;
        public double starRatingTally { get; private set; } = 0.0D;
        public int starRatingCount { get; private set; } = 0;
        public string ownerPlayerId { get; private set; } = "";
        public int epochCreated { get; private set; } = 0;
        public int epochUpdated { get; private set; } = 0;

        private TitlePermission[] permissions { get; set; } = new TitlePermission[0];
        private Dictionary<string, GameComponent> games { get; set; } = new Dictionary<string, GameComponent>();

        public TitleState state { get; private set; } = TitleState.Clean;

        private TitleEvent getInfoSuccess = null;
        private TitleFailureEvent getInfoFailure = null;

        internal TitleComponent() { }

        public int GetPermissionsCount()
        {
            return permissions != null ? permissions.Length : 0;
        }

        public TitlePermission GetPermission(int index)
        {
            if(permissions != null && index >= 0 && index < permissions.Length)
            {
                return permissions[index];
            }
            return null;
        }

        internal bool GetTitleInfo(TitleEvent onSuccess, TitleFailureEvent onFailure)
        {
            if(state != TitleState.Clean)
            {
                if (state == TitleState.RetrievingInfo)
                {
                    getInfoSuccess -= onSuccess;
                    getInfoSuccess += onSuccess;

                    getInfoFailure -= onFailure;
                    getInfoFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("TitleComponent", "GetTitleInfo", "Cannot get title info while title is {9}", state.ToString());
                    return false;
                }
            }

            state = TitleState.RetrievingInfo;

            getInfoSuccess = onSuccess;
            getInfoFailure = onFailure;
            CoinModeManager.SendRequest(Titles.GetInfo(CoinModeSettings.titleId, OnGetInfoSuccess, OnGetInfoFailure));
            return true;
        }

        private void OnGetInfoSuccess(Titles.GetInfoResponse response)
        {
            state = TitleState.Ready;

            titleId = response.properties.titleId != null ? response.properties.titleId : "";
            name = response.properties.name != null ? response.properties.name : "";
            description = response.properties.description != null ? response.properties.description : "";
            enabled = response.properties.enabled != null ? response.properties.enabled.Value : false;
            imageUrl = response.properties.imageUrl != null ? response.properties.imageUrl : "";
            starRatingTally = response.properties.starRatingTally != null ? response.properties.starRatingTally.Value : 0.0D;
            starRatingCount = response.properties.starRatingCount != null? response.properties.starRatingCount.Value : 0;
            ownerPlayerId = response.properties.ownerPlayerId != null? response.properties.ownerPlayerId : "";
            epochCreated = response.properties.epochCreated != null ? response.properties.epochCreated.Value : 0;

            if(response.properties.games != null)
            {
                games.Clear();
                foreach(KeyValuePair<string, GameInfo> game in response.properties.games)
                {
                    GameComponent newComponent = new GameComponent(CoinModeSettings.GetLocalGameAlias(game.Value.gameId), game.Value);
                    games.Add(game.Key, newComponent);
                }
            }

            getInfoSuccess?.Invoke(this);
        }

        private void OnGetInfoFailure(CoinModeErrorResponse errorResponse)
        {
            state = TitleState.Clean;
            getInfoFailure?.Invoke(this, new CoinModeError(errorResponse));
        }

        public bool IsGameValid(string localGameAlias)
        {
            return games.ContainsKey(CoinModeSettings.GetGameId(localGameAlias));
        }

        public bool TryGetGame(string localGameAlias, out GameComponent game)
        {
            return games.TryGetValue(CoinModeSettings.GetGameId(localGameAlias), out game);
        }

        public GameComponent TryGetGame(string localGameAlias)
        {
            GameComponent game;
            games.TryGetValue(CoinModeSettings.GetGameId(localGameAlias), out game);
            return game;
        }

        public bool TryGetGameFromId(string gameId, out GameComponent game)
        {
            return games.TryGetValue(gameId, out game);
        }

        public GameComponent TryGetGameFromId(string gameId)
        {
            GameComponent game;
            games.TryGetValue(gameId, out game);
            return game;
        }

        internal bool ConstructPlayer(int localId, out PlayerComponent player)
        {
            player = null;
            if (state == TitleState.Ready)
            {
                player = new PlayerComponent(localId, permissions);
                return true;
            }
            return false;
        }
    }
}
