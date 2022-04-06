using System;

namespace CoinMode
{
    /// <summary>
    /// Enum used to describe each possible state of a round according to cmapi.
    /// </summary>
    public enum PlayStatus
    {
        /// <summary>Unknown</summary>
        Unknown = 0,
        /// <summary>WaitingToPlay</summary>
        WaitingToPlay = 1,
        /// <summary>InProgress</summary>
        InProgress = 2,
        /// <summary>Completed</summary>
        Completed = 3,
        /// <summary>Quit</summary>
        Quit = 4,
        /// <summary>TimedOut</summary> 
        TimedOut = 5,
        /// <summary>AwaitingVerification</summary>
        AwaitingVerification = 6,
        /// <summary>Refunded</summary>
        Refunded = 7,
        /// <summary>Error</summary>
        Error = 8,
    }

    /// <summary>
    /// Enum used to describe the conditions required for a round to end.
    /// Set in game info on the CoinMode website.
    /// </summary>
    public enum RoundCompletionType
    {
        /// <summary></summary>
        Invalid = 0,
        /// <summary></summary>
        EpochToFinish = 1,
        /// <summary></summary>
        AllSessionsPlayed = 2,
        /// <summary></summary>
        RoundEnd = 3,
    }

    /// <summary>
    /// Enum used to describe the way in which a payout is given at the end of a round.
    /// Set in game info on the CoinMode website.
    /// </summary>
    public enum PayoutType
    {
        /// <summary>no_payout</summary>
        None = 0,
        /// <summary>winner_takes_all</summary>
        WinnerTakesAll = 1,
        /// <summary>score_distribution</summary>
        ScoreDistribution = 2
    }

    /// <summary>
    /// Enum used to describe source of the payout for a round.
    /// Set in game info on the CoinMode website.
    /// </summary>
    public enum PayoutSource
    {
        /// <summary></summary>
        AuthorOnly = 0,
        /// <summary></summary>
        CoinModePromo = 1,
        /// <summary></summary>
        Advertiser = 2,
    }

    /// <summary>
    /// Enum used to describe the permissions a playtoken may have.
    /// Set in title info on the CoinMode website.
    /// </summary>
    [Flags]
    public enum PlaytokenPermissions
    {
        /// <summary>No permissions.</summary>
        None = 0,
        /// <summary>Permission to add bank account.</summary>
        AddBank = 1,
        /// <summaryPermission to send your funds to another player directly. (Only allow this for official CoinMode projects)</summary>
        AllowDirectPayments = 2,
        /// <summary>Permission to charge you entry fees for joining a round.</summary>
        AllowsPayForEntry = 4,
        /// <summary>Permission to read your balance and transaction history.</summary>
        ReadTransactionHistory = 8,
        /// <summary>Allows editing player properties.</summary>
        EditPlayer = 16,
        /// <summary>Permission to list bank accounts.</summary>
        ListBanks = 32,
        /// <summary>Permission to read registered email with CoinMode.</summary>
        ReadEmail = 64,
        /// <summary>Permission to read full location data stored with CoinMode.</summary>
        ReadHomeAddress = 128,
        /// <summary>Permission to read registered mobile number with CoinMode.</summary>
        ReadMobile = 256,
        /// <summary>Permission to remove bank account.</summary>
        RemoveBank = 512,
        /// <summary>Allow swapping one currency to another in your own account.</summary>
        WalletExhange = 1024
    }

    public enum PaymentStatus
    {
        None = 0,
        AwaitingVerification = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        InviteInitiating = 5,
        InviteCancelled = 6,
        InviteExpire = 7
    }

    public enum LocationType
    {
        Geolocation = 0,
        TwoDimensional = 1,
        ThreeDimensional = 2,
        GameLobby = 3
    }

    public static class CoinModeParamHelpers
    {
        public static PlaytokenPermissions PermissionFromString(string permission)
        {
            switch (permission)
            {
                case "add_bank":
                    return PlaytokenPermissions.AddBank;
                case "allows_direct_payments":
                    return PlaytokenPermissions.AllowDirectPayments;
                case "allows_pay_for_entry":
                    return PlaytokenPermissions.AllowsPayForEntry;
                case "allows_reading_tx_history":
                    return PlaytokenPermissions.ReadTransactionHistory;
                case "edit_player":
                    return PlaytokenPermissions.EditPlayer;
                case "list_banks":
                    return PlaytokenPermissions.ListBanks;
                case "read_email":
                    return PlaytokenPermissions.ReadEmail;
                case "read_home_address":
                    return PlaytokenPermissions.ReadHomeAddress;
                case "read_mobile":
                    return PlaytokenPermissions.ReadMobile;
                case "remove_bank":
                    return PlaytokenPermissions.RemoveBank;
                case "wallet_exchange":
                    return PlaytokenPermissions.WalletExhange;
            }
            return PlaytokenPermissions.None;
        }

        public static PayoutType PayoutTypeFromString(string permission)
        {
            switch (permission)
            {
                case "no_payout":
                    return PayoutType.None;
                case "winner_takes_all":
                    return PayoutType.WinnerTakesAll;
                case "score_distribution":
                    return PayoutType.ScoreDistribution;
            }
            return PayoutType.None;
        }

        public static string PayoutTypeUserString(PayoutType type)
        {
            switch (type)
            {
                default:
                case PayoutType.None:
                    return "None";
                case PayoutType.WinnerTakesAll:
                    return "Winner Takes All";
                case PayoutType.ScoreDistribution:
                    return "Score Distribution";
            }
        }
    }
}
