using LightJson;

namespace CoinMode
{
    [System.Serializable]
    public class HighScoreSessionInfo : CoinModeProperties
    {
        public string playerId { get; private set; } = null;
        public string displayName { get; private set; } = null;
        public string avatarImageUrlSmall { get; private set; } = null;
        public double? score { get; private set; } = null;
        public string formattedScore { get; private set; } = null;
        public int? statusId { get; private set; } = null;
        public string statusText { get; private set; } = null;
        public JsonObject clientResults { get; private set; } = null;
        public JsonObject serverResults { get; private set; } = null;
        public double? paidOut { get; private set; } = null;
        public int? position { get; private set; } = null;
        public int? sessionCreatedEpoch { get; private set; } = null;
        public int? sessionStartedEpoch { get; private set; } = null;
        public int? sessionEndedEpoch { get; private set; } = null;
        public bool? selected { get; private set; } = null;


        internal override void FromJson(JsonObject json)
        {
            playerId = json["player_id"];
            displayName = json["player_name"];
            avatarImageUrlSmall = json["avatar_image_url_small"];
            score = json["score"];
            formattedScore = json["formatted_score"];
            statusId = json["status_id"];
            statusText = json["status_text"];
            if(json["results_from_client"].AsJsonObject != null)
            {
                clientResults = json["results_from_client"];
            }
            else if(!string.IsNullOrWhiteSpace(json["results_from_client"].AsString))
            {
                clientResults = JsonValue.Parse(json["results_from_client"].AsString);
            }            
            serverResults = json["results_from_server"];
            // session_paid_out take priority, this should be removed when 
            // field is resolved in api / database
            if (json.ContainsKey("session_paid_out"))
            {
                paidOut = json["session_paid_out"];
            }
            else
            {
                paidOut = json["paid_out"];
            }
            position = json["position"];
            sessionCreatedEpoch = json["session_created"];
            sessionStartedEpoch = json["session_started"];
            sessionEndedEpoch = json["session_ended"];
            selected = json["selected"];
        }
    }

    [System.Serializable]
    public class SessionInfo : CoinModeProperties
    {
        public string sessionId { get; private set; } = null;
        public int? roundStatus { get; private set; } = null;
        public int? sessionStatus { get; private set; } = null;
        public string gameId { get; private set; } = null;
        public double? score { get; private set; } = null;
        public string roundId { get; private set; } = null;
        public string playerId { get; private set; } = null;
        public string titleName { get; private set; } = null;
        public string roundName { get; private set; } = null;
        public string titleId { get; private set; } = null;
        public string titleImageUrl { get; private set; } = null;
        public double? paidIn { get; private set; } = null;
        public double? paidOut { get; private set; } = null;
        public JsonObject clientResults { get; private set; } = null;
        public JsonObject serverResults { get; private set; } = null;    
        public string serverUrl { get; private set; } = null;
        public int? sessionEndedEpoch { get; private set; } = null;
        public int? sessionCreatedEpoch { get; private set; } = null;
        public int? sessionStartedEpoch { get; private set; } = null;        


        internal override void FromJson(JsonObject json)
        {
            sessionId = json["session_id"];
            roundStatus = json["round_status_id"];
            sessionStatus = json["session_status_id"];
            gameId = json["game_id"];
            score = json["score"];
            roundId = json["round_id"];
            playerId = json["player_id"];
            titleName = json["title_name"];
            roundName = json["round_name"];
            titleId = json["title_id"];
            titleImageUrl = json["title_image_url"];
            paidIn = json["paid_in"];
            // session_paid_out take priority, this should be removed when 
            // field is resolved in api / database
            if (json.ContainsKey("session_paid_out"))
            {
                paidOut = json["session_paid_out"];
            }
            else
            {
                paidOut = json["paid_out"];
            }
            clientResults = json["results_from_client"];
            serverResults = json["results_from_server"];
            serverUrl = json["server_url"];
            sessionEndedEpoch = json["session_ended"];
            sessionCreatedEpoch = json["session_created"];
            sessionStartedEpoch = json["session_started"];            
        }
    }
}
