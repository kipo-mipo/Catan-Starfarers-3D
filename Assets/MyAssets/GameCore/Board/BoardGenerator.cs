using System;
using System.Collections.Generic;
using System.Numerics;

namespace MyAssets.GameCore
{
    /// <summary>
    /// Row-pattern board generator with explicit sector-slot placement.
    ///
    /// The hex rows generate the physical graph: nodes + lanes.
    /// Sector slots are anchored to generated node GameObject names (Node_###),
    /// because that is the indexing scheme currently visible and verified in-scene.
    /// </summary>
    public static class BoardGenerator
    {
        public readonly struct RowSpec
        {
            public readonly int Count;
            public readonly float Offset;

            public RowSpec(int count, float offset)
            {
                Count = count;
                Offset = offset;
            }
        }

        public readonly struct SectorSlotSpec
        {
            public readonly string NodeObjectName;
            public readonly StarRating Star;
            public readonly SlotOrientation Orientation;
            public readonly SectorSlotType Type;
            public readonly bool IsStartingSlot;

            public SectorSlotSpec(
                string nodeObjectName,
                StarRating star,
                SlotOrientation orientation,
                SectorSlotType type = SectorSlotType.Planet,
                bool isStartingSlot = false)
            {
                if (string.IsNullOrWhiteSpace(nodeObjectName))
                    throw new ArgumentException("Sector slot node name cannot be empty.", nameof(nodeObjectName));

                NodeObjectName = nodeObjectName;
                Star = star;
                Orientation = orientation;
                Type = type;
                IsStartingSlot = isStartingSlot;
            }
        }

        private static readonly RowSpec[] DefaultRows =
        {
            new RowSpec(2,  -6f),  // Row 0
            new RowSpec(15,  0f),  // Row 1
            new RowSpec(14,  0f),  // Row 2
            new RowSpec(15,  0f),  // Row 3
            new RowSpec(14,  0f),  // Row 4
            new RowSpec(15,  0f),  // Row 5
            new RowSpec(14,  0f),  // Row 6
            new RowSpec(15,  0f),  // Row 7
            new RowSpec(14,  0f),  // Row 8
            new RowSpec(15,  0f),  // Row 9
            new RowSpec(2,  -6f),  // Row 10
        };

        private static readonly string[] DefaultSectorSlotNodeNames =
        {
            "Node_1", "Node_23", "Node_36", "Node_43", "Node_59",
            "Node_91", "Node_96", "Node_140", "Node_146", "Node_157",
            "Node_169", "Node_183", "Node_192", "Node_234", "Node_263",
            "Node_271", "Node_274", "Node_281", "Node_282"
        };

        private static readonly HashSet<string> DefaultSectorSlotNodeNameSet = new(DefaultSectorSlotNodeNames);

        public static string DefaultNodeObjectName(NodeId nodeId) => $"Node_{nodeId.Value}";

        public static bool IsDefaultSectorSlotNode(NodeId nodeId) =>
            DefaultSectorSlotNodeNameSet.Contains(DefaultNodeObjectName(nodeId));

        public static bool IsDefaultSectorSlotNodeObjectName(string nodeObjectName) =>
            DefaultSectorSlotNodeNameSet.Contains(nodeObjectName);

