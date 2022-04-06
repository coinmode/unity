using LightJson;
using System;
using System.Collections.Generic;

namespace CoinMode
{
    [System.Serializable]
    public class MinimalPlayerInfo : CoinModeProperties
    {
        public string displayName { get; private set; } = null;
        public string playerId { get; private set; } = null;
        
        internal override void FromJson(JsonObject json)
        {
            displayName = json["display_name"];
            playerId = json["player_id"];
        }
    }

    [System.Serializable]
    public class MinimalRoundInfo : CoinModeProperties
    {
        public string roundId { get; private set; } = null;
        public string name { get; private set; } = null;
        public string walletId { get; private set; } = null;
        public int? epochStarted { get; private set; } = null;
        public int? epochLocked { get; private set; } = null;
        public int? epochEnded { get; private set; } = null;
        public int? epochToFinish { get; private set; } = null;
        public double? potContribution { get; private set; } = null;
        public double? winningPot { get; private set; } = null;
        public double? playFee { get; private set; } = null;
        public int? roundTypeId { get; private set; } = null;
        public string roundTypeText { get; private set; } = null;
        public int? statusId { get; private set; } = null;
        public string statusText { get; private set; } = null;
        public int? sessionsRemaining { get; private set; } = null;
        public bool? requirePassphrase { get; private set; } = null;
        public MinimalPlayerInfo[] players { get; private set; } = null;
        public bool? playerCanPlay { get; private set; } = null;
        public bool? hasServer { get; private set; } = null;
        public string serverStatus { get; private set; } = null;
        public string serverIp { get; private set; } = null;
        public string serverPort { get; private set; } = null;
        public bool roundExpired
        {
            get
            {
                if (epochToFinish != null)
                {
                    DateTimeOffset epochNow = new DateTimeOffset(DateTime.UtcNow);
                    return epochNow.ToUnixTimeSeconds() > epochToFinish;
                }
                return false;
            }
        }

        internal override void FromJson(JsonObject json)
        {
            roundId = json["round_id"];
            name = json["name"];
            walletId = json["wallet"];
            epochStarted = json["epoch_round_started"];
            epochLocked = json["epoch_round_locked"];
            epochEnded = json["epoch_round_ended"];
            epochToFinish = json["epoch_to_finish"];
            potContribution = json["pot_contribution"];
            winningPot = json["winning_pot"];
            playFee = json["fee_play_session"];
            roundTypeId = json["round_type_id"];
            roundTypeText = json["round_type_text"];
            statusId = json["status_id"];
            statusText = json["status_text"];
            sessionsRemaining = json["sessions_remaining"];
            requirePassphrase = json["requires_passphrase"];
            JsonArray playerArray = json["players"];
            if (playerArray != null)
            {
                players = new MinimalPlayerInfo[playerArray.Count];
                for(int i = 0; i < playerArray.Count; i++)
                {
                    JsonObject playerObject = playerArray[i].AsJsonObject;
                    if(playerObject != null)
                    {
                        MinimalPlayerInfo playerInfo = new MinimalPlayerInfo();
                        playerInfo.FromJson(playerObject);
                        players[i] = playerInfo;
                    }                    
                }
            }
            playerCanPlay = json["player_can_play"];

            JsonObject connectionInfo = json["connection_info"];
            if (connectionInfo != null)
            {
                hasServer = true;
                serverStatus = connectionInfo["server_status"];
                JsonObject serverInfo = connectionInfo["server_connection_info"];
                if (serverInfo != null)
                {
                    serverIp = serverInfo["ip"];
                    serverPort = serverInfo["port"];
                }
            }
            else
            {
                hasServer = false;
            }
        }
    }

    [System.Serializable]
    public class HighScoreRoundInfo : CoinModeProperties
    {
        public string roundId { get; private set; } = null;
        public string roundName { get; private set; } = null;
        public string gameId { get; private set; } = null;
        public string gameName { get; private set; } = null;
        public string mainImageUrl { get; private set; } = null;
        public string serverUrl { get; private set; } = null;
        public int? statusId { get; private set; } = null;
        public string statusText { get; private set; } = null;
        public int? epochStarted { get; private set; } = null;
        public int? epochEnded { get; private set; } = null;
        public int? epochToFinish { get; private set; } = null;
        public double? winningPot { get; private set; } = null;
        public int? sessionsRemaining { get; private set; } = null;
        public int? roundTypeId { get; private set; } = null;
        public string roundTypeText { get; private set; } = null;
        public double? playFee { get; private set; } = null;
        public double? potContribution { get; private set; } = null;
        public JsonObject customJson { get; private set; } = null;
        public int? numberOfPlayers { get; private set; } = null;

