using CoinMode.NetApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static CoinMode.PlayTokenComponent;

namespace CoinMode
{
    public delegate void PlayerEvent(PlayerComponent player);
    public delegate void PlayerFailureEvent(PlayerComponent player, CoinModeError error);
    public delegate void PlayerVerifyEvent(PlayerComponent player, VerificationComponent verificationComponent);

    public class PlayerComponent : CoinModeComponent, ITitlePermissions
    {
        public enum PlayerState
        {
            Clean,
            Registering,
            LoginIdAssigned,
            RequestingPlayToken,
            PlayTokenRequested,
            VerifyingPlayToken,
            PlayTokenVerified,
            GettingProperties,
            Ready,
            SessionAssigned,
        }        

        public int localId { get; private set; } = -1;
        public string loginId { get; private set; } = "";
        public string externalLoginId { get; private set; } = "";

        private PlayTokenComponent playTokenComponent = null;

        public string playToken { get { return playTokenComponent != null ? playTokenComponent.playToken : ""; } }

        public PlaytokenPermissions playTokenPermissions { get { return playTokenComponent != null ? playTokenComponent.permissions : PlaytokenPermissions.None; } }
        public PlayTokenState playTokenState { get { return playTokenComponent != null ? playTokenComponent.state : PlayTokenState.NoneAssigned; } }
        public VerificationComponent playTokenVerification { get { return playTokenComponent != null ? playTokenComponent.verification : null; } }

        public bool licenseRequiresSigning 
        { 
            get 
            { 
                return playTokenComponent != null && playTokenComponent.licenseRequiresSigning; 
            } 
        }

        public ReadOnlyDictionary<string, LicenseProperties> requiredLicenses { get { return playTokenComponent != null ? playTokenComponent.requiredLicenses : null; } }

        public string publicId { get; private set; } = "";
        public string displayName { get; private set; } = "";

        public string avatarSmallUrl { get; private set; } = "";
        public string avatarLargeUrl { get; private set; } = "";

        public string languageShortcode { get; private set; } = "";
        public string country { get; private set; } = "";
        public string countryCode { get; private set; } = "";
        public string displayCurrencyKey { get; private set; } = "";
        public string defaultWalletId { get; private set; } = "";
        public Wallet defaultWallet
        {
            get
            {
                Wallet wallet = null;
                CoinModeManager.walletComponent.TryGetWallet(defaultWalletId, out wallet);
                return wallet;
            }
        }

        public PlayerAuthMode authMode { get; private set; } = PlayerAuthMode.None;

        public bool newUser { get; private set; } = false;

        public PlayerWalletComponent walletComponent { get; private set; } = null;
        public SessionComponent sessionComponent { get; private set; } = null;

        private TitlePermission[] titlePermissions { get; set; } = null;

        public PlayerState state
        {
            get { return _state; }
            private set
            {
                previousState = _state;
                _state = value;
            }
        }
        private PlayerState _state = PlayerState.Clean;

        private PlayerState previousState = PlayerState.Clean;

        private PlayerEvent registerSuccess = null;
        private PlayerFailureEvent registerFailure = null;

        private PlayerEvent requestPlayTokenSuccess = null;
        private PlayerFailureEvent requestPlayTokenFailure = null;        

        private PlayerEvent verifyPlayTokenSuccess = null;
        private PlayerVerifyEvent verifyPlayTokenUpdate = null;
        private PlayerFailureEvent verifyPlayTokenFailure = null;

        private PlayerEvent getPropertiesSuccess = null;
        private PlayerFailureEvent getPropertiesFailure = null;

        internal PlayerComponent(int localId, TitlePermission[] titlePermissions) 
        {
            this.localId = localId;
            this.titlePermissions = titlePermissions;
            state = PlayerState.Clean;            
        }

        public int GetTitlePermissionsCount()
        {
            return titlePermissions != null ? titlePermissions.Length : 0;
        }

        public TitlePermission GetTitlePermission(int index)
        {
            if (titlePermissions != null && index >= 0 && index < titlePermissions.Length)
            {
                return titlePermissions[index];
            }
            return null;
        }

