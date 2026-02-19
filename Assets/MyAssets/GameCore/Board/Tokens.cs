using System.Collections.Generic;

namespace MyAssets.GameCore
{
    public enum TokenPoolId
    {
        // Starting planets (A–D)
        StarterA,
        StarterB,
        StarterC,
        StarterD,

        // One-star planet pools
        OneStarTriangle,
        OneStarSquare,
        OneStarHexagon,

        // Two-star planet pools
        TwoStarTriangle,
        TwoStarSquare,
        TwoStarHexagon,
    }

    public sealed record RollTokenDef(
        int TokenId,             // stable ID within library
        int RollA,               // always present
        int? RollB,              // optional second roll number
        bool RequiresUpgrade,    // mothership upgrade requirement
        bool IsReserveCandidate  // belongs to the "reserve" sub-pool
    );

    public sealed class TokenLibraryData
    {
        public readonly Dictionary<TokenPoolId, List<RollTokenDef>> Pools = new();

        public IReadOnlyList<RollTokenDef> GetPool(TokenPoolId id) =>
            Pools.TryGetValue(id, out var list) ? list : (IReadOnlyList<RollTokenDef>)System.Array.Empty<RollTokenDef>();
    }
}
