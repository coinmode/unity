using System;
using System.Collections.Generic;
using CoinMode.NetApi;

namespace CoinMode
{
    [System.Serializable]
    public sealed class GameComponent : CoinModeComponent
    {
        public delegate void GameRoundEvent(GameComponent game, MinimalRoundInfo[] rounds);

        public delegate void GameEvent(GameComponent game);
        public delegate void GameFailureEvent(GameComponent game, CoinModeError error);

        public enum GameState
        {
            Clean,
            Ready,
            GettingRounds,
        }

        public string localAlias { get; private set; } = "";
        public string gameId { get { return properties != null && properties.gameId != null ? properties.gameId : ""; } }
        public string name { get { return properties != null && properties.name != null ? properties.name : ""; } }
        public string walletId { get { return properties != null && properties.walletId != null ? properties.walletId : ""; } }
        public double createRoundFee { get { return properties != null && properties.feeCreateNewRound != null ? properties.feeCreateNewRound.Value : 0.0D; } }
        public double playFee { get { return properties != null && properties.feePlaySession != null ? properties.feePlaySession.Value : 0.0D; } }
        public PayoutType payoutType { get { return properties != null && properties.payoutTypeId != null ? (PayoutType)properties.payoutTypeId.Value : PayoutType.None; } }

        private GameInfo properties { get; set; } = null;

        public GameState state
        {
            get { return _state; }
            private set
            {
                previousState = _state;
                _state = value;
            }
        }
        private GameState _state = GameState.Clean;
        private GameState previousState = GameState.Clean;

        private HashSet<RoundComponent> rounds = new HashSet<RoundComponent>();
        private Dictionary<string, RoundComponent> assignedRounds = new Dictionary<string, RoundComponent>();

        private GameRoundEvent listRoundsSuccess;
        private GameFailureEvent listRoundsFailure;

        internal GameComponent(string localAlias, GameInfo properties) 
        {
            if (properties == null)
            {
                CoinModeLogging.LogWarning("GameComponent", "GameComponent()", "Provided properties for game {0} are null!", localAlias);
            }

            this.localAlias = localAlias;            
            this.properties = properties;
            state = this.properties != null ? GameState.Ready : GameState.Clean;
        }

        public RoundComponent FindOrConstructRound(string roundId = "")
        {
            if (state != GameState.Ready)
            {
                CoinModeLogging.LogWarning("GameComponent", "ConstructRound", "Cannot construct round for game while game {0} is {1}", properties.gameId, state.ToString());
                return null;
            }

            RoundComponent round;
            if (!string.IsNullOrEmpty(roundId))
            {                
                if (TryGetRound(roundId, out round))
                {
                    CoinModeLogging.LogMessage("GameComponent", "ConstructRound",
                        "Round already exists with id {0} for game {1}, a new round was not constructed, instead the existing round component was returned.",
                        roundId, properties.gameId);
                    return round;
                }
            }

            round = new RoundComponent(this, roundId);
            if (!string.IsNullOrEmpty(roundId))
            {
                assignedRounds.Add(roundId, round);
            }
            else
            {
                rounds.Add(round);
            }
            return round;
        }

        internal void RoundComponentIdAssigned(RoundComponent round, string oldId)
        {
            if (!string.IsNullOrEmpty(oldId))
            {
                RoundComponent oldAssignedRound = null;
                if (assignedRounds.TryGetValue(oldId, out oldAssignedRound))
                {
                    if (round == oldAssignedRound)
                    {
                        assignedRounds.Remove(oldId);
                    }
                    else
                    {
                        CoinModeLogging.LogWarning("GameComponent", "RoundComponentIdAssigned", "Cannot remove existing assigned round as it does not match the round passed in for id {0}",
                            oldId);
                    }
                }
            }

            if (string.IsNullOrEmpty(round.roundId))
            {
                if(!rounds.Contains(round))
                {
                    rounds.Add(round);
                }                
            }
            else
            {
                RoundComponent newAssignedRound = null;
                if (assignedRounds.TryGetValue(round.roundId, out newAssignedRound))
                {
                    if (round == newAssignedRound)
                    {
                        CoinModeLogging.LogWarning("GameComponent", "RoundComponentIdAssigned", "Cannot add assigned round {0} as it has already been assigned at this id",
                            round.roundId);
                    }
                    else
                    {
                        CoinModeLogging.LogWarning("GameComponent",
                            "RoundComponentIdAssigned", "Cannot add assigned round {0} as a different round component has already been assigned this id",
                            round.roundId);
                    }
                }
                else
                {
                    assignedRounds.Add(round.roundId, round);
                }
            }
        }