        public bool AssignUuidOrEmail(string uuidOrEmail)
        {
            return AssignLoginId(PlayerAuthMode.UuidOrEmail, uuidOrEmail, "");
        }

        public bool AssignDiscordId(string discordId, string discordSignInId)
        {
            return AssignLoginId(PlayerAuthMode.Discord, discordId, discordSignInId);
        }

        public bool AssignGoogleId(string googleId, string googleSignInId)
        {
            return AssignLoginId(PlayerAuthMode.Google, googleId, googleSignInId);
        }

        public bool AssignAppleId(string appleId, string appleSignInId)
        {
            return AssignLoginId(PlayerAuthMode.Apple, appleId, appleSignInId);
        }

        private bool AssignLoginId(PlayerAuthMode authMode, string loginId, string externalLoginId)
        {
            if (state != PlayerState.Clean && state != PlayerState.LoginIdAssigned)
            {
                CoinModeLogging.LogWarning("PlayerComponent", "AssignLoginId", "Cannot login id to player {0} while player is {1}", localId.ToString(), state.ToString());
                return false;
            }

            if (string.IsNullOrEmpty(loginId))
            {
                CoinModeLogging.LogWarning("PlayerComponent", "AssignLoginId", "Assigned login id for player {0} is empty!", localId.ToString());
                return false;
            }

            if (!string.IsNullOrEmpty(this.loginId))
            {
                CoinModeLogging.LogMessage("PlayerComponent", "AssignLoginId", "Re-assigning login id from {0} to {1}", this.loginId, loginId);
            }

            state = PlayerState.LoginIdAssigned;

            this.authMode = authMode;
            this.loginId = loginId;
            this.externalLoginId = externalLoginId;
            newUser = false;

            playTokenComponent = new PlayTokenComponent(titlePermissions);
            return true;
        }

        public bool RegisterNewPlayer(string displayName, string email, string mobile, PlayerEvent onSuccess, PlayerFailureEvent onFailure)
        {
            return RegisterNewPlayer(displayName, email, mobile, null, onSuccess, onFailure);
        }

        public bool RegisterNewPlayer(string displayName, string email, string mobile, DateTime? dateOfBirth, PlayerEvent onSuccess, PlayerFailureEvent onFailure)
        {           
            if (state != PlayerState.Clean && state != PlayerState.LoginIdAssigned)
            {
                if (state == PlayerState.Registering)
                {
                    registerSuccess -= onSuccess;
                    registerSuccess += onSuccess;

                    registerFailure -= onFailure;
                    registerFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayerComponent", "RegisterNewPlayer", "Cannot register player {0} while player is {1}", localId.ToString(), state.ToString());
                    return false;
                }
            }

            if(state == PlayerState.LoginIdAssigned)
            {
                playTokenComponent = null;
                authMode = PlayerAuthMode.None;
                loginId = "";
                externalLoginId = "";
                newUser = false;
            }
           
            state = PlayerState.Registering;

            registerSuccess = onSuccess;
            registerFailure = onFailure;            

            string dob = dateOfBirth != null ? dateOfBirth.Value.Year.ToString("D4") + "-" + dateOfBirth.Value.Month.ToString("D2") + "-" + dateOfBirth.Value.Day.ToString("D2") : null;

            CoinModeManager.SendRequest(Players.CreateNewPlayer(displayName, email, mobile, dob, CoinModeSettings.titleId, OnRegisterSuccess, OnRegisterFailure));
            return true;
        }

        private void OnRegisterSuccess(Players.CreateNewPlayerResponse response)
        {
            state = PlayerState.LoginIdAssigned;
            authMode = PlayerAuthMode.UuidOrEmail;
            loginId = response.playerSecretUuid;
            externalLoginId = "";
            newUser = true;

            playTokenComponent = new PlayTokenComponent(titlePermissions);

            registerSuccess?.Invoke(this);
        }

        private void OnRegisterFailure(CoinModeErrorResponse errorResponse)
        {
            state = previousState;
            registerFailure?.Invoke(this, new CoinModeError(errorResponse));
        }