        // Anchored to the generated GameObject names you verified in the hierarchy.
        // Star/orientation are temporary safe defaults until you provide the actual metadata.
        private static readonly SectorSlotSpec[] DefaultSectorSlots =
        {
            new SectorSlotSpec("Node_1",   StarRating.One, SlotOrientation.Down, isStartingSlot: true),
            new SectorSlotSpec("Node_23",  StarRating.One, SlotOrientation.Up),
            new SectorSlotSpec("Node_36",  StarRating.One, SlotOrientation.Down),
            new SectorSlotSpec("Node_43",  StarRating.One, SlotOrientation.Up),
            new SectorSlotSpec("Node_59",  StarRating.Two, SlotOrientation.Up),
            new SectorSlotSpec("Node_91",  StarRating.Two, SlotOrientation.Down),
            new SectorSlotSpec("Node_96",  StarRating.One, SlotOrientation.Up, isStartingSlot: true),
            new SectorSlotSpec("Node_140", StarRating.One, SlotOrientation.Up),
            new SectorSlotSpec("Node_146", StarRating.Two, SlotOrientation.Up),
            new SectorSlotSpec("Node_157", StarRating.Two, SlotOrientation.Down),
            new SectorSlotSpec("Node_169", StarRating.One, SlotOrientation.Down),
            new SectorSlotSpec("Node_183", StarRating.Two, SlotOrientation.Down),
            new SectorSlotSpec("Node_192", StarRating.One, SlotOrientation.Down, isStartingSlot: true),
            new SectorSlotSpec("Node_234", StarRating.One, SlotOrientation.Up),
            new SectorSlotSpec("Node_263", StarRating.One, SlotOrientation.Down),
            new SectorSlotSpec("Node_271", StarRating.One, SlotOrientation.Down),
            new SectorSlotSpec("Node_274", StarRating.Two, SlotOrientation.Up),
            new SectorSlotSpec("Node_281", StarRating.Two, SlotOrientation.Down),
            new SectorSlotSpec("Node_282", StarRating.One, SlotOrientation.Up, isStartingSlot: true),
        };

        public static BoardDefinitionData Generate(float hexSize = 1f)
        {
            return GenerateFromRows(DefaultRows, DefaultSectorSlots, hexSize);
        }

        public static BoardDefinitionData GenerateFromRows(IReadOnlyList<RowSpec> rows, float hexSize = 1f)
        {
            return GenerateFromRows(rows, DefaultSectorSlots, hexSize);
        }

        public static BoardDefinitionData GenerateFromRows(
            IReadOnlyList<RowSpec> rows,
            IReadOnlyList<SectorSlotSpec> sectorSlots,
            float hexSize = 1f)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (rows.Count == 0) throw new ArgumentException("rows cannot be empty.", nameof(rows));
            if (sectorSlots == null) throw new ArgumentNullException(nameof(sectorSlots));
            if (hexSize <= 0f) throw new ArgumentOutOfRangeException(nameof(hexSize));

            var def = new BoardDefinitionData();
            var planetSectorNodeNames = PlanetSectorNodeNamesFromSpecs(sectorSlots);

            var nodeKeyToId = new Dictionary<(long x, long z), NodeId>();
            var laneKeySet = new HashSet<(int a, int b)>();

            int nextNode = 0;
            int nextLane = 0;

            for (int row = 0; row < rows.Count; row++)
            {
                int count = rows[row].Count;
                if (count <= 0)
                    continue;

                for (int col = 0; col < count; col++)
                {
                    var center = HexCenter(rows, row, col, hexSize);

                    var cornerIds = new List<NodeId>(6);
                    for (int i = 0; i < 6; i++)
                    {
                        var c = CornerWorld(center, hexSize, i);
                        var key = QuantKey(c);

                        if (!nodeKeyToId.TryGetValue(key, out var nid))
                        {
                            nid = new NodeId(nextNode++);
                            nodeKeyToId[key] = nid;

                            string objectName = DefaultNodeObjectName(nid);
                            bool isPlanetSectorSlot = planetSectorNodeNames.Contains(objectName);
                            // Sector-slot center nodes are still navigation nodes.
                            // They are blocked for colonies/spaceports, but ships must be able
                            // to move through/click them, especially when the revealed sector is empty.
                            def.Nodes.Add(new BoardDefinitionData.Node(
                                nid,
                                c,
                                isPlanetSectorSlot,
                                true,
                                !isPlanetSectorSlot,
                                true,
                                objectName
                            ));
                        }

                        cornerIds.Add(nid);
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        int a = cornerIds[i].Value;
                        int b = cornerIds[(i + 1) % 6].Value;
                        (int a, int b) key = a < b ? (a, b) : (b, a);

                        if (laneKeySet.Add(key))
                        {
                            def.Lanes.Add(new BoardDefinitionData.Lane(
                                new LaneId(nextLane++),
                                new NodeId(key.a),
                                new NodeId(key.b)
                            ));
                        }
                    }
                }
            }

