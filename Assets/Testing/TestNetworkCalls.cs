using CoinMode;
using CoinMode.NetApi;
using System;
using UnityEngine;

public class TestNetworkCalls : MonoBehaviour
{
    public string displayNameToRegister = "";
    public string emailToRegister = "";
    public string mobileToRegister = "";

    public string playerUuidOrEmailToLogin = "";

    public string emailVerificationKey = "";
    public string smsVerificationKey = "";
    public string gauthVerificationKey = "";
    public string passwordVerificationKey = "";

    private PlayerComponent player;

    private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CoinModeManager.InitialiseTitleEnvironment(null, null);
        }

        if (CoinModeManager.state == CoinModeManager.State.Ready)
        {            
            if(player == null)
            {
                CoinModeManager.ConstructPlayer(out player);
                player.AssignUuidOrEmail(playerUuidOrEmailToLogin);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                player.RegisterNewPlayer(displayNameToRegister, emailToRegister, mobileToRegister, null, null, null);
            }

            if(Input.GetKeyDown(KeyCode.Alpha3))
            {
                player.RequestNewPlayToken(null, null);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) && player.playTokenState != PlayTokenComponent.PlayTokenState.NoneAssigned)
            {
                if(emailVerificationKey != "")
                {
                    player.playTokenVerification.SetKey("email", emailVerificationKey);
                }
                if (smsVerificationKey != "")
                {
                    player.playTokenVerification.SetKey("sms", smsVerificationKey);
                }
                if (gauthVerificationKey != "")
                {
                    player.playTokenVerification.SetKey("gauth", gauthVerificationKey);
                }
                if (passwordVerificationKey != "")
                {
                    player.playTokenVerification.SetKey("password", passwordVerificationKey);
                }
                player.VerifyPlayToken(null, null, null);
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                player.GetProperties(null, null);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            CoinModeManager.SendLocationRequest(Location.ListNearbyPlayers("exampleGame2", OnListNearbyPlayersSuccess, OnListNearbyPlayersFailure));
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            CoinModeManager.SendLocationRequest(Location.AddPlayerLocation("example_id2", "exampleGame2", OnAddPlayerLocationSuccess, OnAddPlayerLocationFailure));
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            CoinModeManager.SendLocationRequest(Location.RemovePlayerLocation("example_id", OnRemovePlayerLocationSuccess, OnRemovePlayerLocationFailure));
        }
    }

    private void OnRemovePlayerLocationSuccess(Location.RemovePlayerLocationResponse response)
    {
        Debug.Log("OnRemovePlayerLocationSuccess");
    }

    private void OnRemovePlayerLocationFailure(CoinModeErrorResponse response)
    {
        Debug.Log("OnRemovePlayerLocationFailure");
    }

    private void OnAddPlayerLocationSuccess(Location.AddPlayerLocationResponse response)
    {
        Debug.Log("OnAddPlayerLocationSuccess");
    }

    private void OnAddPlayerLocationFailure(CoinModeErrorResponse response)
    {
        Debug.Log("OnAddPlayerLocationFailure");
    }

    private void OnListNearbyPlayersSuccess(Location.ListNearbyPlayersResponse response)
    {
        Debug.Log("OnListNearbyPlayersSuccess");
    }

    private void OnListNearbyPlayersFailure(CoinModeErrorResponse response)
    {
        Debug.Log("OnListNearbyPlayersFailure");
    }
}
