using LightJson;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode
{
    public class CoinModePlayerPrefs
    {
        public class CachedPlayerData
        {
            public string uuid = "";
            public string authMode = "";
            public string publicId = "";
            public string playToken = "";
            public string displayName = "";

            internal JsonObject ToJson()
            {
                JsonObject json = new JsonObject();
                json["uuid"] = uuid;
                json["publicId"] = publicId;
                json["playToken"] = playToken;
                json["displayName"] = displayName;
                json["authMode"] = authMode;
                return json;
            }

            internal void FromJson (JsonObject json)
            {
                uuid = json["uuid"];
                publicId = json["publicId"];
                if(json.ContainsKey("playToken"))
                {
                    playToken = json["playToken"];
                }
                else if (json.ContainsKey("loginToken"))
                {
                    playToken = json["loginToken"];
                }                
                displayName = json["displayName"];
                if(json.ContainsKey("authMode"))
                {
                    authMode = json["authMode"];
                }
                else 
                {
                    authMode = PlayerAuthMode.UuidOrEmail.ToString();
                }
            }
        }

        // Just here to handle old legacy data, can be removed in future versions when we are sure everyones cache has been cleared!
        private const string legacyPlayerPrefsKey = "CoinModeCache";
        private const string legacyEncryptedPrefsKey = "CoinModeCacheE";

        private const string productionPlayerPrefsKey = "CoinModeProductionCache";
        private const string stagingPlayerPrefsKey = "CoinModeStagingCache";

        private const string productionEncryptedPrefsKey = "CoinModeProductionCacheE";
        private const string stagingEncryptedPrefsKey = "CoinModeStagingCacheE";

        private static string playerPrefsKey
        {
            get
            {
                switch (CoinModeSettings.environment)
                {
                    case CoinModeEnvironment.Production:
                        return productionPlayerPrefsKey;
                    case CoinModeEnvironment.Staging:
                        return stagingPlayerPrefsKey;
                }
                return "";
            }
        }

        private static string playerPrefsEncryptedKey
        {
            get
            {
                switch (CoinModeSettings.environment)
                {
                    case CoinModeEnvironment.Production:
                        return productionEncryptedPrefsKey;
                    case CoinModeEnvironment.Staging:
                        return stagingEncryptedPrefsKey;
                }
                return "";
            }
        }

        private static CoinModePlayerPrefs instance = null;

        public static bool SaveToPlayerPrefs()
        {
            if(instance != null)
            {
                string encryptedJson = CoinModeEncryption.EncryptString(instance.ToJsonString());
                PlayerPrefs.SetString(playerPrefsKey, encryptedJson);
                PlayerPrefs.SetInt(playerPrefsEncryptedKey, 1);
                return true;
            }
            return false;
        }

        public static bool LoadFromPlayerPrefs()
        {
            // Deleting old cache
            if(PlayerPrefs.HasKey(legacyPlayerPrefsKey))
            {
                PlayerPrefs.DeleteKey(legacyPlayerPrefsKey);
            }

            if (PlayerPrefs.HasKey(legacyEncryptedPrefsKey))
            {
                PlayerPrefs.DeleteKey(legacyEncryptedPrefsKey);
            }

            instance = new CoinModePlayerPrefs();
            if (PlayerPrefs.HasKey(playerPrefsKey))
            {
                if(PlayerPrefs.GetInt(playerPrefsEncryptedKey, 0) == 1)
                {
                    try
                    {
                        string encryptedJson = PlayerPrefs.GetString(playerPrefsKey);
                        instance.FromJsonString(CoinModeEncryption.DecryptString(encryptedJson));
                    }
                    catch(System.Exception e)
                    {
                        CoinModeLogging.LogWarning("CoinModePlayerPrefs", "LoadFromPlayerPrefs", "Failed to decrypt or parse encrypted coin mode cache, clearing cache: {0}", e.Message);
                        PlayerPrefs.DeleteKey(playerPrefsKey);
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        instance.FromJsonString(PlayerPrefs.GetString(playerPrefsKey));
                    }        
                    catch (System.Exception e1)
                    {
                        CoinModeLogging.LogWarning("CoinModePlayerPrefs", "LoadFromPlayerPrefs", "Failed to parse coin mode cache, trying to force decrypt: {0}", e1.Message);
                        try
                        {
                            string encryptedJson = PlayerPrefs.GetString(playerPrefsKey);
                            instance.FromJsonString(CoinModeEncryption.DecryptString(encryptedJson));
                            PlayerPrefs.SetInt(playerPrefsEncryptedKey, 1);
                        }
                        catch (System.Exception e2)
                        {
                            CoinModeLogging.LogWarning("CoinModePlayerPrefs", "LoadFromPlayerPrefs", "Failed to decrypt or parse coin mode cache, clearing cache: {0}", e2.Message);
                            PlayerPrefs.DeleteKey(playerPrefsKey);
                            return false;
                        }                        
                    }
                }
                return true;
            }
            return false;
        }

        public static bool AddCachedPlayerData(string uuid, CachedPlayerData cache)
        {
            if (instance != null)
            {
                int index = GetCachedPlayerDataIndex(uuid);
                if (index >= 0)
                {
                    instance.cachedPlayers[index] = cache;
                }
                else
                {
                    instance.cachedPlayers.Add(cache);
                }
                return true;
            }
            return false;
        }

        public static bool AddCachedPlayToken(string uuid, string playToken)
        {
            if(instance != null)
            {
                CachedPlayerData cache;
                if (TryGetCachedPlayerData(uuid, out cache))
                {
                    cache.playToken = playToken;
                }
                else
                {
                    cache = new CachedPlayerData();
                    cache.uuid = uuid;
                    cache.playToken = playToken;
                    instance.cachedPlayers.Add(cache);
                }
                return true;
            }
            return false;
        }

        public static bool AddCachedDisplayName(string uuid, string displayName)
        {
            if (instance != null)
            {
                CachedPlayerData cache;
                if (TryGetCachedPlayerData(uuid, out cache))
                {
                    cache.displayName = displayName;
                }
                else
                {
                    cache = new CachedPlayerData();
                    cache.uuid = uuid;
                    cache.displayName = displayName;
                    instance.cachedPlayers.Add(cache);
                    return true;
                }
            }
            return false;
        }

        public static bool AddCachedPublicId(string uuid, string publicId)
        {
            if (instance != null)
            {
                CachedPlayerData cache;
                if (TryGetCachedPlayerData(uuid, out cache))
                {
                    cache.publicId = publicId;
                }
                else
                {
                    cache = new CachedPlayerData();
                    cache.uuid = uuid;
                    cache.publicId = publicId;
                    instance.cachedPlayers.Add(cache);
                }
                return true;
            }
            return false;
        }

        public static bool RemoveCachedPlayer(string uuid)
        {
            if (instance != null)
            {
                CachedPlayerData cache;
                if (TryGetCachedPlayerData(uuid, out cache))
                {
                    instance.cachedPlayers.Remove(cache);
                    return true;
                }
            }
            return false;
        }

        public static string GetLastLoggedInPlayer()
        {
            if(instance != null)
            {
                return instance.lastLoggedInPlayer;
            }
            return "";
        }

        public static void SetLastLoggedInPlayer(string uuidOrEmail)
        {
            if (instance != null)
            {
                instance.lastLoggedInPlayer = uuidOrEmail;
            }
        }

        public static bool TryGetCachedPlayerData(string uuid, out CachedPlayerData cache)
        {
            bool found = false;
            cache = null;
            if (instance != null)
            {
                for (int i = 0; i < instance.cachedPlayers.Count; i++)
                {
                    if (instance.cachedPlayers[i].uuid == uuid)
                    {
                        found = true;
                        cache = instance.cachedPlayers[i];
                        break;
                    }
                }
            }
            return found;
        }

        public static bool TryGetCachedPlayToken(string uuid, out string playToken)
        {
            CachedPlayerData cache;
            playToken = "";
            if (TryGetCachedPlayerData(uuid, out cache))
            {
                playToken = cache.playToken;
                return true;
            }
            return false;
        }

        public static bool ClearCache()
        {
            if (instance != null)
            {
                instance.cachedPlayers.Clear();
                instance.lastLoggedInPlayer = "";
                return true;
            }
            return false;
        }

        private static int GetCachedPlayerDataIndex(string uuid)
        {
            if(instance != null)
            {
                for (int i = 0; i < instance.cachedPlayers.Count; i++)
                {
                    if (instance.cachedPlayers[i].uuid == uuid)
                    {
                        return i;
                    }
                }
            }            
            return -1;
        }

        private string lastLoggedInPlayer = "";
        private List<CachedPlayerData> cachedPlayers = new List<CachedPlayerData>();

        private string ToJsonString()
        {
            JsonObject json = new JsonObject();
            json["lastLoggedInPlayer"] = lastLoggedInPlayer;
            JsonArray jsonPlayers = new JsonArray();
            for(int i = 0; i < cachedPlayers.Count; i++)
            {
                jsonPlayers.Add(cachedPlayers[i].ToJson());
            }
            json["cachedPlayers"] = jsonPlayers;
            return json.ToString();
        }

        private void FromJsonString(string jsonString)
        {
            JsonObject json = JsonValue.Parse(jsonString);
            lastLoggedInPlayer = json["lastLoggedInPlayer"];
            JsonArray jsonPlayers = json["cachedPlayers"];
            cachedPlayers.Clear();
            for(int i = 0; i < jsonPlayers.Count; i++)
            {
                CachedPlayerData p = new CachedPlayerData();
                p.FromJson(jsonPlayers[i]);
                cachedPlayers.Add(p);
            }
        }
    }
}
