using CoinMode.NetApi;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode
{
    public delegate void PlayerWalletEvent(PlayerWalletComponent walletComp);
    public delegate void PlayerWalletFailureEvent(PlayerWalletComponent walletComp, CoinModeError error);

    public delegate void PlayerWalletPocketEvent(PlayerWalletComponent walletComp, string pocketWalletId);
    public delegate void PlayerWalletPocketFailureEvent(PlayerWalletComponent walletComp, string pocketWalletId, CoinModeError error);

    public class PlayerWalletComponent : CoinModeComponent
    {
        public class Pocket
        {
            /// <summary>
            /// Matches walletId, e.g: bitcoin_main, bitcoin_test, etc
            /// </summary>
            public string pocketId { get; private set; } = "";

            public double confirmedBalance { get; private set; } = 0.0D;
            public double pendingBalance { get; private set; } = 0.0D;

            public string depositAddress { get; private set; } = "";

            public Texture2D depositAddressQRTexture { get { return _depositAddressQRTexture; } }
            private Texture2D _depositAddressQRTexture = null;

            public Sprite depositAddressQRSprite { get { return _depositAddressQRSprite; } }
            private Sprite _depositAddressQRSprite = null;

            internal Pocket(string walletId)
            {
                this.pocketId = walletId;
                CoinModeQR.CreateQrResources(256, 256, out _depositAddressQRTexture, out _depositAddressQRSprite);
            }

            ~Pocket()
            {
                UnityEngine.Object.Destroy(depositAddressQRSprite);
                UnityEngine.Object.Destroy(depositAddressQRTexture);
            }

            internal void UpdateBalances(double? confirmed, double? pending)
            {
                confirmedBalance = confirmed != null ? confirmed.Value : 0.0D;
                pendingBalance = pending != null ? pending.Value : 0.0D;
            }

            internal void UpdateDepositAddess(string address)
            {
                depositAddress = address;
                CoinModeQR.GenerateQr(_depositAddressQRTexture, address);
            }
        }

        public enum PlayerWalletState
        {
            Clean,
            ObtainingBalances,            
            GettingDepositAddress,
            Ready,
        }        

        public IEnumerable<Pocket> pockets
        {
            get 
            {
                foreach(KeyValuePair<string, Pocket> p in _pockets)
                {
                    yield return p.Value;
                }
            }
        }
        private Dictionary<string, Pocket> _pockets = new Dictionary<string, Pocket>();

        public PlayerWalletState state
        {
            get { return _state; }
            private set
            {
                previousState = _state;
                _state = value;
            }
        }
        private PlayerWalletState _state = PlayerWalletState.Clean;

        private PlayerWalletState previousState = PlayerWalletState.Clean;

        private PlayerWalletEvent getBalancesSuccess = null;
        private PlayerWalletFailureEvent getBalancesFailure = null;

        private PlayerWalletPocketEvent getBalanceSuccess = null;
        private PlayerWalletPocketFailureEvent getBalanceFailure = null;
        private string requestedBalanceWalletId = null;

        private PlayerWalletPocketEvent getDepositAddressSuccess = null;
        private PlayerWalletPocketFailureEvent getDepositAddressFailure = null;
        private string requestedDepositWalletId = null;

        public bool WalletContainsPocket(string walletId)
        {
            return _pockets.ContainsKey(walletId);
        }

        public bool TryGetWalletPocket(string walletId, out Pocket wallet)
        {
            return _pockets.TryGetValue(walletId, out wallet);
        }

        public bool GetBalances(string playToken, PlayerWalletEvent onSuccess, PlayerWalletFailureEvent onFailure)
        {
            if (state != PlayerWalletState.Clean && state != PlayerWalletState.Ready)
            {
                if (state == PlayerWalletState.ObtainingBalances)
                {
                    getBalancesSuccess -= onSuccess;
                    getBalancesSuccess += onSuccess;

                    getBalancesFailure -= onFailure;
                    getBalancesFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayerWalletComponent", "GetBalancs", "Cannot get balances while wallet is {0}", state.ToString());
                    return false;
                }
            }

            if (string.IsNullOrEmpty(playToken))
            {
                CoinModeLogging.LogWarning("PlayerWalletComponent", "GetBalances", "Cannot get balances without providing playtoken.", state.ToString());
                return false;
            }

            state = PlayerWalletState.ObtainingBalances;

            getBalancesSuccess = onSuccess;
            getBalancesFailure = onFailure;

            CoinModeManager.SendRequest(PlayersWallet.GetBalances(playToken, null, OnGetBalancesSuccess, OnGetBalancesFailure));
            return true;
        }

        private void OnGetBalancesSuccess(PlayersWallet.GetBalancesResponse response)
        {            
            foreach(KeyValuePair<string, WalletBalances> pair in response.balances)
            {
                Pocket pocket;
                if(!_pockets.TryGetValue(pair.Key, out pocket))
                {
                    pocket = new Pocket(pair.Key);
                    _pockets[pair.Key] = pocket;
                }

                pocket.UpdateBalances(pair.Value.confirmed, pair.Value.pending);
            }

            state = PlayerWalletState.Ready;
            getBalancesSuccess?.Invoke(this);
        }

        private void OnGetBalancesFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            getBalancesFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool GetBalance(string playToken, string walletId, PlayerWalletPocketEvent onSuccess, PlayerWalletPocketFailureEvent onFailure)
        {
            if (state != PlayerWalletState.Clean && state != PlayerWalletState.Ready)
            {
                if (state == PlayerWalletState.ObtainingBalances)
                {
                    getBalanceSuccess -= onSuccess;
                    getBalanceSuccess += onSuccess;

                    getBalanceFailure -= onFailure;
                    getBalanceFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayerWalletComponent", "GetBalance", "Cannot get balance while wallet is {0}", state.ToString());
                    return false;
                }
            }

            if(string.IsNullOrEmpty(playToken))
            {
                CoinModeLogging.LogWarning("PlayerWalletComponent", "GetBalance", "Cannot get balance without providing playtoken.", state.ToString());
                return false;
            }

            if (string.IsNullOrEmpty(walletId))
            {
                CoinModeLogging.LogWarning("PlayerWalletComponent", "GetBalance", "Cannot get balance without providing wallet id.");
                return false;
            }

            state = PlayerWalletState.ObtainingBalances;

            getBalanceSuccess = onSuccess;
            getBalanceFailure = onFailure;

            requestedBalanceWalletId = walletId;

            CoinModeManager.SendRequest(PlayersWallet.GetBalances(playToken, walletId, OnGetBalanceSuccess, OnGetBalanceFailure));
            return true;
        }        

        private void OnGetBalanceSuccess(PlayersWallet.GetBalancesResponse response)
        {
            WalletBalances balances;
            if(response.balances.TryGetValue(requestedBalanceWalletId, out balances))
            {
                Pocket pocket;
                if (!_pockets.TryGetValue(requestedBalanceWalletId, out pocket))
                {
                    pocket = new Pocket(requestedBalanceWalletId);
                    _pockets[requestedBalanceWalletId] = pocket;
                }

                pocket.UpdateBalances(balances.confirmed, balances.pending);
            }

            state = PlayerWalletState.Ready;
            getBalanceSuccess?.Invoke(this, requestedBalanceWalletId);
        }

        private void OnGetBalanceFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            getBalanceFailure?.Invoke(this, requestedBalanceWalletId, new CoinModeError(response));
        }

        public bool GetDepositAddress(string playToken, string walletId, bool forceNewAddress, PlayerWalletPocketEvent onSuccess, PlayerWalletPocketFailureEvent onFailure)
        {
            if (state != PlayerWalletState.Ready)
            {
                if (state == PlayerWalletState.Ready)
                {
                    getDepositAddressSuccess -= onSuccess;
                    getDepositAddressSuccess += onSuccess;

                    getDepositAddressFailure -= onFailure;
                    getDepositAddressFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("PlayerWalletComponent", "GetDepositAddress", "Cannot get balance while wallet is {0}", state.ToString());
                    return false;
                }
            }

            if (string.IsNullOrEmpty(playToken))
            {
                CoinModeLogging.LogWarning("PlayerWalletComponent", "GetDepositAddress", "Cannot get balance without providing playtoken.");
                return false;
            }

            if (string.IsNullOrEmpty(walletId))
            {
                CoinModeLogging.LogWarning("PlayerWalletComponent", "GetDepositAddress", "Cannot get balance without providing walletId.");
                return false;
            }

            if(!_pockets.ContainsKey(walletId))
            {
                CoinModeLogging.LogWarning("PlayerWalletComponent", "GetDepositAddress", "Wallet does not contain pocket for {0}, please get balance for {0} first.", walletId);
                return false;
            }

            state = PlayerWalletState.GettingDepositAddress;

            getDepositAddressSuccess = onSuccess;
            getDepositAddressFailure = onFailure;

            requestedDepositWalletId = walletId;

            CoinModeManager.SendRequest(PlayersWallet.GetDepositAddress(playToken, walletId, forceNewAddress, OnGetDepositAddressSuccess, OnGetDepositAddressFailure));
            return true;
        }

        private void OnGetDepositAddressSuccess(PlayersWallet.GetDepositAddressResponse response)
        {
            Pocket pocket;
            if (_pockets.TryGetValue(requestedBalanceWalletId, out pocket))
            {
                pocket.UpdateDepositAddess(response.address);
            }

            state = previousState;
            getDepositAddressSuccess?.Invoke(this, requestedDepositWalletId);
        }

        private void OnGetDepositAddressFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            getDepositAddressFailure?.Invoke(this, requestedDepositWalletId, new CoinModeError(response));
        }
    }
}