            ValidateSectorSlotNodeNames(def, sectorSlots);
            AddSectorSlots(def, sectorSlots);
            return def;
        }

        private static string NodeObjectName(BoardDefinitionData.Node node) =>
            string.IsNullOrWhiteSpace(node.ObjectName) ? DefaultNodeObjectName(node.Id) : node.ObjectName;

        private static HashSet<string> PlanetSectorNodeNamesFromSpecs(IReadOnlyList<SectorSlotSpec> sectorSlots)
        {
            var names = new HashSet<string>();
            foreach (var slot in sectorSlots)
            {
                if (slot.Type == SectorSlotType.Planet)
                    names.Add(slot.NodeObjectName);
            }
            return names;
        }

        private static void ValidateSectorSlotNodeNames(
            BoardDefinitionData def,
            IReadOnlyList<SectorSlotSpec> sectorSlots)
        {
            var generated = new HashSet<string>();
            foreach (var node in def.Nodes)
                generated.Add(NodeObjectName(node));

            foreach (var slot in sectorSlots)
            {
                if (!generated.Contains(slot.NodeObjectName))
                    throw new InvalidOperationException($"Sector slot node {slot.NodeObjectName} was requested, but the generated board has no matching generated node name.");
            }
        }

        private static void AddSectorSlots(
            BoardDefinitionData def,
            IReadOnlyList<SectorSlotSpec> sectorSlots)
        {
            var nodeByName = new Dictionary<string, BoardDefinitionData.Node>();
            foreach (var node in def.Nodes)
                nodeByName[NodeObjectName(node)] = node;

            var usedNodeIds = new HashSet<NodeId>();

            for (int i = 0; i < sectorSlots.Count; i++)
            {
                var spec = sectorSlots[i];

                if (!nodeByName.TryGetValue(spec.NodeObjectName, out var node))
                    throw new InvalidOperationException($"Sector slot {i} references missing generated node object name {spec.NodeObjectName}.");

                if (!usedNodeIds.Add(node.Id))
                    throw new InvalidOperationException($"Sector slot {i} duplicates node object name {spec.NodeObjectName}. Move one of the slot specs.");

                var slotId = new SlotId(i);
                def.Slots.Add(new BoardDefinitionData.Slot(
                    slotId,
                    node.Position,
                    spec.Star,
                    spec.Orientation,
                    spec.Type,
                    new List<NodeId> { node.Id }
                ));

                if (spec.IsStartingSlot)
                    def.StartingSlots.Add(slotId);
            }
        }

        private static Vector3 HexCenter(IReadOnlyList<RowSpec> rows, int row, int col, float hexSize)
        {
            float rowSpacing = 1.5f * hexSize;
            float colSpacing = (float)Math.Sqrt(3) * hexSize;
            float rowOrigin = -0.5f * (rows.Count - 1);

            int count = rows[row].Count;
            float z = (rowOrigin + row) * rowSpacing;
            float xStart = (-0.5f * (count - 1) * colSpacing) + (rows[row].Offset * colSpacing);
            float x = xStart + col * colSpacing;
            return new Vector3(x, 0f, z);
        }

        private static Vector3 CornerWorld(Vector3 center, float size, int cornerIndex)
        {
            // Pointy-top hex corners on XZ plane.
            double angle = Math.PI / 180.0 * (30.0 + 60.0 * cornerIndex);
            float x = center.X + size * (float)Math.Cos(angle);
            float z = center.Z + size * (float)Math.Sin(angle);
            return new Vector3(x, 0f, z);
        }

        private static (long x, long z) QuantKey(Vector3 p)
        {
            const double scale = 10000.0;
            long x = (long)Math.Round(p.X * scale);
            long z = (long)Math.Round(p.Z * scale);
            return (x, z);
        }
    }
}
