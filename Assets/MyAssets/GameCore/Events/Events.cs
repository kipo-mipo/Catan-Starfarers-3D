namespace MyAssets.GameCore
{
    public interface IGameEvent { }

    public readonly struct MatchStartedEvent : IGameEvent, System.IEquatable<MatchStartedEvent>
    {
        public readonly int Seed;
        public MatchStartedEvent(int seed) => Seed = seed;
        public bool Equals(MatchStartedEvent other) => Seed == other.Seed;
        public override bool Equals(object obj) => obj is MatchStartedEvent other && Equals(other);
        public override int GetHashCode() => Seed;
    }

    public readonly struct ActionRejectedEvent : IGameEvent, System.IEquatable<ActionRejectedEvent>
    {
        public readonly string Reason;
        public ActionRejectedEvent(string reason) => Reason = reason;
        public bool Equals(ActionRejectedEvent other) => string.Equals(Reason, other.Reason);
        public override bool Equals(object obj) => obj is ActionRejectedEvent other && Equals(other);
        public override int GetHashCode() => Reason == null ? 0 : Reason.GetHashCode();
    }

    public readonly struct ShipMovedEvent : IGameEvent, System.IEquatable<ShipMovedEvent>
    {
        public readonly PlayerId Player;
        public readonly ShipId Ship;
        public readonly NodeId From;
        public readonly NodeId To;

        public ShipMovedEvent(PlayerId player, ShipId ship, NodeId from, NodeId to)
        {
            Player = player; Ship = ship; From = from; To = to;
        }

        public bool Equals(ShipMovedEvent other) =>
            Player.Equals(other.Player) && Ship.Equals(other.Ship) && From.Equals(other.From) && To.Equals(other.To);

        public override bool Equals(object obj) => obj is ShipMovedEvent other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = Player.GetHashCode();
                h = (h * 397) ^ Ship.GetHashCode();
                h = (h * 397) ^ From.GetHashCode();
                h = (h * 397) ^ To.GetHashCode();
                return h;
            }
        }
    }

    public readonly struct SectorRevealedEvent : IGameEvent, System.IEquatable<SectorRevealedEvent>
    {
        public readonly SlotId Slot;
        public readonly SectorPieceId Piece;
        public readonly int Rotation;

        public SectorRevealedEvent(SlotId slot, SectorPieceId piece, int rotation)
        {
            Slot = slot; Piece = piece; Rotation = rotation;
        }

        public bool Equals(SectorRevealedEvent other) =>
            Slot.Equals(other.Slot) && Piece.Equals(other.Piece) && Rotation == other.Rotation;

        public override bool Equals(object obj) => obj is SectorRevealedEvent other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = Slot.GetHashCode();
                h = (h * 397) ^ Piece.GetHashCode();
                h = (h * 397) ^ Rotation;
                return h;
            }
        }
    }

    public readonly struct PlanetTokenLockedEvent : IGameEvent, System.IEquatable<PlanetTokenLockedEvent>
    {
        public readonly PlanetInstanceId Planet;
        public readonly TokenPoolId Pool;

        public PlanetTokenLockedEvent(PlanetInstanceId planet, TokenPoolId pool)
        {
            Planet = planet; Pool = pool;
        }

        public bool Equals(PlanetTokenLockedEvent other) => Planet.Equals(other.Planet) && Pool == other.Pool;
        public override bool Equals(object obj) => obj is PlanetTokenLockedEvent other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Planet.GetHashCode() * 397) ^ (int)Pool;
            }
        }
    }

    public readonly struct PlanetTokenAssignedEvent : IGameEvent, System.IEquatable<PlanetTokenAssignedEvent>
    {
        public readonly PlanetInstanceId Planet;
        public readonly TokenPoolId Pool;
        public readonly int TokenId;
        public readonly int RollA;
        public readonly int? RollB;

        public PlanetTokenAssignedEvent(PlanetInstanceId planet, TokenPoolId pool, int tokenId, int rollA, int? rollB)
        {
            Planet = planet; Pool = pool; TokenId = tokenId; RollA = rollA; RollB = rollB;
        }

        public bool Equals(PlanetTokenAssignedEvent other) =>
            Planet.Equals(other.Planet) && Pool == other.Pool && TokenId == other.TokenId && RollA == other.RollA && RollB == other.RollB;

        public override bool Equals(object obj) => obj is PlanetTokenAssignedEvent other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = Planet.GetHashCode();
                h = (h * 397) ^ (int)Pool;
                h = (h * 397) ^ TokenId;
                h = (h * 397) ^ RollA;
                h = (h * 397) ^ (RollB.HasValue ? RollB.Value : 0);
                return h;
            }
        }
    }
}
