using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LightJson;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    public abstract class CreateRoundFrameworkScreen : CoinModeMenuScreen
    {
        protected abstract CoinModeButton invokingButton { get; }
        protected abstract PlayerComponent player { get; }
        protected abstract GameComponent game { get; }

        protected abstract string roundName { get; }
        protected abstract double potContribution { get; }
        protected abstract string localPassphrase { get; }

        protected abstract JsonObject customJson { get; }
        protected abstract string serverArgs { get; }
        protected abstract bool startSessionImmediately { get; }
        protected abstract SessionComponent existingChallengeSession { get; }

        protected RoundComponent createdRound { get { return _createdRound; } }
        private RoundComponent _createdRound = null;

        protected SessionComponent createdSession { get { return _createdSession; } }
        private SessionComponent _createdSession = null;

        protected void CreateRound()
        {
            controller.Disable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            _createdRound = game.FindOrConstructRound();
            _createdRound.Create(player, null, true, potContribution, roundName,
                localPassphrase, customJson, serverArgs, OnRoundCreateSuccess, OnRoundCreateFailure);
        }

        private void OnRoundCreateSuccess(RoundComponent round)
        {
            GetRoundInfo();
        }

        private void OnRoundCreateFailure(RoundComponent round, CoinModeError error)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        private void GetRoundInfo()
        {
            // Can we initialize a round from an existing object instead of forcefully calling get info?
            controller.Disable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            _createdRound.GetInfo(player, OnRoundGetInfoSuccess, OnRoundGetInfoFailure);
        }

        private void OnRoundGetInfoSuccess(RoundComponent round)
        {
            RequestSession();
        }

        private void OnRoundGetInfoFailure(RoundComponent round, CoinModeError error)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.OpenModalWindow(error.userMessage, new UnityAction(GetRoundInfo), CoinModeMenu.MessageType.Error);
        }

        private void RequestSession()
        {
            controller.Disable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            _createdRound.ConstructSession(out _createdSession);
            _createdSession.Request(player, localPassphrase , null, OnRequestSessionSuccess, OnRequestSessionFailure);
        }

        private void OnRequestSessionSuccess(SessionComponent session)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.DisplayMessage("Session requested!", CoinModeMenu.MessageType.Success);

            player.AssignSession(session);

#if UNITY_2019_3_OR_NEWER
            if (startSessionImmediately || existingChallengeSession != null)
#else
            if (screenData.startSessionImmediately)
#endif
            {
                StartSession();
            }
            else
            {
                controller.OnCreateRoundSuccess(_createdSession);
            }
        }

        private void OnRequestSessionFailure(SessionComponent session, CoinModeError error)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.OpenModalWindow(error.userMessage, new UnityAction(RequestSession), CoinModeMenu.MessageType.Error);
        }

        private void StartSession()
        {
            if (_createdSession.Start(player, OnStartSessionSuccess, OnStartSessionFailure))
            {
                controller.Disable();
                invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            }
        }

        private void OnStartSessionSuccess(SessionComponent session)
        {
#if UNITY_2019_3_OR_NEWER
            if (existingChallengeSession != null)
            {
                StopSession();
            }
            else
            {
                controller.Enable();
                invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                controller.DisplayMessage("Session Started!", CoinModeMenu.MessageType.Success);
                controller.OnCreateRoundSuccess(_createdSession);
            }
#else
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.DisplayMessage("Session Started!", CoinModeMenu.MessageType.Success);
            controller.OnCreateRoundSuccess(_createdSession);
#endif
        }

        private void OnStartSessionFailure(SessionComponent session, CoinModeError error)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.OpenModalWindow(error.userMessage, new UnityAction(StartSession), CoinModeMenu.MessageType.Error);
        }

#if UNITY_2019_3_OR_NEWER
        private void StopSession()
        {
            if (_createdSession.Stop(existingChallengeSession.score, existingChallengeSession.formattedScore,
                existingChallengeSession.sessionData, OnStopSessionSuccess, OnStopSessionFailure))
            {
                controller.Disable();
                invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            }
        }

        private void OnStopSessionSuccess(SessionComponent session)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.DisplayMessage("Challenge Session Submitted!", CoinModeMenu.MessageType.Success);
            controller.OnChallengeFromExistingSuccess(_createdSession);
        }

        private void OnStopSessionFailure(SessionComponent session, CoinModeError error)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.OpenModalWindow(error.userMessage, new UnityAction(StopSession), CoinModeMenu.MessageType.Error);
        }
#endif
    }
}
