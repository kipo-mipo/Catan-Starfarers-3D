using System.Collections.Generic;
using System.Linq;

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
        public readonly Dictionary<SlotId, int> SlotToRotation = new();
        public readonly HashSet<SlotId> RevealedSlots = new();

        // Planet token state (per planet instance).
        // NOTE: PlanetTokenState is defined in Board/PlanetTokens.cs (TokenState/Pool/TokenId/Rolls).
        public readonly Dictionary<PlanetInstanceId, PlanetTokenState> PlanetTokens = new();

        // Ships
        public readonly Dictionary<ShipId, NodeId> ShipPositions = new();
        public readonly Dictionary<ShipId, PlayerId> ShipOwners = new();
        public readonly Dictionary<ShipId, SetupShipType> ShipTypes = new();

        // Setup selection tracking. SetupPlayers is kept for compatibility; SetupTurnOrder is authoritative for turn order.
        public readonly HashSet<PlayerId> SetupPlayers = new();
        public readonly List<PlayerId> SetupTurnOrder = new();
        public readonly Dictionary<PlayerId, NodeId> StartingNodeByPlayer = new(); // legacy: first colony per player

        public SetupRound CurrentSetupRound { get; internal set; } = SetupRound.FirstColony;
        public int CurrentSetupTurnIndex { get; internal set; } = 0;
        public PlayerId SetupStartingPlayer { get; internal set; } = new PlayerId(-1);

        // Structures placed during setup.
        public readonly Dictionary<NodeId, PlayerId> ColonyOwners = new();
        public readonly Dictionary<NodeId, PlayerId> SpaceportOwners = new();
        public readonly Dictionary<PlayerId, UpgradeType> SetupUpgradeByPlayer = new();
        public readonly HashSet<PlayerId> StartingResourcesGranted = new();
        public readonly HashSet<PlayerId> FameMedalGranted = new();

        public PlayerId CurrentSetupPlayer => GetCurrentSetupOrder()[CurrentSetupTurnIndex];

        public IReadOnlyList<PlayerId> GetCurrentSetupOrder()
        {
            if (SetupTurnOrder.Count == 0)
                return System.Array.Empty<PlayerId>();

            if (CurrentSetupRound == SetupRound.SecondColony || CurrentSetupRound == SetupRound.SpaceportShipUpgrade)
                return SetupTurnOrder.AsEnumerable().Reverse().ToArray();

            return SetupTurnOrder;
        }

        public bool IsStructureOccupied(NodeId node) => ColonyOwners.ContainsKey(node) || SpaceportOwners.ContainsKey(node);
        public bool IsShipOccupied(NodeId node) => ShipPositions.ContainsValue(node);

        public GameState(BoardDefinitionData boardDef, SetupDefinitionData setup, TileLibraryData tiles)
        {
            BoardDef = boardDef;
            Setup = setup;
            Tiles = tiles;
            Graph = new BoardGraph(boardDef);
            SlotToTile = new Dictionary<SlotId, TileId>(setup.SlotToTile);

            // Pre-reveal starting slots (if any in setup)
            foreach (var slot in setup.SlotToTile.Keys)
            {
                // no-op: slots get revealed from ship vision later
            }
        }
    }
}
