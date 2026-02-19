using System.Numerics;
using MyAssets.GameCore;
using UnityEngine;

namespace MyAssets.Client.Runtime
{
    public static class BoardDefinitionExporter
    {
        public static BoardDefinitionData Export(Authoring.BoardDefinitionAsset asset)
        {
            var def = new BoardDefinitionData();

            foreach (var n in asset.nodes)
                def.Nodes.Add(new BoardDefinitionData.Node(new NodeId(n.id), ToNum(n.position)));

            foreach (var l in asset.lanes)
                def.Lanes.Add(new BoardDefinitionData.Lane(new LaneId(l.id), new NodeId(l.a), new NodeId(l.b)));

            foreach (var s in asset.slots)
            {
                var neighbors = new System.Collections.Generic.List<NodeId>();
                foreach (var id in s.neighborNodes) neighbors.Add(new NodeId(id));
                def.Slots.Add(new BoardDefinitionData.Slot(new SlotId(s.id), ToNum(s.position), neighbors));
            }

            foreach (var id in asset.startingSlotIds)
                def.StartingSlots.Add(new SlotId(id));

            return def;
        }

        private static Vector3 ToNum(UnityEngine.Vector3 v) => new Vector3(v.x, v.y, v.z);
    }
}