        public bool RequestNewPlayToken(PlayerEvent onSuccess, PlayerFailureEvent onFailure)
        {            
            if (state != PlayerState.LoginIdAssigned && state != PlayerState.PlayTokenRequested)
            {
                if (state == PlayerState.RequestingPlayToken)
                {
                    requestPlayTokenSuccess -= onSuccess;
                    requestPlayTokenSuccess += onSuccess;

                    requestPlayTokenFailure -= onFailure;
                    requestPlayTokenFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayerComponent", "RequestNewPlayToken", "Cannot request new playtoken while player is {0}", state.ToString());
                    return false;
                }
            }

            if(state == PlayerState.PlayTokenRequested)
            {
                playTokenComponent.Reset();
            }

            bool requestSuccess = false;

            switch (authMode)
            {
                case PlayerAuthMode.UuidOrEmail:
                    requestSuccess = playTokenComponent.Request(loginId, OnRequestNewPlayTokenSuccess, OnRequestPlayTokenFailure);
                    break;
                case PlayerAuthMode.Discord:
                    requestSuccess = playTokenComponent.RequestWithDiscord(loginId, externalLoginId, OnRequestNewPlayTokenSuccess, OnRequestPlayTokenFailure);
                    break;
                case PlayerAuthMode.Google:
                    requestSuccess = playTokenComponent.RequestWithGoogle(loginId, externalLoginId, OnRequestNewPlayTokenSuccess, OnRequestPlayTokenFailure);
                    break;
                case PlayerAuthMode.Apple:
                    requestSuccess = playTokenComponent.RequestWithApple(loginId, externalLoginId, OnRequestNewPlayTokenSuccess, OnRequestPlayTokenFailure);
                    break;
            }

            if (requestSuccess)
            {
                requestPlayTokenSuccess = onSuccess;
                requestPlayTokenFailure = onFailure;

                state = PlayerState.RequestingPlayToken;
                return true;
            }
            return false;
        }

        public bool RequestExistingPlayToken(string existingToken, PlayerEvent onSuccess, PlayerFailureEvent onFailure)
        {            
            if (state != PlayerState.LoginIdAssigned && state != PlayerState.PlayTokenRequested)
            {
                if (state == PlayerState.RequestingPlayToken)
                {
                    requestPlayTokenSuccess -= onSuccess;
                    requestPlayTokenSuccess += onSuccess;

                    requestPlayTokenFailure -= onFailure;
                    requestPlayTokenFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayerComponent", "RequestNewPlayToken", "Cannot request existing playtoken while player is {0}", state.ToString());
                    return false;
                }
            }

            if (state == PlayerState.PlayTokenRequested)
            {
                playTokenComponent.Reset();
            }

            bool requestSuccess = false;

            switch (authMode)
            {
                case PlayerAuthMode.UuidOrEmail:
                    requestSuccess = playTokenComponent.RequestExisting(loginId, existingToken, OnRequestExistingPlayTokenSuccess, OnRequestPlayTokenFailure);
                    break;
                case PlayerAuthMode.Discord:
                    requestSuccess = playTokenComponent.RequestExistingWithDiscord(loginId, existingToken, OnRequestExistingPlayTokenSuccess, OnRequestPlayTokenFailure);
                    break;
                case PlayerAuthMode.Google:
                    requestSuccess = playTokenComponent.RequestExistingWithGoogle(loginId, existingToken, OnRequestExistingPlayTokenSuccess, OnRequestPlayTokenFailure);
                    break;
                case PlayerAuthMode.Apple:
                    requestSuccess = playTokenComponent.RequestExistingWithApple(loginId, existingToken, OnRequestExistingPlayTokenSuccess, OnRequestPlayTokenFailure);
                    break;
            }

            if (requestSuccess)
            {
                requestPlayTokenSuccess = onSuccess;
                requestPlayTokenFailure = onFailure;                

                state = PlayerState.RequestingPlayToken;
                return true;
            }
            return false;
        }

        private void OnRequestNewPlayTokenSuccess(PlayTokenComponent playToken)
        {
            state = PlayerState.PlayTokenRequested;
            publicId = playToken.playerPublicId;
            requestPlayTokenSuccess?.Invoke(this);
        }

