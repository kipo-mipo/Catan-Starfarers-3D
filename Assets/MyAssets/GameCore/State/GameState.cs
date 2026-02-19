using System;
using System.Collections.Generic;

namespace GameCore
{
    // Keep this data-only. No UnityEngine, no Mirror.

    public enum MatchPhase
    {
        NotStarted = 0,
        Setup = 1,
        Turn = 2,
        GameOver = 3
    }

    public enum NodeKind
    {
        Empty = 0,
        Planet = 1,
        HomeSystem = 2,
        Pirate = 3
    }

    public enum LaneKind
    {
        Normal = 0
    }

    public enum ColonyKind
    {
        Colony = 0,
        TradeStation = 1
    }

    public sealed class GameState
    {
        public int RulesVersion { get; set; } = 1;
        public int Seed { get; set; }

        public MatchPhase Phase { get; set; } = MatchPhase.NotStarted;

        /// <summary>Index into Players list.</summary>
        public int CurrentPlayerIndex { get; set; } = 0;

        /// <summary>0 for first setup placement pass, 1 for second pass, etc.</summary>
        public int SetupRound { get; set; } = 0;

        /// <summary>Action counter for deterministic salts, replay, snapshots.</summary>
        public int ActionIndex { get; set; } = 0;

        public List<PlayerState> Players { get; } = new();
        public MapState Map { get; set; } = new();

        /// <summary>Ships by ShipId.</summary>
        public Dictionary<int, ShipState> Ships { get; } = new();

        /// <summary>Colonies by NodeId.</summary>
        public Dictionary<int, ColonyState> ColoniesByNode { get; } = new();

        public int CurrentPlayerId =>
            (Players.Count == 0) ? -1 : Players[ClampIndex(CurrentPlayerIndex, Players.Count)].PlayerId;

        static int ClampIndex(int i, int count) => count <= 0 ? 0 : (i < 0 ? 0 : (i >= count ? count - 1 : i));

        public GameState CloneShallow()
        {
            // Shallow clone for "copy-on-write" style changes.
            // Deep copy collections we mutate.
            var copy = new GameState
            {
                RulesVersion = RulesVersion,
                Seed = Seed,
                Phase = Phase,
                CurrentPlayerIndex = CurrentPlayerIndex,
                SetupRound = SetupRound,
                ActionIndex = ActionIndex,
                Map = Map // Map is immutable by convention once generated; replace if you mutate it.
            };

            copy.Players.AddRange(Players);

            foreach (var kv in Ships)
                copy.Ships[kv.Key] = kv.Value;

            foreach (var kv in ColoniesByNode)
                copy.ColoniesByNode[kv.Key] = kv.Value;

            return copy;
        }
    }

    public sealed class PlayerState
    {
        public int PlayerId { get; }
        public string DisplayName { get; set; } = "Player";
        public int SeatIndex { get; set; } = 0;

        public PlayerState(int playerId, int seatIndex)
        {
            PlayerId = playerId;
            SeatIndex = seatIndex;
        }
    }

    public sealed class ShipState
    {
        public int ShipId { get; }
        public int OwnerPlayerId { get; set; }

        /// <summary>Current location by NodeId.</summary>
        public int NodeId { get; set; }

        public ShipState(int shipId, int ownerPlayerId, int nodeId)
        {
            ShipId = shipId;
            OwnerPlayerId = ownerPlayerId;
            NodeId = nodeId;
        }
    }

    public sealed class ColonyState
    {
        public int OwnerPlayerId { get; set; }
        public ColonyKind Kind { get; set; }

        public ColonyState(int ownerPlayerId, ColonyKind kind)
        {
            OwnerPlayerId = ownerPlayerId;
            Kind = kind;
        }
    }

    public sealed class MapState
    {
        public Dictionary<int, MapNode> Nodes { get; } = new();
        public Dictionary<int, MapLane> Lanes { get; } = new();

        /// <summary>Adjacency lookup: NodeId -> neighbors.</summary>
        public Dictionary<int, List<int>> Adjacency { get; } = new();

        public bool HasNode(int nodeId) => Nodes.ContainsKey(nodeId);
        public bool AreAdjacent(int a, int b) =>
            Adjacency.TryGetValue(a, out var list) && list.Contains(b);

        public int? GetLaneIdBetween(int a, int b)
        {
            foreach (var kv in Lanes)
            {
                var lane = kv.Value;
                if ((lane.NodeAId == a && lane.NodeBId == b) || (lane.NodeAId == b && lane.NodeBId == a))
                    return kv.Key;
            }
            return null;
        }

        public void AddNode(MapNode node)
        {
            Nodes[node.NodeId] = node;
            if (!Adjacency.ContainsKey(node.NodeId))
                Adjacency[node.NodeId] = new List<int>();
        }

        public void AddLane(int laneId, MapLane lane)
        {
            Lanes[laneId] = lane;

            if (!Adjacency.ContainsKey(lane.NodeAId)) Adjacency[lane.NodeAId] = new List<int>();
            if (!Adjacency.ContainsKey(lane.NodeBId)) Adjacency[lane.NodeBId] = new List<int>();

            if (!Adjacency[lane.NodeAId].Contains(lane.NodeBId)) Adjacency[lane.NodeAId].Add(lane.NodeBId);
            if (!Adjacency[lane.NodeBId].Contains(lane.NodeAId)) Adjacency[lane.NodeBId].Add(lane.NodeAId);
        }
    }

    public sealed class MapNode
    {
        public int NodeId { get; }
        public NodeKind Kind { get; set; }

        public MapNode(int nodeId, NodeKind kind)
        {
            NodeId = nodeId;
            Kind = kind;
        }
    }

    public sealed class MapLane
    {
        public int NodeAId { get; set; }
        public int NodeBId { get; set; }
        public LaneKind Kind { get; set; } = LaneKind.Normal;

        public MapLane(int a, int b, LaneKind kind = LaneKind.Normal)
        {
            NodeAId = a;
            NodeBId = b;
            Kind = kind;
        }
    }
}
