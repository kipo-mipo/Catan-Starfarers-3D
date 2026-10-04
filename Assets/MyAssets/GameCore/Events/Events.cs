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
        public ActionRejectedEvent(string reason) => Reason = reason ?? "Rejected";
        public bool Equals(ActionRejectedEvent other) => Reason == other.Reason;
        public override bool Equals(object obj) => obj is ActionRejectedEvent other && Equals(other);
        public override int GetHashCode() => Reason?.GetHashCode() ?? 0;
    }

    public readonly struct SetupStepChangedEvent : IGameEvent, System.IEquatable<SetupStepChangedEvent>
    {
        public readonly SetupRound Round;
        public readonly PlayerId CurrentPlayer;

        public SetupStepChangedEvent(SetupRound round, PlayerId currentPlayer)
        {
            Round = round;
            CurrentPlayer = currentPlayer;
        }

        public bool Equals(SetupStepChangedEvent other) => Round == other.Round && CurrentPlayer.Equals(other.CurrentPlayer);
        public override bool Equals(object obj) => obj is SetupStepChangedEvent other && Equals(other);
        public override int GetHashCode() => ((int)Round * 397) ^ CurrentPlayer.Value;
    }

    public readonly struct ColonyPlacedEvent : IGameEvent, System.IEquatable<ColonyPlacedEvent>
    {
        public readonly PlayerId Player;
        public readonly NodeId Node;

        public ColonyPlacedEvent(PlayerId player, NodeId node)
        {
            Player = player;
            Node = node;
        }

        public bool Equals(ColonyPlacedEvent other) => Player.Equals(other.Player) && Node.Equals(other.Node);
        public override bool Equals(object obj) => obj is ColonyPlacedEvent other && Equals(other);
        public override int GetHashCode() => (Player.Value * 397) ^ Node.Value;
    }

    public readonly struct SpaceportPlacedEvent : IGameEvent, System.IEquatable<SpaceportPlacedEvent>
    {
        public readonly PlayerId Player;
        public readonly NodeId Node;

        public SpaceportPlacedEvent(PlayerId player, NodeId node)
        {
            Player = player;
            Node = node;
        }

        public bool Equals(SpaceportPlacedEvent other) => Player.Equals(other.Player) && Node.Equals(other.Node);
        public override bool Equals(object obj) => obj is SpaceportPlacedEvent other && Equals(other);
        public override int GetHashCode() => (Player.Value * 397) ^ Node.Value;
    }

    public readonly struct SetupUpgradeGrantedEvent : IGameEvent, System.IEquatable<SetupUpgradeGrantedEvent>
    {
        public readonly PlayerId Player;
        public readonly UpgradeType Upgrade;

        public SetupUpgradeGrantedEvent(PlayerId player, UpgradeType upgrade)
        {
            Player = player;
            Upgrade = upgrade;
        }

        public bool Equals(SetupUpgradeGrantedEvent other) => Player.Equals(other.Player) && Upgrade == other.Upgrade;
        public override bool Equals(object obj) => obj is SetupUpgradeGrantedEvent other && Equals(other);
        public override int GetHashCode() => (Player.Value * 397) ^ (int)Upgrade;
    }

    public readonly struct StartingResourcesGrantedEvent : IGameEvent, System.IEquatable<StartingResourcesGrantedEvent>
    {
        public readonly PlayerId Player;
        public readonly int Count;

        public StartingResourcesGrantedEvent(PlayerId player, int count)
        {
            Player = player;
            Count = count;
        }

        public bool Equals(StartingResourcesGrantedEvent other) => Player.Equals(other.Player) && Count == other.Count;
        public override bool Equals(object obj) => obj is StartingResourcesGrantedEvent other && Equals(other);
        public override int GetHashCode() => (Player.Value * 397) ^ Count;
    }

    public readonly struct FameMedalGrantedEvent : IGameEvent, System.IEquatable<FameMedalGrantedEvent>
    {
        public readonly PlayerId Player;
        public FameMedalGrantedEvent(PlayerId player) => Player = player;
        public bool Equals(FameMedalGrantedEvent other) => Player.Equals(other.Player);
        public override bool Equals(object obj) => obj is FameMedalGrantedEvent other && Equals(other);
        public override int GetHashCode() => Player.Value;
    }

    public readonly struct ShipPlacedEvent : IGameEvent, System.IEquatable<ShipPlacedEvent>
    {
        public readonly PlayerId Player;
        public readonly ShipId Ship;
        public readonly NodeId Node;

        public ShipPlacedEvent(PlayerId player, ShipId ship, NodeId node)
        {
            Player = player;
            Ship = ship;
            Node = node;
        }

        public bool Equals(ShipPlacedEvent other) => Player.Equals(other.Player) && Ship.Equals(other.Ship) && Node.Equals(other.Node);
        public override bool Equals(object obj) => obj is ShipPlacedEvent other && Equals(other);
        public override int GetHashCode() => ((Player.Value * 397) ^ Ship.Value * 397) ^ Node.Value;
    }

    public readonly struct SetupCompletedEvent : IGameEvent, System.IEquatable<SetupCompletedEvent>
    {
        public readonly PlayerId FirstPlayer;
        public SetupCompletedEvent(PlayerId firstPlayer) => FirstPlayer = firstPlayer;

        public bool Equals(SetupCompletedEvent other) => FirstPlayer.Equals(other.FirstPlayer);
        public override bool Equals(object obj) => obj is SetupCompletedEvent other && Equals(other);
        public override int GetHashCode() => FirstPlayer.Value;
    }

    public readonly struct ShipMovedEvent : IGameEvent, System.IEquatable<ShipMovedEvent>
    {
        public readonly PlayerId Player;
        public readonly ShipId Ship;
        public readonly NodeId From;
        public readonly NodeId To;

        public ShipMovedEvent(PlayerId player, ShipId ship, NodeId from, NodeId to)
        {
            Player = player;
            Ship = ship;
            From = from;
            To = to;
        }

        public bool Equals(ShipMovedEvent other) =>
            Player.Equals(other.Player) && Ship.Equals(other.Ship) && From.Equals(other.From) && To.Equals(other.To);

        public override bool Equals(object obj) => obj is ShipMovedEvent other && Equals(other);
        public override int GetHashCode() => ((Player.Value * 397) ^ Ship.Value * 397) ^ (From.Value * 397) ^ To.Value;
    }

    public readonly struct SectorRevealedEvent : IGameEvent, System.IEquatable<SectorRevealedEvent>
    {
        public readonly SlotId Slot;
        public readonly SectorPieceId Piece;
        public readonly int Rotation;

        public SectorRevealedEvent(SlotId slot, SectorPieceId piece, int rotation)
        {
            Slot = slot;
            Piece = piece;
            Rotation = rotation;
        }

        public bool Equals(SectorRevealedEvent other) => Slot.Equals(other.Slot) && Piece.Equals(other.Piece) && Rotation == other.Rotation;
        public override bool Equals(object obj) => obj is SectorRevealedEvent other && Equals(other);
        public override int GetHashCode() => ((Slot.Value * 397) ^ Piece.Value * 397) ^ Rotation;
    }

    public readonly struct PlanetTokenLockedEvent : IGameEvent, System.IEquatable<PlanetTokenLockedEvent>
    {
        public readonly PlanetInstanceId Planet;
        public PlanetTokenLockedEvent(PlanetInstanceId planet) => Planet = planet;
        public bool Equals(PlanetTokenLockedEvent other) => Planet.Equals(other.Planet);
        public override bool Equals(object obj) => obj is PlanetTokenLockedEvent other && Equals(other);
        public override int GetHashCode() => Planet.GetHashCode();
    }

    public readonly struct PlanetTokenAssignedEvent : IGameEvent, System.IEquatable<PlanetTokenAssignedEvent>
    {
        public readonly PlanetInstanceId Planet;
        public readonly int TokenId;

        public PlanetTokenAssignedEvent(PlanetInstanceId planet, int tokenId)
        {
            Planet = planet;
            TokenId = tokenId;
        }

        public bool Equals(PlanetTokenAssignedEvent other) => Planet.Equals(other.Planet) && TokenId == other.TokenId;
        public override bool Equals(object obj) => obj is PlanetTokenAssignedEvent other && Equals(other);
        public override int GetHashCode() => (Planet.GetHashCode() * 397) ^ TokenId;
    }
}
