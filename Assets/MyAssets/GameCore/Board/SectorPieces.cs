using System.Collections.Generic;

namespace MyAssets.GameCore
{
    public readonly record struct SectorPieceId(int Value);

    public enum SectorPieceType { Empty, Trade, Planetary, StarterPlanetary }

    // “Planet positions remain relative to the piece; orientation changes”:
    // We model each piece as having up to 3 rotations (0/1/2).
    public sealed record PlanetOnPiece(
        int IndexOnPiece,        // 0..n-1
        ResourceType Resource,   // your resources
        StarRating Star,         // one or two star planet
        TokenPoolId TokenPool    // which pool supplies its roll token
    );

    public sealed class SectorPieceDef
    {
        public SectorPieceId Id;
        public SectorPieceType Type;
        public StarRating Star; // one/two star piece
        public List<PlanetOnPiece> Planets = new(); // empty/trade pieces can have none

        // Optional: if your trade stations have attributes later, add here.
    }

    public sealed class SectorPieceLibraryData
    {
        public readonly Dictionary<SectorPieceId, SectorPieceDef> Pieces = new();
    }
}
