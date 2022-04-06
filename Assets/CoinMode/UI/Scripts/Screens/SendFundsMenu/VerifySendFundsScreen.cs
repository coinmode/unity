using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoinMode.NetApi;
using System;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM VerifySendFundsScreen")]
    public class VerifySendFundsScreen : VerifyScreen
    {
        public struct ScreenData
        {
            public PlayerComponent player { get; private set; }
            public UserComponent user { get; private set; }
            public PlayerWalletComponent.Pocket pocket { get; private set; }
            public VerificationComponent verification { get; private set; }
            public string paymentId { get; private set; }            

            public ScreenData(PlayerComponent player, UserComponent user, PlayerWalletComponent.Pocket pocket, VerificationComponent verification, string paymentId)
            {
                this.player = player;
                this.user = user;
                this.pocket = pocket;
                this.verification = verification;
                this.paymentId = paymentId;
            }
        }

        public override bool requiresData { get; } = true;
        private ScreenData screenData = new ScreenData();

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);
            base.OnOpen(screenData.verification);                        
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        protected override void VerifyAction()
        {
            base.VerifyAction();
            CoinModeManager.SendRequest(PlayersWalletTransferFunds.Verify(screenData.player.publicId, screenData.paymentId, verificationComponent.GetKeys(),
                OnVerifyTransferFundsSuccess, OnVerifyTransferFundsFailure));
        }

        private void OnVerifyTransferFundsSuccess(PlayersWalletTransferFunds.VerifyTransferResponse response)
        {
            verificationComponent.UpdateErrors(response.failedVerification);
            if((PaymentStatus)response.paymentStatusId == PaymentStatus.Completed)
            {
                OnVerifySuccess();
                TransferCompleteScreen.ScreenData data = new TransferCompleteScreen.ScreenData(screenData.player, screenData.user, screenData.pocket);
                controller.SwitchScreen<TransferCompleteScreen>(data);                
            }
            else
            {
                OnVerifyUpdate();
            }            
        }

        private void OnVerifyTransferFundsFailure(CoinModeErrorResponse response)
        {
            OnVerifyFailure();
        }

        protected override void RejectAction()
        {            
            base.RejectAction();
        }
    }
}