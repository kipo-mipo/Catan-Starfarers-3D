namespace MyAssets.GameCore
{
    public enum PlayerCountMode
    {
        Four,
        FiveSix
    }

    public sealed class SetupRules
    {
        public PlayerCountMode Mode;

        // Current board layout has 19 sector slots and 4 starting slots.
        // 19 - 4 = 15 non-starting sector placements.
        public int NonStartingSlotsToFill = 15;

        // Pool sizes include one extra unused piece.
        // For 15 placed sectors, total pool size should be 16.
        public int PlanetaryPool;
        public int TradePool;
        public int EmptyPool;

        public int PlanetaryOneStar;
        public int PlanetaryTwoStar;

        public int TradeOneStar;
        public int TradeTwoStar;

        public int EmptyOneStar;
        public int EmptyTwoStar;

        public static SetupRules For(PlayerCountMode mode)
        {
            if (mode == PlayerCountMode.Four)
            {
                return new SetupRules
                {
                    Mode = mode,

                    PlanetaryPool = 8,
                    PlanetaryOneStar = 5,
                    PlanetaryTwoStar = 3,

                    TradePool = 4,
                    TradeOneStar = 2,
                    TradeTwoStar = 2,

                    EmptyPool = 4,
                    EmptyOneStar = 2,
                    EmptyTwoStar = 2,

                    NonStartingSlotsToFill = 15
                };
            }

            return new SetupRules
            {
                Mode = mode,

                PlanetaryPool = 10,
                PlanetaryOneStar = 6,
                PlanetaryTwoStar = 4,

                TradePool = 5,
                TradeOneStar = 2,
                TradeTwoStar = 3,

                EmptyPool = 1,
                EmptyOneStar = 1,
                EmptyTwoStar = 0,

                NonStartingSlotsToFill = 15
            };
        }
    }
}
