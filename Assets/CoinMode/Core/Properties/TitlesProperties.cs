using LightJson;
using System.Collections.Generic;

namespace CoinMode
{
    [System.Serializable]
    public class TitlePermission : CoinModeProperties
    {
        public string permissionId { get; private set; } = null;
        public string title { get; private set; } = null;
        public string description { get; private set; } = null;        

        internal TitlePermission() { }

        internal override void FromJson(JsonObject json)
        {
            permissionId = json["permission_id"];
            description = json["description"];

            // Replace with regex
            if(permissionId != null)
            {
                string[] strings = permissionId.Split('_');
                for(int i = 0; i < strings.Length; i++)
                {
                    char[] chars = strings[i].ToCharArray();
                    if(chars.Length > 0)
                    {
                        chars[0] = char.ToUpper(chars[0]);
                        strings[i] = new string(chars);
                    }
                }
                title = string.Join(" ", strings);
            }
        }
    }

    [System.Serializable]
    public class TitleInfo : CoinModeProperties
    {
        public string titleId { get; private set; } = null;
        public string name { get; private set; } = null;
        public string description { get; private set; } = null;
        public bool? enabled { get; private set; } = null;
        public string imageUrl { get; private set; } = null;
        public double? starRatingTally { get; private set; } = null;
        public int? starRatingCount { get; private set; } = null;
        public string ownerPlayerId { get; private set; } = null;
        public int? epochCreated { get; private set; } = null;
        public int? epochUpdated { get; private set; } = null;
        public TitlePermission[] permissions { get; private set; } = null;
        public Dictionary<string, GameInfo> games { get; private set; } = null;

        internal TitleInfo() { }

        internal override void FromJson(JsonObject json)
        {
            titleId = json["title_id"];
            JsonObject titleDetails = json["title_details"];
            if(titleDetails != null)
            {
                name = titleDetails["title_name"];
                description = titleDetails["title_description"];
                enabled = titleDetails["title_enabled"];
                imageUrl = titleDetails["title_image_url"];
                starRatingTally = titleDetails["title_star_rating_tally"];
                starRatingCount = titleDetails["title_star_rating_count"];
                ownerPlayerId = titleDetails["title_owner_player_id"];
                epochCreated = titleDetails["epoch_created"];
                epochUpdated = titleDetails["epoch_updated"];
                JsonArray titlePermissions = titleDetails["title_permissions"];
                if(titlePermissions != null)
                {
                    permissions = new TitlePermission[titlePermissions.Count];                    
                    for (int i = 0; i < titlePermissions.Count; i++)
                    {
                        JsonObject permissionObject = titlePermissions[i];
                        if (permissionObject != null)
                        {
                            TitlePermission newPermissions = new TitlePermission();
                            newPermissions.FromJson(permissionObject);
                            permissions[i] = newPermissions;
                        }                        
                    }
                }
            }
            JsonArray titleGames = json["games"];
            if(titleGames != null)
            {
                games = new Dictionary<string, GameInfo>();
                for(int i = 0; i < titleGames.Count; i++)
                {
                    JsonObject gameObject = titleGames[i];
                    if (gameObject != null)
                    {
                        GameInfo gameProperties = new GameInfo();
                        gameProperties.FromJson(gameObject);
                        if(gameProperties.gameId != null)
                        {
                            games.Add(gameProperties.gameId, gameProperties);
                        }
                    }
                }
            }
        }
    }
}
