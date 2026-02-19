namespace MyAssets.GameCore
{
    public enum PlayerCountMode { Four, FiveSix }

    public sealed class SetupRules
    {
        public PlayerCountMode Mode;

        // Non-starting slots are 15 total minus 4 starters = 11 placements.
        public int NonStartingSlotsToFill = 11;

        // Composition pool sizes (includes the “one extra not used” concept)
        public int PlanetaryPool;
        public int TradePool;
        public int EmptyPool;

        // Star breakdown for each pool
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
                    PlanetaryPool = 8, PlanetaryOneStar = 5, PlanetaryTwoStar = 3,
                    TradePool = 4, TradeOneStar = 2, TradeTwoStar = 2,
                    EmptyPool = 4, EmptyOneStar = 2, EmptyTwoStar = 2,
                    NonStartingSlotsToFill = 11
                };
            }

            // 5–6 expansion
            return new SetupRules
            {
                Mode = mode,
                PlanetaryPool = 10, PlanetaryOneStar = 6, PlanetaryTwoStar = 4,
                TradePool = 5, TradeOneStar = 2, TradeTwoStar = 3,
                EmptyPool = 1, EmptyOneStar = 1, EmptyTwoStar = 0,
                NonStartingSlotsToFill = 11
            };
        }
    }
}
