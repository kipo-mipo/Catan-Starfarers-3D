using System.Collections.Generic;
using System.Numerics;

namespace MyAssets.GameCore
{
    public enum SlotOrientation
    {
        Up,   // λ pattern (one lane pointed up) — use your naming if you prefer
        Down  // Y pattern (one lane pointed down)
    }

    public enum StarRating
    {
        One = 1,
        Two = 2
    }

    public sealed class BoardDefinitionData
    {
        public sealed record Node(NodeId Id, Vector3 Position);
        public sealed record Lane(LaneId Id, NodeId A, NodeId B);

        // A fixed sector location on the board.
        public sealed record Slot(
            SlotId Id,
            Vector3 Position,
            StarRating Star,
            SlotOrientation Orientation,
            List<NodeId> NeighborNodes
        );

        public readonly List<Node> Nodes = new();
        public readonly List<Lane> Lanes = new();
        public readonly List<Slot> Slots = new();

        // 4 starting planet sector slots (fixed + revealed)
        public readonly HashSet<SlotId> StartingSlots = new();
    }
}
