namespace MyAssets.GameCore
{
    public interface IGameAction { }

    public readonly record struct StartMatchAction(int Seed) : IGameAction;

    public readonly record struct MoveShipAction(PlayerId Player, ShipId Ship, NodeId From, NodeId To) : IGameAction;

    public readonly record struct EndTurnAction(PlayerId Player) : IGameAction;

    public readonly record struct SettleNextToPlanetAction(PlayerId Player, PlanetInstanceId Planet) : IGameAction;
}
