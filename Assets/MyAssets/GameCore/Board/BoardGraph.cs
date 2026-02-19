using System.Collections.Generic;

namespace MyAssets.GameCore
{
    public sealed class BoardGraph
    {
        private readonly Dictionary<NodeId, List<NodeId>> _adj = new();

        public BoardGraph(BoardDefinitionData def)
        {
            foreach (var lane in def.Lanes)
            {
                AddEdge(lane.A, lane.B);
                AddEdge(lane.B, lane.A);
            }
        }

        public IReadOnlyList<NodeId> Neighbors(NodeId node) =>
            _adj.TryGetValue(node, out var list) ? list : (IReadOnlyList<NodeId>)System.Array.Empty<NodeId>();

        public bool AreAdjacent(NodeId a, NodeId b) =>
            _adj.TryGetValue(a, out var list) && list.Contains(b);

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
