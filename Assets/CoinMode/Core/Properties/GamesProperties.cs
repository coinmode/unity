using LightJson;

namespace CoinMode
{
    [System.Serializable]
    public class GameInfo : CoinModeProperties
    {
        public string gameId { get; private set; } = null;
        public string titleId { get; private set; } = null;
        public string name { get; private set; } = null;
        public string description { get; private set; } = null;
        public bool? enabled { get; private set; } = null;
        public bool? requiresAccountVerificationSupport { get; private set; } = null;
        public bool? listPublically { get; private set; } = null;
        public int? epochCreated { get; private set; } = null;
        public int? epochOpenDate { get; private set; } = null;
        public int? epochCloseDate { get; private set; } = null;
        public int? epochUpdated { get; private set; } = null;
        public int? verifiedFairEpoch { get; private set; } = null;
        public float? roundDurationSeconds { get; private set; } = null;
        public int? autoCreateRounds { get; private set; } = null;
        public int? remainingRounds { get; private set; } = null;
        public float? maxTimePerPlay { get; private set; } = null;
        public string urlToRunGame { get; private set; } = null;
        public string whiteListGameOrigins { get; private set; } = null;
        public string mainImageUrl { get; private set; } = null;
        public string screenshots { get; private set; } = null;
        public string keywords { get; private set; } = null;
        public string supportedPlatforms { get; private set; } = null;
        public bool? allowRepeatPlayInGame { get; private set; } = null;
        public bool? allowRepeatPlayInRound { get; private set; } = null;
        public bool? permissionSubscription { get; private set; } = null;
        public bool? permissionEmail { get; private set; } = null;
        public bool? permissionPhone { get; private set; } = null;
        public bool? permissionChargeToPlay { get; private set; } = null;
        public bool? permissionChargeIap { get; private set; } = null;
        public bool? requireLockToStartRound { get; private set; } = null;
        public string walletId { get; private set; } = null;
        public int? payoutTypeId { get; private set; } = null;
        public string payoutTypeText { get; private set; } = null;
        public int? roundTypeIdNextRound { get; private set; } = null;
        public string roundTypeIdNextRoundText { get; private set; } = null;
        public double? fixedPotAmount { get; private set; } = null;
        public int? payoutSourceId { get; private set; } = null;
        public string payoutSourceIdText { get; private set; } = null;
        public double? feeCreateNewRound { get; private set; } = null;
        public double? feePlaySession { get; private set; } = null;
        public double? coinModeFeePercent { get; private set; } = null;
        public double? coinModeNewRoundPercent { get; private set; } = null;
        public string authorPlayerDisplayName { get; private set; } = null;
        public string authorPlayerImageUrl { get; private set; } = null;
        public int? authorPlayerEpochDeveloper { get; private set; } = null;
        public string authorPlayerCountry { get; private set; } = null;
        public string highlightDataSchema { get; private set; } = null;
        public string ratingIngamePurchases { get; private set; } = null;
        public string ratingGambling { get; private set; } = null;
        public string ratingFear { get; private set; } = null;
        public string ratingDrugs { get; private set; } = null;
        public string ratingSex { get; private set; } = null;
        public string ratingStereotypes { get; private set; } = null;
        public string ratingSwearing { get; private set; } = null;
        public string ratingViolence { get; private set; } = null;
        public float? starRatingTally { get; private set; } = null;
        public int? starRatingCount { get; private set; } = null;
        public float? starRatingNormalised { get; private set; } = null;
        public bool? playerCanPlay { get; private set; } = null;

        internal GameInfo() { }

        internal override void FromJson(JsonObject json)
        {
            gameId = json["game_id"];
            titleId = json["title_id"];
            name = json["name"];
            description = json["description"];
            enabled = json["enabled"];
            requiresAccountVerificationSupport = json["requires_account_with_verification_method"];
            listPublically = json["list_publically"];
            epochCreated = json["epoch_created"];
            epochOpenDate = json["epoch_opendate"];
            epochCloseDate = json["epoch_closedate"];
            epochUpdated = json["epoch_updated"];
            verifiedFairEpoch = json["verified_fair_epoch_time"];
            roundDurationSeconds = json["round_duration_in_seconds"];
            autoCreateRounds = json["auto_create_rounds"];
            remainingRounds = json["remaining_rounds"];
            maxTimePerPlay = json["max_time_per_play"];
            urlToRunGame = json["url_to_run_game"];
            whiteListGameOrigins = json["whitelist_game_origins"];
            mainImageUrl = json["main_image_url"];
            screenshots = json["screenshots"];
            keywords = json["keywords"];
            supportedPlatforms = json["supported_platforms"];
            allowRepeatPlayInGame = json["allow_repeat_play_in_game"];
            allowRepeatPlayInRound = json["allow_repeat_play_in_round"];
            permissionSubscription = json["permission_subscription"];
            permissionEmail = json["permission_email"];
            permissionPhone = json["permission_phone"];
            permissionChargeToPlay = json["permission_charge_to_play"];
            permissionChargeIap = json["permission_charge_iap"];
            requireLockToStartRound = json["require_lock_to_start_round"];
            walletId = json["wallet"];
            if (json.ContainsKey("payout_type_id"))
            {
                payoutTypeId = json["payout_type_id"];
            }
            else
            {
                payoutTypeId = json["roundtype_id_next_round"];                
            }
            payoutTypeText = json["payout_type_text"];
            roundTypeIdNextRound = json["roundtype_id_next_round"];
            roundTypeIdNextRoundText = json["roundtype_id_next_round_text"];
            fixedPotAmount = json["fixed_pot_amount"];
            payoutSourceId = json["payout_source_id"];
            payoutSourceIdText = json["payout_source_text"];
            feeCreateNewRound = json["fee_create_new_round"];
            feePlaySession = json["fee_play_session"];
            coinModeFeePercent = json["coinmode_fee_percent"];
            coinModeNewRoundPercent = json["coinmode_new_round_percent"];
            authorPlayerDisplayName = json["author_player_display_name"];
            authorPlayerImageUrl = json["author_player_image_url"];
            authorPlayerEpochDeveloper = json["author_player_epoch_developer"];
            authorPlayerCountry = json["author_player_country"];
            highlightDataSchema = json["highlight_data_schema"];
            ratingIngamePurchases = json["rating_ingame_purchases"];
            ratingGambling = json["rating_gambling"];
            ratingFear = json["rating_fear"];
            ratingDrugs = json["rating_drugs"];
            ratingSex = json["rating_sex"];
            ratingStereotypes = json["rating_stereotypes"];
            ratingSwearing = json["rating_swearing"];
            ratingViolence = json["rating_violence"];
            starRatingTally = json["star_rating_tally"];
            starRatingCount = json["star_rating_count"];
            starRatingNormalised = json["star_rating_normalised"];
            playerCanPlay = json["player_can_play"];
        }
    }
}
