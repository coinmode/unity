using UnityEngine;

namespace CoinMode
{
    public static class CoinModeLogging
    {
        internal enum LoggingMode
        {
            LogAll = 0,
            WarningsAndErrors = 1,
            OnlyErrors = 2,
            None = 3,
        }

        private enum LogType
        {
            Message = 0,
            Warning = 1,
            Error = 2,
        }

        private static LoggingMode mode { get { return CoinModeSettings.loggingMode; } }

        public static void LogMessage(string originClass, string originFunction, string message, params object[] args)
        {
            if(mode < LoggingMode.WarningsAndErrors)
            {
                Log(LogType.Message, originClass, originFunction, message, args);
            }            
        }

        public static void LogWarning(string originClass, string originFunction, string message, params object[] args)
        {
            if(mode < LoggingMode.OnlyErrors)
            {
                string s = Log(LogType.Warning, originClass, originFunction, message, args);
                Analytics.RecordWarning(s);
            }            
        }

        public static void LogError(string originClass, string originFunction, string message, params object[] args)
        {
            if(mode < LoggingMode.None)
            {
                string s = Log(LogType.Error, originClass, originFunction, message, args);
                Analytics.RecordError(s);
            }            
        }

        private static string Log(LogType type, string originClass, string originFunction, string message, params object[] args)
        {
#if UNITY_EDITOR
            string fullMessage = "<color=#fdb714>" + BuildLogString(originClass, originFunction, message, args);
#else
            string fullMessage = BuildLogString(originClass, originFunction, message, args);
#endif
            switch (type)
            {
                case LogType.Message:
                    Debug.Log(fullMessage);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(fullMessage);
                    break;
                case LogType.Error:
                    Debug.LogError(fullMessage);
                    break;
            }
            return fullMessage;
        }

        private static string BuildLogString(string originClass, string originFunction, string message, params object[] args)
        {
            string userMessage = string.Format(message, args);
            return string.Format("[{0}.{1}]</color> {2}", originClass, originFunction, userMessage);
        }
    }
}