        internal override void FromJson(JsonObject json)
        {
            roundId = json["round_id"];
            roundName = json["round_name"];
            gameId = json["game_id"];
            gameName = json["game_name"];
            mainImageUrl = json["main_image_url"];
            serverUrl = json["server_url"];
            statusId = json["status_id"];
            statusText = json["status_text"];
            epochStarted = json["epoch_round_started"];
            epochToFinish = json["epoch_to_finish"];
            epochEnded = json["epoch_round_ended"];
            winningPot = json["winning_pot"];
            sessionsRemaining = json["sessions_remaining"];
            roundTypeId = json["round_type_id"];
            roundTypeText = json["round_type_text"];
            playFee = json["fee_play_session"];
            potContribution = json["pot_contribution"];
            customJson = json["custom_json"];
            numberOfPlayers = json["number_of_players"];
        }
    }

    [System.Serializable]
    public class RoundInfo : CoinModeProperties
    {
        public string roundId { get; private set; } = null;
        public string roundName { get; private set; } = null;
        public string gameId { get; private set; } = null;
        public string gameName { get; private set; } = null;
        public string mainImageUrl { get; private set; } = null;
        public string serverUrl { get; private set; } = null;
        public int? statusId { get; private set; } = null;
        public string statusText { get; private set; } = null;
        public int? epochStarted { get; private set; } = null;
        public int? epochEnded { get; private set; } = null;
        public int? epochToFinish { get; private set; } = null;
        public double? winningPot { get; private set; } = null;
        public int? sessionsRemaining { get; private set; } = null;
        public int? roundTypeId { get; private set; } = null;
        public string roundTypeText { get; private set; } = null;
        public string walletId { get; private set; } = null;
        public double? playFee { get; private set; } = null;
        public double? potContribution { get; private set; } = null;
        public JsonObject customJson { get; private set; } = null;
        public double? coinModeEarnings { get; private set; } = null;
        public bool? requireLock { get; private set; } = null;
        public bool? playerCanPlay { get; private set; } = null;
        public bool? requirePassphrase { get; private set; } = null;
        public Dictionary<string, AdvertData> advertData { get; private set; } = new Dictionary<string, AdvertData>();
        public bool? hasServer { get; private set; } = null;
        public string serverStatus { get; private set; } = null;
        public string serverIp { get; private set; } = null;
        public string serverPort { get; private set; } = null;
        public int? payoutTypeId { get; private set; } = null;
        public string payoutTypeText { get; private set; } = null;

        internal override void FromJson(JsonObject json)
        {
            roundId = json["round_id"];
            roundName = json["round_name"];
            gameId = json["game_id"];
            gameName = json["game_name"];
            mainImageUrl = json["main_image_url"];
            serverUrl = json["server_url"];
            statusId = json["status_id"];
            statusText = json["status_text"];
            epochStarted = json["epoch_round_started"];
            epochToFinish = json["epoch_to_finish"];
            epochEnded = json["epoch_round_ended"];
            winningPot = json["winning_pot"];
            sessionsRemaining = json["sessions_remaining"];
            roundTypeId = json["round_type_id"];
            roundTypeText = json["round_type_text"];
            walletId = json["wallet"];
            playFee = json["fee_play_session"];
            potContribution = json["pot_contribution"];            
            if (json.ContainsKey("custom_json"))
            {
                if(json["custom_json"].IsString)
                {
                    customJson = JsonValue.Parse(json["custom_json"]).AsJsonObject;
                }
                else if(json["custom_json"].IsJsonObject)
                {
                    customJson = json["custom_json"];
                }
            }            
            coinModeEarnings = json["coinmode_earnings"];
            requireLock = json["require_lock_to_start_round"];            
            playerCanPlay = json["player_can_play"];
            requirePassphrase = json["requires_passphrase"];

            JsonObject advertObject = json["adverts"];
            if(advertObject != null)
            {
                foreach(KeyValuePair<string, JsonValue> pair in advertObject)
                {
                    AdvertData adData = new AdvertData();
                    adData.FromJson(pair.Value);
                    advertData.Add(pair.Key, adData);
                }
            }
            JsonObject connectionInfo = json["connection_info"];
            if(connectionInfo != null)
            {
                hasServer = true;
                serverStatus = connectionInfo["server_status"];
                JsonObject serverInfo = connectionInfo["server_connection_info"];
                if(serverInfo != null)
                {
                    serverIp = serverInfo["ip"];
                    serverPort = serverInfo["port"];
                }
            }
            else
            {
                hasServer = false;
            }
            payoutTypeId = json["round_type_id"];
            payoutTypeText = json["round_type_text"];
        }
    }
}
