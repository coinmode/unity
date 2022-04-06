using System.Collections.Generic;
using UnityEngine.Events;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    public abstract class SessionFrameworkScreen : CoinModeMenuScreen
    {
        protected abstract CoinModeButton invokingButton { get; }

        protected JoinRoundScreenData screenData = new JoinRoundScreenData();

        private string passphrase = null;

        protected bool IsPasswordValid()
        {
            return !string.IsNullOrWhiteSpace(passphrase);
        }

        protected void UpdatePassphrase(string passphrase)
        {
            this.passphrase = string.IsNullOrWhiteSpace(passphrase) ? null : passphrase;
        }

        protected void RequestSession()
        {
            if (screenData.round.playFee > 0.0D || screenData.round.potContribution > 0.0D)
            {
                CurrencyConversion conversion;
                Wallet gameWallet = null;
                screenData.game.TryGetGameWallet(out gameWallet);

                double cost = screenData.round.playFee + screenData.round.potContribution;
                CoinModeManager.walletComponent.ConvertBaseUnitToCurrency(gameWallet, cost, screenData.player.displayCurrencyKey, out conversion);                

                List<CoinModeModalWindow.ModalWindowAction> actions = new List<CoinModeModalWindow.ModalWindowAction>();
                actions.Add(new CoinModeModalWindow.ModalWindowAction("Yes", new UnityAction(RequestSessionConfirmed)));
                actions.Add(new CoinModeModalWindow.ModalWindowAction("No", null));
                controller.OpenModalWindow(string.Format("Resquesting this session costs {0}\nDo you want to continue?", conversion.targetCurrencyString), actions);
            }
            else
            {
                RequestSessionConfirmed();
            }            
        }

        private void RequestSessionConfirmed()
        {
            controller.Disable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            screenData.session.Request(screenData.player, passphrase, "", OnRequestSessionSuccess, OnRequestSessionFailure);
        }

        private void OnRequestSessionSuccess(SessionComponent session)
        {
            screenData.player.AssignSession(session);
            controller.DisplayMessage("Session Requested!", CoinModeMenu.MessageType.Success);
            if (screenData.startSessionImmediately)
            {
                StartSession();
            }
            else
            {
                controller.Enable();
                invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);                
                controller.OnPlayGameSuccess();
            }            
        }

        private void OnRequestSessionFailure(SessionComponent session, CoinModeError error)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        protected void StartSession()
        {            
            if (screenData.session.Start(screenData.player, OnStartSessionSuccess, OnStartSessionFailure))
            {
                controller.Disable();
                invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            }
        }

        private void OnStartSessionSuccess(SessionComponent session)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.DisplayMessage("Session Started!", CoinModeMenu.MessageType.Success);
            controller.OnPlayGameSuccess();
        }

        private void OnStartSessionFailure(SessionComponent session, CoinModeError error)
        {
            controller.Enable();
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }
    }
}
