using System.Collections.Generic;
using MyAssets.GameCore;
using UnityEngine;

namespace MyAssets.Client.Runtime
{
    public static class BoardDefinitionExporter
    {
        public static BoardDefinitionData Export(Authoring.BoardDefinitionAsset asset)
        {
            if (asset == null) throw new System.ArgumentNullException(nameof(asset));

            var def = new BoardDefinitionData();
            var planetSectorNodeIds = new HashSet<int>();
            foreach (var slot in asset.slots)
            {
                if (slot.type != Authoring.BoardDefinitionAsset.SectorSlotTypeValue.Planet || slot.neighborNodes == null)
                    continue;

                foreach (var nodeId in slot.neighborNodes)
                    planetSectorNodeIds.Add(nodeId);
            }

            foreach (var n in asset.nodes)
            {
                var nodeId = new NodeId(n.id);
                string objectName = string.IsNullOrWhiteSpace(n.objectName)
                    ? BoardGenerator.DefaultNodeObjectName(nodeId)
                    : n.objectName;
                bool isPlanetSectorSlot = planetSectorNodeIds.Contains(n.id);
                def.Nodes.Add(new BoardDefinitionData.Node(
                    nodeId,
                    ToNum(n.position),
                    isPlanetSectorSlot,
                    !isPlanetSectorSlot,
                    !isPlanetSectorSlot,
                    !isPlanetSectorSlot,
                    objectName
                ));
            }

            foreach (var l in asset.lanes)
                def.Lanes.Add(new BoardDefinitionData.Lane(new LaneId(l.id), new NodeId(l.a), new NodeId(l.b)));

            foreach (var s in asset.slots)
            {
                var neighbors = new List<NodeId>();
                if (s.neighborNodes != null)
                {
                    foreach (var id in s.neighborNodes)
                        neighbors.Add(new NodeId(id));
                }

                def.Slots.Add(new BoardDefinitionData.Slot(
                    new SlotId(s.id),
                    ToNum(s.position),
                    ToCoreStar(s.star),
                    ToCoreOrientation(s.orientation),
                    ToCoreSlotType(s.type),
                    neighbors
                ));
            }

            foreach (var id in asset.startingSlotIds)
                def.StartingSlots.Add(new SlotId(id));

            return def;
        }

        private static StarRating ToCoreStar(Authoring.BoardDefinitionAsset.SlotStarRating value)
        {
            return value == Authoring.BoardDefinitionAsset.SlotStarRating.Two
                ? StarRating.Two
                : StarRating.One;
        }

        private static SlotOrientation ToCoreOrientation(Authoring.BoardDefinitionAsset.SlotOrientationValue value)
        {
            return value == Authoring.BoardDefinitionAsset.SlotOrientationValue.Down
                ? SlotOrientation.Down
                : SlotOrientation.Up;
        }

        private static SectorSlotType ToCoreSlotType(Authoring.BoardDefinitionAsset.SectorSlotTypeValue value)
        {
            return value == Authoring.BoardDefinitionAsset.SectorSlotTypeValue.Trade
                ? SectorSlotType.Trade
                : SectorSlotType.Planet;
        }

        private static System.Numerics.Vector3 ToNum(UnityEngine.Vector3 v)
            => new System.Numerics.Vector3(v.x, v.y, v.z);
    }
}
