using System.Collections.Generic;

namespace MyAssets.GameCore
{
    public sealed class GameState
    {
        public MatchPhase Phase { get; internal set; } = MatchPhase.Setup;

        public BoardDefinitionData BoardDef { get; }
        public BoardGraph Graph { get; }
        public SetupDefinitionData Setup { get; }
        public TileLibraryData Tiles { get; }

        // Authoritative placements:
        public readonly Dictionary<SlotId, TileId> SlotToTile;

        // Fog-of-war: revealed slots (public once revealed)
        public readonly Dictionary<SlotId, SectorPieceId> SlotToPiece = new();
        
        // Rotation State
        public readonly Dictionary<SlotId, int> SlotToRotation = new();

        // Reveal state
        public readonly HashSet<SlotId> RevealedSlots = new();

        // Planet token state (per planet instance)
        public readonly Dictionary<PlanetInstanceId, PlanetTokenState> PlanetTokens = new();

        // Ships
        public readonly Dictionary<ShipId, NodeId> ShipPositions = new();
        public readonly Dictionary<ShipId, PlayerId> ShipOwners = new();

        public GameState(BoardDefinitionData boardDef, SetupDefinitionData setup, TileLibraryData tiles)
        {
            BoardDef = boardDef;
            Setup = setup;
            Tiles = tiles;
            Graph = new BoardGraph(boardDef);
            SlotToTile = new Dictionary<SlotId, TileId>(setup.SlotToTile);

            // Pre-reveal starting slots
            foreach (var s in boardDef.StartingSlots)
                RevealedSlots.Add(s);
        }
    }
}
