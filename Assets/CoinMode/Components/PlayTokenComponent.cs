using CoinMode.NetApi;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CoinMode
{
    public delegate void PlayTokenEvent(PlayTokenComponent playToken);
    public delegate void PlayTokenFailureEvent(PlayTokenComponent playToken, CoinModeError error);
    public delegate void PlayTokenVerifyEvent(PlayTokenComponent playToken, VerificationComponent verificationComponent);

    public class PlayTokenComponent : CoinModeComponent, ITitlePermissions
    {                
        public enum PlayTokenState
        {
            Clean,
            RequestingExisting,
            Requesting,
            Requested,
            Verifying,
            PartVerified,
            Verified,
            Cancelling,
            Cancelled,
            Expired,
            Unknown,
            NoneAssigned
        }        

        public PlayTokenState state
        {
            get { return _state; }
            private set
            {
                previousState = _state;
                _state = value;
            }
        }
        private PlayTokenState _state = PlayTokenState.Clean;

        private PlayTokenState previousState = PlayTokenState.Clean;

        public string playToken { get; private set; } = "";
        public string playerLoginId { get; private set; } = "";
        public string playerExternalLoginId { get; private set; } = "";
        public string playerPublicId { get; private set; } = "";
        public string titleId { get; private set; } = "";
        public bool isActivated { get; private set; } = false;

        public PlaytokenPermissions permissions { get; private set; } = PlaytokenPermissions.None;

        public PlayerAuthMode authMode { get; private set; } = PlayerAuthMode.None;

        public VerificationComponent verification = new VerificationComponent();

        public bool licenseRequiresSigning
        {
            get
            {
                return requiredLicenses != null && requiredLicenses.Count != signedLicenses.Count;
            }
        }

        public ReadOnlyDictionary<string, LicenseProperties> requiredLicenses { get; private set; } = null;

        private List<string> signedLicenses = null;

        private TitlePermission[] titlePermissions;

        private PlayTokenEvent getInfoSuccess = null;
        private PlayTokenFailureEvent getInfoFailure = null;

        private PlayTokenEvent requestSuccess = null;
        private PlayTokenFailureEvent requestFailure = null;

        private PlayTokenEvent verifySuccess = null;
        private PlayTokenVerifyEvent verifyUpdate = null;
        private PlayTokenFailureEvent verifyFailure = null;

        private PlayTokenEvent cancelSuccess = null;
        private PlayTokenFailureEvent cancelFailure = null;        

        internal PlayTokenComponent(TitlePermission[] titlePermissions) 
        {
            this.titlePermissions = titlePermissions;
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

        public bool RequestExisting(string uuidOrEmail, string playToken, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            return RequestExisting(PlayerAuthMode.UuidOrEmail, uuidOrEmail, playToken, onSuccess, onFailure);
        }

        public bool RequestExistingWithDiscord(string discordId, string playToken, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            return RequestExisting(PlayerAuthMode.Discord, discordId, playToken, onSuccess, onFailure);
        }

        public bool RequestExistingWithGoogle(string googleId, string playToken, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            return RequestExisting(PlayerAuthMode.Google, googleId, playToken, onSuccess, onFailure);
        }

        public bool RequestExistingWithApple(string appleId, string playToken, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            return RequestExisting(PlayerAuthMode.Apple, appleId, playToken, onSuccess, onFailure);
        }

        private bool RequestExisting(PlayerAuthMode authMode, string loginId, string playToken, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            if (state != PlayTokenState.Clean)
            {
                if (state == PlayTokenState.RequestingExisting)
                {
                    getInfoSuccess -= onSuccess;
                    getInfoSuccess += onSuccess;

                    getInfoFailure -= onFailure;
                    getInfoFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayTokenComponent", "RequestExisting", "Cannot request existing play token while  {0}", state.ToString());
                    return false;
                }
            }

            if (string.IsNullOrEmpty(playToken))
            {
                CoinModeLogging.LogWarning("PlayTokenComponent", "RequestExisting", "Cannot request existing play token without valid play token string!");
                return false;
            }

            state = PlayTokenState.RequestingExisting;

            this.authMode = authMode;

            playerLoginId = loginId;
            playerExternalLoginId = "";

            getInfoSuccess = onSuccess;
            getInfoFailure = onFailure;

            CoinModeManager.SendRequest(PlayersPlayTokens.GetInfo(playToken, OnGetInfoSuccess, OnGetInfoFailure));
            return true;
        }

        private void OnGetInfoSuccess(PlayersPlayTokens.GetInfoResponse response)
        {
            if(response.titleId == CoinModeSettings.titleId)
            {
                if (response.isActive.Value)
                {
                    state = PlayTokenState.Verified;
                    permissions = ParsePermissions(titlePermissions);
                }
                else
                {
                    if (response.activeStatus != null)
                    {
                        if (!response.activeStatus.wasActivated.Value)
                        {
                            bool partVerified = false;
                            foreach (KeyValuePair<string, string> verification in response.verificationMethodStatus)
                            {
                                if (verification.Value == "approved")
                                {
                                    partVerified = true;
                                    break;
                                }
                            }
                            state = partVerified ? PlayTokenState.PartVerified : PlayTokenState.Requested;
                        }
                        else if (response.activeStatus.wasCancelled.Value)
                        {
                            state = PlayTokenState.Cancelled;
                        }
                        else if (response.activeStatus.hasExpired.Value)
                        {
                            state = PlayTokenState.Expired;
                        }
                        else
                        {
                            state = PlayTokenState.Unknown;
                        }
                    }
                    else
                    {
                        state = PlayTokenState.Unknown;
                    }
                }

                playToken = response.playToken;
                playerPublicId = response.publicPlayerId;
                isActivated = response.isActive.Value;
                titleId = response.titleId;                

                getInfoSuccess?.Invoke(this);
            }
            else
            {
                state = previousState;
                getInfoFailure?.Invoke(this, new CoinModeError(CoinModeErrorBase.ErrorType.Client, "CLIENT TITLE ID MISMATCH", 
                    "The play token title id does not match the title id provided in the CoinMode settings for the project"));
            }            
        }

        private void OnGetInfoFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            getInfoFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool Request(string uuidOrEmail, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            return Request(PlayerAuthMode.UuidOrEmail, uuidOrEmail, "", onSuccess, onFailure);
        }

        public bool RequestWithDiscord(string discordId, string discordSignInId, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            return Request(PlayerAuthMode.Discord, discordId, discordSignInId, onSuccess, onFailure);
        }

        public bool RequestWithGoogle(string googleId, string googleSignInId, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            return Request(PlayerAuthMode.Google, googleId, googleSignInId, onSuccess, onFailure);
        }

        public bool RequestWithApple(string appleId, string appleSignInId, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            return Request(PlayerAuthMode.Apple, appleId, appleSignInId, onSuccess, onFailure);
        }

        private bool Request(PlayerAuthMode authMode, string loginId, string externalLoginId, PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            if (state != PlayTokenState.Clean)
            {
                if (state == PlayTokenState.Requesting)
                {
                    requestSuccess -= onSuccess;
                    requestSuccess += onSuccess;

                    requestFailure -= onFailure;
                    requestFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayTokenComponent", "Request", "Cannot request play token while  {0}", state.ToString());
                    return false;
                }
            }

            if (string.IsNullOrEmpty(loginId) || (authMode != PlayerAuthMode.UuidOrEmail && string.IsNullOrEmpty(externalLoginId)))
            {
                CoinModeLogging.LogWarning("PlayTokenComponent", "Request", "Cannot request play token without login id!");
                return false;
            }

            state = PlayTokenState.Requesting;

            this.authMode = authMode;

            playerLoginId = loginId;
            playerExternalLoginId = externalLoginId;

            requestSuccess = onSuccess;
            requestFailure = onFailure;

            PlayersPlayTokens.RequestPlayTokenRequest request = null;

            switch (authMode)
            {
                default:
                case PlayerAuthMode.UuidOrEmail:
                    request = PlayersPlayTokens.Request(playerLoginId, CoinModeSettings.titleId, "", OnRequestSuccess, OnRequestFailure);
                    break;
                case PlayerAuthMode.Discord:
                    request = PlayersPlayTokens.RequestWithDiscord(playerLoginId, playerExternalLoginId, CoinModeSettings.titleId, "", OnRequestSuccess, OnRequestFailure);
                    break;
                case PlayerAuthMode.Google:
                    request = PlayersPlayTokens.RequestWithGoogle(playerLoginId, playerExternalLoginId, CoinModeSettings.titleId, "", OnRequestSuccess, OnRequestFailure);
                    break;
                case PlayerAuthMode.Apple:
                    request = PlayersPlayTokens.RequestWithApple(playerLoginId, playerExternalLoginId, CoinModeSettings.titleId, "", OnRequestSuccess, OnRequestFailure);
                    break;
            }

            CoinModeManager.SendRequest(request);
            return true;
        }

        private void OnRequestSuccess(PlayersPlayTokens.RequestPlayTokenResponse response)
        {
            state = PlayTokenState.Requested;

            playToken = response.playToken;
            playerPublicId = response.publicPlayerId;
            titleId = response.titleId;
            isActivated = response.isActivated.Value;
            verification.Clear();
            if (response.requiredVerification != null)
            {
                verification.SetUp(response.requiredVerification);
            }
            if(response.requireseLicenseSigning.HasValue && response.requireseLicenseSigning.Value)
            {
                requiredLicenses = new ReadOnlyDictionary<string, LicenseProperties>(response.requiredLicenses);
                signedLicenses = new List<string>();
            }
            requestSuccess?.Invoke(this);
        }

        private void OnRequestFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            requestFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool SignLicense(string licenseId, bool sign)
        {
            if (state != PlayTokenState.Requested && state != PlayTokenState.PartVerified)
            {
                CoinModeLogging.LogWarning("PlayTokenComponent", "SignLicense", "Cannot sign a license for playtoken to while it is {0}", state.ToString());
                return false;
            }

            bool foundLicense = false;
            foreach(KeyValuePair<string, LicenseProperties> license in requiredLicenses)
            {
                if(license.Value.licenseId == licenseId)
                {
                    foundLicense = true;
                    break;
                }
            }

            if (!foundLicense)
            {
                CoinModeLogging.LogWarning("PlayTokenComponent", "SignLicense", "Cannot sign license for playtoken, licenseId {0} is not found in required licenses", licenseId);
                return false;
            }

            if(sign)
            {
                if(!signedLicenses.Contains(licenseId))
                {
                    signedLicenses.Add(licenseId);
                }
                else
                {
                    CoinModeLogging.LogMessage("PlayTokenComponent", "SignLicense", "License {0} already signed for playtoken!", licenseId);
                }
            }
            else
            {
                if (signedLicenses.Contains(licenseId))
                {
                    signedLicenses.Remove(licenseId);
                }
                else
                {
                    CoinModeLogging.LogMessage("PlayTokenComponent", "SignLicense", "Cannot revoke license {0} for playtoken, license had not yet been signed!", licenseId);
                    return false;
                }
            }

            return true;
        }

        public bool IsLicenseSigned(string licenseId)
        {
            return signedLicenses.Contains(licenseId);
        }

        public bool Verify(PlayTokenEvent onSuccess, PlayTokenVerifyEvent onUpdate, PlayTokenFailureEvent onFailure)
        {
            if (state != PlayTokenState.Requested && state != PlayTokenState.PartVerified)
            {
                if (state == PlayTokenState.Verifying)
                {
                    verifySuccess -= onSuccess;
                    verifySuccess += onSuccess;

                    verifyUpdate -= onUpdate;
                    verifyUpdate += onUpdate;

                    verifyFailure -= onFailure;
                    verifyFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayTokenComponent", "Verify", "Cannot verify a playtoken to while it is {0}", state.ToString());
                    return false;
                }
            }

            if (requiredLicenses != null && signedLicenses.Count != requiredLicenses.Count)
            {
                CoinModeLogging.LogWarning("PlayTokenComponent", "Verify", "Cannot verify playtoken until all required licenses have been signed!");
                return false;
            }

            verifySuccess = onSuccess;
            verifyUpdate = onUpdate;
            verifyFailure = onFailure;

            state = PlayTokenState.Verifying;

            Dictionary<string, string> verificationKeys = new Dictionary<string, string>();
            foreach (KeyValuePair<string, VerificationMethod> pair in verification.requiredMethods)
            {
                if (pair.Value.status == VerificationStatus.Pending && !string.IsNullOrEmpty(pair.Value.verificationKey))
                {
                    verificationKeys.Add(pair.Key, pair.Value.verificationKey);
                }
            }

            PlayersPlayTokens.VerifyPlayTokenRequest request = null;

            switch (authMode)
            {
                case PlayerAuthMode.UuidOrEmail:
                    request = PlayersPlayTokens.Verify(playToken, verificationKeys, signedLicenses, OnVerifySuccess, OnVerifyFailure);
                    break;
                case PlayerAuthMode.Discord:
                    request = PlayersPlayTokens.VerifyWithDiscord(playToken, playerExternalLoginId, signedLicenses, OnVerifySuccess, OnVerifyFailure);
                    break;
                case PlayerAuthMode.Google:
                    request = PlayersPlayTokens.VerifyWithGoogle(playToken, playerExternalLoginId, signedLicenses, OnVerifySuccess, OnVerifyFailure);
                    break;
                case PlayerAuthMode.Apple:
                    request = PlayersPlayTokens.VerifyWithApple(playToken, playerExternalLoginId, signedLicenses, OnVerifySuccess, OnVerifyFailure);
                    break;
            }

            CoinModeManager.SendRequest(request);
            return true;
        }        

        private void OnVerifySuccess(PlayersPlayTokens.VerifyPlayTokenResponse response)
        {
            if(response.playToken != playToken)
            {
                CoinModeLogging.LogWarning("PlayTokenComponent", "OnVerifyPlayTokenSuccess", "Recieved verification for {0}, expected {1}", response.playToken, playToken);
                return;
            }

            verification.UpdateFromStatus(response.verificationMethodStatus);
            verification.UpdateErrors(response.errorMessages);

            isActivated = response.isActivated != null ? response.isActivated.Value : false;

            if(isActivated)
            {
                verification.Clear();
                state = PlayTokenState.Verified;
                permissions = ParsePermissions(titlePermissions);
                verifySuccess?.Invoke(this);
            }
            else
            {
                if (verification.approvedCount > 0)
                {
                    state = PlayTokenState.PartVerified;
                }
                else
                {
                    state = PlayTokenState.Requested;
                }

                verifyUpdate?.Invoke(this, verification);
            }
        }

        private void OnVerifyFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            verifyFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool Cancel(PlayTokenEvent onSuccess, PlayTokenFailureEvent onFailure)
        {
            if (state != PlayTokenState.Requested && state != PlayTokenState.PartVerified && state != PlayTokenState.Verified)
            {
                if (state == PlayTokenState.Cancelling)
                {
                    cancelSuccess -= onSuccess;
                    cancelSuccess += onSuccess;

                    cancelFailure -= onFailure;
                    cancelFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayTokenComponent", "Cancel", "Cannot cancel a playtoken to while it is {0}", state.ToString());
                    return false;
                }
            }

            cancelSuccess = onSuccess;
            cancelFailure = onFailure;

            state = PlayTokenState.Cancelling;

            CoinModeManager.SendRequest(PlayersPlayTokens.Cancel(playToken, OnCancelSuccess, OnCancelFailure));
            return true;
        }

        private void OnCancelSuccess(PlayersPlayTokens.CancelPlayTokenResponse response)
        {
            state = PlayTokenState.Cancelled;
            cancelSuccess?.Invoke(this);
        }

        private void OnCancelFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            cancelFailure?.Invoke(this, new CoinModeError(response));
        }

        private PlaytokenPermissions ParsePermissions(TitlePermission[] permissions)
        {
            PlaytokenPermissions playTokenPermissions = PlaytokenPermissions.None;
            if (permissions != null)
            {
                for(int i = 0; i < permissions.Length; i++)
                {
                    playTokenPermissions |= CoinModeParamHelpers.PermissionFromString(permissions[i].permissionId);
                }
            }
            return playTokenPermissions;
        }        

        public void Reset()
        {
            state = PlayTokenState.Clean;
            previousState = PlayTokenState.Clean;
            authMode = PlayerAuthMode.None;

            playToken = "";
            playerLoginId = "";
            playerExternalLoginId = "";
            playerPublicId = "";
            titleId = "";
            isActivated = false;
            verification.Clear();

            requiredLicenses = null;
            signedLicenses = null;

            getInfoSuccess = null;
            getInfoFailure = null;
            requestSuccess = null;
            requestFailure = null;
            verifySuccess = null;
            verifyUpdate = null;
            verifyFailure = null;
            cancelSuccess = null;
            cancelFailure = null;
        }
    }
}