using System.Collections.Generic;

namespace MyAssets.GameCore
{
    public readonly struct SectorPieceId : System.IEquatable<SectorPieceId>
    {
        public readonly int Value;

        public SectorPieceId(int value) => Value = value;

        public bool Equals(SectorPieceId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SectorPieceId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }

    public enum SectorPieceType
    {
        Empty,
        Trade,
        Planetary,
        StarterPlanetary
    }

    public enum TradeSpecies
    {
        None = 0,
        SpeciesMerchants = 1,
        SpeciesGreenfolk = 2,
        SpeciesScientists = 3,
        SpeciesDiplomats = 4,
        SpeciesTravelers = 5
    }

    // Planet positions remain relative to the piece; orientation changes.
    // We model each piece as having up to 3 rotations: 0, 1, or 2.
    public sealed record PlanetOnPiece(
        int IndexOnPiece,
        ResourceType Resource,
        StarRating Star,
        TokenPoolId TokenPool
    );

    public sealed class SectorPieceDef
    {
        public SectorPieceId Id;
        public SectorPieceType Type;
        public StarRating Star;
        public List<PlanetOnPiece> Planets = new();

        // True only for expansion/add-on sectors that should not appear in four-player setup.
        public bool FiveSixOnly = false;

        // Only valid when Type == SectorPieceType.Trade.
        public TradeSpecies TradeSpecies = TradeSpecies.None;
    }

    public sealed class SectorPieceLibraryData
    {
        public readonly Dictionary<SectorPieceId, SectorPieceDef> Pieces = new();
    }
}
