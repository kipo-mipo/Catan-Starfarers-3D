using System.Collections.Generic;

namespace MyAssets.GameCore
{
    public sealed class BoardGraph
    {
        private readonly Dictionary<NodeId, BoardDefinitionData.Node> _nodes = new();
        private readonly Dictionary<NodeId, List<NodeId>> _adj = new();

        public BoardGraph(BoardDefinitionData def)
        {
            foreach (var node in def.Nodes)
                _nodes[node.Id] = node;

            foreach (var lane in def.Lanes)
            {
                if (!IsPassable(lane.A) || !IsPassable(lane.B))
                    continue;

                AddEdge(lane.A, lane.B);
                AddEdge(lane.B, lane.A);
            }
        }

        public bool HasNode(NodeId node) => _nodes.ContainsKey(node);

        public bool IsPlanetSectorSlot(NodeId node) =>
            _nodes.TryGetValue(node, out var data) && data.IsPlanetSectorSlot;

        public bool IsPassable(NodeId node) =>
            _nodes.TryGetValue(node, out var data) && data.IsPassable;

        public bool IsLandable(NodeId node) =>
            _nodes.TryGetValue(node, out var data) && data.IsLandable && !data.IsPlanetSectorSlot;

        public bool IsInteractable(NodeId node) =>
            _nodes.TryGetValue(node, out var data) && data.IsInteractable;

        public IReadOnlyList<NodeId> Neighbors(NodeId node) =>
            _adj.TryGetValue(node, out var list) ? list : (IReadOnlyList<NodeId>)System.Array.Empty<NodeId>();

        public bool AreAdjacent(NodeId a, NodeId b) =>
            IsPassable(a) && IsPassable(b) && _adj.TryGetValue(a, out var list) && list.Contains(b);

        private void AddEdge(NodeId from, NodeId to)
        {
            if (!_adj.TryGetValue(from, out var list))
            {
                list = new List<NodeId>();
                _adj[from] = list;
            }
            if (!list.Contains(to)) list.Add(to);
        }
    }
}
