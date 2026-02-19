using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAssets.GameCore
{
    public sealed class SetupResult
    {
        public readonly Dictionary<SlotId, SectorPieceId> SlotToPiece = new();
        public readonly Dictionary<SlotId, int> SlotToRotation = new(); // 0..2
        public SectorPieceId UnusedPiece;
    }

    public static class SetupGenerator
    {
        public static SetupResult Generate(
            BoardDefinitionData board,
            SectorPieceLibraryData pieces,
            SetupRules rules,
            int seed
        )
        {
            var rng = new Random(seed);
            var result = new SetupResult();

            // Non-starting slots (hidden at start)
            var targetSlots = board.Slots.Where(s => !board.StartingSlots.Contains(s.Id)).ToList();
            if (targetSlots.Count != rules.NonStartingSlotsToFill)
                throw new InvalidOperationException($"Expected {rules.NonStartingSlotsToFill} non-starting slots, got {targetSlots.Count}.");

            // Candidate pieces for random placement (exclude starter pieces)
            var candidates = pieces.Pieces.Values
                .Where(p => p.Type is SectorPieceType.Planetary or SectorPieceType.Trade or SectorPieceType.Empty)
                .ToList();

            // Split by type/star
            List<SectorPieceDef> Filter(SectorPieceType type, StarRating star) =>
                candidates.Where(p => p.Type == type && p.Star == star).ToList();

            // Build pool based on rules
            var pool = new List<SectorPieceDef>();
            pool.AddRange(Pick(rng, Filter(SectorPieceType.Planetary, StarRating.One), rules.PlanetaryOneStar));
            pool.AddRange(Pick(rng, Filter(SectorPieceType.Planetary, StarRating.Two), rules.PlanetaryTwoStar));
            pool.AddRange(Pick(rng, Filter(SectorPieceType.Trade, StarRating.One), rules.TradeOneStar));
            pool.AddRange(Pick(rng, Filter(SectorPieceType.Trade, StarRating.Two), rules.TradeTwoStar));
            pool.AddRange(Pick(rng, Filter(SectorPieceType.Empty, StarRating.One), rules.EmptyOneStar));
            pool.AddRange(Pick(rng, Filter(SectorPieceType.Empty, StarRating.Two), rules.EmptyTwoStar));

            // We should have pool size = slots to fill + 1 (one unused)
            if (pool.Count != rules.NonStartingSlotsToFill + 1)
                throw new InvalidOperationException($"Pool size mismatch: expected {rules.NonStartingSlotsToFill + 1}, got {pool.Count}.");

            Shuffle(rng, pool);

            // Assign pieces to slots respecting star rating (slot star must match piece star)
            // Bruteforce simple greedy for now; we can improve later if needed.
            var remaining = new List<SectorPieceDef>(pool);

            foreach (var slot in targetSlots)
            {
                var matchIndex = remaining.FindIndex(p => p.Star == slot.Star);
                if (matchIndex < 0)
                    throw new InvalidOperationException($"No remaining piece matches slot star {slot.Star} for slot {slot.Id.Value}");

                var chosen = remaining[matchIndex];
                remaining.RemoveAt(matchIndex);

                result.SlotToPiece[slot.Id] = chosen.Id;

                // Rotation applies to planet sectors; others can be 0
                result.SlotToRotation[slot.Id] = (chosen.Type == SectorPieceType.Planetary) ? rng.Next(0, 3) : 0;
            }

            // One unused piece left
            result.UnusedPiece = remaining[0].Id;
            return result;
        }

        private static List<SectorPieceDef> Pick(Random rng, List<SectorPieceDef> from, int count)
        {
            if (from.Count < count) throw new InvalidOperationException("Not enough pieces in library for requested composition.");
            var copy = new List<SectorPieceDef>(from);
            Shuffle(rng, copy);
            return copy.Take(count).ToList();
        }

        private static void Shuffle<T>(Random rng, IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
