using CoinMode.NetApi;
using System;
using System.Collections.Generic;

namespace CoinMode
{
    public delegate void UserEvent(UserComponent user);
    public delegate void UserFailureEvent(UserComponent user, CoinModeError error);

    public class UserComponent : CoinModeComponent
    {
        public enum UserState
        {
            Clean,
            PublicIdAssigned,
            GettingProperties,
            Ready,
        }

        public string publicId { get; private set; } = "";
        public string displayName { get; private set; } = "";

        public string avatarSmallUrl { get; private set; } = "";
        public string avatarLargeUrl { get; private set; } = "";

        public string languageShortcode { get; private set; } = "";
        public string country { get; private set; } = "";
        public string countryCode { get; private set; } = "";

        public UserState state
        {
            get { return _state; }
            private set
            {
                previousState = _state;
                _state = value;
            }
        }
        private UserState _state = UserState.Clean;

        private UserState previousState = UserState.Clean;

        private UserEvent getPropertiesSuccess = null;
        private UserFailureEvent getPropertiesFailure = null;

        internal UserComponent() { }

        public bool AssignPublicId(string publicId)
        {
            if (state != UserState.Clean && state != UserState.PublicIdAssigned)
            {
                CoinModeLogging.LogWarning("UserComponent", "AssignPublicId", "Cannot assign public id to user {0} while user is {1}", publicId.ToString(), state.ToString());
                return false;
            }

            if (string.IsNullOrEmpty(publicId))
            {
                CoinModeLogging.LogWarning("UserComponent", "AssignPublicId", "Assigned public id for user {0} is empty!", publicId.ToString());
                return false;
            }

            if (!string.IsNullOrEmpty(this.publicId))
            {
                CoinModeLogging.LogMessage("UserComponent", "AssignPublicId", "Re-assigning public id from {0} to {1}", this.publicId, publicId);
            }

            state = UserState.PublicIdAssigned;
            this.publicId = publicId;
            return true;
        }

        public bool GetProperties(UserEvent onSuccess, UserFailureEvent onFailure)
        {
            if (state != UserState.PublicIdAssigned)
            {
                if (state == UserState.GettingProperties)
                {
                    getPropertiesSuccess -= onSuccess;
                    getPropertiesSuccess += onSuccess;

                    getPropertiesFailure -= onFailure;
                    getPropertiesFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("UserComponent", "GetProperties", "Cannot get properties for user {0} while player is {1}", publicId.ToString(), state.ToString());
                    return false;
                }
            }

            state = UserState.GettingProperties;

            getPropertiesSuccess = onSuccess;
            getPropertiesFailure = onFailure;

            CoinModeManager.SendRequest(Players.GetProperties(publicId, OnGetPropertiesSuccess, OnGetPropertiesFailure));
            return true;
        }

        private void OnGetPropertiesSuccess(Players.GetPropertiesResponse response)
        {
            state = UserState.Ready;
            
            displayName = response.displayName != null ? response.displayName : "";
            avatarSmallUrl = response.avatarImageUrlSmall != null ? response.avatarImageUrlSmall : "";
            avatarLargeUrl = response.avatarImageUrlLarge != null ? response.avatarImageUrlLarge : "";
            languageShortcode = response.languageShortcode != null ? response.languageShortcode : "";
            country = response.country != null ? response.country : "";
            countryCode = response.countryCode != null ? response.countryCode : "";

            getPropertiesSuccess?.Invoke(this);
        }

        private void OnGetPropertiesFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            getPropertiesFailure?.Invoke(this, new CoinModeError(response));
        }
    }
}
