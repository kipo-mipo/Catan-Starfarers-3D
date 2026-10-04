namespace MyAssets.GameCore
{
    public interface IGameAction { }

    public readonly struct StartMatchAction : IGameAction, System.IEquatable<StartMatchAction>
    {
        public readonly int Seed;
        public StartMatchAction(int seed) => Seed = seed;

        public bool Equals(StartMatchAction other) => Seed == other.Seed;
        public override bool Equals(object obj) => obj is StartMatchAction other && Equals(other);
        public override int GetHashCode() => Seed;
    }

    public readonly struct ChooseStartingNodeAction : IGameAction, System.IEquatable<ChooseStartingNodeAction>
    {
        public readonly PlayerId Player;
        public readonly NodeId Node;

        public ChooseStartingNodeAction(PlayerId player, NodeId node)
        {
            Player = player;
            Node = node;
        }

        public bool Equals(ChooseStartingNodeAction other) => Player.Equals(other.Player) && Node.Equals(other.Node);
        public override bool Equals(object obj) => obj is ChooseStartingNodeAction other && Equals(other);
        public override int GetHashCode() => (Player.Value * 397) ^ Node.Value;
    }

    public readonly struct CompleteSetupSpaceportShipAction : IGameAction, System.IEquatable<CompleteSetupSpaceportShipAction>
    {
        public readonly PlayerId Player;
        public readonly NodeId ColonyNode;
        public readonly NodeId ShipNode;
        public readonly SetupShipType ShipType;
        public readonly UpgradeType Upgrade;

        public CompleteSetupSpaceportShipAction(PlayerId player, NodeId colonyNode, NodeId shipNode, SetupShipType shipType, UpgradeType upgrade)
        {
            Player = player;
            ColonyNode = colonyNode;
            ShipNode = shipNode;
            ShipType = shipType;
            Upgrade = upgrade;
        }

        public bool Equals(CompleteSetupSpaceportShipAction other) =>
            Player.Equals(other.Player) && ColonyNode.Equals(other.ColonyNode) && ShipNode.Equals(other.ShipNode) &&
            ShipType == other.ShipType && Upgrade == other.Upgrade;

        public override bool Equals(object obj) => obj is CompleteSetupSpaceportShipAction other && Equals(other);
        public override int GetHashCode() => (((Player.Value * 397) ^ ColonyNode.Value) * 397 ^ ShipNode.Value) * 397 ^ (int)ShipType * 17 ^ (int)Upgrade;
    }

    public readonly struct MoveShipAction : IGameAction, System.IEquatable<MoveShipAction>
    {
        public readonly PlayerId Player;
        public readonly ShipId Ship;
        public readonly NodeId From;
        public readonly NodeId To;

        public MoveShipAction(PlayerId player, ShipId ship, NodeId from, NodeId to)
        {
            Player = player;
            Ship = ship;
            From = from;
            To = to;
        }

        public bool Equals(MoveShipAction other) =>
            Player.Equals(other.Player) && Ship.Equals(other.Ship) && From.Equals(other.From) && To.Equals(other.To);

        public override bool Equals(object obj) => obj is MoveShipAction other && Equals(other);
        public override int GetHashCode() => ((Player.Value * 397) ^ Ship.Value * 397) ^ (From.Value * 397) ^ To.Value;
    }

    public readonly struct EndTurnAction : IGameAction, System.IEquatable<EndTurnAction>
    {
        public readonly PlayerId Player;
        public EndTurnAction(PlayerId player) => Player = player;

        public bool Equals(EndTurnAction other) => Player.Equals(other.Player);
        public override bool Equals(object obj) => obj is EndTurnAction other && Equals(other);
        public override int GetHashCode() => Player.Value;
    }

    public readonly struct SettleNextToPlanetAction : IGameAction, System.IEquatable<SettleNextToPlanetAction>
    {
        public readonly PlayerId Player;
        public readonly NodeId Node;

        public SettleNextToPlanetAction(PlayerId player, NodeId node)
        {
            Player = player;
            Node = node;
        }

        public bool Equals(SettleNextToPlanetAction other) => Player.Equals(other.Player) && Node.Equals(other.Node);
        public override bool Equals(object obj) => obj is SettleNextToPlanetAction other && Equals(other);
        public override int GetHashCode() => (Player.Value * 397) ^ Node.Value;
    }
}
