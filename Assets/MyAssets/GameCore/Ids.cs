namespace MyAssets.GameCore
{
    // C# 9 compatible value objects (no record-struct).
    public readonly struct PlayerId : System.IEquatable<PlayerId>
    {
        public readonly int Value;
        public PlayerId(int value) => Value = value;
        public bool Equals(PlayerId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is PlayerId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }

    public readonly struct ShipId : System.IEquatable<ShipId>
    {
        public readonly int Value;
        public ShipId(int value) => Value = value;
        public bool Equals(ShipId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ShipId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }

    public readonly struct NodeId : System.IEquatable<NodeId>
    {
        public readonly int Value;
        public NodeId(int value) => Value = value;
        public bool Equals(NodeId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is NodeId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }

    public readonly struct LaneId : System.IEquatable<LaneId>
    {
        public readonly int Value;
        public LaneId(int value) => Value = value;
        public bool Equals(LaneId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is LaneId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }

    // sector location on the fixed board
    public readonly struct SlotId : System.IEquatable<SlotId>
    {
        public readonly int Value;
        public SlotId(int value) => Value = value;
        public bool Equals(SlotId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SlotId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }

    // unique physical piece / tile id
    public readonly struct TileId : System.IEquatable<TileId>
    {
        public readonly int Value;
        public TileId(int value) => Value = value;
        public bool Equals(TileId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is TileId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();
    }

    // Planet identity on the board = (slot, planet index within the sector piece definition)
    public readonly struct PlanetInstanceId : System.IEquatable<PlanetInstanceId>
    {
        public readonly SlotId Slot;
        public readonly int PlanetIndexOnPiece;

        public PlanetInstanceId(SlotId slot, int planetIndexOnPiece)
        {
            Slot = slot;
            PlanetIndexOnPiece = planetIndexOnPiece;
        }

        public bool Equals(PlanetInstanceId other) =>
            Slot.Equals(other.Slot) && PlanetIndexOnPiece == other.PlanetIndexOnPiece;

        public override bool Equals(object obj) => obj is PlanetInstanceId other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Slot.GetHashCode() * 397) ^ PlanetIndexOnPiece;
            }
        }

        public override string ToString() => $"{Slot.Value}:{PlanetIndexOnPiece}";
    }
}
