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

    public readonly struct MoveShipAction : IGameAction, System.IEquatable<MoveShipAction>
    {
        public readonly PlayerId Player;
        public readonly ShipId Ship;
        public readonly NodeId From;
        public readonly NodeId To;

        public MoveShipAction(PlayerId player, ShipId ship, NodeId from, NodeId to)
        {
            Player = player; Ship = ship; From = from; To = to;
        }

        public bool Equals(MoveShipAction other) =>
            Player.Equals(other.Player) && Ship.Equals(other.Ship) && From.Equals(other.From) && To.Equals(other.To);

        public override bool Equals(object obj) => obj is MoveShipAction other && Equals(other);

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

    public readonly struct EndTurnAction : IGameAction, System.IEquatable<EndTurnAction>
    {
        public readonly PlayerId Player;
        public EndTurnAction(PlayerId player) => Player = player;

        public bool Equals(EndTurnAction other) => Player.Equals(other.Player);
        public override bool Equals(object obj) => obj is EndTurnAction other && Equals(other);
        public override int GetHashCode() => Player.GetHashCode();
    }

    public readonly struct SettleNextToPlanetAction : IGameAction, System.IEquatable<SettleNextToPlanetAction>
    {
        public readonly PlayerId Player;
        public readonly PlanetInstanceId Planet;

        public SettleNextToPlanetAction(PlayerId player, PlanetInstanceId planet)
        {
            Player = player; Planet = planet;
        }

        public bool Equals(SettleNextToPlanetAction other) =>
            Player.Equals(other.Player) && Planet.Equals(other.Planet);

        public override bool Equals(object obj) => obj is SettleNextToPlanetAction other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Player.GetHashCode() * 397) ^ Planet.GetHashCode();
            }
        }
    }
}