        private void OnRequestExistingPlayTokenSuccess(PlayTokenComponent playToken)
        {
            state = playToken.state == PlayTokenState.Verified ? PlayerState.PlayTokenVerified : PlayerState.PlayTokenRequested;
            publicId = playToken.playerPublicId;
            requestPlayTokenSuccess?.Invoke(this);
        }

        private void OnRequestPlayTokenFailure(PlayTokenComponent playToken, CoinModeError error)
        {
            state = previousState;
            requestPlayTokenFailure?.Invoke(this, error);
        }

        public bool SignLicense(string licenseId, bool sign)
        {
            if (state != PlayerState.PlayTokenRequested)
            {
                CoinModeLogging.LogWarning("PlayerComponent", "SignLicense", "Cannot sign license for playtoken while player is {0}", state.ToString());
                return false;
            }

            return playTokenComponent.SignLicense(licenseId, sign);
        }

        public bool IsLicenseSigned(string licenseId)
        {
            return playTokenComponent.IsLicenseSigned(licenseId);
        }

        public bool VerifyPlayToken(PlayerEvent onSuccess, PlayerVerifyEvent onUpdate, PlayerFailureEvent onFailure)
        {            
            if (state != PlayerState.PlayTokenRequested)
            {
                if (state == PlayerState.VerifyingPlayToken)
                {
                    verifyPlayTokenSuccess -= onSuccess;
                    verifyPlayTokenSuccess += onSuccess;

                    verifyPlayTokenFailure -= onFailure;
                    verifyPlayTokenFailure += onFailure;

                    verifyPlayTokenUpdate -= onUpdate;
                    verifyPlayTokenUpdate += onUpdate;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayerComponent", "VerifyPlayToken", "Cannot verify playtoken while player is {0}", state.ToString());
                    return false;
                }
            }

            if (playTokenComponent.Verify(OnVerifyPlayTokenSuccess, OnVerifyPlayTokenUpdate, OnVerifyPlayTokenFailure))
            {
                verifyPlayTokenSuccess = onSuccess;
                verifyPlayTokenFailure = onFailure;
                verifyPlayTokenUpdate = onUpdate;

                state = PlayerState.VerifyingPlayToken;
                return true;
            }
            return false;
        }

        private void OnVerifyPlayTokenSuccess(PlayTokenComponent playToken)
        {
            state = PlayerState.PlayTokenVerified;
            verifyPlayTokenSuccess?.Invoke(this);
        }

        private void OnVerifyPlayTokenUpdate(PlayTokenComponent playToken, VerificationComponent verificationComponent)
        {
            state = previousState;
            verifyPlayTokenUpdate?.Invoke(this, verificationComponent);
        }

        private void OnVerifyPlayTokenFailure(PlayTokenComponent playToken, CoinModeError error)
        {
            state = previousState;
            verifyPlayTokenFailure?.Invoke(this, error);
        }

        public bool GetProperties(PlayerEvent onSuccess, PlayerFailureEvent onFailure)
        {
            if (state != PlayerState.PlayTokenVerified)
            {
                if (state == PlayerState.GettingProperties)
                {
                    getPropertiesSuccess -= onSuccess;
                    getPropertiesSuccess += onSuccess;

                    getPropertiesFailure -= onFailure;
                    getPropertiesFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayerComponent", "GetPlayerProperties", "Cannot get properties for player {0} while player is {1}", localId.ToString(), state.ToString());
                    return false;
                }
            }

            state = PlayerState.GettingProperties;

            getPropertiesSuccess = onSuccess;
            getPropertiesFailure = onFailure;

            CoinModeManager.SendRequest(Players.GetProperties(playTokenComponent.playToken, publicId, OnGetPropertiesSuccess, OnGetPropertiesFailure));
            return true;
        }

