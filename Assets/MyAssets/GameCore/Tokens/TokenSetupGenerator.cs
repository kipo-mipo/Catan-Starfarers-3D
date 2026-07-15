using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAssets.GameCore
{
    public sealed class TokenSetupResult
    {
        public readonly Dictionary<PlacedPlanetKey, PlanetTokenState> PlanetTokens = new();
    }

    public readonly struct PlacedPlanetKey : IEquatable<PlacedPlanetKey>
    {
        public readonly SlotId SlotId;
        public readonly int PlanetIndexOnPiece;

        public PlacedPlanetKey(SlotId slotId, int planetIndexOnPiece)
        {
            SlotId = slotId;
            PlanetIndexOnPiece = planetIndexOnPiece;
        }

        public bool Equals(PlacedPlanetKey other)
        {
            return SlotId.Equals(other.SlotId)
                && PlanetIndexOnPiece == other.PlanetIndexOnPiece;
        }

        public override bool Equals(object obj)
        {
            return obj is PlacedPlanetKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SlotId.GetHashCode() * 397) ^ PlanetIndexOnPiece;
            }
        }

        public override string ToString()
        {
            return $"Slot {SlotId.Value}, Planet {PlanetIndexOnPiece}";
        }
    }

    public static class TokenSetupGenerator
    {
        public static TokenSetupResult Generate(
            SectorPieceLibraryData pieces,
            IReadOnlyDictionary<SlotId, SectorPieceId> slotToPiece,
            TokenLibraryData tokenLibrary,
            int seed)
        {
            var result = new TokenSetupResult();
            var rng = new Random(seed);
            var mutablePools = CloneAndShufflePools(tokenLibrary, rng);

            AssignTokensForPlacedPieces(slotToPiece, pieces, mutablePools, result);

            return result;
        }

        private static Dictionary<TokenPoolId, Queue<RollTokenDef>> CloneAndShufflePools(
            TokenLibraryData tokenLibrary,
            Random rng)
        {
            var mutablePools = new Dictionary<TokenPoolId, Queue<RollTokenDef>>();

            foreach (var pair in tokenLibrary.Pools)
            {
                var shuffled = pair.Value.ToList();
                Shuffle(shuffled, rng);
                mutablePools[pair.Key] = new Queue<RollTokenDef>(shuffled);
            }

            return mutablePools;
        }

        private static void AssignTokensForPlacedPieces(
            IReadOnlyDictionary<SlotId, SectorPieceId> slotToPiece,
            SectorPieceLibraryData pieces,
            Dictionary<TokenPoolId, Queue<RollTokenDef>> mutablePools,
            TokenSetupResult result)
        {
            foreach (var pair in slotToPiece)
            {
                var slotId = pair.Key;
                var pieceId = pair.Value;

                if (!pieces.Pieces.TryGetValue(pieceId, out var pieceDef))
                    throw new InvalidOperationException($"Missing sector piece {pieceId.Value}.");

                foreach (var planet in pieceDef.Planets)
                {
                    var token = DrawToken(planet.TokenPool, mutablePools);
                    var key = new PlacedPlanetKey(slotId, planet.IndexOnPiece);

                    result.PlanetTokens[key] = new PlanetTokenState(
                        token.RequiresUpgrade ? TokenState.SpecialTokenPresent : TokenState.Assigned,
                        planet.TokenPool,
                        token.TokenId,
                        token.RollA,
                        token.RollB);
                }
            }
        }

        private static RollTokenDef DrawToken(
            TokenPoolId poolId,
            Dictionary<TokenPoolId, Queue<RollTokenDef>> mutablePools)
        {
            if (!mutablePools.TryGetValue(poolId, out var queue))
                throw new InvalidOperationException($"No token pool exists for {poolId}.");

            if (queue.Count == 0)
                throw new InvalidOperationException($"Token pool {poolId} is empty. You assigned more planets to this pool than tokens available.");

            return queue.Dequeue();
        }

        private static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