        public bool ListRounds(PlayerComponent player, bool listReadyToJoin, bool listInProgress, bool listCompleted, GameRoundEvent onSuccess, GameFailureEvent onFailure)
        {
            if (state != GameState.Ready)
            {
                if (state == GameState.GettingRounds)
                {
                    listRoundsSuccess -= onSuccess;
                    listRoundsSuccess += onSuccess;

                    listRoundsFailure -= onFailure;
                    listRoundsFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("GameComponent", "ListRounds", "Cannout list rounds while game {0} is {1}", localAlias, state);
                    return false;
                }
            }

            if (player == null)
            {
                CoinModeLogging.LogWarning("GameComponent", "ListRounds", "Cannot list rounds, player is null");
                return false;
            }

            if (string.IsNullOrWhiteSpace(player.playToken))
            {
                CoinModeLogging.LogWarning("GameComponent", "ListRounds", "Players playToken is empty!");
                return false;
            }

            listRoundsSuccess = onSuccess;
            listRoundsFailure = onFailure;

            state = GameState.GettingRounds;
            CoinModeManager.SendRequest(GamesRound.List(gameId, player.publicId, player.playToken, listReadyToJoin, listInProgress, listCompleted, null, null, OnListRoundsSuccess, OnListRoundsFailure));
            return true;
        }

        private void OnListRoundsSuccess(GamesRound.ListRoundsResponse response)
        {
            state = GameState.Ready;
            listRoundsSuccess?.Invoke(this, response.rounds);
        }

        private void OnListRoundsFailure(CoinModeErrorResponse response)
        {
            _state = previousState;
            listRoundsFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool ContainsRound(RoundComponent round)
        {
            return rounds.Contains(round) || !string.IsNullOrEmpty(round.roundId) ? 
                assignedRounds.ContainsKey(round.roundId) && assignedRounds[round.roundId] == round : assignedRounds.ContainsValue(round);
        }

        public bool ContainsRound(string roundId)
        {
            if(assignedRounds.ContainsKey(roundId))
            {
                return true;
            }

            bool containsRound = false;
            foreach (RoundComponent round in rounds)
            {
                if (round.roundId == roundId)
                {
                    containsRound = true;
                    break;
                }
            }
            return containsRound;
        }

        public bool RemoveRound(string roundId)
        {            
            if(assignedRounds.Remove(roundId))
            {
                return true;
            }
            RoundComponent round = null;
            if (TryGetUnassignedRound(roundId, out round))
            {
                return rounds.Remove(round);
            }
            return false;
        }

        public bool RemoveRound(RoundComponent round)
        {
            if (assignedRounds.Remove(round.roundId))
            {
                return true;
            }
            if (rounds.Remove(round))
            {
                return true;
            }
            return false;
        }

        private bool TryGetRound(string roundId, out RoundComponent round)
        {
            round = null;
            if (assignedRounds.TryGetValue(roundId, out round))
            {
                return true;
            }
            if (TryGetUnassignedRound(roundId, out round))
            {
                return true;
            }
            return false;
        }

        public void ClearRounds()
        {
            rounds.Clear();
            assignedRounds.Clear();
        }

        private bool TryGetUnassignedRound(string roundId, out RoundComponent round)
        {
            round = null;
            bool containsRound = false;
            foreach (RoundComponent r in rounds)
            {
                if (r.roundId == roundId)
                {
                    round = r;
                    containsRound = true;
                    break;
                }
            }
            return containsRound;
        }        

        public bool TryGetGameWallet(out Wallet gameWallet)
        {
            return CoinModeManager.walletComponent.TryGetWallet(walletId, out gameWallet);
        }
    }
}
