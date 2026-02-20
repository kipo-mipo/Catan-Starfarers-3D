namespace MyAssets.GameCore
{
    public enum TokenState
    {
        Hidden,
        Assigned,
        SpecialTokenPresent,
        ClearedAssignedFromReserve
    }

    public readonly struct PlanetTokenState
    {
        public readonly TokenState State;
        public readonly TokenPoolId Pool;
        public readonly int? TokenId;
        public readonly int? RollA;
        public readonly int? RollB;

        public PlanetTokenState(TokenState state, TokenPoolId pool, int? tokenId, int? rollA, int? rollB)
        {
            State = state;
            Pool = pool;
            TokenId = tokenId;
            RollA = rollA;
            RollB = rollB;
        }
    }
}
