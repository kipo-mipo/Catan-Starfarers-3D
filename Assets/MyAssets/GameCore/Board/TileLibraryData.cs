using System.Collections.Generic;

namespace MyAssets.GameCore
{
    public abstract record TileDef(TileId Id, SectorType Type);

    public sealed record EmptyTileDef(TileId Id)
        : TileDef(Id, SectorType.Empty);

    public sealed record TradeStationTileDef(TileId Id /* add attributes later */)
        : TileDef(Id, SectorType.TradeStation);

    public sealed record PlanetTileDef(
        TileId Id,
        ResourceType Resource,
        int RollToken,     // your “token for roll numbers”
        int Yield,         // if needed
        int StarValue      // if needed
    ) : TileDef(Id, SectorType.Planet);

    public sealed class TileLibraryData
    {
        public readonly Dictionary<TileId, TileDef> Tiles = new();
    }
}
