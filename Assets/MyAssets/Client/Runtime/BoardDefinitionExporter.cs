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
                var neighbors = new System.Collections.Generic.List<MyAssets.GameCore.NodeId>();
                foreach (var id in s.neighborNodes) neighbors.Add(new MyAssets.GameCore.NodeId(id));

                // TEMP DEFAULTS until you add these fields to your BoardDefinitionAsset:
                var star = MyAssets.GameCore.StarRating.One;
                var orientation = MyAssets.GameCore.SlotOrientation.Up;

                def.Slots.Add(new MyAssets.GameCore.BoardDefinitionData.Slot(
                    new MyAssets.GameCore.SlotId(s.id),
                    ToNum(s.position),
                    star,
                    orientation,
                    neighbors
                ));
            }

            foreach (var id in asset.startingSlotIds)
                def.StartingSlots.Add(new SlotId(id));

            return def;
        }

        private static System.Numerics.Vector3 ToNum(UnityEngine.Vector3 v)
            => new System.Numerics.Vector3(v.x, v.y, v.z);

    }
}
