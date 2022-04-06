using System;

namespace CoinMode
{
    public enum CryptoWalletUnit
    {
        Default,
        Base
    }

    public enum CyptoWallet
    {
        Bitcoin = 0,
        BitcoinTest = 1,
        EURX = 2,
        GBPX = 3,
        USDX = 4,
    }

    public static class CoinModeWallet
    {
        private static double doubleRoundLimit = 1e16d;
        private const int maxRoundingDigits = 15;
        private static double[] roundPower10Double = new double[] {
          1E0, 1E1, 1E2, 1E3, 1E4, 1E5, 1E6, 1E7, 1E8,
          1E9, 1E10, 1E11, 1E12, 1E13, 1E14, 1E15
        };

        private static readonly string[] CryptoWalletIds =
        {
            "bitcoin_main",
            "bitcoin_test",
            "eurx_main",
            "gbpx_main",
            "usdx_main"
        };

        public static double RoundTowardZero(double value, int digits)
        {
            if ((digits < 0) || (digits > maxRoundingDigits))
                throw new ArgumentOutOfRangeException("digits", digits, "is less than 0 or greater than 15.");
            if (Math.Abs(value) < doubleRoundLimit)
            {
                double power10 = roundPower10Double[digits];
                value *= power10;
                value = Math.Floor(value);
                value /= power10;
            }
            return value;
        }        

        public static string GetCryptoWalletId(CyptoWallet type)
        {
            return CryptoWalletIds[(int)type];
        }      
    }
}
