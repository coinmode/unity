using CoinMode.NetApi;
using System.Collections.Generic;

namespace CoinMode
{
    public delegate void CoinModeFailure(CoinModeError error);
    public delegate void CoinModeMultiFailure(CoinModeMultiError errors);

    public abstract class CoinModeErrorBase
    {
        public enum ErrorType
        {
            Network,
            Http,
            CoinMode,
            Client,
            Generic,
            Unknown,
        }

        public ErrorType type { get; protected set; } = ErrorType.Unknown;
    }

    public class CoinModeError : CoinModeErrorBase
    {                
        public string error { get; private set; } = "";
        public string userMessage { get; private set; } = "";
        
        internal CoinModeError(CoinModeErrorResponse errorResponse)
        {
            if(errorResponse.networkError)
            {
                type = ErrorType.Network;
            }
            else if(errorResponse.httpError)
            {
                type = ErrorType.Http;
            }
            else
            {
                type = ErrorType.CoinMode;
            }

            error = errorResponse.coinModeError != "" ? errorResponse.coinModeError : errorResponse.userMessage;
            userMessage = errorResponse.userMessage;
        }
        
        internal CoinModeError(ErrorType type, string error, string message)
        {
            this.type = type;
            this.error = error;
            this.userMessage = message;
        }
    }

    public class CoinModeMultiError : CoinModeErrorBase
    {
        public Dictionary<string, string> errors { get; private set; } = null;

        internal CoinModeMultiError(ErrorType type, Dictionary<string, string> errors)
        {
            this.type = type;
            this.errors = errors;
        }
    }
}
