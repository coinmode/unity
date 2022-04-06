using LightJson;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode.NetApi
{
    public static class Location
    {
        private static readonly string[] LocationTypes =
        {
            "geolocation",
            "2d",
            "3d",
            "lobby"
        };

        public delegate void ListNearbyPlayersSuccess(ListNearbyPlayersResponse response);

        public static ListNearbyPlayersRequest ListNearbyPlayers(string lobby, ListNearbyPlayersSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new ListNearbyPlayersRequest("lobby", lobby, null, null, null, null, null, null, null, onSuccess, onFailure);
        }

        public static ListNearbyPlayersRequest ListNearbyPlayers(double? longitude, double? latitude, double? range, ListNearbyPlayersSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new ListNearbyPlayersRequest("geolocation", null, longitude, latitude, range, null, null, null, null, onSuccess, onFailure);
        }

        public static ListNearbyPlayersRequest ListNearbyPlayers(Vector3? location, double? range, ListNearbyPlayersSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            double? x = null, y = null, z = null;
            if(location != null)
            {
                x = location.Value.x;
                y = location.Value.y;
                z = location.Value.z;
            }
            return new ListNearbyPlayersRequest("3d", null, null, null, null, x, y, z, range, onSuccess, onFailure);
        }

        public static ListNearbyPlayersRequest ListNearbyPlayers(Vector2? location, double? range, ListNearbyPlayersSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            double? x = null, y = null;
            if (location != null)
            {
                x = location.Value.x;
                y = location.Value.y;
            }
            return new ListNearbyPlayersRequest("2d", null, null, null, null, x, y, null, range, onSuccess, onFailure);
        }

        public class ListNearbyPlayersRequest : CoinModeLocationRequest<ListNearbyPlayersResponse>
        {
            private ListNearbyPlayersSuccess onRequestSuccess;            

            internal ListNearbyPlayersRequest(string type, string world, double? longitude, double? latitude, double? rangeMeters, double? x, double? y, double? z,
                double? maxDistance, ListNearbyPlayersSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "list";                

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("type", type);
                requestJson.AddIfNotNull("world", world);
                requestJson.AddIfNotNull("longitude", longitude);
                requestJson.AddIfNotNull("latitude", latitude);
                requestJson.AddIfNotNull("range_meters", rangeMeters);
                requestJson.AddIfNotNull("x", x);
                requestJson.AddIfNotNull("y", y);
                requestJson.AddIfNotNull("z", z);
                requestJson.AddIfNotNull("max_distance", maxDistance);

            }

            protected override ListNearbyPlayersResponse ConstructSuccessResponse()
            {
                return new ListNearbyPlayersResponse();
            }

            protected override void RequestSuccess(ListNearbyPlayersResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class ListNearbyPlayersResponse : CoinModeResponse
        {
            public class PlayerLocation : CoinModeProperties
            {
                public string playerId { get; private set; } = null;
                public string displayName { get; private set; } = null;
                public string type { get; private set; } = null;
                public double? amount { get; private set; } = null;
                public string walletId { get; private set; } = null;
                public int? epochCreated { get; private set; } = null;
                public int? epochExpires { get; private set; } = null;
                public string avatarUrlSmall { get; private set; } = null;
                public string avatarUrlLarge { get; private set; } = null;
                public int? trust { get; private set; } = null;

                internal override void FromJson(JsonObject json)
                {
                    playerId = json["player_id"];
                    displayName = json["display_name"];
                    type = json["type"];
                    amount = json["amount"];
                    walletId = json["wallet"];
                    epochCreated = json["epoch_created"];
                    epochExpires = json["epoch_expires"];
                    avatarUrlSmall = json["avatar_image_url_small"];
                    avatarUrlLarge = json["avatar_image_url_large"];
                    trust = json["trust"];
                }
            }

            public PlayerLocation[] players { get; private set; } = null;
            public double? range { get; private set; } = null;

            internal ListNearbyPlayersResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                JsonArray playerArray = json["found"];
                if(playerArray != null)
                {
                    players = new PlayerLocation[playerArray.Count];
                    for(int i = 0; i < playerArray.Count; i++)
                    {
                        PlayerLocation p = new PlayerLocation();
                        p.FromJson(playerArray[i]);
                        players[i] = p;
                    }
                }
                range = json["range_meters"];
            }
        }

        public delegate void AddPlayerLocationSuccess(AddPlayerLocationResponse response);

        public static AddPlayerLocationRequest AddPlayerLocation(string playerId, string lobby, AddPlayerLocationSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new AddPlayerLocationRequest(playerId,"lobby", lobby,null, null, null, null, null, null, null, null, null, null, onSuccess, onFailure);
        }

        public static AddPlayerLocationRequest AddPlayerLocation(string playerId, double? longitude, double? latitude, double? range, AddPlayerLocationSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new AddPlayerLocationRequest(playerId, "geolocation", null, longitude, latitude, null, null, null, range, null, null, null, null, onSuccess, onFailure);
        }

        public static AddPlayerLocationRequest AddPlayerLocation(string playerId, Vector3? location, double? range, AddPlayerLocationSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            double? x = null, y = null, z = null;
            if (location != null)
            {
                x = location.Value.x;
                y = location.Value.y;
                z = location.Value.z;
            }
            return new AddPlayerLocationRequest(playerId, "3d", null, null, null, x, y, z, null, null, null, null, null, onSuccess, onFailure);
        }

        public static AddPlayerLocationRequest AddPlayerLocation(string playerId, Vector2? location, double? range, AddPlayerLocationSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            double? x = null, y = null;
            if (location != null)
            {
                x = location.Value.x;
                y = location.Value.y;
            }
            return new AddPlayerLocationRequest(playerId, "2d", null, null, null, x, y, null, null, null, null, null, null, onSuccess, onFailure);
        }

        public class AddPlayerLocationRequest : CoinModeLocationRequest<AddPlayerLocationResponse>
        {
            private AddPlayerLocationSuccess onRequestSuccess;

            internal AddPlayerLocationRequest(string playerId, string type, string world, double? longitude, double? latitude, double? x, double? y, double? z, double? rangeMeters,
                string walletId, string invoiceId, double? amount, double? expiresSecnds, AddPlayerLocationSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "add";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("player_id", playerId);
                requestJson.AddIfNotNull("type", type);
                requestJson.AddIfNotNull("world", world);
                requestJson.AddIfNotNull("longitude", longitude);
                requestJson.AddIfNotNull("latitude", latitude);
                requestJson.AddIfNotNull("x", x);
                requestJson.AddIfNotNull("y", y);
                requestJson.AddIfNotNull("z", z);
                requestJson.AddIfNotNull("range_meters", rangeMeters);
                requestJson.AddIfNotNull("wallet", walletId);
                requestJson.AddIfNotNull("invoice_id", invoiceId);
                requestJson.AddIfNotNull("amount", amount);
                requestJson.AddIfNotNull("expires_seconds", expiresSecnds);
            }

            protected override AddPlayerLocationResponse ConstructSuccessResponse()
            {
                return new AddPlayerLocationResponse();
            }

            protected override void RequestSuccess(AddPlayerLocationResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class AddPlayerLocationResponse : CoinModeResponse
        {
            public string playerId { get; private set; } = null;
            public string type { get; private set; } = null;
            public string world { get; private set; } = null;
            public double? longitude { get; private set; } = null;
            public double? latitude { get; private set; } = null;
            public double? x { get; private set; } = null;
            public double? y { get; private set; } = null;
            public double? z { get; private set; } = null;
            public double? range { get; private set; } = null;
            public double? amount { get; private set; } = null;
            public string walletId { get; private set; } = null;
            public int? epochCreated { get; private set; } = null;
            public int? epochExpires { get; private set; } = null;

            internal AddPlayerLocationResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                playerId = json["player_id"];
                type = json["type"];
                world = json["world"];
                longitude = json["longitude"];
                latitude = json["latitude"];
                x = json["x"];
                y = json["y"];
                z = json["z"];
                range = json["range_meters"];
                amount = json["amount"];
                walletId = json["wallet"];
                epochCreated = json["epoch_created"];
                epochExpires = json["epoch_expires"];
            }
        }

        public delegate void RemovePlayerLocationSuccess(RemovePlayerLocationResponse response);

        public static RemovePlayerLocationRequest RemovePlayerLocation(string playerId, RemovePlayerLocationSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new RemovePlayerLocationRequest(playerId, onSuccess, onFailure);
        }

        public class RemovePlayerLocationRequest : CoinModeLocationRequest<RemovePlayerLocationResponse>
        {
            private RemovePlayerLocationSuccess onRequestSuccess;

            internal RemovePlayerLocationRequest(string playerId, RemovePlayerLocationSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "remove";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("player_id", playerId);
            }

            protected override RemovePlayerLocationResponse ConstructSuccessResponse()
            {
                return new RemovePlayerLocationResponse();
            }

            protected override void RequestSuccess(RemovePlayerLocationResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class RemovePlayerLocationResponse : CoinModeResponse
        {
            public int? removed { get; private set; } = null;

            internal RemovePlayerLocationResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                removed = json["removed"];
            }
        }
    }    
}