        private void OnGetPropertiesSuccess(Players.GetPropertiesResponse response)
        {
            state = PlayerState.Ready;
            
            displayName = response.displayName != null ? response.displayName : "";
            avatarSmallUrl = response.avatarImageUrlSmall != null ? response.avatarImageUrlSmall : "";
            avatarLargeUrl = response.avatarImageUrlLarge != null ? response.avatarImageUrlLarge : "";
            languageShortcode = response.languageShortcode != null ? response.languageShortcode : "";
            country = response.country != null ? response.country : "";
            countryCode = response.countryCode != null ? response.countryCode : "";
            displayCurrencyKey = response.displayCurrency != null ? response.displayCurrency : CoinModeManager.defaultLocalCurrency;
            defaultWalletId = response.defaultWallet != null ? response.defaultWallet : CoinModeManager.defaultWallet;

            walletComponent = new PlayerWalletComponent();

            CoinModePlayerPrefs.CachedPlayerData data = new CoinModePlayerPrefs.CachedPlayerData();
            data.playToken = playTokenComponent.playToken;
            data.publicId = publicId;
            data.uuid = loginId;
            data.authMode = authMode.ToString();
            data.displayName = displayName;

            CoinModePlayerPrefs.SetLastLoggedInPlayer(loginId);
            CoinModePlayerPrefs.AddCachedPlayerData(loginId, data);
            CoinModePlayerPrefs.SaveToPlayerPrefs();

            getPropertiesSuccess?.Invoke(this);
        }

        private void OnGetPropertiesFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            getPropertiesFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool AssignSession(SessionComponent session)
        {
            if (state == PlayerState.SessionAssigned)
            {
                if (sessionComponent.state != SessionComponent.SessionState.Clean && sessionComponent.state != SessionComponent.SessionState.RoundIdAssigned &&
                    sessionComponent.state != SessionComponent.SessionState.Requested && sessionComponent.state != SessionComponent.SessionState.Stopped)
                {
                    CoinModeLogging.LogWarning("PlayerComponent", "AssignSession", "Cannot assign session {0} to player {1} while player is already has session {2} assigned and is {3}",
                    session.sessionId, localId.ToString(), sessionComponent.sessionId, sessionComponent.state.ToString());
                    return false;
                }                
            }
            else if (state != PlayerState.Ready)
            {
                CoinModeLogging.LogWarning("PlayerComponent", "AssignSession", "Cannot assign session {0} to player {1} while player is {2}", session.sessionId, localId.ToString(), state.ToString());
                return false;
            }

            if (session.state != SessionComponent.SessionState.RoundIdAssigned && session.state != SessionComponent.SessionState.Requested)
            {
                CoinModeLogging.LogWarning("PlayerComponent", "AssignSession", "Cannot assign session {0} to player {1} while session is {2}", session.sessionId, localId.ToString(), session.state.ToString());
                return false;
            }

            state = PlayerState.SessionAssigned;
            sessionComponent = session;
            return true;
        }

        public bool ClearSession()
        {
            if (state != PlayerState.SessionAssigned)
            {
                CoinModeLogging.LogWarning("PlayerComponent", "ClearSession", "Cannot clear session from player {0} while player is {1}", localId.ToString(), state.ToString());
                return false;
            }

            if (sessionComponent.state != SessionComponent.SessionState.Clean && sessionComponent.state != SessionComponent.SessionState.RoundIdAssigned &&
                sessionComponent.state != SessionComponent.SessionState.Requested && sessionComponent.state != SessionComponent.SessionState.Stopped)
            {
                    CoinModeLogging.LogWarning("PlayerComponent", "AssignSession", "Cannot clear session from player {0} while session is {1}", localId.ToString(), sessionComponent.state.ToString());
                return false;
            }

            state = PlayerState.Ready;
            sessionComponent = null;
            return true;
        }

        public void Reset()
        {
            publicId = "";
            displayName = "";

            playTokenComponent = null;
            walletComponent = null;
            sessionComponent = null;

            registerSuccess = null;
            registerFailure = null;

            getPropertiesSuccess = null;
            getPropertiesFailure = null;

            if(!string.IsNullOrEmpty(loginId))
            {
                state = PlayerState.LoginIdAssigned;
                previousState = PlayerState.Clean;
            }
            else if(localId > -1)
            {
                state = PlayerState.Clean;
                previousState = PlayerState.Clean;
            }
            else
            {
                state = PlayerState.Clean;
                previousState = PlayerState.Clean;
            }
        }
    }
}
