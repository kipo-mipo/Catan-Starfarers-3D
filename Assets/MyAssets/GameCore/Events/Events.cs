namespace MyAssets.GameCore
{
    public interface IGameEvent { }

    public readonly record struct MatchStartedEvent(int Seed) : IGameEvent;

    public readonly record struct ActionRejectedEvent(string Reason) : IGameEvent;

    public readonly record struct ShipMovedEvent(PlayerId Player, ShipId Ship, NodeId From, NodeId To) : IGameEvent;

    public readonly record struct SectorRevealedEvent(SlotId Slot, SectorPieceId Piece, int Rotation) : IGameEvent;

    public readonly record struct PlanetTokenLockedEvent(PlanetInstanceId Planet, TokenPoolId Pool) : IGameEvent;

    public readonly record struct PlanetTokenAssignedEvent(PlanetInstanceId Planet, TokenPoolId Pool, int TokenId, int RollA, int? RollB) : IGameEvent;

}
