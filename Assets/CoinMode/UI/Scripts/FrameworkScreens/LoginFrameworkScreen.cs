using CoinMode.NetApi;
using System;
using System.Collections.Generic;

namespace CoinMode.UI
{
    public abstract class LoginFrameworkScreen : CoinModeMenuScreen
    {
        protected abstract CoinModeButton invokingButton { get; }

        protected PlayerComponent player
        {
            get { return _player; }
            set
            {
                _player = value;
                if(loginComponent != null)
                {
                    loginComponent.player = _player;
                }
            }
        }
        protected PlayerComponent _player = null;

        internal LoginFrameworkComponent loginComponent { get; private set; } = null;

        protected override void Start()
        {
            base.Start();
            LoginFrameworkComponent.GetButtonEvent getButtonEvent = delegate ()
            {
                return invokingButton;
            };
            loginComponent = new LoginFrameworkComponent(controller, getButtonEvent);
            loginComponent.player = _player;
        }
    }
}
